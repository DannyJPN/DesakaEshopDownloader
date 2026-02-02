using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Desaka.EventBus;

public static class EventBusServiceCollectionExtensions
{
    public static IServiceCollection AddDesakaEventBus(this IServiceCollection services, IConfiguration configuration)
    {
        var section = configuration.GetSection(EventBusOptions.SectionName);
        services.Configure<EventBusOptions>(section);
        var options = section.Get<EventBusOptions>() ?? new EventBusOptions();
        if (options.Enabled)
        {
            services.AddSingleton<IEventBus, RabbitMqEventBus>();
        }
        else
        {
            services.AddSingleton<IEventBus, NoOpEventBus>();
        }
        return services;
    }
}
