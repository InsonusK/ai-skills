namespace BuildingBlocks.MediatR;

// Scoped nesting-depth counter. Only UnitOfWorkBehavior reads/writes it.
public sealed class UnitOfWorkContext
{
    private int _depth;
    public void Enter() => Interlocked.Increment(ref _depth);
    public void Leave() => Interlocked.Decrement(ref _depth);
    public int Depth => _depth;
}
