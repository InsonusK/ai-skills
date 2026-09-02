---
name: plateau-offline-sync-service--csproj-module-domain-rules
description: Project {Module}.Domain.Rules in the plateau-offline-sync-service plateau — the module's fifth project, holding every centralized business predicate (Rule), portable to any .NET service or client
whenToUse: when centralizing a duplicated condition into a Rule, editing a Rule, or checking that Domain.Rules stays portable (FluentValidation + Interfaces only, no I/O)
domain: skill
type: template
plateau: offline-sync-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/offline-sync-service
created_by:
  - "[[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]]"
---

# Goal
- Give every business predicate for the module one dedicated project — isolatable for mutation testing, and reusable by another .NET service (or a Blazor/offline client) without adopting this service's `DomainException` / pipeline conventions.
- Exists only once a condition is genuinely duplicated across two or more consumers (`{ValueObject}` ctor, entity method, `{ValueObject}PropertyValidator`, `{Dto}Validator`, `{Feature}Check`) — applying VP4 is a refactor, not a prerequisite.

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create.md|{Module}.Domain.Rules.csproj.create]]

# Core Principles
- References **FluentValidation and `{Module}.Interfaces` only** — never a repository, `DbContext`, `{Module}.Domain`, or `{Module}.Application`. Never performs I/O.
- The module's fifth project, on top of the base `Interfaces`/`Application`/`Domain`/`Api` — justified because Rules needs project-level isolation the base four cannot give.
- A `{Rule}` class is `IsValid()` (pure predicate) + one `IRuleBuilder` extension (the only place `ErrorCode`/`Message`/`State` are declared) + `Check()` (a `static readonly InlineValidator<TWrapper>` built once).
- Rejection codes are `public const string` next to the rule, format `{ModuleName}.{Class}.{Reason}`, prefix from `Common/ModuleInfo.ModuleName` — no central registry file.
- Classification (Format / Semantic / Domain) is only about where the wrapper's values come from — the rule itself never knows.

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Domain.Rules
```

## Project Structure
- /{ModuleName}.Domain.Rules
  - /Common/[ModuleInfo.cs](./classes/plateau-offline-sync-service--class-module-info.skill.md)
  - [{Rule}.cs](./classes/plateau-offline-sync-service--class-rule.skill.md)
  - {ModuleName}.Domain.Rules.csproj

`{ModuleName}.Domain.Rules.Spec` (a sibling directory of `.feature` files, **not a project**) is the shared Gherkin source every layer's step definitions link in — see the sln skill.

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Common/ModuleInfo.cs | Module-name constant, source of every rejection code's prefix | [[./classes/plateau-offline-sync-service--class-module-info.skill.md\|class-module-info]] |
| /{Rule}.cs | `IsValid()` + `IRuleBuilder` extension + `Check()` | [[./classes/plateau-offline-sync-service--class-rule.skill.md\|class-rule]] |

## NuGet Packages
| Package | Purpose |
| --- | --- |
| FluentValidation | `IRuleBuilder<T,TProperty>`, `InlineValidator<T>`, `ValidationResult` — the whole mechanism |

## What Does NOT Belong Here
- Any I/O or data loading — that is the caller's job (a handler, a DI-injected async wrapper, `CustomAsync`).
- A pre-computed verdict passed in — the rule performs the comparison itself, over already-loaded raw values.
- `{Module}.Domain` / `{Module}.Application` types, a repository, a `DbContext`.

## Allowed Dependencies
- `{Module}.Interfaces` (for `Soft{ValueObject}` types)
- NuGet: `FluentValidation`

# Rules
MUST:
- Reference only `FluentValidation` and `{Module}.Interfaces`; live under `/src/Modules/{ModuleName}/{ModuleName}.Domain.Rules`.
- Declare `ErrorCode` / `Message` / `State` exactly once per rule, inside the `IRuleBuilder` extension; combine multiple conditions on one wrapper into a single public extension with `private` individual `Must()` calls.
- Keep `IsValid()` a pure synchronous predicate; build `Check()`'s `InlineValidator` once (`static readonly`).
- Only centralize a condition already duplicated in two or more consumers; delete every local copy on redirect.
- Never reference a repository / `DbContext` / `{Module}.Domain` / `{Module}.Application`; never perform I/O; never accept a pre-computed boolean.

__Applied solutions:__
- [[../../../../solutions/solution-domain-rules.skill/solution-domain-rules.skill.md|solution-domain-rules]] - [[../../../../solutions/solution-domain-rules.skill/Implementation/{Module}.Domain.Rules.csproj.create/{Rule}.cs.create.md|{Rule}.cs.create]]

# Check list
- [ ] `{Module}.Domain.Rules.csproj` references only `FluentValidation` + `{Module}.Interfaces`.
- [ ] Every rejection code is `public const string` next to its rule, `{ModuleName}.{Class}.{Reason}`.
- [ ] Every rule has exactly one `IRuleBuilder` extension; `Check()` reuses a single `static readonly InlineValidator`.
- [ ] No repository / `DbContext` / `{Module}.Domain` / `{Module}.Application` reference; no I/O.
