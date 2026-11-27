using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using PersonalFinanceTracker.Services.Interfaces;

namespace PersonalFinanceTracker.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IWebHostEnvironment _env;
    private readonly IConfiguration _configuration;

    public TenantResolutionMiddleware(RequestDelegate next, IWebHostEnvironment env, IConfiguration configuration)
    {
        _next = next;
        _env = env;
        _configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        string? tenantId = null;

        foreach (var header in context.Request.Headers)
        {
            if (string.Equals(header.Key, "X-Tenant-ID", StringComparison.OrdinalIgnoreCase))
            {
                tenantId = header.Value.FirstOrDefault();
                break;
            }
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            if (_env.IsDevelopment() || 
                _env.IsEnvironment("Test") || 
                _env.IsEnvironment("Testing") || 
                string.Equals(_env.EnvironmentName, "Test", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(_env.EnvironmentName, "Testing", StringComparison.OrdinalIgnoreCase))
            {
                tenantId = _configuration["TenantSettings:DefaultTenantId"] ?? "default-tenant";
            }
            else
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new { message = "X-Tenant-ID header is missing." });
                return;
            }
        }

        tenantContext.TenantId = tenantId.Trim();
        await _next(context);
    }
}
