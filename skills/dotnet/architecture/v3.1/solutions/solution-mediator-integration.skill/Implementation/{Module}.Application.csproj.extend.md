---
description: Add feature folder layout, handlers, validators, and module DI self-registration
name: "{Module}.Application.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/command-integration
  - element/module-application-csproj
---

# Goals
- Own all CommandHandler implementations, per-command validators, and the module's DI registration
- Structure each feature as a vertical slice — handler and validator co-located in one folder
- Self-register all handlers and validators via assembly scan

# Core Principles
- One feature folder per write operation under `/Features`
- Handler file named `{FeatureName}.Handler.cs`, class named `{FeatureName}Handler`
- Validator file named `{FeatureName}.Validator.cs`, class named `{FeatureName}Validator`
- Each module exposes one `Register{ModuleName}Module()` extension method
- Pipeline behaviors are NOT registered here — that is App.Host's responsibility

# Implementation changes

**AS IS** (from `plateau-stateless-non-interactive-service`, via `solution-sln-structure`):
```
/{Module}.Application
  /Handlers
  /Validators
  /Specifications
  {Module}.Application.csproj
```
Allowed Dependencies: `{Module}.Interfaces`, `{Module}.Domain`, `{OtherModule}.Interfaces` (other modules), `Shared`. No NuGet packages.

**TO BE** (after this solution):
```
/{Module}.Application
  /Features
    /{FeatureName}
      {FeatureName}.Handler.cs
      {FeatureName}.Validator.cs
  /Specifications
  {Module}ApplicationRegistration.cs
  {Module}.Application.csproj
```
`/Handlers` from the plateau is superseded by the per-feature `/Features/{FeatureName}` layout — a handler no longer lives in a flat `/Handlers` folder. `/Specifications` is unchanged and still referenced by handlers for named-spec loading. `/Validators` is untouched here; the per-command `{FeatureName}.Validator.cs` this solution places under `/Features/{FeatureName}` is a different, separate file from anything `solution-dto-property-validators` places under `/Validators/Property|Model|Async` — the former composes the latter via `SetValidator`. Allowed Dependencies: unchanged. NuGet: adds `MediatR`, `Ardalis.Result`, `FluentValidation`, `FluentValidation.DependencyInjectionExtensions`.

# Structure

## Project Structure
```
/{Module}.Application
  /Features
    /{FeatureName}
      {FeatureName}.Handler.cs
      {FeatureName}.Validator.cs
  {Module}ApplicationRegistration.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Features/{FeatureName} | One subfolder per feature — handler and validator co-located |
| {FeatureName}.Handler.cs | Command handler implementation |
| {FeatureName}.Validator.cs | Transport correctness validator |
| {Module}ApplicationRegistration.cs | Module DI self-registration extension |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Provides `IRequestHandler<TRequest, TResponse>` |
| `Ardalis.Result` | latest stable | Provides `Result<T>` return type |
| `FluentValidation` | latest stable | Provides `AbstractValidator<T>` |
| `FluentValidation.DependencyInjectionExtensions` | latest stable | Provides `AddValidatorsFromAssembly` |

# Allowed Dependencies
- Shared
- `{Module}.Interfaces`
- `{Module}.Domain`
- `{OtherModule}.Interfaces` for cross-module dispatch

# Rules

## MUST
- Each feature in its own subfolder under `/Features`
- Handler file named `{FeatureName}.Handler.cs`
- Handler class named `{FeatureName}Handler`
- Validator file named `{FeatureName}.Validator.cs`
- Validator class named `{FeatureName}Validator`
- Module exposes `Register{ModuleName}Module(IServiceCollection, IConfiguration)` extension method
- Handlers registered via `AddMediatR` assembly scan
- Validators registered via `AddValidatorsFromAssembly`

## MUST NOT
- Pipeline behaviors registered inside module registration
- Handler contain business logic — delegate to domain entities and services
- Handler call `SaveChangesAsync`
- Handler reference `DbContext` directly — use `IRepository<T>` from Shared
- Validator inject repositories or services — purely declarative
- Validator contain business rules

# Anti-patterns
- `CreateTaskCommandHandler.cs` as file name — use `CreateTask.Handler.cs`
- Manual handler registration: `services.AddTransient<CreateTaskHandler>()` — use assembly scan
- Business rule in handler or validator
- Validator placed outside its feature folder

# Check list
- [ ] `/Features/{FeatureName}` folder exists for each command
- [ ] Handler file named `{FeatureName}.Handler.cs`
- [ ] Validator file named `{FeatureName}.Validator.cs`
- [ ] `{Module}ApplicationRegistration.cs` exists
- [ ] Handlers registered via `AddMediatR` scan
- [ ] Validators registered via `AddValidatorsFromAssembly` scan
