---
name: vp-row-version-in-dotnet
description: The row version variation point in dotnet — a row that changes has a uint Version configured IsRowVersion(), which Npgsql maps to xmin and EF Core compares in UpdateAsync, DeleteAsync, and a selector's UpdateAsync and reads back, a row that never changes is an INeverChanges, the service saves under the version the caller read and turns RowChangedException into a concurrent change, and every item the API answers carries its id and its version as text
whenToUse: when a dotnet application with vp-postgres-in-dotnet has a table whose rows change after they are created, when you add a command that changes such a row, or when you review how a dotnet service saves a change
updated: 20261104
tags:
  - stack/dotnet
  - framework/ef-core
  - concern/architecture
  - app-type/service
---

# Goal
- **The row version variation point in dotnet** - A dotnet application that keeps every property of [vp-row-version](library/architecture/spec/vp-row-version.skill/vp-row-version.skill.md); this skill details its implementation for dotnet and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only what [attach.allow](./attach.allow) names, which the repository's attach check verifies against [vp-postgres-in-dotnet](library/architecture/dotnet/vp-postgres-in-dotnet.skill/vp-postgres-in-dotnet.skill.md)'s example: no file of `Postgres/Repositories` changes.

# Core Principle
- **EF Core compares it** - A `uint Version` configured `IsRowVersion()` is PostgreSQL's `xmin` for Npgsql; EF Core puts it into the `WHERE` of every update and delete of the row, reads the new one back, and throws `DbUpdateConcurrencyException`, which the base repository already turns into `RowChangedException`.
- **The caller's version is the one saved** - A command reads the row to apply its rule, then saves under the version the caller sent, never under the one it just read.

```
src/
├── Infrastructure/Postgres/Postgres.Abstractions/Rows/
│   ├── ItemRow.cs                     uint Version
│   └── INeverChanges.cs               the mark of a row with no row version
├── Infrastructure/Postgres/Postgres/
│   ├── Rows/ItemRowConfiguration.cs   Version: IsRowVersion()
│   └── Migrations/                    AddItemRowVersion: no DDL, the snapshot knows the row version
├── Infrastructure/Postgres/Postgres.Tests/
│   ├── features/row-version.feature   the copy of the shared scenarios, on a table of notes
│   └── Steps/RowVersionSteps.cs       their steps, scoped to the feature
├── Modules/Todo/Todo.Domain/Entities/KeptItem.cs   an item with its id and the version a caller read
├── Modules/Todo/Todo.Application/Services/         ItemAsync, SaveAsync under the item's version
└── Modules/Todo/Todo.Application/Commands/CompleteItemHandler.cs   the sample
```

# Rule

## MUST

### Configure the row version on the row
Give every row of a table whose rows change a `uint Version` property configured `IsRowVersion()`, generate the migration with `make migration`, and give every other row the interface `INeverChanges` of [INeverChanges.cs](./examples/src/Infrastructure/Postgres/Postgres.Abstractions/Rows/INeverChanges.cs), copied into `Postgres.Abstractions/Rows`.

```csharp
public sealed class ItemRow
{
    public long Id { get; set; }
    public required string ListId { get; set; }
    public required string Title { get; set; }
    public bool Done { get; set; }
    public uint Version { get; set; }
}

row.Property(item => item.Version).IsRowVersion();   // ItemRowConfiguration: xmin, a system column
public sealed class EventRow : INeverChanges { ... }
```

- Violation: a `byte[]` or `long` version, which Npgsql maps to a column of the application's own; `IsConcurrencyToken()` on a column the code sets; the migration of the row version left out, so the model snapshot differs; a row with neither mark.
- Risk: a column the code moves is blind to a raw `UPDATE`; a pending model change fails `ModelChanges.feature`.
- Fix: the scenario "Every row of the application is marked" of the shared copy fails while an entity of `AppDbContext` has neither a row version nor `INeverChanges`.

### Save under the caller's version
Read the row through the service to apply the entity's rule, set the version the caller sent on the kept item, and save it with the repository's `UpdateAsync`; turn `RowChangedException` into a concurrent change of the module's errors, with the exception as its cause.

```csharp
var kept = await lists.ItemAsync(command.ListId, command.ItemId, cancellationToken);   // the item as it is now
kept.Item.Complete();                                                                 // the entity's rule
kept.Version = command.Version;                                                       // the version the caller read
var saved = await lists.SaveAsync(command.ListId, kept, cancellationToken);           // UpdateAsync(new ItemRow { ..., Version }); the new version on saved
```

- Violation: `kept.Version` left at the value just read; a tracked row updated in place, so EF Core compares the version it read; a retry of `SaveAsync` after the concurrent change.
- Risk: a save under the version just read overwrites the change the caller never saw, which is what the check prevents.
- Fix: the service builds a new `ItemRow` with the caller's version; a version that is no number matches no row and is a concurrent change too.

### Refuse a change with no version
Throw the module's `Invalid` error (`TodoErrors.NoVersion()`) for a command that changes a row and carries an empty version, before the service is asked.
- Risk: a caller that forgot the field turns the check into a comparison with 0 and is told of a concurrent change it did not make.
- Fix: `CompleteItemHandler` checks it first; `CompleteItem.feature` proves the lists are not asked.

### Carry the version as text through every answer
Give every item the API answers its id and its version, `string version` in the proto contract, from `AddItem`, `ListItems`, and the change's own answer.
- Violation: the contract typed by the row's `uint`; `AddItem` answering the title alone.
- Risk: the caller cannot make its first change, or the contract breaks when the engine changes.
- Fix: `KeptItem` carries them from the service to the command; `Message` of each entry point maps them.

### Run the shared scenarios
Name both `conformance/` folders in [conformance.copy](./conformance.copy): the example is a full copy of the PostgreSQL one, so it runs those scenarios too.
- Risk: a change of EF Core or of the base repository breaks the version check, and only these scenarios show it.
- Fix: `make conformance-sync` writes the copies; `Persistence.feature` proves the whole application answers 409 for a completion under a version another connection moved.

# Check list
- [ ] Every entity of `AppDbContext` has a `uint` row version configured `IsRowVersion()`, or implements `INeverChanges`; the migration of each row version is generated.
- [ ] No file of `Postgres/Repositories` was changed.
- [ ] A change saves a new row with the caller's version; `RowChangedException` becomes a concurrent change with the cause; no service retries it.
- [ ] A change with an empty version is `Invalid` before the service is asked.
- [ ] Every item the API answers carries its id and its version as text.
- [ ] `dotnet test` passes with `TEST_DATABASE_DSN` set; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports no copy changed.

# Example
A completion that another caller beat, with the example's real types:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the PostgreSQL example with the row version attached: `CompleteItem` over gRPC and HTTP, proven in `src/Modules/Todo/Todo.Application.Tests/features/CompleteItem.feature` and `src/App/App.Host.Tests/features/Persistence.feature`.
