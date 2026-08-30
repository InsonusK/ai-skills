using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sample.Domain.Entities;

namespace Sample.Domain.Configurations;

public sealed class TaskItemConfig : IEntityTypeConfiguration<TaskItem>
{
    public const string TableName = "TaskItems";

    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable(TableName);
        builder.HasKey(t => t.Id);

        builder.OwnsOne(t => t.AssigneeEmail);

        builder.Property(t => t.Version)
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate()
            .HasColumnName("xmin")
            .HasColumnType("xid");

        builder.Property(t => t.UserCreatedDateTime).IsRequired();
        builder.Property(t => t.ServerCreatedDateTime).IsRequired();
        builder.Property(t => t.UserUpdatedDateTime).IsRequired();
        builder.Property(t => t.ServerUpdatedDateTime).IsRequired();
    }
}
