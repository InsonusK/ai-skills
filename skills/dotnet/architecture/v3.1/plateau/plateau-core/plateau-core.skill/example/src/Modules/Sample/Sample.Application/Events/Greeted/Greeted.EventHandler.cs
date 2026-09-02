using MediatR;
using Microsoft.Extensions.Logging;
using Sample.Interfaces.Events;

namespace Sample.Application.Events.Greeted;

public sealed class GreetedEventHandler(ILogger<GreetedEventHandler> logger)
    : INotificationHandler<Interfaces.Events.Greeted>
{
    public Task Handle(Interfaces.Events.Greeted e, CancellationToken ct)
    {
        logger.LogInformation("Greeted: {Rendered} at {At}", e.Rendered, e.At);
        return Task.CompletedTask;
    }
}
