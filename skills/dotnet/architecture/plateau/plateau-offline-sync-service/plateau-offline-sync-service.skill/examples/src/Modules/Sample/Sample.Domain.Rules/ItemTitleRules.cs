using FluentValidation;
using FluentValidation.Results;
using Sample.Domain.Rules.Common;
using Sample.Interfaces.ValueObjects;

namespace Sample.Domain.Rules;

// The one place the title condition lives. Called fail-fast by ItemTitle's constructor and
// collect-all by SoftItemTitlePropertyValidator — same logic, no divergence.
public static class ItemTitleRules
{
    public const string RequiredCode = ModuleInfo.ModuleName + ".ItemTitle.Required";
    public const string TooLongCode = ModuleInfo.ModuleName + ".ItemTitle.TooLong";

    public static bool IsRequired(this SoftItemTitle t) => !string.IsNullOrWhiteSpace(t.Value);
    public static bool IsWithinLength(this SoftItemTitle t) => t.Value is null || t.Value.Length <= 100;

    private static IRuleBuilderOptions<T, SoftItemTitle> RequiredRule<T>(this IRuleBuilder<T, SoftItemTitle> rule)
        => rule.Must(x => x.IsRequired()).WithErrorCode(RequiredCode).WithMessage("Title is required.");

    private static IRuleBuilderOptions<T, SoftItemTitle> LengthRule<T>(this IRuleBuilder<T, SoftItemTitle> rule)
        => rule.Must(x => x.IsWithinLength()).WithErrorCode(TooLongCode).WithMessage("Title must not exceed 100 characters.");

    // Single public entry point — the individual Must() calls cannot be invoked separately.
    public static IRuleBuilderOptions<T, SoftItemTitle> ItemTitleIsValid<T>(this IRuleBuilder<T, SoftItemTitle> rule)
        => rule.RequiredRule().LengthRule();

    private static readonly InlineValidator<SoftItemTitle> Validator = new();
    static ItemTitleRules() => Validator.RuleFor(x => x).ItemTitleIsValid();
    public static ValidationResult Check(this SoftItemTitle t) => Validator.Validate(t);
}
