using Microsoft.AspNetCore.Mvc;
using MyWorkItem.Api.Infrastructure.Exceptions;

namespace MyWorkItem.Api.Infrastructure;

public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        logger.LogError(exception, "Unhandled exception caught by middleware.");

        switch (exception)
        {
            case AppValidationException validationException:
                await WriteValidationProblemAsync(context, validationException);
                return;
            case AppUnauthorizedException unauthorizedException:
                await WriteProblemAsync(context, StatusCodes.Status401Unauthorized, unauthorizedException.Message);
                return;
            case AppForbiddenException forbiddenException:
                await WriteProblemAsync(context, StatusCodes.Status403Forbidden, forbiddenException.Message);
                return;
            case AppNotFoundException notFoundException:
                await WriteProblemAsync(context, StatusCodes.Status404NotFound, notFoundException.Message);
                return;
            default:
                await WriteProblemAsync(
                    context,
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.");
                return;
        }
    }

    private static Task WriteValidationProblemAsync(
        HttpContext context,
        AppValidationException exception)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;

        var problemDetails = new ValidationProblemDetails(
            exception.Errors.ToDictionary(entry => entry.Key, entry => entry.Value))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed.",
            Detail = exception.Message,
            Instance = context.Request.Path,
        };

        return context.Response.WriteAsJsonAsync(problemDetails);
    }

    private static Task WriteProblemAsync(
        HttpContext context,
        int statusCode,
        string detail)
    {
        context.Response.StatusCode = statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = "Request failed.",
            Detail = detail,
            Instance = context.Request.Path,
        };

        return context.Response.WriteAsJsonAsync(problemDetails);
    }
}
