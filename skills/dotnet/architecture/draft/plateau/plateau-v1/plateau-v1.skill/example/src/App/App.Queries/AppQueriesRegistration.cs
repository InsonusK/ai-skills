using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace App.Queries;

public static class AppQueriesRegistration
{
    public static IServiceCollection RegisterAppQueries(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(AppQueriesRegistration).Assembly));
        return services;
    }
}
