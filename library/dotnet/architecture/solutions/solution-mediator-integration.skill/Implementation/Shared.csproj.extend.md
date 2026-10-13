---
description: Add the MediatR package and the ICommand / IQuery / INotificationEvent markers to Shared
name: Shared.csproj
element_kind: project
change_kind: extend
tags:
  - solution/mediator-integration
  - element/shared-csproj
---

# Goals
- Make `ICommand`, `IQuery`, and `INotificationEvent` available to every layer without coupling to `BuildingBlocks`.
- Bring the `MediatR` package into `Shared`.

# Core Principles
- `Shared` holds only markers and contracts — no implementations.
- All three markers pass straight through to MediatR's `IRequest<T>` / `INotification`; a command declares its `Result<T>` explicitly as the type argument.

# Implementation changes

**AS IS** — the state after `solution-sln-structure`: `Shared` is an almost-empty leaf project referencing only `Ardalis.Result` (for the `Result` type). There is no `/MediatR` folder and no `MediatR` package.

**TO BE** — this solution adds:
```
/Shared
  /MediatR
    ICommand.cs            — ICommand : IRequest<Result>, ICommand<TResponse> : IRequest<TResponse>
    IQuery.cs              — IQuery<TResponse> : IRequest<TResponse>
    INotificationEvent.cs  — INotificationEvent : INotification
  Shared.csproj            — + <PackageReference Include="MediatR" />
```
No project references are added (`Shared` stays a leaf).

# Structure

## Project Structure
```
/Shared
  /MediatR
    ICommand.cs
    IQuery.cs
    INotificationEvent.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /MediatR | The three request-kind marker interfaces |

# NuGet Packages
| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| MediatR | central | `IRequest<T>` / `INotification` the markers extend |

`Ardalis.Result` is already referenced by `Shared` (from `solution-sln-structure`). Versions live in `Directory.Packages.props`.

# Allowed Dependencies
- No project references (`Shared` stays a leaf).
- NuGet: `MediatR`, `Ardalis.Result`.

# Rules

## MUST
- Reference `MediatR` in `Shared.csproj`, versionless.
  - Risk: without it the marker interfaces do not compile.
  - Fix: `<PackageReference Include="MediatR" />` + a `<PackageVersion>` in `Directory.Packages.props`.
- Place all three markers in `/Shared/MediatR`; `namespace Shared.MediatR`.
  - Risk: markers scattered elsewhere in `Shared` are hard to find and break the folder/namespace convention every solution assumes.
  - Fix: one folder, one file per marker.
- Never add FluentValidation, EF Core, or any implementation code to `Shared`.
  - Risk: `Shared` is referenced by every layer — a framework or implementation here is inherited everywhere and cannot be swapped.
  - Fix: markers and contracts only.
- Never define a marker in `BuildingBlocks`.
  - Risk: a module would have to reference the technical-pattern layer to declare a request.
  - Fix: markers live in `Shared`.

# Check list
- [ ] `MediatR` referenced in `Shared.csproj` (versionless).
- [ ] `/Shared/MediatR/{ICommand,IQuery,INotificationEvent}.cs` all exist, `namespace Shared.MediatR`.
- [ ] `ICommand : IRequest<Result>`, `ICommand<TResponse> : IRequest<TResponse>`, `IQuery<TResponse> : IRequest<TResponse>`, `INotificationEvent : INotification`.
- [ ] No FluentValidation/EF Core package and no implementation in `Shared`.
