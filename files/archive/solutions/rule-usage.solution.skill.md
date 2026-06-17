---
uid:
name: rule-usage
description: defines where domain rules must be used and how they flow from domain-rule classes into Value Objects, Entities, Domain Services, and Validators
domain: skill
type: architecture
version: 20260607
tags:
  - skill/architecture/solution
  - dotnet
  - domain
  - rules
triggers:
  - where to use domain rules
  - validate with domain rule
  - rule consumption pattern
---
# Goal
Define where domain rules are consumed across the codebase. A rule is defined once and used in multiple places — Value Object constructors, Entity methods, Domain Services, and Application Validators. Without this solution, the same business predicate gets reimplemented in each place with slight variations, creating silent inconsistencies.

# Core Principles
- Rule is defined once — all consumers reference the same rule class
- Rule returns `bool` — the consumer decides whether to throw or collect the error
- Domain consumers (VO, Entity, Service) throw `DomainException` on rule violation
- Application Validator collects rule violations via FluentValidation — never throws
- No consumer reimplements rule logic inline

# Depend on
_none — this is a root solution_

# Flow
```
domain-rule.class defined in {Module}.Domain/Rules
    ↓
Value Object constructor calls rule → throws DomainException if violated
Entity method calls rule → throws DomainException if violated  
Domain Service calls rule → throws DomainException if violated
    ↓
Application Validator calls same rule via Must() → collects error if violated
    ↓
All errors returned together in Result.Invalid — never one at a time
```

# Implementation

## {Rule}.cs — `{Module}.Domain/Rules`
Define rule as static extension method. Returns `bool` only — never throws.
```csharp
public static class TitleRules
{
    public static bool IsValidLength(this string value) 
        => !string.IsNullOrWhiteSpace(value) && value.Length <= 200;
}
```

## {ValueObject}.cs — `{Module}.Domain/ValueObjects`
Constructor calls rule, throws `DomainException` if violated.
```csharp
public sealed record TaskTitle
{
    public string Value { get; }
    public TaskTitle(string value)
    {
        if (!value.IsValidLength())
            throw new DomainException("Title is invalid");
        Value = value;
    }
}
```

## {Entity}.cs — `{Module}.Domain/Entities`
Behavior method calls rule, throws `DomainException` if violated.
```csharp
public void SetTitle(string title)
{
    if (!title.IsValidLength())
        throw new DomainException("Title is invalid");
    Title = title;
}
```

## {FeatureName}.Validator.cs — `{Module}.Application/Commands/{FeatureName}`
Validator calls same rule via `Must()` — collects error, never throws.
```csharp
RuleFor(x => x.Title)
    .Must(t => t.IsValidLength())
    .WithMessage("Title is invalid");
```

# Example
```csharp
// 1. Rule defined once
public static class TitleRules
{
    public static bool IsValidLength(this string value)
        => !string.IsNullOrWhiteSpace(value) && value.Length <= 200;
}

// 2. VO uses rule — throws on invalid
public sealed record TaskTitle(string Value)
{
    public TaskTitle(string value) : this(value)
    {
        if (!value.IsValidLength())
            throw new DomainException("Title is invalid");
    }
}

// 3. Validator uses same rule — collects error
RuleFor(x => x.Title)
    .Must(t => t.IsValidLength())
    .WithMessage("Title is invalid");

// Result: validator catches invalid input before domain is reached.
// Domain rule is still the last line of defense.
```

# Rules
MUST:
- Every business predicate defined as a rule in `{Module}.Domain/Rules`
- All consumers reference the rule — never reimplement inline
- Domain consumers throw `DomainException` when rule returns `false`
- Application Validator uses `Must(rule)` — never throws, collects errors
MUST NOT:
- Rule throw internally — returns `bool` only
- Validator reimplement rule logic inline with lambda
- Same predicate exist in both rule class and validator lambda

# Anti-patterns
- Inline validation in validator: `.Must(t => !string.IsNullOrWhiteSpace(t) && t.Length <= 200)` — extract to rule
- Rule throws `DomainException` itself — rule returns `bool`, caller throws
- VO validates differently than validator — silent inconsistency, user gets past validator then hits domain exception

# Checklist
- [ ] Rule defined in `{Module}.Domain/Rules`
- [ ] Rule returns `bool`
- [ ] VO constructor calls rule and throws `DomainException`
- [ ] Entity methods call rule and throw `DomainException`
- [ ] Validator calls same rule via `Must()`
- [ ] No inline predicate logic in validator or domain that duplicates a rule

# Unittest TestCases
- [ ] When rule returns false Then VO constructor throws DomainException
- [ ] When rule returns false Then validator collects error without throwing
- [ ] When rule returns false in entity method Then DomainException thrown
- [ ] When same invalid value sent via HTTP Then validator catches it before domain is reached

# Relations
- domain-rule.class.skill.md — rule class structure
- value-object.class.skill.md — VO consumes rules in constructor
- entity-behavior.solution.skill.md — entity methods consume rules
- feature-validator.class.skill.md — validator consumes rules via Must()
