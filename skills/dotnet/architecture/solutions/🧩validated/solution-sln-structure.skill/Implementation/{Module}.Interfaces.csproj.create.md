---
description: Provide the single stable public surface through which other modules interact with this module
name: "{Module}.Interfaces.csproj"
element_kind: project
change_kind: create
---
# Goals
- Provide the single stable public surface through which other modules interact with this module
- Declare all write intent contracts (commands), read intent contracts (queries), response shapes (DTOs), and integration event contracts

# Core Principles
- Interfaces is a declarations-only project — no business logic, no implementation

# Structure

## Project Structure
```
/{ModuleName}.Interfaces
  /Commands
  /Queries
  /DTOs
  /Events
  {ModuleName}.Interfaces.csproj
```

## Directory and class skills
| `Directory\|file` | Description                 |
| ----------------- | --------------------------- |
| /Commands         | Write intent contracts      |
| /Queries          | Read intent contracts       |
| /DTOs             | Response shapes             |
| /Events           | Integration event contracts |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Business logic — belongs to Domain
- Implementation — belongs to Application
- Infrastructure concerns — belongs to App.Infrastructure

# Allowed Dependencies
- Shared

# Rules

## MUST
- Interfaces contains only declarations — records, interfaces, DTOs
- All commands declared here
- All queries declared here
- All integration events declared here

## MUST NOT
- Interfaces reference Domain, Application, or any infrastructure project
- Interfaces contain any implementation code

# Anti-patterns
- Placing command handlers in Interfaces — handlers belong in Application
- Placing domain entities in Interfaces — use DTOs for cross-module data shapes
- Referencing another module's Domain from Interfaces

# Check list
- [ ] /Commands folder exists
- [ ] /Queries folder exists
- [ ] /DTOs folder exists
- [ ] /Events folder exists
- [ ] Interfaces.csproj references only Shared
- [ ] No implementation code in any file
