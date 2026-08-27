using Ardalis.Result;
using MediatR;
using Sample.Domain.Entities;
using Sample.Interfaces.Commands;

namespace Sample.Application.Handlers;

public class GreetCommandHandler : IRequestHandler<GreetCommand, Result<string>>
{
    public Task<Result<string>> Handle(GreetCommand request, CancellationToken cancellationToken)
    {
        var message = Greeting.For(request.Name);
        return Task.FromResult(Result.Success(message));
    }
}
