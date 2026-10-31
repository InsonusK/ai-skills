using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Postgres.Rows;

/// <summary>
/// One row of list_items. A row of the store, not the domain's Item: this project
/// sees no domain (skill app-layers-in-dotnet), and the service over the store
/// turns rows into entities, so the entity's rules are checked on the way out too.
/// </summary>
public sealed class ItemRow
{
    public long Id { get; set; }
    public required string ListId { get; set; }
    public required string Title { get; set; }
    public bool Done { get; set; }
}

/// <summary>The table of <see cref="ItemRow"/>: the id gives the insertion order, the unique index the rule of one title per list.</summary>
public sealed class ItemRowConfiguration : IEntityTypeConfiguration<ItemRow>
{
    public void Configure(EntityTypeBuilder<ItemRow> row)
    {
        row.ToTable("list_items");
        row.HasKey(item => item.Id);
        row.Property(item => item.Id).HasColumnName("id");
        row.Property(item => item.ListId).HasColumnName("list_id");
        row.Property(item => item.Title).HasColumnName("title");
        row.Property(item => item.Done).HasColumnName("done");
        row.HasIndex(item => new { item.ListId, item.Title }).IsUnique().HasDatabaseName("list_items_list_id_title_key");
    }
}
