---
name: vp-row-version-in-dotnet
description: The row version variation point in dotnet — an entity that changes has a uint Version configured IsRowVersion(), which Npgsql maps to xmin and EF Core compares in UpdateAsync, DeleteAsync, and UpdateRangeAsync and reads back, an entity that never changes is an INeverChanges of Shared.Persistence, the handler reads the entity untracked, saves it under the version the caller read, and DbUpdateConcurrencyException is the concurrent change the store failures middleware answers, and every item the API answers carries its id and its version as text
whenToUse: when a dotnet application with vp-postgres-in-dotnet has a table whose rows change after they are created, when you add a command that changes such a row, or when you review how a dotnet handler saves a change
updated: 20261112
tags:
  - stack/dotnet
  - framework/ef-core
  - concern/architecture
  - app-type/service
---

# Goal
- **The row version variation point in dotnet** - A dotnet application that keeps every property of [vp-row-version](library/architecture/spec/vp-row-version.skill/vp-row-version.skill.md); this skill details its implementation for dotnet and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only what [attach.allow](./attach.allow) names, which the repository's attach check verifies against [vp-postgres-in-dotnet](library/architecture/dotnet/vp-postgres-in-dotnet.skill/vp-postgres-in-dotnet.skill.md)'s example: no file of the store's mechanism (`Postgres`, `Postgres.Testing`, the middleware) changes.

# Core Principle
- **EF Core compares it** - A `uint Version` configured `IsRowVersion()` is PostgreSQL's `xmin` for Npgsql; EF Core puts it into the `WHERE` of every update and delete of the entity, reads the new one back, and throws `DbUpdateConcurrencyException`, which the store failures middleware already answers as a concurrent change.
- **The caller's version is the one saved** - A handler reads the entity untracked, applies its rule, and saves it under the version the caller sent — `UpdateAsync` attaches it, so the caller's version is the original EF Core compares — never under the one just read.

```
src/
├── Shared.Persistence/
│   └── INeverChanges.cs                     the mark of an entity with no row version
├── Modules/Todo/Todo.Domain/
│   ├── Entities/Item.cs                     uint Version, ChangeUnder(version)
│   └── Configurations/ItemConfiguration.cs  Version: IsRowVersion() (xmin, a system column)
├── Infrastructure/Postgres/Postgres/
│   └── Migrations/                          AddItemRowVersion: no DDL in the SQL, the snapshot knows the row version
├── Infrastructure/Postgres/Postgres.Tests/
│   ├── features/row-version.feature         the copy of the shared scenarios, on a table of notes
│   └── Steps/RowVersionSteps.cs             their steps, scoped to the feature
└── Modules/Todo/Todo.Application/
    ├── Specifications/ItemsOfList.cs        ItemOfListById: read untracked
    └── Commands/CompleteItemHandler.cs      the sample
```

# Rule

## MUST

### Configure the row version on the entity
Give every entity of a table whose rows change a `uint Version` property configured `IsRowVersion()` in its `IEntityTypeConfiguration`, generate the migration with `make migration`, and give every other entity the interface `INeverChanges` of [INeverChanges.cs](./examples/src/Shared.Persistence/INeverChanges.cs).

```csharp
public uint Version { get; private set; }   // Item: the engine moves it, no code sets it

item.Property(entity => entity.Version).IsRowVersion();   // ItemConfiguration: xmin, a system column
public sealed class Event : INeverChanges { ... }
```

- Violation: a `byte[]` or `long` version, which Npgsql maps to a column of the application's own; `IsConcurrencyToken()` on a column the code sets; the migration of the row version left out, so the model snapshot differs; an entity with neither mark.
- Risk: a column the code moves is blind to a raw `UPDATE`; a pending model change fails `model-changes-in-dotnet.feature`.
- Fix: the scenario "Every row of the application is marked" of the shared copy fails while an entity of `AppDbContext` has neither a row version nor `INeverChanges`.

### Save under the caller's version
Read the entity with a specification that asks `AsNoTracking()`, call the entity's method, set the version the caller sent, and save it with the repository's `UpdateAsync`; the concurrent change arrives as `DbUpdateConcurrencyException`, which the store failures middleware answers — the handler writes for its success alone.

```csharp
var item = await items.FirstOrDefaultAsync(new ItemOfListById(command.ListId, command.ItemId), cancellationToken)
    ?? throw TodoErrors.NoItem(command.ListId, command.ItemId);   // read untracked
item.Complete();                                                  // the entity's rule
item.ChangeUnder(command.Version);                                // the version the caller read
await items.UpdateAsync(item, cancellationToken);                 // attaches: the caller's version is the original compared; the new version is read back
```

- Violation: a tracked read saved under the version it just read, so the check covers the command's own milliseconds only; `item.Version` left at the value just read; a retry of the save after the concurrent change.
- Risk: a save under the version just read overwrites the change the caller never saw, which is what the check prevents.
- Fix: the untracked entity attaches with the caller's version as the original (`ItemOfListById` says why); `ChangeUnder` parses the text, and a version that is no number becomes 0, which matches no row and is a concurrent change too.

### Refuse a change with no version
Throw the module's `Invalid` error (`TodoErrors.NoVersion()`) for a command that changes an existing row and carries an empty version, before the store is asked.
- Risk: a caller that forgot the field turns the check into a comparison with 0 and is told of a concurrent change it did not make.
- Fix: `CompleteItemHandler` checks it first; `complete-item-in-dotnet.feature` proves the items are not read then.

### Carry the version as text through every answer
Give every item the API answers its id and its version, `string version` in the proto contract, from `AddItem`, `ListItems`, and the change's own answer.
- Violation: the contract typed by the row's `uint`; `AddItem` answering the title alone, so the caller cannot make its first change.
- Risk: the caller has nothing to give back, or the contract breaks when the engine changes.
- Fix: `ItemView` carries them from the handler to the entry point (`Id` and `Version` are null only for a planned item, which nothing keeps); `Message` of each entry point maps them.

### Run the shared scenarios
Name both `conformance/` folders in [conformance.copy](./conformance.copy): the example is a full copy of the PostgreSQL one, so it runs those scenarios too.
- Risk: a change of EF Core or of the base repository breaks the version check, and only these scenarios show it.
- Fix: `make conformance-sync` writes the copies; `complete-item-in-dotnet.feature` of `App.Host.Tests` proves the whole application answers 409 for a completion under a version another connection moved.

# Check list
- [ ] Every entity of `AppDbContext` has a `uint` row version configured `IsRowVersion()`, or implements `INeverChanges`; the migration of each row version is generated.
- [ ] No file of the store's mechanism (`Postgres`, `Postgres.Testing`, `Middleware`) was changed.
- [ ] A change reads the entity untracked and saves it under the caller's version; `DbUpdateConcurrencyException` is the concurrent change; no handler retries it.
- [ ] A change with an empty version is `Invalid` before the store is asked.
- [ ] Every item the API answers carries its id and its version as text.
- [ ] `dotnet test` passes with `TEST_DATABASE_DSN` set; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports no copy changed.

# Example
A completion that another caller beat, with the example's real types:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the PostgreSQL example with the row version attached: `CompleteItem` over gRPC and HTTP, proven in `src/Modules/Todo/Todo.Application.Tests/Commands/features/complete-item-in-dotnet.feature` and `src/App/App.Host.Tests/features/complete-item-in-dotnet.feature`.
