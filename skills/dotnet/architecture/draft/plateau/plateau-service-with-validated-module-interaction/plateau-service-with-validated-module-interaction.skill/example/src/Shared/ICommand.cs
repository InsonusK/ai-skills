using Ardalis.Result;
using MediatR;

namespace Shared;

public interface ICommand : IRequest<Result> { }

public interface ICommand<T> : IRequest<Result<T>> { }
