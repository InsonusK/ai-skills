---
name: rule-as-irulebuilder-extension
description: How the reusable business-predicate abstraction (Rule) is shaped so it works uniformly across VO constructors, FluentValidation validators, async cross-aggregate checks, and external reuse.
problem: A Rule must be callable fail-fast from a VO constructor, collect-all from a FluentValidation validator, and carry ErrorCode/Message/State/Severity for the frontend — without reinventing FluentValidation's own machinery and without requiring Domain to depend on FluentValidation for the wrong reasons.
decision: Rule is a static class holding a primitive `bool` predicate plus a FluentValidation `IRuleBuilder<T,TProperty>` extension method (and, where a standalone value exists, a `Check()` convenience wrapping an `InlineValidator`); it lives in a dedicated, reusable `Domain.Rules` project that references FluentValidation directly.
---

# Problem

A business predicate ("Complexity must be non-negative", "Transaction amount must not exceed Account balance") needs to be:
- callable fail-fast from a Value Object constructor (throw `DomainException` on the first violation, never a partially-invalid object);
- callable collect-all from a FluentValidation `AbstractValidator<T>` (return every violation in the request in one pass);
- carrying an `ErrorCode`, a default `Message`, and structured `State` a frontend can build its own text from;
- callable from an async context (`MustAsync`/`CustomAsync`) when the values it needs must first be loaded from a repository;
- reusable, unmodified, by another .NET service that only wants the condition, not this service's `DomainException`/pipeline conventions.

An earlier iteration of this solution introduced a custom `RuleResult` type (`Shared.Rule`) to carry `ErrorCode`/`Severity`/`Context` without depending on FluentValidation. That type turned out to duplicate a subset of `FluentValidation.Results.ValidationFailure` for no benefit once FluentValidation became this organization's fixed standard across all services — the constraint the custom type existed to satisfy (Domain must not depend on FluentValidation) no longer held.

# Selected variant

