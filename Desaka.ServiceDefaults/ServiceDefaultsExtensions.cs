using Desaka.ServiceDefaults.ApiKey;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Desaka.ServiceDefaults;

public static class ServiceDefaultsExtensions
{
    public static IServiceCollection AddDesakaServiceDefaults(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks();
        services.Configure<ApiKeyOptions>(configuration.GetSection(ApiKeyOptions.SectionName));
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        return services;
    }

    public static IApplicationBuilder UseDesakaApiKey(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ApiKeyMiddleware>();
    }

    public static IApplicationBuilder UseDesakaSwaggerIfDev(this IApplicationBuilder app, IWebHostEnvironment env)
    {
        if (env.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }
        return app;
    }

    public static IEndpointRouteBuilder MapDesakaHealth(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health");
        return endpoints;
    }
}
