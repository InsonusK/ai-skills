using Ardalis.Result;
using Sample.Interfaces.ValueObjects;
using Shared.MediatR;

namespace Sample.Interfaces.Commands;

// Business fields first (fixed command property order from solution-mediator-integration).
public record GreetCommand(SoftGreeting Message) : ICommand<Result<GreetResult>>;

public record GreetResult(string Rendered, DateTimeOffset At);
