using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Desaka.ServiceDefaults.ApiKey;

public sealed class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ApiKeyOptions _options;

    public ApiKeyMiddleware(RequestDelegate next, IOptions<ApiKeyOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var headerName = string.IsNullOrWhiteSpace(_options.HeaderName) ? "X-Api-Key" : _options.HeaderName;
        if (!context.Request.Headers.TryGetValue(headerName, out var provided) || string.IsNullOrWhiteSpace(provided))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Missing API key.");
            return;
        }

        if (!string.Equals(provided.ToString(), _options.Key, StringComparison.Ordinal))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid API key.");
            return;
        }

        await _next(context);
    }
}
