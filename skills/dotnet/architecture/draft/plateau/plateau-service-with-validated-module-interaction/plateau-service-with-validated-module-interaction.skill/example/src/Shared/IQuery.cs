using Ardalis.Result;
using MediatR;

namespace Shared;

public interface IQuery<T> : IRequest<Result<T>>
{
}
