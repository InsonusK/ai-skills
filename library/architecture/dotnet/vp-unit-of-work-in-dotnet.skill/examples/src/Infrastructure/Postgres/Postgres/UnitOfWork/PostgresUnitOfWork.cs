// Source: skill vp-unit-of-work-in-dotnet

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Shared.UnitOfWork;

namespace Postgres.UnitOfWork;

/// <summary>
/// The unit of work over <see cref="AppDbContext"/>, one per command scope. It
/// watches the context's saves: the first save after <see cref="TryBegin"/> opens
/// the transaction, so a command that only reads holds none, and a store keeps
/// calling SaveChangesAsync, unaware of it. EF Core protects each save inside the
/// transaction with a savepoint, so a duplicate key a command handles leaves the
/// transaction usable. Statements that do not pass through SaveChanges
/// (ExecuteUpdate, ExecuteDelete, raw SQL) open no transaction.
/// </summary>
public sealed class PostgresUnitOfWork : SaveChangesInterceptor, IUnitOfWork
{
    private bool _begun;
    private IDbContextTransaction? _transaction;

    public bool TryBegin()
    {
        if (_begun)
        {
            return false;
        }
        _begun = true;
        return true;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        if (_begun)
        {
            _transaction ??= eventData.Context!.Database.BeginTransaction();
        }
        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
    {
        if (_begun)
        {
            _transaction ??= await eventData.Context!.Database.BeginTransactionAsync(cancellationToken);
        }
        return result;
    }

    public async Task CommitAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
        {
            return;
        }
        try
        {
            await _transaction.CommitAsync(cancellationToken);
        }
        catch (Exception error) when (StoreFailures.IsUnavailable(error))
        {
            throw UnitOfWorkErrors.Unavailable(error);
        }
    }

    public async Task RollbackAsync(CancellationToken cancellationToken)
    {
        if (_transaction is null)
        {
            return;
        }
        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        catch (Exception error) when (StoreFailures.IsUnavailable(error))
        {
            // The connection is gone, and the server dropped the transaction with it.
        }
    }
}

public static class PostgresUnitOfWorkModule
{
    /// <summary>
    /// Registers the unit of work of a command scope and shows it every save of
    /// that scope's context. Called in "// [services]" after AddPostgres.
    /// </summary>
    public static IServiceCollection AddPostgresUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<PostgresUnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<PostgresUnitOfWork>());
        return services.ConfigureDbContext<AppDbContext>((provider, options) =>
            options.AddInterceptors(provider.GetRequiredService<PostgresUnitOfWork>()));
    }
}
