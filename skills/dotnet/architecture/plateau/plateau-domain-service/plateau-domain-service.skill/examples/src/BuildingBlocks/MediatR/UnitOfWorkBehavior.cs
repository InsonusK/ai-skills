using MediatR;
using Shared.MediatR;
using Shared.UnitOfWork;

namespace BuildingBlocks.MediatR;

// Registered last. Commits once, after the outermost command handler completes.
public sealed class UnitOfWorkBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork, UnitOfWorkContext context)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        context.Enter();
        try
        {
            var response = await next();
            if (context.Depth == 1)
                await unitOfWork.SaveChangesAsync(ct);
            return response;
        }
        finally
        {
            context.Leave();
        }
    }
}