**Selected variant:** [[#Rule as bool primitive + IRuleBuilder extension]]

# Searched variants

## Rule as bool primitive + IRuleBuilder extension (selected)

### Description

Each rule is a static class with up to three members:
1. `IsValid(this TValue value) : bool` — the pure predicate, no I/O, mutation-tested in isolation.
2. `{Name}IsValid<T>(this IRuleBuilder<T, TValue> rule) : IRuleBuilderOptions<T, TValue>` — the FluentValidation wiring: `Must(...).WithErrorCode(...).WithMessage(...).WithState(...)`, the single place these four are declared.
3. `Check(this TValue value) : ValidationResult` — a convenience that runs a `static readonly InlineValidator<TValue>` wired once through member 2, for callers (VO constructors, raw predicate tests) that just want a full verdict without building a validator themselves.

`TValue` is either a named `SoftVO` (Format validation, or a Semantic/Domain wrapper worth naming) or an anonymous tuple (a Semantic/Domain wrapper local to one rule). `Domain.Rules` is a separate, reusable csproj — referencing `{Module}.Interfaces` and `FluentValidation` directly — so an external .NET service (or another module in this solution) can call `{Rule}IsValid()` in its own `RuleFor()` chain, or `Check()` directly, without adopting this service's exception/pipeline conventions.

VO constructors and Entity methods call `Check()` and pick the first `Error`-severity failure:
```csharp
var result = value.Check();
var blocking = result.Errors.FirstOrDefault(e => e.Severity == Severity.Error);
if (blocking is not null)
    throw new DomainException(blocking.ErrorCode, blocking.ErrorMessage);
```
`Errors.Any(e => e.Severity == Severity.Error)`/`FirstOrDefault` is used deliberately instead of `!result.IsValid` — `ValidationResult.IsValid` is `!Errors.Any()` regardless of `Severity`, so a future `Warning`-severity rule sharing the same `Check()` would otherwise incorrectly block construction. No rule uses `Warning` today, but every throw-site is written this way from the start so nothing needs retrofitting later.

### Benefits

- Zero reinvention: `ErrorCode`/`Message`/`State`/`Severity` are exactly FluentValidation's own `ValidationFailure` fields, not a parallel type.
- Fail-fast (VO) and collect-all (DTO/Command validator) both read the same `ValidationResult` — one is "look at the first Error", the other is "let FluentValidation collect all of them" — no divergent logic paths.
- `Domain.Rules` referencing FluentValidation directly is free: the package itself has no transitive dependencies, and FluentValidation is already mandatory across every consuming service.
- Async cross-aggregate rules that already have a sync `Check()` reuse it as-is via `CustomAsync` + `context.AddFailure(failure)` — no re-declaration of `ErrorCode`/`Message` at the async call site (see `TransactionWithdrawalCheck` in [domain-validation.md](../domain-validation.md)).

### Costs

- `Domain.Rules` now carries a real NuGet dependency (`FluentValidation`) — a service that genuinely cannot take that dependency cannot reuse it as-is.
- Two call sites exist for `Must`/`WithErrorCode` in the async case with no sync counterpart (existence-style checks) — the async wrapper must declare its own `ErrorCode`/`Message`, because there is nothing to forward from.

## RuleResult custom type (Shared.Rule)

### Description

A hand-written `record RuleResult(bool IsAccepted, string? RejectionCode, RuleSeverity Severity, object? Context)` with `Accept()`/`Reject()`/`Flag()` factories, referenced by a `Rule<T>` delegate type. `Domain.Rules` stayed free of any FluentValidation dependency; callers translated `RuleResult` to whatever shape they needed (FluentValidation `ValidationFailure`, HTTP response, etc.).

### Benefits

- Domain never references FluentValidation, satisfying a "Domain has zero external dependencies" rule taken at face value.
- Fail-fast and collect-all are trivially compatible — `RuleResult` is just a value, no execution model to reconcile.

### Costs

- Duplicates a subset of `FluentValidation.Results.ValidationFailure` field-for-field, with no behavior FluentValidation didn't already provide once FluentValidation is a mandatory dependency everywhere else.
- Every adapter (VO, PropertyValidator, DtoValidator) needs its own translation code between `RuleResult` and whatever FluentValidation expects downstream — an extra layer that produced no observable benefit in this codebase.
- A second, parallel severity/error-code vocabulary to keep in sync with FluentValidation's own, purely because of a dependency constraint that no longer held.

## Rule as an instantiated FluentValidation object (PropertyValidator&lt;T,TProperty&gt;)

### Description

Each rule is its own `PropertyValidator<T, TProperty>` subclass (FluentValidation's low-level building block for `.Must()`/`.NotEmpty()` etc.), instantiated once as a singleton and plugged into `RuleFor(...).SetValidator(...)`.

### Benefits

- Fully idiomatic FluentValidation — no extension-method indirection.

### Costs

- Already rejected by this codebase's own prior ADR (`use-abstract-validator-for-soft-value-objects`): `PropertyValidator<T, TProperty>` is bound to a specific parent type `T` and cannot be resolved generically as `IValidator<TProperty>` by another module through DI.
- Cannot be called synchronously from a VO constructor without instantiating a validator object inside the constructor — against the "Rules are static, never instantiated" principle already established for this domain layer.

## Bool-only static Rule + centralized RejectionCodes.cs registry

### Description

The pre-session baseline: `{X}Rules.IsValid(this SoftX) : bool`, with every rejection code declared as a `public const string` in one `{Module}.Interfaces/RejectionCodes.cs` file, referenced by name from every adapter (VO, PropertyValidator).

### Benefits

- One file to scan for every rejection code a module can produce.
- Simple, no FluentValidation coupling anywhere.

### Costs

- No carrier for `Severity`/structured `State`/default `Message` — every adapter that needed one of these had to invent its own convention.
- A central registry file becomes a second point of truth alongside the rule itself; nothing enforced that a code declared there was actually used by the rule it named.
- Replaced by decentralized `public const string ...Code`/`...Message` declared next to the rule that produces them, format `{ModuleName}.{Класс}.{Причина}`, with an architecture test asserting uniqueness across the `Domain.Rules` assembly instead of a file a human has to read.
