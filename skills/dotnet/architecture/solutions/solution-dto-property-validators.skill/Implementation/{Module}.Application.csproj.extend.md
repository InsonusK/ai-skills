---
description: Add Validators/Property, Validators/Model, Validators/Async folders to {Module}.Application and ensure assembly-scan registration
project_name: "{Module}.Application"
name: "{Module}.Application.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/dto-property-validators
  - element/module-application-csproj
---

# Goals
- Give every module a consistent place for property validators, DTO validators, and async cross-aggregate check wrappers

# Implementation changes

**AS IS** (the state after `solution-sln-structure`):
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
  /Handlers
  /Validators
    /Property
      {ValueObject}PropertyValidator.cs
    /Model
      {Dto}.Validator.cs
    /Async
      {Feature}Check.cs
  /Specifications
  {Module}.Application.csproj
```
Allowed Dependencies: unchanged. NuGet: adds `FluentValidation`, with `AddValidatorsFromAssembly` called for this assembly.

`/Validators/Property` and `/Validators/Model` hold the reusable, cross-module-resolvable field/DTO validators this solution owns — distinct from the per-command validator a later `solution-mediator-integration` places under `/Features/{FeatureName}`, which composes these via `SetValidator` rather than duplicating them.

# Rule changes

## MUST
- Add `/Validators/Property`, `/Validators/Model`, `/Validators/Async` folders to `{Module}.Application`
- Call `AddValidatorsFromAssembly` for `{Module}.Application`'s own assembly
- Never register a validator manually instead of via assembly scan

