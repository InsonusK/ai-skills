using MediatR;

namespace Shared;

public interface ICommand : IRequest { }

public interface ICommand<TResponse> : IRequest<TResponse> { }
