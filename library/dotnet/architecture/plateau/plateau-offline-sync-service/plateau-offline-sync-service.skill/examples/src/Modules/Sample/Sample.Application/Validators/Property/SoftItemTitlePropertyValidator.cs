using FluentValidation;
using Sample.Domain.Rules;
using Sample.Interfaces.ValueObjects;

namespace Sample.Application.Validators.Property;

// VP4: the condition is centralized in ItemTitleRules; this validator just calls its extension,
// resolvable cross-module via IValidator<SoftItemTitle>.
public sealed class SoftItemTitlePropertyValidator : AbstractValidator<SoftItemTitle>
{
    public SoftItemTitlePropertyValidator() => RuleFor(x => x).ItemTitleIsValid();
}
