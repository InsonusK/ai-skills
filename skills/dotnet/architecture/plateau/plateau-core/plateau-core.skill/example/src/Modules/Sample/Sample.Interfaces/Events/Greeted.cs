using Shared.MediatR;

namespace Sample.Interfaces.Events;

public record Greeted(string Rendered, DateTimeOffset At) : INotificationEvent;
