---
description: Add /Validators folder for Soft{ValueObject} property validators and DTO validators, and ensure FluentValidation scans them
name: "{Module}.Application.csproj"
element_kind: project
change_kind: extend
---

# Goals
- Own all property validators for `Soft{ValueObject}` types
- Own all validators for public DTOs declared in `{Module}.Interfaces`
- Register validators through the existing FluentValidation assembly scan

# Core Principles
- Validators are implementations and belong in `{Module}.Application`
- Validators must be registered by `AddValidatorsFromAssembly` for the `{Module}.Application` assembly
- Validators validate values only by calling Rules
- Other modules consume validators through the generic `IValidator<T>` abstraction

# Structure

## Project Structure
```
/{Module}.Application
  /Validators
    {ValueObject}PropertyValidator.cs
    {Dto}Validator.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Validators | Property validators for Soft VOs and validators for public DTOs |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `FluentValidation` | latest stable | Provides `AbstractValidator<T>` |
| `FluentValidation.DependencyInjectionExtensions` | latest stable | Provides `AddValidatorsFromAssembly` |

# What Does NOT Belong Here
- `Soft{ValueObject}` declarations — belong in `{Module}.Interfaces`
- Domain invariant enforcement — belongs in `{Module}.Domain`
- Business rules — belong in `{Module}.Domain`

# Allowed Dependencies
- Shared
- `{Module}.Interfaces`
- `{Module}.Domain`
- `{OtherModule}.Interfaces` for cross-module dispatch
- `FluentValidation`

# Rules
MUST:
- Add `/Validators` folder containing `{ValueObject}PropertyValidator.cs` and `{Dto}Validator.cs`
- Reference `FluentValidation` packages
- Ensure `AddValidatorsFromAssembly(typeof({Module}.Application.AssemblyMarker).Assembly)` is called in `{Module}.Application` registration. If another solution (for example `solution-command-integration.skill`) already registers validators from this assembly, that registration satisfies this requirement.

MUST NOT:
- Put validators in `{Module}.Interfaces`
- Inject repositories, `DbContext`, or services into property validators
- Add business rules to validators
- Add inline FluentValidation predicates that duplicate Rule logic

# Anti-patterns
- Referencing another module's `{Module}.Application` to instantiate a concrete validator
- Forgetting to register validators from the `{Module}.Application` assembly

# Check list
- [ ] `/Validators` folder exists
- [ ] `FluentValidation` packages referenced
- [ ] Validators are registered by assembly scan
