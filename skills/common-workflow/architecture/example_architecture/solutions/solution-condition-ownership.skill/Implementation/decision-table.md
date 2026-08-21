---
description: What each branch of the condition-ownership decision produces
element_kind: repository
change_kind: create
tags:
  - solution/condition-ownership
  - element/decision-table
---

# Decision table

| Owners of the condition right now | Branch | What gets applied |
| --- | --- | --- |
| 1 (only `Email`'s constructor) | Keep local | Nothing beyond [[../../solution-value-object.skill/solution-value-object.skill.md\|solution-value-object]]. `Email` keeps its own `IsValidFormat` predicate. |
| 2+ (`Email`'s constructor *and* `ChangeCustomerEmailCommandValidator`) | Centralize | Apply [[../../solution-domain-rule.skill/solution-domain-rule.skill.md\|solution-domain-rule]]: create `EmailRule`, redirect both owners to call it. |

## One owner (before the transport validator exists)
```csharp
// Email.cs keeps its own predicate — this is solution-value-object alone, unmodified.
private static bool IsValidFormat(string value) =>
    !string.IsNullOrWhiteSpace(value) && value.Contains('@');
```

## Two owners (after solution-transport-validation is applied)
```csharp
// Both call the same EmailRule — this is solution-domain-rule's Email.cs.extend.md and
// ChangeCustomerEmailCommandValidator.cs.extend.md applied together.
EmailRule.Check(value);            // inside Email's constructor
.Must(EmailRule.IsValid)           // inside ChangeCustomerEmailCommandValidator
```
