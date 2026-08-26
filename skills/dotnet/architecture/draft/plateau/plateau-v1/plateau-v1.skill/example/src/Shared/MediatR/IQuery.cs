using MediatR;

namespace Shared;

public interface IQuery<TResponse> : IRequest<TResponse> { }
