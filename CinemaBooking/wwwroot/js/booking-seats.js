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
    const availability = document.getElementById("availability-message");
    const submitButton = document.getElementById("submit-booking");

    const formatter = new Intl.NumberFormat("vi-VN");

    let price = null;
    let ready = false;
    let submitting = false;
    let loading = false;
    let stopped = false;
    let lastUpdated = 0;

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
            submitting ||
            selected.length === 0 ||
            selected.length > 10;
    }

    function stopWithMessage(message) {
        stopped = true;
        ready = false;
        syncText.textContent = message;
        updateSummary();
    }

    async function refreshSeats() {
        if (loading || submitting || stopped) return;

        loading = true;

        const controller = new AbortController();
        const timeout = setTimeout(() => controller.abort(), 10000);

        try {
            const response = await fetch(form.dataset.seatsUrl, {
                method: "GET",
                credentials: "same-origin",
                cache: "no-store",
                headers: {
                    "Accept": "application/json"
                },
                signal: controller.signal
            });

            // Không xử lý trang HTML đăng nhập như dữ liệu JSON.
            if (response.redirected ||
                response.status === 401 ||
                response.status === 403) {
                stopWithMessage(
                    "Phiên đăng nhập không còn hợp lệ. " +
                    "Vui lòng tải lại trang và đăng nhập.");
                return;
            }

            if (response.status === 404) {
                stopWithMessage(
                    "Suất chiếu không còn mở bán. " +
                    "Vui lòng quay lại danh sách phim.");
                return;
            }

            if (!response.ok) {
                throw new Error("Không đọc được trạng thái ghế.");
            }

            const data = await response.json();

            if (!Array.isArray(data.seats) ||
                typeof data.basePrice !== "number" ||
                !Number.isFinite(data.basePrice) ||
                data.basePrice <= 0) {
                throw new Error("Dữ liệu không hợp lệ.");
            }

            // Không sửa giao diện nếu người dùng đã gửi đơn
            // trong lúc yêu cầu API đang chạy.
            if (submitting) return;

            const seatsById = new Map(
                data.seats.map(seat => [String(seat.seatId), seat])
            );

            // Phòng hoặc sơ đồ ghế đã thay đổi: cần tải lại
            // toàn bộ trang để thông tin hiển thị thống nhất.
            if (data.seats.length !== choices.length ||
                choices.some(input => !seatsById.has(input.value))) {
                stopWithMessage(
                    "Sơ đồ ghế đã thay đổi. Vui lòng tải lại trang.");
                return;
            }

            const removed = [];
            const priceChanged = price !== null && price !== data.basePrice;

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

            if (priceChanged) {
                // Yêu cầu chọn lại để người dùng nhận biết giá mới.
                choices.forEach(input => input.checked = false);

                errorText.textContent =
                    "Giá vé đã thay đổi. Vui lòng kiểm tra giá và chọn lại ghế.";
            } else if (removed.length > 0) {
                errorText.textContent =
                    "Ghế " + removed.join(", ") +
                    " đã được đặt hoặc ngừng sử dụng. Vui lòng chọn ghế khác.";
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
            if (!submitting) {
                ready = false;
                syncText.textContent =
                    "Chưa cập nhật được trạng thái ghế. " +
                    "Tạm khóa đặt vé và sẽ tự thử lại.";
                updateSummary();
            }
        } finally {
            clearTimeout(timeout);
            loading = false;
        }
    }

    choices.forEach(input => {
        input.addEventListener("change", () => {
            errorText.textContent = "";

            if (selectedSeats().length > 10) {
                input.checked = false;
                errorText.textContent =
                    "Mỗi lần chỉ được đặt tối đa 10 ghế.";
            }

            updateSummary();
        });
    });

    form.addEventListener("submit", event => {
        const count = selectedSeats().length;

        if (submitting || !ready || count < 1 || count > 10) {
            event.preventDefault();
            return;
        }

        // Khi tab để lâu hoặc trình duyệt tạm ngừng cập nhật.
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

    // Khi quay lại trang bằng nút Back của trình duyệt.
    window.addEventListener("pageshow", () => {
        submitting = false;
        stopped = false;
        ready = false;
        submitButton.textContent = "Xác nhận đặt vé";
        updateSummary();
        void refreshSeats();
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

    updateSummary();
    void refreshSeats();
})();