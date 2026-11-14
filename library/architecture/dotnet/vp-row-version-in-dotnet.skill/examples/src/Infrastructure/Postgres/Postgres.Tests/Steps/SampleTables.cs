using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using Postgres.Repositories;
using Shared.Persistence;

namespace Postgres.Tests.Steps;

// The tables of the shared scenarios: two entities with a relation, a task
// referring to its owner. They exist only in this project: the application
// never ships them.

public sealed class Owner
{
    public long Id { get; set; }
    public required string Name { get; set; }
}

public sealed class OwnerTask
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public Owner? Owner { get; set; }
    public required string Title { get; set; }
    public bool Done { get; set; }
}

public sealed class SampleContext(DbContextOptions<SampleContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Owner>(owner =>
        {
            owner.ToTable("owners");
            owner.Property(entity => entity.Id).HasColumnName("id");
            owner.Property(entity => entity.Name).HasColumnName("name");
        });
        model.Entity<OwnerTask>(task =>
        {
            task.ToTable("tasks");
            task.Property(entity => entity.Id).HasColumnName("id");
            task.Property(entity => entity.OwnerId).HasColumnName("owner_id");
            task.Property(entity => entity.Title).HasColumnName("title");
            task.Property(entity => entity.Done).HasColumnName("done");
            task.HasOne(entity => entity.Owner).WithMany().HasForeignKey(entity => entity.OwnerId);
            task.HasIndex(entity => new { entity.OwnerId, entity.Title }).IsUnique();
        });
    }
}

public sealed class UnclosedTasks : Specification<OwnerTask>
{
    public UnclosedTasks(string owner) => Query.Where(task => task.Owner!.Name == owner && !task.Done);
}

public sealed class TaskByTitle : Specification<OwnerTask>, ISingleResultSpecification<OwnerTask>
{
    public TaskByTitle(string title) => Query.Where(task => task.Title == title);
}

public sealed class FirstOfUnclosedTasks : Specification<OwnerTask>
{
    public FirstOfUnclosedTasks(string owner) => Query.Where(task => task.Owner!.Name == owner && !task.Done).OrderBy(task => task.Title).Take(1);
}

/// <summary>An entity with statements of its own, past SaveChanges, as one of an application would have them.</summary>
public sealed class TaskRepository(DbContext context) : PostgresRepository<OwnerTask>(context)
{
    public Task<int> InsertUnlessThereAsync(long ownerId, string title, CancellationToken cancellationToken) =>
        WriteAsync(
            () => Context.Database.ExecuteSqlAsync(
                $"INSERT INTO tasks (owner_id, title, done) VALUES ({ownerId}, {title}, false) ON CONFLICT DO NOTHING",
                cancellationToken),
            cancellationToken);

    public Task<int> RenameAsync(string from, string to, CancellationToken cancellationToken) =>
        WriteAsync(
            () => Context.Set<OwnerTask>().Where(task => task.Title == from).ExecuteUpdateAsync(task => task.SetProperty(entity => entity.Title, to), cancellationToken),
            cancellationToken);

    /// <summary>Makes the database answer a failure of the given code.</summary>
    public Task<int> FailWithAsync(string sqlState, CancellationToken cancellationToken)
    {
        var statement = $"DO $$ BEGIN RAISE EXCEPTION 'failure' USING ERRCODE = '{sqlState}'; END $$;";
        return Context.Database.ExecuteSqlRawAsync(statement, cancellationToken);
    }
}
