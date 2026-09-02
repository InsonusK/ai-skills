---
name: plateau-offline-sync-service--class-rule
description: Class {Rule} in the plateau-offline-sync-service plateau — a centralized business predicate (IsValid + IRuleBuilder extension + Check) in {Module}.Domain.Rules
whenToUse: when creating or editing a centralized Rule, or classifying one as Format / Semantic / Domain
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Encode a reusable business predicate as a single source of truth — condition, rejection code, default message, structured state — callable fail-fast (VO/entity ctor) and collect-all (DTO/command validator) with no divergent logic.

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create.md|{Rule}.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- `static class {Concept}Rules` (plural — one or more conditions on one named wrapper) or `{Concept}Rule` (singular — one condition on an anonymous tuple).
- A rule is always: bundle the values into a **wrapper**, then `IsValid()` / `IRuleBuilder` extension / `Check()` over it. Format = the wrapper is a container property; Semantic = assembled from the container's own fields; Domain = assembled after a caller-side `Load`. The rule never knows which.
- `ErrorCode` / `WithMessage` / `WithState` are declared **once**, in the `IRuleBuilder` extension; multiple conditions on one wrapper fold into one public extension with `private` `Must()` calls.
- `Check()` uses a `static readonly InlineValidator<TWrapper>` built once in the static ctor. A blocking check reads `result.Errors.Any(e => e.Severity == Severity.Error)`, never bare `IsValid`.
- Never performs I/O; never accepts a pre-computed verdict.

# Implementation
```csharp
// Skill: plateau-offline-sync-service--class-rule
// Plateau: offline-sync-service
// Version: 20260902000000
using FluentValidation;
using FluentValidation.Results;
using {Module}.Domain.Rules.Common;
using {Module}.Interfaces.ValueObjects;

namespace {Module}.Domain.Rules;

public static class {ValueObject}Rules
{
    public const string RequiredCode = ModuleInfo.ModuleName + ".{ValueObject}.Required";
    public const string TooLongCode  = ModuleInfo.ModuleName + ".{ValueObject}.TooLong";

    public static bool IsRequired(this Soft{ValueObject} v) => !string.IsNullOrWhiteSpace(v.Value);
    public static bool IsWithinLength(this Soft{ValueObject} v) => v.Value is null || v.Value.Length <= 100;

    private static IRuleBuilderOptions<T, Soft{ValueObject}> RequiredRule<T>(this IRuleBuilder<T, Soft{ValueObject}> r)
        => r.Must(x => x.IsRequired()).WithErrorCode(RequiredCode).WithMessage("… is required.");
    private static IRuleBuilderOptions<T, Soft{ValueObject}> LengthRule<T>(this IRuleBuilder<T, Soft{ValueObject}> r)
        => r.Must(x => x.IsWithinLength()).WithErrorCode(TooLongCode).WithMessage("… must not exceed 100 characters.");

    public static IRuleBuilderOptions<T, Soft{ValueObject}> {ValueObject}IsValid<T>(this IRuleBuilder<T, Soft{ValueObject}> r)
        => r.RequiredRule().LengthRule();

    private static readonly InlineValidator<Soft{ValueObject}> Validator = new();
    static {ValueObject}Rules() => Validator.RuleFor(x => x).{ValueObject}IsValid();
    public static ValidationResult Check(this Soft{ValueObject} v) => Validator.Validate(v);
}
```

__Applied solutions:__
- [[../../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create.md|{Rule}.cs.create]]

# Rules
MUST:
- Declare `ErrorCode` / `Message` / `State` once, in the `IRuleBuilder` extension; fold multiple conditions on one wrapper into one public extension with `private` `Must()` calls.
- Keep `IsValid()` pure and synchronous; build `Check()`'s `InlineValidator` once.
- Assemble a Semantic wrapper from the container's own fields with no I/O; a Domain rule compares already-loaded raw values, never a pre-computed bool.
- Name the wrapper only when the field combination is a reusable concept; otherwise an anonymous tuple.
- Never `new` a rule; never re-declare `WithErrorCode`/`WithMessage` at a call site that has a `Check()` to forward from.
- Never apply several plateau templates per class.

# Check list
- [ ] `IsValid()` pure; one `IRuleBuilder` extension declaring the code/message/state.
- [ ] `Check()` reuses a `static readonly InlineValidator`.
- [ ] Rejection codes `public const string`, `{ModuleName}.{Class}.{Reason}`.
- [ ] No I/O; a Domain rule performs the comparison itself.

# Unittest TestCases
- [ ] WHEN a wrapper is checked with an invalid value THEN `Check()` reports the paired error code.
- [ ] WHEN the combined extension runs THEN both conditions are evaluated; the individual `Must()` calls are not independently callable.
