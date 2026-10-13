using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.Domain.Entities;
using Sample.Domain.ValueObjects;

namespace Sample.Domain.Configurations;

public sealed class TodoItemConfig : IEntityTypeConfiguration<TodoItem>
{
    public const string TableName = nameof(TodoItem);
    public const string VersionedEntityName = "TodoItem";
    public const string UX_Guid = "UX_" + TableName + "_Guid";

    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.HasKey(e => e.Id);

        // VP6: unique index on Guid is the last-line idempotency guard (behind GuidResolvingBehavior).
        // Production adds .HasDatabaseName(UX_Guid) (relational extension, omitted for the in-memory provider).
        builder.HasIndex(e => e.Guid).IsUnique();

        builder.Property(e => e.Title)
            .HasConversion(t => t.Value, v => new ItemTitle(v))
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.IsDone).IsRequired();
        builder.Property(e => e.Version).IsConcurrencyToken();
        builder.Property(e => e.ServerCreatedDateTime).IsRequired();
        builder.Property(e => e.UserCreatedDateTime).IsRequired();
        builder.Property(e => e.ServerUpdatedDateTime).IsRequired();
        builder.Property(e => e.UserUpdatedDateTime).IsRequired();
    }
}
