using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.Domain.Entities;
using Sample.Domain.ValueObjects;

namespace Sample.Domain.Configurations;

// One config per entity. Owns table/column/constraint names as constants, and the stable
// business name the concurrency infrastructure routes on.
public sealed class TodoItemConfig : IEntityTypeConfiguration<TodoItem>
{
    public const string TableName = nameof(TodoItem);
    public const string VersionedEntityName = "TodoItem";

    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        // builder.ToTable(TableName) requires Microsoft.EntityFrameworkCore.Relational — omitted
        // here because the example runs on the in-memory provider. TableName stays as the constant
        // tests and error handling reference.
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title)
            .HasConversion(t => t.Value, v => new ItemTitle(v))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.IsDone).IsRequired();

        // Production maps Version to xmin: .IsRowVersion() / IsConcurrencyToken() + ValueGeneratedOnAddOrUpdate().
        builder.Property(e => e.Version).IsConcurrencyToken();

        builder.Property(e => e.ServerCreatedDateTime).IsRequired();
        builder.Property(e => e.UserCreatedDateTime).IsRequired();
        builder.Property(e => e.ServerUpdatedDateTime).IsRequired();
        builder.Property(e => e.UserUpdatedDateTime).IsRequired();
    }
}
