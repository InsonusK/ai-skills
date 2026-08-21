---
description: Transport-shape validator for ChangeCustomerEmailCommand
project_name: "{Module}.Application"
name: ChangeCustomerEmailCommandValidator
element_kind: class
change_kind: create
tags:
  - solution/transport-validation
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
            .Must(v => v.Contains('@'))
            .WithMessage("Email must contain '@'.");
    }
}
```

# Rule changes

## MUST
- Register this validator via assembly scan, not by manual construction.
  - Risk: a manually-wired validator is easy to forget when adding a new command, silently leaving it unvalidated.
  - Fix: use FluentValidation's `AddValidatorsFromAssembly` (or equivalent) so every validator is discovered automatically.
