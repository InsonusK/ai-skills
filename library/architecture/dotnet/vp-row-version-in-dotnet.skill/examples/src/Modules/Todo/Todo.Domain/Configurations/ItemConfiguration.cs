using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Todo.Domain.Entities;

namespace Todo.Domain.Configurations;

/// <summary>
/// The table of <see cref="Item"/>: the id gives the order of adding, the
/// unique index the rule of one title per list. The module owns its tables:
/// AppDbContext applies every configuration of this assembly.
/// </summary>
public sealed class ItemConfiguration : IEntityTypeConfiguration<Item>
{
    public void Configure(EntityTypeBuilder<Item> item)
    {
        item.ToTable("list_items");
        item.HasKey(entity => entity.Id);
        item.Property(entity => entity.Id).HasColumnName("id");
        item.Property(entity => entity.ListId).HasColumnName("list_id");
        item.Property(entity => entity.Title).HasColumnName("title");
        item.Property(entity => entity.Done).HasColumnName("done");
        // The row version (skill vp-row-version): PostgreSQL's xmin, a system
        // column, so the migration of it has no DDL.
        item.Property(entity => entity.Version).IsRowVersion();
        item.HasIndex(entity => new { entity.ListId, entity.Title }).IsUnique().HasDatabaseName("list_items_list_id_title_key");
    }
}
