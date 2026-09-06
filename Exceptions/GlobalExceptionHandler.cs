using Microsoft.AspNetCore.Diagnostics;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "An unexpected error occurred.");

        httpContext.Response.StatusCode = 500;

        await httpContext.Response.WriteAsJsonAsync(
            new
            {
                statusCode = 500,
                message = "Something went wrong."
            },
            cancellationToken);

        return true;
    }
}