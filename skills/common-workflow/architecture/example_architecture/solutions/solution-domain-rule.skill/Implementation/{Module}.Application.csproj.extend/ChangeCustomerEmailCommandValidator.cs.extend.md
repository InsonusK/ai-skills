---
description: Redirect the transport validator to call EmailRule.IsValid instead of its own local condition
project_name: "{Module}.Application"
name: ChangeCustomerEmailCommandValidator
element_kind: class
change_kind: extend
tags:
  - solution/domain-rule
  - element/changecustomeremailcommandvalidator-cs
---

# Implementation changes
```csharp
// {Module}.Application/Commands/ChangeCustomerEmailCommandValidator.cs
public class ChangeCustomerEmailCommandValidator : AbstractValidator<ChangeCustomerEmailCommand>
{
    public ChangeCustomerEmailCommandValidator()
    {
        RuleFor(x => x.NewEmail)
            .NotEmpty()
            .Must(EmailRule.IsValid)
            .WithMessage("Email must contain '@'.");
    }
}
```

# Rule changes

## MUST
- Call `EmailRule.IsValid` from `.Must(...)` instead of the validator's own inline lambda.
  - Risk: keeping the inline lambda alongside `EmailRule` leaves two copies of the same condition, defeating the reason this solution exists.
  - Fix: replace the lambda body with a direct reference to `EmailRule.IsValid`, as shown above.
