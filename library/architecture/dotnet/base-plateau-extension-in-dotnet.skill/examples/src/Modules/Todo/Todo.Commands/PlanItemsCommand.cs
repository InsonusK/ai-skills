using System.Text.Json.Serialization;
using MediatR;
using Shared.Access;

namespace Todo.Commands;

/// <summary>Asks to plan one open to-do item per title.</summary>
public sealed record PlanItemsCommand(IReadOnlyList<string> Titles) : IRequest<PlanItemsResult>, IRestricted, IBatch
{
    // The marks are no data of the command: [JsonIgnore] keeps them out of
    // its payload in the log.

    /// <summary>Marks PlanItemsCommand <see cref="IRestricted"/>: only a planner may plan.</summary>
    [JsonIgnore]
    public string RequiredRole => "planner";

    /// <summary>Marks PlanItemsCommand <see cref="IBatch"/>: its size is the number of titles.</summary>
    [JsonIgnore]
    public int BatchSize => Titles.Count;
}

/// <summary>The planned items, in the order of the titles.</summary>
public sealed record PlanItemsResult(IReadOnlyList<ItemView> Items);

/// <summary>An item as callers see it; domain types never leave the application.</summary>
public sealed record ItemView(string Title, bool Done);
