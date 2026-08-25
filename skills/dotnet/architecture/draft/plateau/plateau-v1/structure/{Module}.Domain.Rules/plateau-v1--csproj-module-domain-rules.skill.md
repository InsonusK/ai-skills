---
name: csproj-module-domain-rules
description: Project {Module}.Domain.Rules in the v1 plateau
whenToUse: when a condition has been duplicated across a VO constructor, an Entity method, a PropertyValidator, or a DTO/Command validator, and needs one centralized, reusable home
domain: skill
type: template
plateau: v1
version: 20260824150000
tags:
  - skill/template/csproj
  - plateau/v1
created_by:
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Give every centralized business predicate for this module one dedicated, portable project — isolatable for mutation testing without pulling in Entities or other Domain-layer code
- Let another .NET service reuse a rule's condition, unmodified, without adopting this service's `DomainException`/pipeline conventions

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create.md|{Module}.Domain.Rules.csproj.create]]

# Core Principles
- A rule is always: bundle the values it needs into a wrapper, then apply `IsValid()`/`IRuleBuilder`-extension/`Check()` to it — Format (wrapper already a container property), Semantic (wrapper assembled from the container's own fields), and Domain (wrapper assembled from data loaded elsewhere) differ only in where the wrapper's values come from
- `ErrorCode`/default `Message`/`State` are declared exactly once per rule, inside the `IRuleBuilder` extension — every consumer calls it or forwards its `ValidationResult`, never re-declares
- Never performs I/O — loading is always the caller's job (Handler, DI-injected async wrapper, `CustomAsync`/`MustAsync`)
- Applying this project to centralize a condition is optional and never speculative — only once the same condition is genuinely duplicated in two or more of `{ValueObject}.cs`/`{EntityName}.cs`/`{ValueObject}PropertyValidator.cs`/`{Dto}.Validator.cs`

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create.md|{Rule}.cs.create]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Domain.Rules
```

## Project Structure
- /{Module}.Domain.Rules
  - /Common
    - [ModuleInfo.cs](./classes/plateau-v1--class-module-info.skill.md)
  - [{Rule}.cs](./classes/plateau-v1--class-rule.skill.md)
  - {Module}.Domain.Rules.csproj

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create.md|{Module}.Domain.Rules.csproj.create]]

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Common/ModuleInfo.cs | Module name constant, source of every rejection code's prefix | [[./classes/plateau-v1--class-module-info.skill.md\|class-module-info]] |
| {Rule}.cs | `IsValid()` + `IRuleBuilder` extension + `Check()`, one class per centralized condition | [[./classes/plateau-v1--class-rule.skill.md\|class-rule]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `FluentValidation` | same as `{Module}.Application` | `IRuleBuilder<T,TProperty>`, `AbstractValidator<T>`, `InlineValidator<T>`, `ValidationResult` |

## What Does NOT Belong Here
- Entity/Value Object definitions — belong to `{Module}.Domain`
- Handlers, DTO validators, DI registration — belong to `{Module}.Application`
- Any repository/`DbContext` reference or I/O of any kind

## Allowed Dependencies
- `{Module}.Interfaces` — for the `Soft{ValueObject}` types a rule's wrapper is often built from
- FluentValidation

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create.md|{Module}.Domain.Rules.csproj.create]]

# Rules
MUST:
- Reference `{Module}.Interfaces` and `FluentValidation` only
- Every rejection code `public const string`, next to the rule that produces it, format `{ModuleName}.{Class}.{Reason}`
- Every rule have exactly one `IRuleBuilder` extension declaring `Must`/`WithErrorCode`/`WithMessage`/`WithState`
MUST NOT:
- Reference a repository, `DbContext`, `{Module}.Domain`, or `{Module}.Application`
- Perform any I/O
- Be applied to a condition that is not genuinely duplicated in at least two consumers

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create.md|{Module}.Domain.Rules.csproj.create]]

# Check list
- [ ] `{Module}.Domain.Rules.csproj` references only `{Module}.Interfaces` and FluentValidation
- [ ] Every centralized condition was previously duplicated in at least two consumers
- [ ] No repository/`DbContext`/`{Module}.Domain`/`{Module}.Application` reference anywhere

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create.md|{Module}.Domain.Rules.csproj.create]]
