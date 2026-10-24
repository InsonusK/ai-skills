using MediatR;
using Todo.Domain.Services;
using Todo.Commands;

namespace Todo.Application.Commands;

/// <summary>
/// Handles <see cref="PlanItemsCommand"/>. Every service it uses would be a
/// constructor parameter typed by an interface declared in this project; the
/// domain service has no I/O, so the handler creates it.
/// </summary>
public sealed class PlanItemsHandler : IRequestHandler<PlanItemsCommand, PlanItemsResult>
{
    private readonly Planner _planner = new();

    public Task<PlanItemsResult> Handle(PlanItemsCommand command, CancellationToken cancellationToken)
    {
        var items = _planner.Plan(command.Titles);
        var views = items.Select(item => new ItemView(item.Title, item.Done)).ToList();
        return Task.FromResult(new PlanItemsResult(views));
    }
}
