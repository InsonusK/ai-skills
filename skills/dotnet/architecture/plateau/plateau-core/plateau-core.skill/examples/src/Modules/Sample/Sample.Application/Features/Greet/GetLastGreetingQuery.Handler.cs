using Ardalis.Result;
using MediatR;
using Sample.Interfaces.Queries;

namespace Sample.Application.Features.Greet;

public sealed class GetLastGreetingHandler(Features.Greet.GreetingStore store)
    : IRequestHandler<GetLastGreetingQuery, Result<GreetingDto>>
{
    public Task<Result<GreetingDto>> Handle(GetLastGreetingQuery request, CancellationToken ct)
        => Task.FromResult(store.Last is { } g
            ? Result.Success(new GreetingDto(g.Rendered, g.At))
            : Result<GreetingDto>.NotFound());
}
