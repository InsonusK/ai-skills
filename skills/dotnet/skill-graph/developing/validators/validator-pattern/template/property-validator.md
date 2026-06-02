---
name: validator-property-template
description: Template of property validator
metadata:
  domain: dotnet
  tags:
    - dotnet
    - validator-pattern
    - property-validator
---
# Template of property validator

## [2.2] Custom Property Rule
*Use for complex logic or external dependencies (Repositories).*
```C#
using FluentValidation;
using FluentValidation.Validators;

namespace {ProjectNamespace}.Validators.Properties;

public class Is{Rule}Validator<T> : PropertyValidator<T, {{ Property Type }}> 
//where T : {InterfaceModel} - Use T definition in case when Rule depends on specific other fields
{
    public override string Name => Code;
    public const string Code = "IsNot{Rule}"; // Error code
    
    protected override string GetDefaultMessageTemplate(string errorCode) 
        => $"..."; // Error description
        
    public override bool IsValid(ValidationContext<T> context, string value)
    {
        // For DI, use context.GetServiceProvider().GetRequiredService<T>();
        return {Logic};
    }
}
```

## [2.1] Extension Property Validator
*Use for wrapping built-in rules with custom metadata.*

```C#
using FluentValidation;
namespace {ProjectNamespace}.Validators.Properties;

public static class Is{Rule}ValidationExtensions 
{
    public static string Code = "IsNot{Rule}"
    public static IRuleBuilderOptions<T, string> Is{Rule}<T>(this IRuleBuilder<T, string> ruleBuilder) // add: where T : {EntityName}.IBody if applicable
    {
        return ruleBuilder
            .{BuiltInRule}() // e.g., NotEmpty()
            .WithErrorCode(Code)
            .WithMessage("{{ error description }}");
    }
}
```