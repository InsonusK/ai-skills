---
name: entity-configuration-lives-in-domain
description: Why EF Core IEntityTypeConfiguration<T> classes live in {Module}.Domain/Configurations, adding a Microsoft.EntityFrameworkCore reference to a project VP1 otherwise keeps infrastructure-free
problem: solution-domain-behaviour (VP1) establishes {Module}.Domain as referencing only Shared + {Module}.Interfaces, with no EF Core. solution-domain-configuration (VP2) needs one IEntityTypeConfiguration<T> per entity somewhere. Placing it in {Module}.Domain contradicts that rule; placing it outside Domain splits an entity's mapping away from the entity.
decision: Put the configuration classes in {Module}.Domain/Configurations and let {Module}.Domain take a package reference to Microsoft.EntityFrameworkCore for the IEntityTypeConfiguration<T> / EntityTypeBuilder<T> abstractions only — no provider, no DbContext. solution-domain-behaviour's "no EF Core" rule is scoped to allow this one abstractions-only reference. Cross-module foreign-key configs still live in App.Infrastructure.
tags:
  - solution/domain-configuration
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem

`solution-domain-behaviour` (VP1) creates `{Module}.Domain.csproj` with a deliberately narrow dependency set — `Shared` + `{Module}.Interfaces`, and explicitly **no EF Core** (persistence is VP2, a later and separately-gated concern). The domain project holds entities and value objects expressed in plain C#, with zero persistence attributes.

`solution-domain-configuration` (VP2) introduces the EF Core mapping layer: one `IEntityTypeConfiguration<T>` per entity, owning table names, indexes, constraint-name constants, relations, concurrency-token mapping, and `OwnsOne` value-object mappings. That configuration has to live in some project. Two forces pull against each other:

- An entity and its mapping change together (add a property → map its column; add an invariant that needs a unique index → name and configure that index). Keeping them in the same project and folder tree keeps that edit local and reviewable as one unit.
- `{Module}.Domain` was defined as infrastructure-free. `IEntityTypeConfiguration<T>` and `EntityTypeBuilder<T>` come from `Microsoft.EntityFrameworkCore`, so hosting the config there means the domain project references an ORM package.

# Selected variant

**Selected variant:** [[#Configuration classes in {Module}.Domain/Configurations, abstractions-only EF Core reference (selected)]]

`{Module}.Domain` gains a `/Configurations` folder with one `{Entity}Config.cs` per entity and a package reference to `Microsoft.EntityFrameworkCore` — used **only** for `IEntityTypeConfiguration<T>`, `EntityTypeBuilder<T>`, and `ApplyConfigurationsFromAssembly`. No provider package (`.Sqlite`, `.SqlServer`, in-memory), no `DbContext`, no repository lives in `Domain`. `solution-domain-behaviour`'s "no EF Core" rule is narrowed by the Wave-1+2 audit (S6) to "`solution-domain-configuration` may add an `IEntityTypeConfiguration`-only reference." Cross-module foreign-key configuration — which by definition spans two bounded contexts — stays in `App.Infrastructure/Persistence/Configurations`.

# Searched variants

## Configuration classes in {Module}.Domain/Configurations, abstractions-only EF Core reference (selected)

### Description
One `{Entity}Config.cs` per entity under `{Module}.Domain/Configurations`. `{Module}.Domain` references `Microsoft.EntityFrameworkCore` for the configuration abstractions only. The `DbContext` (in `App.Infrastructure`) calls `ApplyConfigurationsFromAssembly` over each module's `Domain` assembly to discover them.

### Benefits
- An entity and its persistence mapping live in the same project and are edited, reviewed, and versioned as one unit — the most common change (a new property and its column) touches one folder tree.
- The domain entity classes themselves stay attribute-free — all mapping is fluent and lives in the config class, so the "zero EF attributes on entities" rule is enforceable by the Cecil architecture test.
- `Microsoft.EntityFrameworkCore` is the abstractions-carrying package; without a provider it pulls in no database engine, so `{Module}.Domain` still cannot open a connection, run a migration, or resolve a `DbContext`.
- Assembly-scan registration means adding an entity + its config needs no edit to the `DbContext`.

### Costs
- `{Module}.Domain` is no longer literally dependency-free — it carries one ORM abstraction package, and `solution-domain-behaviour`'s rule needs an explicit carve-out (S6) rather than reading as an absolute.
- A careless developer could add `EntityTypeBuilder` extension methods that need a provider (`ToTable`, `HasDatabaseName`) and only discover the missing `Microsoft.EntityFrameworkCore.Relational` reference at build time in a downstream project — a papercut the plateau examples hit and document.

## Configuration classes in App.Infrastructure

### Description
Put every `IEntityTypeConfiguration<T>` in `App.Infrastructure/Persistence/Configurations`, alongside the `DbContext` and the cross-module FK configs. `{Module}.Domain` keeps its original `Shared` + `{Module}.Interfaces` dependency set unchanged.

### Benefits
- `{Module}.Domain` stays exactly as `solution-domain-behaviour` defined it — no EF Core reference, no rule carve-out.
- All persistence concerns (context, configs, migrations) sit in one project.

### Costs
- An entity's shape and its mapping are split across two projects — every property addition is a two-project edit, and a reviewer sees the entity change without the mapping change unless they open both.
- `App.Infrastructure` references every module's `Domain` and accumulates every module's mapping — it becomes a cross-context junk drawer, and the module boundary stops being visible in the persistence layer.
- Module-internal mapping (this module's own indexes and relations) is no longer encapsulated by the module.

## A dedicated {Module}.Infrastructure project per module

### Description
Add a third module project, `{Module}.Infrastructure`, between `{Module}.Domain` and `App.Infrastructure`, to hold that module's `IEntityTypeConfiguration<T>` classes (and later its repository implementations).

### Benefits
- `{Module}.Domain` stays EF-free and each module's mapping is still encapsulated by the module.
- Clean layering: domain, then module infrastructure, then host infrastructure.

### Costs
- Triples the per-module project count for the persistence step (Domain + Infrastructure + the existing Interfaces/Application), against this catalog's explicit "base module set reduced from 4 projects to 2" direction — every module pays the project, the `.csproj`, and the solution-folder entry whether or not it needs the seam.
- Still splits the entity from its mapping across projects, just with an extra project in between — it does not buy back the locality that the selected variant has.
- The repository implementations that would justify a `{Module}.Infrastructure` are, in this architecture, generic (`Repository<T>` in `App.Infrastructure`) — so the project would hold only configs, which is what `{Module}.Domain/Configurations` already does with one fewer project.
