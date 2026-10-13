using Sample.Interfaces.ValueObjects;
using Shared.Exceptions;

namespace Sample.Domain.ValueObjects;

// Strict (VP3): inherits the permissive shape, validates in the constructor, throws on an
// invalid value. An entity property typed ItemTitle can never hold an invalid title.
public sealed record ItemTitle : SoftItemTitle
{
    public ItemTitle(string value) : base(value)
    {
        if (!IsValid(value))
            throw new DomainException("Sample.ItemTitle.Invalid", "Title must be 1..100 non-blank characters.");
    }

    private static bool IsValid(string value) => !string.IsNullOrWhiteSpace(value) && value.Length <= 100;

    public static implicit operator string(ItemTitle t) => t.Value;
}
