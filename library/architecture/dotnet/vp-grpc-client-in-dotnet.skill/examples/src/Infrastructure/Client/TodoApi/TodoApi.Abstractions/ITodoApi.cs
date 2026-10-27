using Client.Abstractions;

namespace TodoApi.Abstractions;

/// <summary>An item as the todo API answers it, the adapter's own type.</summary>
public sealed record TodoApiItem(string Title, bool Done);

/// <summary>
/// The todo API of another service, one method per method of its contract this
/// application calls, in the API's terms. A method reports technical outcomes
/// only - it throws <see cref="ApiUnavailableException"/> or <see cref="ApiStatusException"/> -
/// and the service over it decides what they mean.
/// </summary>
public interface ITodoApi
{
    /// <summary>ItemsService.PlanItems: one open item per title, planned by the API.</summary>
    Task<IReadOnlyList<TodoApiItem>> PlanItemsAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken);
}
