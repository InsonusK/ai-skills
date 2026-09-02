using Ardalis.Result;
using MediatR;
using Sample.Interfaces.Commands;
using Sample.Interfaces.Events;

namespace Sample.Application.Features.Greet;

// Baseline handler shape: guard -> work/dispatch -> return Result<T>.
// No persistence, no domain layer at plateau-core.
public sealed class GreetHandler(IPublisher publisher, GreetingStore store) : IRequestHandler<GreetCommand, Result<GreetResult>>
{
    public async Task<Result<GreetResult>> Handle(GreetCommand request, CancellationToken ct)
    {
        var rendered = $"Hello, {request.Message.Value}!";
        var at = DateTimeOffset.UtcNow;
        store.Last = new GreetResult(rendered, at);

        await publisher.Publish(new Greeted(rendered, at), ct);
        return Result.Success(new GreetResult(rendered, at));
    }
}

// In-memory stand-in for a store; persistence is VP2.
public sealed class GreetingStore { public GreetResult? Last { get; set; } }
