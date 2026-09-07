using FluentValidation;
using Sample.Domain.Rules;
using Sample.Interfaces.ValueObjects;
using Shared.Exceptions;

namespace Sample.Domain.ValueObjects;

// Strict (VP3): inherits the permissive shape and rejects an invalid value at construction.
// VP4: the condition now lives once in ItemTitleRules — this constructor forwards to Check().
public sealed record ItemTitle : SoftItemTitle
{
    public ItemTitle(string value) : base(value)
    {
        // Errors.Any(Severity == Error), not !IsValid — IsValid ignores Severity.
        var blocking = this.Check().Errors.FirstOrDefault(e => e.Severity == Severity.Error);
        if (blocking is not null)
            throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);
    }

    public static implicit operator string(ItemTitle t) => t.Value;
}
