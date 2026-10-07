using FluentValidation;
using MultiTenantEmployeeApi.Common.Exceptions;

namespace MultiTenantEmployeeApi.Common.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            var details = ex.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());

            await WriteError(context, StatusCodes.Status400BadRequest,
                new ApiError("ValidationFailed", "One or more validation errors occurred", details));
        }
        catch (NotFoundException ex)
        {
            await WriteError(context, StatusCodes.Status404NotFound, new ApiError("NotFound", ex.Message));
        }
        catch (ConflictException ex)
        {
            await WriteError(context, StatusCodes.Status409Conflict, new ApiError("Conflict", ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            await WriteError(context, StatusCodes.Status500InternalServerError,
                new ApiError("ServerError", "Something went wrong"));
        }
    }

    private static Task WriteError(HttpContext context, int statusCode, ApiError error)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(error));
    }
}
