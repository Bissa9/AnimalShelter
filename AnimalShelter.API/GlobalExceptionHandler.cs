using Microsoft.AspNetCore.Diagnostics;
using AnimalShelter.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace AnimalShelter.API;

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

    var statusCode =
        exception switch
        {
            ResourceNotFoundException =>
                StatusCodes.Status404NotFound,

            ConflictException =>
                StatusCodes.Status409Conflict,

            Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException =>
                StatusCodes.Status409Conflict,

            _ =>
                StatusCodes.Status500InternalServerError
        };

    var problemDetails = new ProblemDetails
    {
        Status = statusCode,
        Title = statusCode switch
        {
            StatusCodes.Status404NotFound => "Resource not found",
            StatusCodes.Status409Conflict => "Conflict",
            _ => "An unexpected error occurred."
        },
        Detail = statusCode == StatusCodes.Status500InternalServerError
            ? "An unexpected error occurred."
            : exception.Message
    };

httpContext.Response.StatusCode = statusCode;
httpContext.Response.ContentType = "application/problem+json";

await httpContext.Response.WriteAsJsonAsync(
    problemDetails,
    cancellationToken);

    return true;
}


}
