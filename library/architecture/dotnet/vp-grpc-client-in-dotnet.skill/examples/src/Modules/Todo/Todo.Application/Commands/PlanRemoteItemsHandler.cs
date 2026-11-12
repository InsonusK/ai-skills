using MediatR;
using Todo.Application.Services;
using Todo.Commands;

namespace Todo.Application.Commands;

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
