using Microsoft.EntityFrameworkCore;
using MultiTenantEmployeeApi.Common;
using MultiTenantEmployeeApi.Data;

namespace MultiTenantEmployeeApi.Tenancy;

public class TenantMiddleware
{
    public const string TenantHeader = "X-Tenant-Id";

    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider, AppDbContext db)
    {
        // only the api routes need a tenant (swagger etc. don't)
        if (!context.Request.Path.StartsWithSegments("/api"))
        {
            await _next(context);
            return;
        }

        var headerValue = context.Request.Headers[TenantHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(headerValue))
        {
            await WriteError(context, StatusCodes.Status400BadRequest, "TenantMissing", $"{TenantHeader} header is required");
            return;
        }

        if (!Guid.TryParse(headerValue, out var tenantId))
        {
            await WriteError(context, StatusCodes.Status400BadRequest, "TenantInvalid", $"{TenantHeader} must be a valid UUID");
            return;
        }

        var tenantExists = await db.Tenants.AnyAsync(t => t.Id == tenantId);
        if (!tenantExists)
        {
            await WriteError(context, StatusCodes.Status403Forbidden, "TenantNotFound", "Unknown tenant");
            return;
        }

        tenantProvider.SetTenant(tenantId);

        await _next(context);
    }

    private static Task WriteError(HttpContext context, int statusCode, string code, string message)
    {
        context.Response.StatusCode = statusCode;
        return context.Response.WriteAsJsonAsync(ApiResponse<object>.Fail(new ApiError(code, message)));
    }
}
