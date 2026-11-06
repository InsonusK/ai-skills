using Microsoft.Extensions.DependencyInjection;
using Todo.Application.Commands;

namespace Todo.Application;

/// <summary>
/// The Todo module as the composition root adds it: the handler of every
/// command of the module - MediatR finds each in this assembly, so a new
/// command adds only its files - and the lists the handlers keep.
/// </summary>
public static class TodoModule
{
    public static IServiceCollection AddTodoModule(this IServiceCollection services) =>
        services
            .AddMediatR(mediatr => mediatr.RegisterServicesFromAssemblyContaining<PlanItemsHandler>())
            .AddSingleton<KeptLists>();   // the lists of the process; the DB variation point replaces it
}
