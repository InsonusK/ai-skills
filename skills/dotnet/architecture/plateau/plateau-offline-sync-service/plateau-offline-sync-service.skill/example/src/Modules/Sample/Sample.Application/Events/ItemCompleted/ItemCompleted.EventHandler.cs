using MediatR;
using Microsoft.Extensions.Logging;

namespace Sample.Application.Events.ItemCompleted;

public sealed class ItemCompletedEventHandler(ILogger<ItemCompletedEventHandler> logger)
    : INotificationHandler<Sample.Interfaces.Events.ItemCompleted>
{
    public Task Handle(Sample.Interfaces.Events.ItemCompleted e, CancellationToken ct)
    {
        logger.LogInformation("Item {ItemId} completed at {At}", e.ItemId, e.At);
        return Task.CompletedTask;
    }
}
