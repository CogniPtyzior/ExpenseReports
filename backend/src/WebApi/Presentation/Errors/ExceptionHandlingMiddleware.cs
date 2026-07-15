// Converts known application exceptions into stable HTTP error responses.
using FluentValidation;
using WebApi.Core.Errors;
using WebApi.Core.Exceptions;

namespace WebApi.Presentation.Errors;

/// <summary>
/// Presentation middleware ensuring domain and application errors never leak implementation details.
/// </summary>
internal sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException exception)
        {
            var message = exception.Errors.FirstOrDefault()?.ErrorMessage ?? exception.Message;
            await WriteAsync(context, StatusCodes.Status400BadRequest, "validation.failed", message);
        }
        catch (DomainException exception)
        {
            await WriteAsync(context, StatusCodes.Status400BadRequest, exception.Code, exception.Message);
        }
        catch (NotFoundException exception)
        {
            await WriteAsync(context, StatusCodes.Status404NotFound, exception.Code, exception.Message);
        }
        catch (ConflictException exception)
        {
            await WriteAsync(context, StatusCodes.Status409Conflict, exception.Code, exception.Message);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Unhandled API exception.");
            await WriteAsync(context, StatusCodes.Status500InternalServerError, "internal.error", "An unexpected error occurred.");
        }
    }

    private static async Task WriteAsync(HttpContext context, int statusCode, string code, string message)
    {
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new ApiError(code, message));
    }
}