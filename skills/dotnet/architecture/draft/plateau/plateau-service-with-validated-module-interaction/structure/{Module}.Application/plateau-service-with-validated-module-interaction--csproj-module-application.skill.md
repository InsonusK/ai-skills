---
name: csproj-module-application
description: Project {Module}.Application in the service-with-validated-module-interaction plateau
whenToUse: when adding or editing a handler, validator, or specification in {Module}.Application, or deciding whether new code belongs here
domain: skill
type: template
plateau: service-with-validated-module-interaction
version: 20260822140000
tags:
  - skill/template/csproj
  - plateau/service-with-validated-module-interaction
created_by:
  - "[[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
  - "[[../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]]"
  - "[[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]]"
---

# Goal
- Orchestrate use cases by connecting the API contract to the domain model
- Give every public RequestDto and value-concept property a FluentValidation validator, resolvable cross-module through `IValidator<T>`
- Own all command handlers, per-command validators, and this module's DI self-registration

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

# Core Principles
- Application coordinates — it never contains business logic
- Application knows its own Domain and its own Interfaces
- Application may reference other modules' Interfaces for cross-module dispatch
- One feature folder per write operation under `/Features` — handler and its transport-correctness validator co-located
- `/Validators/Property` and `/Validators/Model` hold the reusable, cross-module-resolvable field/DTO validators — distinct from the per-command validator under `/Features/{FeatureName}`, which composes them via `SetValidator` rather than duplicating them
- Each module exposes one `Register{ModuleName}Module()` extension method; pipeline behaviors are NOT registered here — that is App.Host's responsibility

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

# Structure

## Solution place
```
/src/Modules/{ModuleName}/{ModuleName}.Application
```

## Project Structure
- /{Module}.Application
  - /Features
    - /{FeatureName}
      - [{FeatureName}.Handler.cs](./classes/plateau-service-with-validated-module-interaction--class-feature-handler.skill.md)
      - [{FeatureName}.Validator.cs](./classes/plateau-service-with-validated-module-interaction--class-feature-validator.skill.md)
  - /Validators
    - /Property
      - [{ValueObject}PropertyValidator.cs](./classes/plateau-service-with-validated-module-interaction--class-property-validator.skill.md)
    - /Model
      - [{Dto}.Validator.cs](./classes/plateau-service-with-validated-module-interaction--class-dto-validator.skill.md)
    - /Async
      - [{Feature}Check.cs](./classes/plateau-service-with-validated-module-interaction--class-feature-check.skill.md)
  - /Specifications
  - [{Module}ApplicationRegistration.cs](./classes/plateau-service-with-validated-module-interaction--class-module-application-registration.skill.md)
  - {Module}.Application.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Features/{FeatureName} | Handler + transport-correctness validator for one command | [[./classes/plateau-service-with-validated-module-interaction--class-feature-handler.skill.md\|class-feature-handler]], [[./classes/plateau-service-with-validated-module-interaction--class-feature-validator.skill.md\|class-feature-validator]] |
| /Validators/Property | Reusable `Soft{ValueObject}` validators, resolvable cross-module | [[./classes/plateau-service-with-validated-module-interaction--class-property-validator.skill.md\|class-property-validator]] |
| /Validators/Model | Reusable RequestDto validators, resolvable cross-module | [[./classes/plateau-service-with-validated-module-interaction--class-dto-validator.skill.md\|class-dto-validator]] |
| /Validators/Async | DI-injected async cross-aggregate checks | [[./classes/plateau-service-with-validated-module-interaction--class-feature-check.skill.md\|class-feature-check]] |
| /Specifications | Query specifications | |
| {Module}ApplicationRegistration.cs | Module DI self-registration (handlers + validators via assembly scan) | [[./classes/plateau-service-with-validated-module-interaction--class-module-application-registration.skill.md\|class-module-application-registration]] |

## NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Provides `IRequestHandler<TRequest, TResponse>` |
| `Ardalis.Result` | latest stable | Provides `Result<T>` return type |
| `FluentValidation` | latest stable | Provides `AbstractValidator<T>` |
| `FluentValidation.DependencyInjectionExtensions` | latest stable | Provides `AddValidatorsFromAssembly` |

## Allowed Dependencies
- {Module}.Interfaces (own module)
- {Module}.Domain (own module)
- {OtherModule}.Interfaces (other modules — contracts only)
- Shared

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

# Rules
MUST:
- Application references only own Interfaces, own Domain, other modules' Interfaces, and Shared
- One feature folder per write operation under `/Features`, handler and validator co-located
- Every `Soft{ValueObject}`/RequestDto has its validator under `/Validators/Property`/`/Validators/Model`, registered via `AddValidatorsFromAssembly`
- A cross-aggregate condition needing preloaded data is a `{Feature}Check` under `/Validators/Async`, never inline in a validator
- Module exposes exactly one `Register{ModuleName}Module()` extension method
MUST NOT:
- Application reference another module's Domain or Application
- Application contain business logic — delegate to Domain
- A `{FeatureName}.Validator.cs` duplicate rules already defined in `{ValueObject}PropertyValidator`/`{Dto}Validator`
- Pipeline behaviors registered inside module registration

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]

# Check list
- [ ] Application.csproj does not reference another module's Domain or Application
- [ ] No business logic in any handler class
- [ ] Every `Soft{ValueObject}`/RequestDto has a validator under `/Validators/Property`/`/Validators/Model`
- [ ] Every cross-aggregate async condition is a `{Feature}Check` under `/Validators/Async`
- [ ] `{Module}ApplicationRegistration.cs` registers handlers and validators via assembly scan

__Applied solutions:__
- [[../../../../solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]] - [[../../../../solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md|{Module}.Application.csproj.create]]
- [[../../../../solutions/solution-dto-property-validators.skill/solution-dto-property-validators.skill.md|solution-dto-property-validators]] - [[../../../../solutions/solution-dto-property-validators.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
- [[../../../../solutions/solution-command-integration.skill/solution-command-integration.skill.md|solution-command-integration]] - [[../../../../solutions/solution-command-integration.skill/Implementation/{Module}.Application.csproj.extend.md|{Module}.Application.csproj.extend]]
