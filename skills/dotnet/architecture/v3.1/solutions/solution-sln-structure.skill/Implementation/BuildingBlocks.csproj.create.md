---
description: Implement reusable framework-level patterns consumed by App.Host and infrastructure across all modules
name: BuildingBlocks.csproj
element_kind: project
change_kind: create
tags:
  - solution/sln-structure
  - element/buildingblocks-csproj
---

# Goals
- Implement application technical patterns used by App.Host and infrastructure across all modules
- Provide pipeline behaviors, repository implementations, and cross-cutting utilities

# Core Principles
- BuildingBlocks depends only on Shared
- BuildingBlocks does NOT define common interfaces — it consumes interfaces from Shared
- All pipeline behavior implementations live here — registered once in App.Host, used by all modules

# Structure

## Project Structure
```
/BuildingBlocks
  /MediatR
    UnitOfWorkContext.cs
    UnitOfWorkBehavior.cs
    ValidationBehavior.cs
  /Outbox
    OutboxMessage.cs
    OutboxDispatcher.cs
  /Concurrency
    ETagEncoder.cs
    EntityVersionResolver.cs
  BuildingBlocks.csproj
```

## Directory and class skills
| `Directory\|file` | Description                                    |
| ----------------- | ---------------------------------------------- |
| /MediatR          | Pipeline behavior implementations and context  |
| /Outbox           | OutboxMessage model and dispatcher             |
| /Concurrency      | ETag encoder and entity version resolver       |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Business logic — belongs to Domain
- Module-specific handlers or validators — belong to module Application
- Common interface definitions — belong to Shared

# Allowed Dependencies
- Shared

# Rules

## MUST
- All pipeline behavior implementations defined here
- BuildingBlocks depends only on Shared
- BuildingBlocks does not define common interfaces — only implements patterns using interfaces from Shared
- Never buildingBlocks reference any module project
- Never buildingBlocks reference App.Infrastructure or App.Queries
- Never buildingBlocks contain business logic
- Never buildingBlocks define common interfaces — only Shared defines interfaces

## SHOULD
- Avoid placing domain entities in BuildingBlocks — they belong in module Domain
- Avoid adding module-specific handlers or validators in BuildingBlocks
- Avoid defining common interfaces in BuildingBlocks — they belong in Shared

# Check list
- [ ] BuildingBlocks.csproj references only Shared
- [ ] BuildingBlocks.csproj contains only pattern implementations
- [ ] No common interface definitions in BuildingBlocks
