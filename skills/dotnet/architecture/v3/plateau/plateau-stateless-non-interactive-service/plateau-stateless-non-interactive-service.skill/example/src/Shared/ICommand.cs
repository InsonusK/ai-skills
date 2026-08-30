using MediatR;

namespace Shared;

public interface ICommand<TResponse> : IRequest<TResponse>
{
}
