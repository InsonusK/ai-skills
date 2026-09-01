---
name: solution-dto-property-validators
description: The DTO/Command validation mechanism — a FluentValidation AbstractValidator<Soft{ValueObject}> per value concept and an AbstractValidator<{Dto}> per public DTO, each owning its own local condition, registered by assembly scan and resolvable cross-module through IValidator<T>.
whenToUse: when a public RequestDto or Command needs FluentValidation coverage for a value-concept property or for the DTO as a whole, or when another module needs to validate a Soft{ValueObject}/DTO it received without referencing the owning module's Domain or Application types directly.
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - concern/architecture
  - validation
  - framework/fluent-validation
  - dto
  - cross-module
  - solution/dto-property-validators
  - stack/dotnet
creates:
  - "{Module}.Application.Validators.Property.{ValueObject}PropertyValidator.cs"
  - "{Module}.Application.Validators.Model.{Dto}Validator.cs"
  - "{Module}.Application.Validators.Async.{Feature}Check.cs"
extends:
  - "{Module}.Application.csproj"
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
built_on_plateau:
adr:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/adr/use-abstract-validator-for-soft-value-objects.md|Use AbstractValidator for Soft{ValueObject} validators]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/adr/dto-validators-only-for-request-dtos.md|DTO validators only for RequestDto by default]]"
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/adr/defer-feature-check-loading-to-persistence-solution.md|Defer {Feature}Check's Load to the persistence-introducing solution]]"
---

# Goal
- Let each module publish a FluentValidation validator for every public RequestDto and value-concept property it owns
- Let other modules validate values and DTOs they receive through `IValidator<T>` resolved from DI, without referencing this module's `Domain` or `Application` types directly
- Make this solution work completely on its own, with every validator owning its own condition locally

# Capabilities
- Other modules validate a `Soft{ValueObject}` by resolving `IValidator<Soft{ValueObject}>` from DI
- Other modules validate a DTO by resolving `IValidator<{Dto}>` from DI
- Isolated, per-field conformance coverage without assembling a whole valid DTO around the field under test
- A cross-aggregate condition that needs preloaded data is checked before the Handler runs, through a small DI-injected async wrapper — usable once the module has some data-loading abstraction (e.g. `solution-repository-integration`'s `IReadRepository<T>`) to inject; see Boundaries

# Core Principles
- `{ValueObject}PropertyValidator : AbstractValidator<Soft{ValueObject}>` owns its own condition, written locally in this file
- `{Dto}Validator : AbstractValidator<{Dto}>` composes property validators via `SetValidator(IValidator<Soft{ValueObject}>)` for every value-concept property, and checks a cross-field condition spanning several of the DTO's own fields locally, with `.Must(...)`
- `{ValueObject}PropertyValidator` exists even though it could look like a one-line pass-through: it gives other modules DI decoupling (they depend on `IValidator<Soft{ValueObject}>`, never on this module's `Application` internals), and gives one field its own isolated conformance surface
- Every validator is registered via `AddValidatorsFromAssembly` — never manually
- ResponseDto does not get a validator by default; only when a concrete requirement (external contract check, untrusted response source) explicitly demands one
- A condition that needs preloaded data becomes a small, DI-injected async wrapper class (`{Feature}Check`) that loads the data and checks it locally, via `CustomAsync` — this solution defines the *pattern* only; it does not create a repository or any other data-loading abstraction, and `{Feature}Check`'s `Load` step is deliberately left unimplemented until the solution that introduces one supplies the concrete body via its own `.extend.md` on this same class (see Boundaries and [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/adr/defer-feature-check-loading-to-persistence-solution.md|Defer {Feature}Check's Load to the persistence-introducing solution]])
- This solution does not require a shared rules abstraction to exist — a later, optional `solution-domain-behaviour`/`solution-domain-rules` may keep validator-side and Entity-side conditions in sync some other way, but every validator here already works standalone

# Boundaries
- Which `Soft{ValueObject}`/DTO exists to validate is decided by [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]] (Soft VOs, common) and, for the strict Domain-side type, `solution-value-objects` (VP3) — not by this solution.
- Whether a validator's condition agrees with the same concept's Entity-side enforcement (`solution-domain-behaviour`, VP1) is not guaranteed or checked here — the two are written independently, and `solution-domain-rules` (VP4) is the mechanism for keeping a duplicated condition in one place. This solution does **not** `depends_on` either.
- The baseline (after `solution-sln-structure` + `solution-mediator-integration`) has no repository or other data-loading abstraction. `{Feature}Check`'s `Load` method is therefore left unimplemented here — a documented shape, not a usable capability — until `solution-repository-integration` (VP2) supplies the concrete `Load` body via its own `{Feature}Check.cs.extend.md` on this same class. See [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/adr/defer-feature-check-loading-to-persistence-solution.md|Defer {Feature}Check's Load to the persistence-introducing solution]].

