---
description: The single exception type for an entity invariant violation
project_name: Shared
name: "DomainException.cs"
element_kind: class
change_kind: create
tags:
  - solution/domain-behaviour
  - element/domain-exception-cs
---

# Goals
- One exception type, carrying a stable machine code plus a human message, thrown whenever a guarded entity method's condition fails.

# Implementation changes

```csharp
// Shared/Exceptions/DomainException.cs
namespace Shared.Exceptions;

public sealed class DomainException : Exception
{
    public string Code { get; }

    public DomainException(string code, string message) : base(message) => Code = code;
}
```

`Code` convention: `{ModuleName}.{Entity}.{Reason}` (e.g. `Task.Comment.Required`).

# Rules

## MUST
- Give every `DomainException` a `Code` in `{ModuleName}.{Entity}.{Reason}` form.
  - Risk: message-only exceptions cannot be matched by a caller, a test, or a rejection-code architecture test.
  - Fix: always pass `code` and `message`.
- Keep it `sealed` with no subtypes.
  - Risk: subtypes fragment the single type `solution-mediator-exception-handler` recognises for expected-vs-unexpected mapping.
  - Fix: one type; the `Code` distinguishes cases.

# Check list
- [ ] `DomainException` is `sealed`, in `Shared.Exceptions`, with a `Code` property.
- [ ] Every throw site passes both `code` and `message`.
