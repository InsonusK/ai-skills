using MediatR;

namespace Todo.Commands;

/// <summary>Asks the todo service of another team to plan one open item per title.</summary>
public sealed record PlanRemoteItemsCommand(IReadOnlyList<string> Titles) : IRequest<PlanRemoteItemsResult>;

/// <summary>The items the todo service planned, in the order of the titles.</summary>
public sealed record PlanRemoteItemsResult(IReadOnlyList<ItemView> Items);
