using MediatR;
using Shared;
using Shared.UnitOfWork;

namespace BuildingBlocks.MediatR;

public sealed class UnitOfWorkContext
{
    public int Depth { get; private set; }
    public void Enter() => Depth++;
    public void Leave() => Depth--;
}

public class UnitOfWorkBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand
{
    private readonly UnitOfWorkContext _context;
    private readonly IUnitOfWork _unitOfWork;

    public UnitOfWorkBehavior(UnitOfWorkContext context, IUnitOfWork unitOfWork)
    {
        _context = context;
        _unitOfWork = unitOfWork;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        _context.Enter();
        try
        {
            var response = await next();

            if (_context.Depth == 1)
                await _unitOfWork.SaveChangesAsync(ct);

            return response;
        }
        finally
        {
            _context.Leave();
        }
    }
}
