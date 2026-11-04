using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Postgres.Abstractions.Rows;

namespace Postgres.Rows;

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
        // The version: a uint row version is PostgreSQL's xmin for Npgsql, a
        // system column no migration makes.
        row.Property(item => item.Version).IsRowVersion();
        row.HasIndex(item => new { item.ListId, item.Title }).IsUnique().HasDatabaseName("list_items_list_id_title_key");
    }
}
