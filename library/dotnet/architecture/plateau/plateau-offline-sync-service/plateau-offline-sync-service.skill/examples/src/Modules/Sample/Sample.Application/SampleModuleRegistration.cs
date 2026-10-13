using Ardalis.Result;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Sample.Application.Concurrency;
using Sample.Application.Resolvers;
using Sample.Interfaces.Commands;
using Shared.Guid;

namespace Sample.Application;

public static class SampleModuleRegistration
{
    public static IServiceCollection AddSampleModule(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(SampleModuleRegistration).Assembly));
        services.AddValidatorsFromAssembly(typeof(SampleModuleRegistration).Assembly);

        services.AddScoped<TodoItemVersionResolver>();                                     // VP5
        services.AddScoped<IGuidResolver<Result<AddItemResult>>, CreateTodoItemGuidResolver>(); // VP6

        return services;
    }
}
