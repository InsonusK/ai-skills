using Ardalis.Result;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Sample.Application.Resolvers;
using Sample.Application.Validators.Async;
using Sample.Interfaces.Commands;
using Shared.Guid;

namespace Sample.Application;

public static class SampleApplicationRegistration
{
    public static IServiceCollection RegisterSampleModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(SampleApplicationRegistration).Assembly));

        services.AddValidatorsFromAssembly(typeof(SampleApplicationRegistration).Assembly);

        services.AddScoped<IGuidResolver<Result<CreateAttachmentResult>>, CreateAttachmentGuidResolver>();
        services.AddScoped<AttachmentTaskExistsCheck>();

        return services;
    }
}
