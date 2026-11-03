namespace Postgres.Abstractions.Rows;

/// <summary>
/// One row of list_items. A row of the table, not the domain's Item: this project
/// sees no domain (skill app-layers-in-dotnet), and the service over the
/// repository turns rows into entities, so the entity's rules are checked on the
/// way out too.
/// </summary>
public sealed class ItemRow
{
    public long Id { get; set; }
    public required string ListId { get; set; }
    public required string Title { get; set; }
    public bool Done { get; set; }
}
