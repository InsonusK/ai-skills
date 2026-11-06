using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Todo.Domain.Entities;

namespace Postgres.Configurations.Todo;

/// <summary>
/// The tables of the aggregate <see cref="ItemList"/>: the list, keyed by the
/// caller's name, and its items, owned by it, so they are loaded and saved only
/// with their list and an item removed from it is deleted. The id of an item
/// gives the order it was added in; the unique index holds the list's rule of
/// one title per list under two concurrent commands.
/// </summary>
public sealed class ItemListConfiguration : IEntityTypeConfiguration<ItemList>
{
    public void Configure(EntityTypeBuilder<ItemList> list)
    {
        list.ToTable("lists");
        list.HasKey(root => root.Id);
        list.Property(root => root.Id).HasColumnName("id");
        list.OwnsMany(root => root.Items, item =>
        {
            item.ToTable("list_items");
            item.WithOwner().HasForeignKey("ListId");
            item.Property<string>("ListId").HasColumnName("list_id");
            item.HasKey(child => child.Id);
            item.Property(child => child.Id).HasColumnName("id").UseIdentityByDefaultColumn();
            item.Property(child => child.Title).HasColumnName("title");
            item.Property(child => child.Done).HasColumnName("done");
            item.HasIndex("ListId", nameof(Item.Title)).IsUnique().HasDatabaseName("list_items_list_id_title_key");
        });
        list.Navigation(root => root.Items).HasField("_items");
    }
}
