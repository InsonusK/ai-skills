using Todo.Domain.Entities;

namespace Todo.Application.Services;

/// <summary>Plans items with the todo service - what <c>PlanRemoteItemsHandler</c> needs of it.</summary>
public interface IRemotePlanner
{
    /// <summary>One open item per title, planned by the todo service.</summary>
    Task<IReadOnlyList<Item>> PlanAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken);
}
