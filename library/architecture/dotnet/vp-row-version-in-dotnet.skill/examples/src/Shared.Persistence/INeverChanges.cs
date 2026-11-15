// Source: skill vp-row-version-in-dotnet

namespace Shared.Persistence;

/// <summary>
/// Marks an entity of a table whose rows never change once inserted (skill
/// vp-row-version): it has no row version, and nothing updates it. Every entity
/// of the model either declares its row version (a <c>uint Version</c>
/// configured <c>IsRowVersion()</c>) or carries this mark; a scenario of the
/// shared copy fails on an entity with neither.
/// </summary>
/// <example><c>public sealed class Event : INeverChanges { ... }</c></example>
public interface INeverChanges;
