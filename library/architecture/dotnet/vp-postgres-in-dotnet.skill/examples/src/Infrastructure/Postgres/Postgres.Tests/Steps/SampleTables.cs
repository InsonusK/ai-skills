using Ardalis.Specification;
using Microsoft.EntityFrameworkCore;
using Postgres.Repositories;

namespace Postgres.Tests.Steps;

// The tables of the shared scenarios: two tables with a relation, and a row with
// a version. They exist only in this project: the application never ships them.

public sealed class OwnerRow
{
    public long Id { get; set; }
    public required string Name { get; set; }
}

public sealed class TaskRow
{
    public long Id { get; set; }
    public long OwnerId { get; set; }
    public OwnerRow? Owner { get; set; }
    public required string Title { get; set; }
    public bool Done { get; set; }
}

/// <summary>A row that takes the engine's version, opt-in per table.</summary>
public sealed class VersionedRow
{
    public long Id { get; set; }
    public required string Title { get; set; }
    public uint Version { get; set; }
}

/// <summary>A projection across the relation; properties, not a constructor, so that EF Core can order by them.</summary>
public sealed class OwnerAndTitle
{
    public required string Owner { get; init; }
    public required string Title { get; init; }
}

public sealed class SampleContext(DbContextOptions<SampleContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<OwnerRow>(row =>
        {
            row.ToTable("owners");
            row.Property(owner => owner.Id).HasColumnName("id");
            row.Property(owner => owner.Name).HasColumnName("name");
        });
        model.Entity<TaskRow>(row =>
        {
            row.ToTable("tasks");
            row.Property(task => task.Id).HasColumnName("id");
            row.Property(task => task.OwnerId).HasColumnName("owner_id");
            row.Property(task => task.Title).HasColumnName("title");
            row.Property(task => task.Done).HasColumnName("done");
            row.HasOne(task => task.Owner).WithMany().HasForeignKey(task => task.OwnerId);
            row.HasIndex(task => new { task.OwnerId, task.Title }).IsUnique();
        });
        model.Entity<VersionedRow>(row =>
        {
            row.ToTable("versioned");
            row.Property(versioned => versioned.Id).HasColumnName("id");
            row.Property(versioned => versioned.Title).HasColumnName("title");
            row.Property(versioned => versioned.Version).IsRowVersion();
        });
    }
}

public sealed class UnclosedTasks : Specification<TaskRow>
{
    public UnclosedTasks(string owner) => Query.Where(task => task.Owner!.Name == owner && !task.Done);
}

public sealed class OpenTasks : Specification<TaskRow>
{
    public OpenTasks() => Query.Where(task => !task.Done);
}

public sealed class TaskById : Specification<TaskRow>
{
    public TaskById(long id) => Query.Where(task => task.Id == id);
}

public sealed class TaskByTitle : Specification<TaskRow>
{
    public TaskByTitle(string title) => Query.Where(task => task.Title == title);
}

public sealed class TitlesWithOwners : Specification<TaskRow, OwnerAndTitle>
{
    public TitlesWithOwners(string owner)
    {
        Query.Where(task => task.Owner!.Name == owner);
        Query.Select(task => new OwnerAndTitle { Owner = task.Owner!.Name, Title = task.Title });
    }
}

public sealed class WithPage : Specification<TaskRow>
{
    public WithPage() => Query.Skip(1).Take(1);
}

public sealed class WithOrder : Specification<TaskRow>
{
    public WithOrder() => Query.OrderBy(task => task.Id);
}

/// <summary>A table with statements of its own, past the contract, as a table of an application would have them.</summary>
public sealed class TaskRepository(DbContext context) : PostgresRepository<TaskRow>(context)
{
    public Task<int> InsertUnlessThereAsync(long ownerId, string title, CancellationToken cancellationToken) =>
        WriteAsync(
            () => Context.Database.ExecuteSqlAsync(
                $"INSERT INTO tasks (owner_id, title, done) VALUES ({ownerId}, {title}, false) ON CONFLICT DO NOTHING",
                cancellationToken),
            $"upsert into {Table}",
            cancellationToken);

    public Task<int> RenameAsync(string from, string to, CancellationToken cancellationToken) =>
        WriteAsync(
            () => Context.Set<TaskRow>().Where(task => task.Title == from).ExecuteUpdateAsync(task => task.SetProperty(row => row.Title, to), cancellationToken),
            $"rename in {Table}",
            cancellationToken);

    /// <summary>Makes the database answer a failure of the given code.</summary>
    public Task<int> FailWithAsync(string sqlState, CancellationToken cancellationToken)
    {
        var statement = $"DO $$ BEGIN RAISE EXCEPTION 'failure' USING ERRCODE = '{sqlState}'; END $$;";
        return ReadAsync(() => Context.Database.ExecuteSqlRawAsync(statement, cancellationToken), Table);
    }
}
