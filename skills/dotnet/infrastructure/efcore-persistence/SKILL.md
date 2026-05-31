---
name: efcore-persistence
description: Defines EF Core persistence conventions.
metadata:
  domain: dotnet
  tags:
    - dotnet
    - infrastructure
    - efcore-persistence
---
## DbContext

Each module may own its own DbContext.

Use explicit schema names per module.

## Entity Configuration

Use IEntityTypeConfiguration<T>.

Avoid large OnModelCreating methods.

## Rules

* Prefer explicit configuration
* Use indexes intentionally
* Configure delete behaviors explicitly
* Avoid lazy loading

## Migrations

Migrations must remain deterministic.

Never manually edit generated migrations unless required.
