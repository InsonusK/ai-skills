---
description: Add command record conventions and MediatR/Ardalis.Result packages to Interfaces
name: "{Module}.Interfaces.csproj"
change_kind: extend
---

# Goals
- Own all Command record declarations and their associated result records for this module
- Be the only project other modules depend on when dispatching commands to this module

# Core Principles
- Commands are declarations only — records with properties, no methods, no logic
- Result records are declared alongside their command in the same file
- Both Command and Result are `record` types — immutable by design

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

MUST:
- All commands for this module declared in `/{Module}.Interfaces/Commands`
- Each command file contains the command record and its result record
- Commands implement `ICommand<Result<T>>` from Shared

MUST NOT:
- Commands contain any logic or methods
- Commands reference Domain entity types — input properties are primitives or shared value types only
- Interfaces project reference Domain, Application, or infrastructure projects

# Anti-patterns
- Declaring command handlers or validators in Interfaces
- Referencing another module's Domain from Interfaces

# Check list
- [ ] `/Commands` folder exists
- [ ] Each command file contains command and result records
- [ ] `MediatR` package referenced
- [ ] `Ardalis.Result` package referenced
- [ ] Interfaces references only Shared
