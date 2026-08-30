using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Sample.Api.Grpc;

namespace App.Host.DependencyInjection;

public static class GrpcRegistration
{
    public static IServiceCollection AddGrpcApi(this IServiceCollection services)
    {
        services.AddGrpc();
        return services;
    }

    public static WebApplication UseGrpcApi(this WebApplication app)
    {
        app.MapGrpcService<TaskGrpcService>();

        return app;
    }
}
