// Source: skill vp-logging-in-dotnet

namespace Shared.Logging;

/// <summary>
/// Marks a property whose value is secret: the logging variation point writes
/// it as <c>***</c>, whatever its value - a text, a number, an object, a list.
/// On a positional record write <c>[property: LogMask]</c>; the attribute is
/// valid on properties only, so a mark that would land on the constructor
/// parameter instead - and mask nothing - does not compile.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class LogMaskAttribute : Attribute;

/// <summary>
/// Marks a property that is never logged at all - large, or of no use in a
/// log. On a positional record write <c>[property: LogOmit]</c>.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = true)]
public sealed class LogOmitAttribute : Attribute;
