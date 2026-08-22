using Ardalis.Result;
using MediatR;

namespace Shared;

public interface ICommand : IRequest { }

public interface ICommand<T> : IRequest<Result<T>>
{
}
