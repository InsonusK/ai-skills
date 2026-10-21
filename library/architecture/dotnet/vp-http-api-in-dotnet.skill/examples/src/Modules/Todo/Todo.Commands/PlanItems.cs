using MediatR;

namespace Todo.Commands;

/// <summary>Asks to plan one open to-do item per title.</summary>
public sealed record PlanItems(IReadOnlyList<string> Titles) : IRequest<PlanItemsResult>;

/// <summary>The planned items, in the order of the titles.</summary>
public sealed record PlanItemsResult(IReadOnlyList<ItemView> Items);

/// <summary>An item as callers see it; domain types never leave the application.</summary>
public sealed record ItemView(string Title, bool Done);
