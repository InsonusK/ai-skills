using FluentValidation;
using FluentValidation.Results;
using Sample.Domain.Rules.Common;
using Sample.Interfaces.ValueObjects;

namespace Sample.Domain.Rules;

public static class TitleRules
{
    public const string RequiredCode = ModuleInfo.ModuleName + ".Title.Required";
    public const string MaxLengthCode = ModuleInfo.ModuleName + ".Title.MaxLength";

    public const string RequiredMessageTemplate = "Title is required.";
    public const string MaxLengthMessageTemplate = "Title must not exceed 200 characters, but was {0}.";

    public static bool IsRequired(this SoftTitle title) => !string.IsNullOrWhiteSpace(title.Value);

    public static bool IsMaxLength(this SoftTitle title) => title.Value is null || title.Value.Length <= 200;

    private static IRuleBuilderOptions<T, SoftTitle> RequiredRule<T>(this IRuleBuilder<T, SoftTitle> rule)
        => rule.Must(x => x.IsRequired())
               .WithErrorCode(RequiredCode)
               .WithMessage(RequiredMessageTemplate)
               .WithState((_, x) => new { x.Value });

    private static IRuleBuilderOptions<T, SoftTitle> MaxLengthRule<T>(this IRuleBuilder<T, SoftTitle> rule)
        => rule.Must(x => x.IsMaxLength())
               .WithErrorCode(MaxLengthCode)
               .WithMessage((_, x) => string.Format(MaxLengthMessageTemplate, x.Value?.Length ?? 0))
               .WithState((_, x) => new { x.Value });

    public static IRuleBuilderOptions<T, SoftTitle> TitleIsValid<T>(this IRuleBuilder<T, SoftTitle> rule)
        => rule.RequiredRule().MaxLengthRule();

    private static readonly InlineValidator<SoftTitle> _validator = new();
    static TitleRules() => _validator.RuleFor(x => x).TitleIsValid();
    public static ValidationResult Check(this SoftTitle title) => _validator.Validate(title);
}
