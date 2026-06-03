---
name: entity-validator-property-template
description: Template of property validator
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - validator
    - property-validator
---
# Template of property validator

## Code naming convension
You must follow this naming convension for validation error code: `{EntityName}.{ EntityName Property }.{ Rule }`

> **IMPORTANT:** Validation error codes for the same logical check (e.g. "Name is empty") MUST use the same code string, even when the check is applied to different entities or models. This allows clients to handle validation errors uniformly.

## Sepparate property validator

### # When to use this Template
When you need to realise complex custom logic **that is not available in built-in FluentValidation rules**.
> **DO NOT** create a custom `PropertyValidator` just to wrap a built-in FluentValidation rule (e.g. `NotEmpty`, `MaximumLength`, `Must`). Use built-in rules directly in the validator class instead.
### Template
```CSharp
using FluentValidation;
using FluentValidation.Validators;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Validators.Properties;

public class {EntityName}{{ Property }}{{ Rule }}Validator<T> : PropertyValidator<T, {{ Property Type }}> where T : {EntityName}.IBody // or {EntityName}
{
    public override string Name => Code;
    public const string Code = $"{nameof({EntityName})}.{nameof({EntityName}.{{ Property }})}.{{ Rule }}"; // Error code
    
    protected override string GetDefaultMessageTemplate(string errorCode) 
        => $"..."; // Error description
        
    public override bool IsValid(ValidationContext<T> context, string value)
    {
        //validation logic        
    }
}
```

## Extension property validator
### # When to use this Template
When you need to compose **multiple built-in FluentValidation rules** together with a custom error code, or to wrap an **async validator** (e.g. `SetAsyncValidator`) that requires a repository or external dependency.
> **DO NOT** create an extension method just to rename a single built-in rule call (e.g. `.NotEmpty()`, `.MaximumLength()`). Use the built-in rules directly unless you need a custom error code or composition.
> The `T` type constraint (`where T : {EntityName}.IBody`) is optional: use it only if the extension logically applies exclusively to objects implementing that interface. For DTOs that mirror the entity's properties without implementing `IBody`, omit the constraint.
### Template

```CSharp
using FluentValidation;
namespace {ProjectNamespace}.Entities.{EntityName}Entity.Validators.Properties;

public static class {EntityName}{{ Property }}ValidationExtensions 
{
    public static string Code = $"{nameof({EntityName})}.{nameof({EntityName}.{{ Property }})}.{{ Rule }}"
    public static IRuleBuilderOptions<T, string> Is{{ Rule }}<T>(this IRuleBuilder<T, string> ruleBuilder) // add: where T : {EntityName}.IBody if applicable
    {
        return ruleBuilder
            .// Validation logic (async validator, complex composition, etc.)
            .WithErrorCode(Code)
            .WithMessage("{{ error description }}");
    }
}
```

## Rules
- Only create a custom property validator or extension method when the validation logic is **not** available as a single built-in FluentValidation rule.
- For built-in rules (`NotEmpty`, `MaximumLength`, `Must`, etc.), call them directly inside the validator class — do not wrap them in a separate extension.
- The same logical validation check must use the **same error code** string across different entities and models.
- Instead of T use Entity type or IBody. Property validator or Extension should be applicable only for validating the concrete property it is named for.