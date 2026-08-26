---
description: EF Core migrations only — separated from App.Infrastructure to keep the main infrastructure project clean
name: App.Infrastructure.Migrations.csproj
element_kind: project
change_kind: create
tags:
  - solution/sln-structure
  - element/app-infrastructure-migrations-csproj
---

# Goals
- Keep EF Core migrations in a separate project from App.Infrastructure

# Core Principles
- Migrations are the only code that directly references DbContext for schema changes
- This project is referenced only by App.Host at startup

# Structure

## Project Structure
```
/App.Infrastructure.Migrations
  App.Infrastructure.Migrations.csproj
```

## Directory and class skills
| `Directory\|file` | Description |
| ----------------- | ----------- |

# NuGet Packages
| Package | Version constraint | Purpose |
| --- | --- | --- |

# What Does NOT Belong Here
- Business logic — belongs to Domain
- Repository implementations — belong to App.Infrastructure
- Query handlers — belong to App.Queries or module Application

# Allowed Dependencies
- App.Infrastructure
- {ModuleName}.Domain (all modules)

# Rules

## MUST
- Contain only EF Core migrations

## MUST NOT
- Contain business logic
- Be referenced by any module project

# Check list
- [ ] Only migration classes present
- [ ] No business logic in any file
