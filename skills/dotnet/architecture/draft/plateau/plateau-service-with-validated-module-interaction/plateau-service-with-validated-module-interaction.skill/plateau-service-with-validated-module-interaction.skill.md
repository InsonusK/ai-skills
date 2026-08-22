---
name: service-with-validated-module-interaction
description: Composes the foundation plateau with Value Objects, entity behavior, a request-validation pipeline, cross-module DTO/VO validators, and the full command dispatch chain — a module now has real business logic and is callable through a validated MediatR command, still with no persistence and no HTTP API surface.
whenToUse: when scaffolding a module that needs real domain invariants and a validated write path before any persistence or API exists — or when reviewing whether Value Objects, entity behavior, the validation pipeline, DTO/VO validators, or command wiring follow this baseline
domain: skill
type: template
version: 20260822140000
tags:
  - skill/template/plateau
  - plateau/service-with-validated-module-interaction
parent_plateaus:
  - "[[skills/dotnet/architecture/draft/plateau/plateau-stateless-non-interactive-service/plateau-stateless-non-interactive-service.skill.md|plateau-stateless-non-interactive-service]]"
standalone: false
created_by:
  - "[[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]]"
  - "[[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]]"
  - "[[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]]"
  - "[[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
  - "[[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
---

# Goal
Give a module real business logic and a validated, callable write path, still with no persistence and no HTTP API:
- Eliminate primitive obsession via Value Objects — a permissive `Soft{ValueObject}` in `{Module}.Interfaces`, a strict, self-validating `{ValueObject}` in `{Module}.Domain`
- Give entities behavior methods that validate before mutating, with bulky logic extracted to static domain services
- Give every MediatR request a validation pipeline gate, and every `Soft{ValueObject}`/RequestDto a reusable, cross-module-resolvable FluentValidation validator
- Give every module a full command chain: `ICommand`, immutable command records, a handler/validator per feature, and DI self-registration wired into the composition root

# Core Principles
- Inherited from [[skills/dotnet/architecture/draft/plateau/plateau-stateless-non-interactive-service/plateau-stateless-non-interactive-service.skill.md|plateau-stateless-non-interactive-service]]: fixed four-project module shape, centralized pipeline/module registration in App.Host, global unhandled-exception handling, and the `make unit-test`/`mutation-test`/`test-report`/`test-and-report` conformance gate — this plateau does not change any of that.
- Value semantics: a property carrying invariant state or business meaning is a Value Object, not a primitive. `Soft{ValueObject}` (Interfaces, validation-agnostic) and `{ValueObject}` (Domain, self-validating, inherits from the Soft base) are two strengths of the same type, never duplicated shapes. See [[../../../solutions/solution-value-objects.skill/solution-value-objects.skill.md|solution-value-objects]].
- Entity behavior: every state-changing method validates via a condition it owns before mutating, and throws `DomainException` on failure; bulky or multi-step logic moves to a static domain service extension that still mutates only through the entity's own guarded methods. See [[../../../solutions/solution-domain-behaviour.skill/solution-domain-behaviour.skill.md|solution-domain-behaviour]].
- Validation pipeline: `ValidationBehavior`, registered right after `ExceptionHandlingBehavior` in `AddPipeline()`, intercepts every `IRequest<TResponse>`, collects all validator errors, and short-circuits with `Result.Invalid` before any handler runs. See [[../../../solutions/solution-validation-behavior.skill/solution-validation-behavior.skill.md|solution-validation-behavior]].
- Cross-module validators: `{ValueObject}PropertyValidator`/`{Dto}Validator` are the *only* way another module checks a `Soft{ValueObject}`/DTO it received — resolved through `IValidator<T>` via DI, never by referencing this module's `Application`/`Domain` types. A condition needing preloaded data is a `{Feature}Check`, the only place a repository call is allowed in this validation layer. See [[../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]].
- Command chain: `ICommand<TResponse>` (Shared) → immutable command record (`{Module}.Interfaces/Commands`) → `{FeatureName}Handler`/`{FeatureName}Validator` co-located under `{Module}.Application/Features/{FeatureName}` → `Register{ModuleName}Module()` wired into `ModuleRegistration.AddModules()`. A command validator composes `{ValueObject}PropertyValidator`/`{Dto}Validator` via `SetValidator` for cross-module properties — it never re-declares their rules. See [[../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]].
- Persistence is explicitly out of scope: a handler that touches a persisted entity injects `IRepository<T>`/`IReadRepository<T>` only once a persistence-introducing plateau (composing `solution-repository-integration`) is layered on top — a command with no persisted state (pure domain computation, an external call, orchestration) is a complete, valid application of this plateau on its own.
- Not standalone: `standalone: false` — no HTTP API surface exists yet (see `plateau-service-with-api`), so nothing external can reach a command except a test or another module's mediator call.

