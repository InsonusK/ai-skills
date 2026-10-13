using Ardalis.Result;
using Shared.MediatR;

namespace Sample.Interfaces.Queries;

public record GetLastGreetingQuery : IQuery<Result<GreetingDto>>;

public record GreetingDto(string Rendered, DateTimeOffset At);
