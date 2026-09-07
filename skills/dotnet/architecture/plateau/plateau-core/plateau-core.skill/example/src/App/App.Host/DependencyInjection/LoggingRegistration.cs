using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace App.Host.DependencyInjection;

public static class LoggingRegistration
{
    public static IServiceCollection AddAppLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddConfiguration(configuration.GetSection("Logging"));
            builder.AddSimpleConsole(o => { o.SingleLine = true; o.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ "; });
        });
        return services;
    }
}
