using Client.Abstractions;
using Shared.Errors;
using Todo.Domain.Entities;
using TodoApi.Abstractions;

namespace Todo.Application.Services;

/// <summary>
/// The service over the todo API: it speaks the application's language and
/// turns the API's technical outcomes into the error kinds - an API that does
/// not answer is unavailable; titles the API rejects are invalid, with its
/// message; any other answer is a failure of this service.
/// </summary>
public sealed class RemotePlannerService(ITodoApi api) : IRemotePlanner
{
    public async Task<IReadOnlyList<Item>> PlanAsync(IReadOnlyList<string> titles, CancellationToken cancellationToken)
    {
        IReadOnlyList<TodoApiItem> answered;
        try
        {
            answered = await api.PlanItemsAsync(titles, cancellationToken);
        }
        catch (ApiUnavailableException unavailable)
        {
            throw RemotePlannerErrors.Unavailable(unavailable);
        }
        catch (ApiStatusException rejected) when (rejected.Code == ApiStatusCode.InvalidArgument)
        {
            throw RemotePlannerErrors.TitlesRejected(rejected.StatusMessage, rejected);
        }
        return answered.Select(ToItem).ToList();
    }

    private static Item ToItem(TodoApiItem answered)
    {
        Item item;
        try
        {
            item = new Item(answered.Title);
            if (answered.Done)
            {
                item.Complete();
            }
        }
        catch (ErrorKindException broken)
        {
            // Not the inner exception: its kind would reach the caller, and an
            // item the other service got wrong is a failure of this one, never
            // the caller's invalid input.
            throw new InvalidOperationException($"the todo service answered the item \"{answered.Title}\": {broken.Message}");
        }
        return item;
    }
}