# Adr
- [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/adr/use-abstract-validator-for-soft-value-objects.md|Use AbstractValidator for Soft{ValueObject} validators]]
  - Selected variant: `AbstractValidator<Soft{ValueObject}>`, not `PropertyValidator<T,TProperty>` — the latter cannot be resolved generically as `IValidator<Soft{ValueObject}>` by another module through DI
- [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/adr/dto-validators-only-for-request-dtos.md|DTO validators only for RequestDto by default]]
  - Selected variant: validators created by default only for RequestDto; ResponseDto validators only when explicitly required
- [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/adr/defer-feature-check-loading-to-persistence-solution.md|Defer {Feature}Check's Load to the persistence-introducing solution]]
  - Selected variant: `{Feature}Check.cs.create.md` leaves `Load` unimplemented; `solution-repository-integration` supplies the concrete body via a `{Feature}Check.cs.extend.md` targeting the same class

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj]] - hosts every validator this solution creates
- [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/solution-soft-value-objects.skill.md|solution-soft-value-objects]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-soft-value-objects.skill/Implementation/{Module}.Interfaces.csproj.extend/Soft{ValueObject}.cs.create.md|Soft{ValueObject}.cs]] - the type every `{ValueObject}PropertyValidator` validates
- [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-validation-behavior.skill/Implementation/BuildingBlocks.csproj.extend/ValidationBehavior.cs.create.md|ValidationBehavior.cs]] - runs every validator this solution creates before the handler

NUGET:
- `FluentValidation` {version} - `AbstractValidator<T>`, `IValidator<T>`, `CustomAsync` (version in `Directory.Packages.props`)

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj]] - extend - Add `/Validators/Property`, `/Validators/Model`, `/Validators/Async` folders, ensure `AddValidatorsFromAssembly` is called
  - [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md|{ValueObject}PropertyValidator.cs]] - create - `AbstractValidator<Soft{ValueObject}>` with its own local condition
  - [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md|{Dto}.Validator.cs]] - create - `AbstractValidator<{Dto}>` composing property validators and local cross-field conditions
  - [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md|{Feature}Check.cs]] - create - DI-injected async wrapper that preloads data and checks a locally-owned cross-aggregate condition

# Workflow

## Validate a value-concept property (happy path)

```mermaid
sequenceDiagram
    autonumber
    participant Owner as Owning module
    participant PV as {ValueObject}PropertyValidator
    participant Consumer as Consuming module

    Owner->>PV: RuleFor(x => x).Must(IsValid) — condition owned locally
    Consumer->>PV: resolves IValidator<Soft{ValueObject}> from DI
    Consumer->>Consumer: RuleFor(x => x.Field).SetValidator(propertyValidator)
```

## Cross-aggregate async check

1. A repository-bound `{Feature}Check` class loads the values a cross-aggregate condition needs.
2. It checks the condition locally and adds a failure into `ValidationContext` via `context.AddFailure(...)` when it fails.
3. The Command validator references it with `RuleFor(x => x).CustomAsync(check.CheckAsync)`.
4. `ValidationBehavior` runs this before the Handler — the client sees the rejection without the Handler ever running; the Handler's own preload and Entity method call (`solution-domain-behaviour`) remain the authoritative backstop.

# Rules

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend.md#MUST|{Module}.Application.csproj]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{ValueObject}PropertyValidator.cs.create.md#MUST|{ValueObject}PropertyValidator.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Dto}.Validator.cs.create.md#MUST|{Dto}.Validator.cs]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md#MUST|{Feature}Check.cs]]

- Never validate a value-concept property inline in a `{Dto}Validator` — compose `SetValidator(IValidator<Soft{ValueObject}>)` instead.
  - Risk: the same condition is then written in two places (the property validator and every DTO validator that touches it) and they drift.
  - Fix: `{ValueObject}PropertyValidator` owns the condition; every `{Dto}Validator` composes it via `SetValidator`.
- Never let another module reach these validators by referencing this module's `Application` types.
  - Risk: a compile-time reference into `{Module}.Application` breaks the bounded-context boundary.
  - Fix: cross-module code resolves `IValidator<Soft{ValueObject}>` / `IValidator<{Dto}>` from DI.

## SHOULD
- [[skills/dotnet/architecture/v3.1/solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend/{Feature}Check.cs.create.md#SHOULD|{Feature}Check.cs]]

# Check list
- [ ] Every `{ValueObject}PropertyValidator` extends `AbstractValidator<Soft{ValueObject}>` and owns a fully local condition
- [ ] Every `{Dto}Validator` composes `SetValidator(IValidator<Soft{ValueObject}>)` for each value-concept property, never validates a value-concept property inline
- [ ] Every validator is registered via `AddValidatorsFromAssembly`
- [ ] ResponseDto has a validator only when an explicit requirement exists
- [ ] Every `{Feature}Check` loads data and checks its condition in the same class, no comparison duplicated elsewhere
- [ ] `{Feature}Check.cs.create.md`'s `Load` does not reference a concrete data-loading abstraction — that is left to the persistence-introducing solution's `.extend.md`
- [ ] Other modules resolve validators through `IValidator<T>`, never by referencing this module's `Application` types
