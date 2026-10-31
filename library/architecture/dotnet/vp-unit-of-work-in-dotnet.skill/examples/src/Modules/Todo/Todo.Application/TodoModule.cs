using Microsoft.Extensions.DependencyInjection;
using Todo.Application.Commands;
using Todo.Application.Services;

namespace Todo.Application;

/// <summary>
/// The Todo module as the composition root adds it: the handler of every
/// command of the module - MediatR finds each in this assembly, so a new
/// command adds only its files - and the services the handlers use.
/// </summary>
public static class TodoModule
{
    /// <summary>
    /// Registers the module's handlers and their services; the store the
    /// services keep the lists in (<c>IListStore</c>) is the infrastructure's,
    /// registered in the composition root's "// [services]". Scoped like the
    /// store: a singleton would hold one context for every command.
    /// </summary>
    public static IServiceCollection AddTodoModule(this IServiceCollection services) =>
        services
            .AddMediatR(mediatr => mediatr.RegisterServicesFromAssemblyContaining<PlanItemsHandler>())
            .AddScoped<IItemLists, ItemListsService>();
}
