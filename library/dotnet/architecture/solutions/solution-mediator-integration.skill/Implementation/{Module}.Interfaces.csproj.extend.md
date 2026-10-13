---
description: Add command record conventions and MediatR/Ardalis.Result packages to Interfaces
name: "{Module}.Interfaces.csproj"
element_kind: project
change_kind: extend
tags:
  - solution/mediator-integration
  - element/module-interfaces-csproj
---

# Goals
- Own all Command record declarations and their associated result records for this module
- Be the only project other modules depend on when dispatching commands to this module

# Core Principles
- Commands are declarations only — records with properties, no methods, no logic
- Result records are declared alongside their command in the same file
- Both Command and Result are `record` types — immutable by design

# Implementation changes

**AS IS** (the state after `solution-sln-structure`) — `/Commands` already exists as an empty folder:
```
/{Module}.Interfaces
  /Commands
  /Queries
  /DTOs
  /Events
  {Module}.Interfaces.csproj
```
Allowed Dependencies: `Shared`. No NuGet packages.

**TO BE** (after this solution) — `/Commands` is populated:
```
/{Module}.Interfaces
  /Commands
    {Command}.cs
  /Queries
  /DTOs
  /Events
  {Module}.Interfaces.csproj
```
Allowed Dependencies: unchanged. NuGet: adds `MediatR`, `Ardalis.Result`.

# Structure

## Project Structure
```
/{Module}.Interfaces
  /Commands
    {Command}.cs
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| /Commands | Write intent contract declarations for this module |
| {Command}.cs | Command record and its result record |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |
| `MediatR` | latest stable | Required for `ICommand<T>` marker usage |
| `Ardalis.Result` | latest stable | Required for `Result<T>` return type usage |

# Allowed Dependencies
- Shared

# Rules

## MUST
- All commands for this module declared in `/{Module}.Interfaces/Commands`
- Each command file contains the command record and its result record
- Commands implement `ICommand<T>` (or `ICommand` when no payload is returned) from Shared
- Never put logic or methods on a command
- Never reference a `Domain` entity type from a command — input properties are primitives or shared value types only
- Never reference `Domain`, `Application`, or an infrastructure project from `{Module}.Interfaces`

## SHOULD
- Avoid declaring command handlers or validators in Interfaces
- Avoid referencing another module's Domain from Interfaces

# Check list
- [ ] `/Commands` folder exists
- [ ] Each command file contains command and result records
- [ ] `MediatR` package referenced
- [ ] `Ardalis.Result` package referenced
- [ ] Interfaces references only Shared
