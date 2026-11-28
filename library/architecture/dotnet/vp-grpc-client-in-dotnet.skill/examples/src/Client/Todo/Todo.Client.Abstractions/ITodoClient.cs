using TodoExampleService.Client.Core.Abstractions;

namespace TodoExampleService.Client.Todo.Abstractions;

/// <summary>
/// The logical todo API contract shared by transport implementations.
/// Operations return API DTOs or normalized technical outcomes; service owns
/// Domain Model adaptation and interpretation of those outcomes.
/// </summary>
public interface ITodoClient
{
    /// <summary>Plan one item for each requested title through the provider's logical operation.</summary>
    Task<IReadOnlyList<TodoItemDto>> PlanItemsAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken);
}
