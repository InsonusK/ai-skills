using Shared.MediatR;

namespace Sample.Interfaces.Events;

public record ItemCompleted(int ItemId, DateTimeOffset At) : INotificationEvent;
