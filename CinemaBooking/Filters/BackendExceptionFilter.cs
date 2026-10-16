using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CinemaBooking.Filters;

// Local action handlers keep form values. This filter covers failures outside those handlers.
public sealed class BackendExceptionFilter(ILogger<BackendExceptionFilter> logger) : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is OperationCanceledException &&
            context.HttpContext.RequestAborted.IsCancellationRequested)
        {
            return;
        }

        var isApi = context.HttpContext.Request.Path.StartsWithSegments("/api");
        var isStorageFailure = context.Exception is DbUpdateException or SqlException or TimeoutException;
        if (!isApi && !isStorageFailure)
        {
            return;
        }

        logger.LogError(context.Exception, "Không thể xử lý {Method} {Path}",
            context.HttpContext.Request.Method, context.HttpContext.Request.Path);

        const string message = "Không thể xử lý dữ liệu lúc này. Vui lòng tải lại trang và kiểm tra trạng thái trước khi thử lại.";
        var status = isStorageFailure || context.Exception is IOException
            ? StatusCodes.Status503ServiceUnavailable
            : StatusCodes.Status500InternalServerError;

        context.Result = isApi
            ? new ObjectResult(new { message }) { StatusCode = status }
            : new ContentResult
            {
                Content = message,
                ContentType = "text/plain; charset=utf-8",
                StatusCode = status
            };
        context.ExceptionHandled = true;
    }
}