# Capabilities
- domain
  - Value Objects at both strengths (`Soft{ValueObject}`, `{ValueObject}`), structurally equal, immutable, self-validating.
  - Entities with guarded behavior methods and, when needed, static domain services for bulky logic — invalid state is unreachable.
- validation
  - Every MediatR request (command or query) is validated by `ValidationBehavior` before its handler runs, with the full error set returned as `Result.Invalid`.
  - Every `Soft{ValueObject}`/RequestDto has a reusable validator any module can resolve via `IValidator<T>`, without depending on this module's `Application`/`Domain` internals.
  - A cross-aggregate condition needing preloaded data runs before the handler too, via a `{Feature}Check` wired through `CustomAsync`.
- commands
  - A full, uniform command chain per module: marker interface, immutable record, co-located handler/validator, module self-registration — callable via `_mediator.Send()` from a test, another module, or (once composed with an API-publishing plateau) an HTTP endpoint.
  - Cross-module writes always go through `_mediator.Send()`, never a direct method call.
- testing
  - Every new class from this plateau (Value Objects, entity behavior, validators, handlers) is provable the same way the foundation plateau already requires — a `.feature` scenario plus step definitions in the matching `*.Tests` project, aggregated into the same `make unit-test`/`mutation-test`/`test-report` gate.

# Usecases

## Add a value-object-shaped field to an existing entity
1. Declare `Soft{ValueObject}` in `{Module}.Interfaces/ValueObjects` — a plain record, no validation.
2. Declare `{ValueObject} : Soft{ValueObject}` in `{Module}.Domain/ValueObjects`, validating via its own local predicate.
3. Retype the entity property from a primitive to `{ValueObject}`; move any inline `if`-check out of the entity into the Value Object's constructor.
4. DTOs and other modules reference `Soft{ValueObject}`, never `{ValueObject}` directly.

## Add a new command end-to-end
1. Declare the command record (and its result record) in `{Module}.Interfaces/Commands`, implementing `ICommand<T>` (or `ICommand` when no payload is returned).
2. Create `{FeatureName}.Handler.cs`/`{FeatureName}.Validator.cs` under `{Module}.Application/Features/{FeatureName}`. The validator covers transport correctness only, composing `IValidator<Soft{ValueObject}>`/`IValidator<{Dto}>` via `SetValidator` for any cross-module property.
3. The handler guards, calls into the domain (an entity method or a domain service), and returns a typed `Result<T>` — no persisted entity exists yet in this plateau, so there is no load/stage step until a persistence-introducing plateau is composed on top.
4. `{Module}ApplicationRegistration.cs` picks up the handler and validator automatically via assembly scan — nothing to wire by hand beyond the module's own `Register{ModuleName}Module()` call already present in `ModuleRegistration.AddModules()`.
5. Prove the command's rule as a Gherkin scenario in the matching `*.Tests` project (`{Module}.Application.Tests` for the handler's orchestration, `{Module}.Domain.Tests` for the entity's own invariant).

## Reject an invalid request before the handler runs
1. A caller sends a command with a malformed `Soft{ValueObject}` field or a missing required property.
2. `ValidationBehavior` runs every registered `IValidator<TRequest>` — including the command's own `{FeatureName}Validator` and, through `SetValidator`, the cross-module `{ValueObject}PropertyValidator`/`{Dto}Validator` it composes — collects every error, and returns `Result.Invalid(errors)`.
3. The handler never runs. This exact contract — full error collection, no partial validation — is itself provable as a Gherkin scenario, the same way `ExceptionHandlingBehavior`'s contract is proven in the foundation plateau.

## Cross-module validation without a project reference
1. Module B needs to validate a `Soft{ValueObject}` or DTO it received from Module A, without referencing Module A's `Application` or `Domain` projects.
2. Module B resolves `IValidator<Soft{ValueObject}>`/`IValidator<{Dto}>` from DI — registered by Module A's own `AddValidatorsFromAssembly` call — and calls `.Validate(...)` or composes it via `SetValidator` in its own validator.
3. No direct project reference between the two modules' `Application`/`Domain` projects is ever added.

# Example
A runnable example lives in [`./example`](./example). It is built on top of the [`plateau-stateless-non-interactive-service`](../../plateau-stateless-non-interactive-service/plateau-stateless-non-interactive-service.skill.md) example and extends it with:
- `ValidationBehavior` registered after `ExceptionHandlingBehavior`;
- `FluentValidation` validators for commands and `Soft{ValueObject}` types;
- `SoftEmail`/`Email` Value Objects and a `TaskItem` entity with guarded behavior;
- a `CreateTaskCommand` with co-located handler, validator, and DI self-registration;
- Reqnroll scenarios proving the validation pipeline and entity behavior.

See `example/README.md` for how to run it and execute the conformance-testing gate.
