namespace Postgres.Abstractions.Rows;

/// <summary>
/// Marks a row of a table whose rows never change once inserted (skill
/// vp-row-version): it has no row version, and a service never updates it. Every
/// row of the model either has a row version or this mark; a scenario fails on
/// a row with neither.
/// </summary>
/// <example><c>public sealed class EventRow : INeverChanges { ... }</c></example>
public interface INeverChanges;
