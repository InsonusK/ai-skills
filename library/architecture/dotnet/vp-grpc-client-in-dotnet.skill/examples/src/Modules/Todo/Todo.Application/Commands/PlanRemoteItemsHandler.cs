using MediatR;
using TodoExampleService.Modules.Todo.Application.Services;
using TodoExampleService.Modules.Todo.Commands;

namespace TodoExampleService.Modules.Todo.Application.Commands;

/// <summary>
/// Handles <see cref="PlanRemoteItemsCommand"/> with the todo service: titles
/// it rejects are invalid; a todo service that does not answer is unavailable.
/// </summary>
public sealed class PlanRemoteItemsHandler(IRemotePlanner planner) : IRequestHandler<PlanRemoteItemsCommand, PlanRemoteItemsResult>
{
    public async Task<PlanRemoteItemsResult> Handle(PlanRemoteItemsCommand command, CancellationToken cancellationToken)
    {
        var items = await planner.PlanAsync(command.Titles, cancellationToken);
        return new PlanRemoteItemsResult(items.Select(item => new ItemView(item.Title, item.Done)).ToList());
    }
}
