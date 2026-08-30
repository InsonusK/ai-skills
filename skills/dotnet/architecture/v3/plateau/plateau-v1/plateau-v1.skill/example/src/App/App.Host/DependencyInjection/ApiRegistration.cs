using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Sample.Api;
using Sample.Api.MinimalApi;

namespace App.Host.DependencyInjection;

public static class ApiRegistration
{
    public static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddProblemDetails();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(SampleApiSwaggerRegistration.DocumentName, new OpenApiInfo
            {
                Title = SampleApiSwaggerRegistration.Title,
                Version = SampleApiSwaggerRegistration.Version
            });
            options.DocInclusionPredicate((docName, apiDesc) => docName switch
            {
                SampleApiSwaggerRegistration.DocumentName => SampleApiSwaggerRegistration.MatchesRoute(apiDesc.RelativePath is null ? null : "/" + apiDesc.RelativePath),
                _ => false
            });
        });
        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.MapControllers();
        app.MapSampleSystemEndpoints();

        app.UseSwagger();
        app.UseSwaggerUI(options =>
            options.SwaggerEndpoint($"/swagger/{SampleApiSwaggerRegistration.DocumentName}/swagger.json", SampleApiSwaggerRegistration.Title));

        return app;
    }
}
