(() => {
    "use strict";

    const form = document.getElementById("booking-form");
    if (!form) return;

    const choices = [...form.querySelectorAll(".seat-choice")];
    const selectedText = document.getElementById("selected-seats");
    const totalText = document.getElementById("total-price");
    const unitPriceText = document.getElementById("unit-price");
    const errorText = document.getElementById("seat-error");
    const syncText = document.getElementById("seat-sync-status");
    const selectionText = document.getElementById("selection-status");
    const availability = document.getElementById("availability-message");
    const submitButton = document.getElementById("submit-booking");

    const token = form.querySelector(
        'input[name="__RequestVerificationToken"]'
    )?.value;

    const formatter = new Intl.NumberFormat("vi-VN");

    let price = null;
    let ready = false;
    let restored = false;
    let submitting = false;
    let loading = false;
    let saving = false;
    let dirty = false;
    let stopped = false;
    let lastUpdated = 0;
    let revision = 0;

    function selectedSeats() {
        return choices.filter(input => input.checked && !input.disabled);
    }

    function updateSummary() {
        const selected = selectedSeats();

        selectedText.textContent = selected.length
            ? selected.map(input => input.dataset.label).join(", ")
            : "Chưa chọn";

        totalText.textContent =
            formatter.format(selected.length * (price ?? 0)) + " đ";

        submitButton.disabled =
            !ready ||
            !restored ||
            submitting ||
            saving ||
            dirty ||
            selected.length === 0 ||
            selected.length > 10;
    }

    function stopWithMessage(message) {
        stopped = true;
        ready = false;
        syncText.textContent = message;
        updateSummary();
    }

    async function requestJson(url, options = {}) {
        const response = await fetch(url, {
            credentials: "same-origin",
            cache: "no-store",
            ...options
        });

        if (response.redirected ||
            response.status === 401 ||
            response.status === 403) {
            stopWithMessage(
                "Phiên đăng nhập không còn hợp lệ. " +
                "Vui lòng tải lại trang và đăng nhập.");
            throw new Error("Authentication required");
        }

        if (response.status === 404) {
            stopWithMessage(
                "Suất chiếu không còn mở bán. " +
                "Vui lòng quay lại danh sách phim.");
            throw new Error("Showtime unavailable");
        }

        if (!response.ok) {
            const error = await response.json().catch(() => null);
            throw new Error(error?.message || "Không thực hiện được yêu cầu.");
        }

        return await response.json();
    }

    function markSelectionChanged() {
        revision++;
        dirty = true;
        selectionText.textContent = "Đang lưu lựa chọn...";
        updateSummary();
    }

    async function saveSelection() {
        if (!restored || saving || loading ||
            submitting || stopped || !dirty) {
            return;
        }

        saving = true;
        selectionText.textContent = "Đang lưu lựa chọn...";
        updateSummary();

        try {
            while (dirty && !submitting && !stopped) {
                const sentRevision = revision;
                const ids = selectedSeats().map(input => Number(input.value));

                const controller = new AbortController();
                const timeout = setTimeout(() => controller.abort(), 10000);
                let data;

                try {
                    data = await requestJson(form.dataset.selectionUrl, {
                        method: "POST",
                        headers: {
                            "Accept": "application/json",
                            "Content-Type": "application/json",
                            "RequestVerificationToken": token
                        },
                        body: JSON.stringify({ seatIds: ids }),
                        signal: controller.signal
                    });
                } finally {
                    clearTimeout(timeout);
                }

                if (!Array.isArray(data.seatIds)) {
                    throw new Error("Dữ liệu lựa chọn không hợp lệ.");
                }

                if (sentRevision !== revision) {
                    continue;
                }

                const accepted = new Set(data.seatIds.map(String));
                const removed = [];

                for (const input of choices) {
                    if (input.checked && !accepted.has(input.value)) {
                        input.checked = false;
                        removed.push(input.dataset.label);
                    }
                }

                if (removed.length > 0) {
                    errorText.textContent =
                        "Ghế " + removed.join(", ") +
                        " không còn hợp lệ và đã được bỏ chọn.";
                }

                dirty = false;

                selectionText.textContent = accepted.size > 0
                    ? "Đã lưu lựa chọn tạm. Ghế chưa được giữ chỗ."
                    : "Đã xóa lựa chọn tạm.";

                updateSummary();
            }
        } catch (error) {
            if (!stopped) {
                const message = error.name === "AbortError"
                    ? "Lưu lựa chọn quá thời gian chờ."
                    : "Chưa lưu được lựa chọn.";
                selectionText.textContent = message +
                    " Lựa chọn hiện tại vẫn được giữ; hệ thống sẽ thử lại.";
            }
        } finally {
            saving = false;
            updateSummary();
        }
    }

    async function refreshSeats() {
        if (loading || saving || submitting || stopped) return;

        loading = true;

        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 10000);

        try {
            const data = await requestJson(form.dataset.seatsUrl, {
                headers: { "Accept": "application/json" },
                signal: controller.signal
            });

            if (!Array.isArray(data.seats) ||
                typeof data.basePrice !== "number" ||
                !Number.isFinite(data.basePrice) ||
                data.basePrice <= 0) {
                throw new Error("Dữ liệu ghế không hợp lệ.");
            }

            if (submitting) return;

            const seatsById = new Map(
                data.seats.map(seat => [String(seat.seatId), seat])
            );

            if (data.seats.length !== choices.length ||
                choices.some(input => !seatsById.has(input.value))) {
                stopWithMessage(
                    "Sơ đồ ghế đã thay đổi. Vui lòng tải lại trang.");
                return;
            }

            let saved = null;

            if (!restored) {
                saved = await requestJson(form.dataset.selectionUrl, {
                    headers: { "Accept": "application/json" },
                    signal: controller.signal
                });

                if (!Array.isArray(saved.seatIds) ||
                    !Array.isArray(saved.removedSeatIds)) {
                    throw new Error("Dữ liệu session không hợp lệ.");
                }
            }

            const priceChanged =
                price !== null && price !== data.basePrice;

            const removed = [];

            for (const input of choices) {
                const seat = seatsById.get(input.value);
                const available = seat.isActive && !seat.isBooked;
                const label = input.nextElementSibling;

                if (!available && input.checked) {
                    removed.push(input.dataset.label);
                    input.checked = false;
                }

                input.disabled = !available;

                label.classList.toggle("btn-secondary", !available);
                label.classList.toggle("btn-outline-primary", available);

                label.title = seat.isBooked
                    ? "Đã đặt"
                    : !seat.isActive
                        ? "Ngừng sử dụng"
                        : "Ghế trống";
            }

            price = data.basePrice;
            unitPriceText.textContent = formatter.format(price) + " đ";

            if (saved !== null) {
                const savedIds = new Set(saved.seatIds.map(String));

                for (const input of choices) {
                    input.checked =
                        !input.disabled && savedIds.has(input.value);
                }

                restored = true;

                if (saved.removedSeatIds.length > 0) {
                    errorText.textContent =
                        "Một số ghế đã lưu không còn trống " +
                        "hoặc đã ngừng sử dụng nên không được khôi phục.";
                }

                markSelectionChanged();
            }

            if (priceChanged) {
                choices.forEach(input => input.checked = false);
                errorText.textContent =
                    "Giá vé đã thay đổi. Vui lòng kiểm tra và chọn lại ghế.";
                markSelectionChanged();
            } else if (removed.length > 0) {
                errorText.textContent =
                    "Ghế " + removed.join(", ") +
                    " đã được đặt hoặc ngừng sử dụng.";
                markSelectionChanged();
            }

            availability.classList.toggle(
                "d-none",
                choices.some(input => !input.disabled)
            );

            ready = true;
            lastUpdated = Date.now();

            syncText.textContent =
                "Đã cập nhật lúc " +
                new Date().toLocaleTimeString("vi-VN") +
                ". Tự kiểm tra mỗi 5 giây.";

            updateSummary();
        } catch {
            if (!submitting && !stopped) {
                ready = false;
                syncText.textContent =
                    "Chưa cập nhật được dữ liệu. " +
                    "Tạm khóa đặt vé và sẽ tự thử lại.";
                updateSummary();
            }
        } finally {
            clearTimeout(timeout);
            loading = false;
        }

        await saveSelection();
    }

    choices.forEach(input => {
        input.addEventListener("change", () => {
            errorText.textContent = "";

            if (selectedSeats().length > 10) {
                input.checked = false;
                errorText.textContent =
                    "Mỗi lần chỉ được đặt tối đa 10 ghế.";
            }

            markSelectionChanged();
            void saveSelection();
        });
    });

    form.addEventListener("submit", event => {
        const count = selectedSeats().length;

        if (submitting || !ready || !restored ||
            saving || dirty || count < 1 || count > 10) {
            event.preventDefault();
            return;
        }

        if (Date.now() - lastUpdated > 15000) {
            event.preventDefault();
            ready = false;
            syncText.textContent =
                "Đang kiểm tra lại ghế. Vui lòng xác nhận sau khi cập nhật.";
            updateSummary();
            void refreshSeats();
            return;
        }

        submitting = true;
        submitButton.disabled = true;
        submitButton.textContent = "Đang xử lý...";
    });

    choices.forEach(input => input.disabled = true);

    window.addEventListener("pageshow", event => {
        if (event.persisted) {
            window.location.reload();
        }
    });

    document.addEventListener("visibilitychange", () => {
        if (!document.hidden && !submitting) {
            ready = false;
            updateSummary();
            void refreshSeats();
        }
    });

    setInterval(() => {
        if (!document.hidden) {
            void refreshSeats();
        }
    }, 5000);

    if (!token) {
        stopWithMessage(
            "Thiếu mã xác thực biểu mẫu. Vui lòng tải lại trang.");
        return;
    }

    updateSummary();
    void refreshSeats();
})();
