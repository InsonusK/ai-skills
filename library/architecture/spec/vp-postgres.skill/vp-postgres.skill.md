---
name: vp-postgres
description: The PostgreSQL variation point — one database behind stores written in the technique's terms, a service over each store that turns its outcomes (duplicate key, no rows, unavailable) into error kinds, DATABASE_DSN that no log prints, migrations applied by a one-shot job or behind MIGRATE_ON_START, a readiness check, and scenarios on the real engine at TEST_DATABASE_DSN, each on a database of its own
whenToUse: when an application built on the base plateau must keep its data between runs in PostgreSQL, when you add a table, a store, or a migration, when you replace an in-memory store with a database, or when you review a store
updated: 20261031
tags:
  - stack
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/test-database-from-the-project.md
---

# Goal
- **Data outlives the process** - What a command stored is there after a restart and for every replica.
- **Commands do not change** - The database replaces an in-memory store behind the service: no command, entity, or domain service changes, as [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md) promises for infrastructure.
- **The schema travels with the code** - Every change of a table is a migration in the repository, applied before the code that needs it starts.
- **Failures are told apart** - A taken key is a conflict, a database that does not answer is unavailable, anything else is a defect of the service.

# Core Principle
- **A store saves at once** - Each store method finishes its write before it returns, so the service sees the database's outcome and turns it into a kind. A command that must keep several writes together takes the unit of work variation point (`vp-unit-of-work`), which puts a transaction around them with no change to a store.
- **Rows below, entities above** - A store speaks rows and statements; the service over it builds entities from rows, so the entity's rules guard what is read too.
- **One application, one database** - One schema, one migration history; another service reaches this data through this service's API, never its tables.

@import "./diagrams/concept.mmd" {as="mermaid"}

# Scope
- Covers one PostgreSQL database of one application: stores, migrations, settings, readiness, and how their scenarios get a database. A transaction across several writes is `vp-unit-of-work`; SQLite is `vp-sqlite`.
- The implementation for each stack is described in its stack-specialized extension: `vp-postgres-in-dotnet`.

# Rule

## MUST

### Reach the database only through stores
Put every statement in a store of the infrastructure layer, behind an interface in the technique's terms, with one service over each store per [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md).
- Violation: a command running a query; a service holding the driver's connection; a store method named `AddItemToList` that takes an entity.
- Risk: the technique spreads upward, so changing a query or the engine edits commands.
- Fix: the store has `Insert(listId, row)`, the service `Add(listId, item)`.

### Report a store's outcomes in the store's terms
Make a store report the outcomes a service must tell apart as outcomes of its own: a taken unique key, no rows, a database that does not answer; let the service turn each into its kind and let every other failure pass with no kind.
- Violation: a service reading a PostgreSQL error code; a service catching every failure of the store as conflict.
- Risk: the engine's codes leak above the store, or a broken statement is answered as the caller's mistake.
- Fix: duplicate key → conflict, no rows → not found, unavailable → unavailable; a broken statement stays `INTERNAL` and shows in the log.

### Configure the database by DATABASE_DSN
Read the database from `DATABASE_DSN` (required, the driver's own form) and refuse a missing or unusable value at start, naming the variable and never repeating its value.
- Violation: a settings object that prints the DSN; a failure that quotes the value.
- Risk: the password reaches the log, and from there everyone who reads it.
- Fix: a settings type that does not print its DSN; a failure that names only the variable.

### Apply migrations in one mode
Keep every schema change as a migration in the repository, and apply them as [devops-service-deploy](library/devops/core/devops-service-deploy.skill/devops-service-deploy.skill.md) requires: by a one-shot job, `/app/migrate` of the image, that exits 0 on a database already up to date; or at start behind `MIGRATE_ON_START` (default `false`) for a deployment pinned to one instance, a failure stopping the start.
- Violation: the application migrating on every start with several replicas; a table changed by hand.
- Risk: replicas race for the migration; a schema changed outside the repository is missing in the next environment.
- Fix: the job by default; a scenario for the job's exit codes.

### Bring the readiness check
Give the health variation point a check that the database answers a query, so readiness holds traffic back during an outage.
- Risk: a service that cannot reach its database keeps receiving traffic and answers every call unavailable.
- Fix: pass the check in the composition root per [vp-health](library/architecture/spec/vp-health.skill/vp-health.skill.md); liveness stays free of it.

### Test stores on the real engine
Run the scenarios of stores and migrations on a real PostgreSQL the project provides, by `TEST_DATABASE_DSN` or a server running in its dev container, each scenario on a database it creates and drops, and let a scenario fail, never skip, when none is there. Decision recorded in [test-database-from-the-project](./adr/test-database-from-the-project.md).
- Violation: stores tested on an in-memory provider or SQLite; a scenario skipped without the DSN; scenarios sharing rows.
- Risk: a unique index, a transaction, or an error code behaves differently on another engine; a skipped scenario reads as a pass in CI; scenarios depend on their order.
- Fix: the helper of the example creates the database; the DSN needs the right to create databases.

### Attach without editing commands
Attach the variation point by changing no command, entity, or domain service; only the composition root, the service over the store, and the build files.
- Risk: a store that reaches into commands cannot be replaced by another store, or removed.
- Fix: the stack extension lists the files it changes; a check compares its example with the base against that list.

# Check list
- [ ] Every statement is in a store; a service is the only caller of a store, and it builds entities from rows.
- [ ] A taken key, no rows, and an unavailable database are outcomes of the store, turned into kinds by the service.
- [ ] `DATABASE_DSN` is required; no failure and no log repeats its value.
- [ ] Migrations run in exactly one mode; the job exits 0 on an up-to-date database.
- [ ] The readiness check of the database is passed to the health variation point when the service has one.
- [ ] Store scenarios run on a real PostgreSQL, each on its own database, and fail without one.
