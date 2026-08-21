---
name: csproj-module-domain-rules
description: Dedicated, reusable project holding every centralized business predicate (Rule) for this bounded context — an optional fifth project beyond the module's base four, added only when a condition is found duplicated across two or more consumers
domain: skill
type: template
version: 20260821
plateau: default
tags:
  - skill/template/csproj
  - plateau/default
  - stack/dotnet
  - concern/architecture

created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]]"
---

# Goal
- Give a module one dedicated, reusable project for business predicates already duplicated across a VO constructor, an Entity method, a PropertyValidator, or a DTO/Command validator
- Isolate Rules for mutation testing without pulling in Entities or other Domain-layer code
- Let another .NET service reuse a rule's condition, unmodified, without adopting this service's `DomainException`/pipeline conventions

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create|{Module}.Domain.Rules.csproj]]

# Core Principles
- This project exists only once a condition is found genuinely duplicated across two or more consumers — it is not part of the module's guaranteed base four projects (`Api`/`Application`/`Domain`/`Interfaces`), it is added on top per `solution-sln-structure`'s own base-set-plus-extension rule
- References only `FluentValidation` and `{Module}.Interfaces` (for `Soft{ValueObject}` types) — never `{Module}.Domain`, `{Module}.Application`, a repository, or `DbContext`
- Every rule is `IsValid()` + one `IRuleBuilder` extension + `Check()` — see `class-rule.skill.md`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create|{Module}.Domain.Rules.csproj]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Domain.Rules
```

## Project Structure
- /{Module}.Domain.Rules
  - /Common
    - [ModuleInfo.cs](skills/dotnet/architecture/plateau/default/structure/{Module}.Domain.Rules/classes/class-module-info.skill.md)
  - [{Rule}.cs](skills/dotnet/architecture/plateau/default/structure/{Module}.Domain.Rules/classes/class-rule.skill.md)
  - {Module}.Domain.Rules.csproj

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create|{Module}.Domain.Rules.csproj]]

## NuGet Packages
| Package | Purpose |
| --- | --- |
| `FluentValidation` | `IRuleBuilder<T,TProperty>`, `AbstractValidator<T>`, `InlineValidator<T>`, `ValidationResult` |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create|{Module}.Domain.Rules.csproj]]

## What Does NOT Belong Here
- A condition used by exactly one consumer — stays local to that consumer (see `solution-value-objects`/`solution-domain-behaviour`/`solution-dto-property-validators`'s own Boundaries)
- Entity types, Value Object types, or any Domain-layer class — those live in `{Module}.Domain`
- Repository or `DbContext` references, or any I/O
- The Try/Confirm saga's Handler/Consumer orchestration — that lives in `{Module}.Application`, following `solution-command-integration`

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create|{Module}.Domain.Rules.csproj]]

# Rules
MUST:
	- Reference only `FluentValidation` and `{Module}.Interfaces`
	- Live at `/src/Modules/{ModuleName}/{ModuleName}.Domain.Rules`
	- Exist only when a condition is genuinely duplicated across two or more consumers
MUST NOT:
	- Reference `{Module}.Domain`, `{Module}.Application`, a repository, or `DbContext`
	- Be created speculatively, before any real duplication is found

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create|{Module}.Domain.Rules.csproj]]

# Check list
- [ ] `{Module}.Domain.Rules.csproj` exists only when applied — a module without duplicated conditions does not have this project
- [ ] References only `FluentValidation` and `{Module}.Interfaces`
- [ ] No repository/`DbContext`/`{Module}.Domain`/`{Module}.Application` reference anywhere in the project
- [ ] Every rule redirected here from a consumer no longer has a local copy of the same condition

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/solution-domain-rules.skill|solution-domain-rules]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create|{Module}.Domain.Rules.csproj]]
