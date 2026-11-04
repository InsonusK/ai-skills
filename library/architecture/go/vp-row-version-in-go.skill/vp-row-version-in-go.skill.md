---
name: vp-row-version-in-go
description: The row version variation point in Go — a row that changes declares postgres.Version on xmin (read-only, no migration), the base repository of vp-postgres-in-go compares it in Update, Delete, and a selector's Update and puts the new one on the row, a row that never changes is a tables.NeverChanges, the service saves under the version the caller read and turns ErrRowChanged into ErrConcurrentChange, and every item the API answers carries its id and its version as text
whenToUse: when a Go application with vp-postgres-in-go has a table whose rows change after they are created, when you add a command that changes such a row, or when you review how a Go service saves a change
updated: 20261104
tags:
  - stack/go
  - concern/architecture
  - app-type/service
---

# Goal
- **The row version variation point in Go** - A Go application that keeps every property of [vp-row-version](library/architecture/spec/vp-row-version.skill/vp-row-version.skill.md); this skill details its implementation for Go and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only what [attach.allow](./attach.allow) names, which the repository's attach check verifies against [vp-postgres-in-go](library/architecture/go/vp-postgres-in-go.skill/vp-postgres-in-go.skill.md)'s example: no file of `internal/infrastructure/postgres` changes.

# Core Principle
- **One field declares it** - `Version postgres.Version` on the `xmin` column makes the base repository compare the version on every write of the row; nothing else of the infrastructure changes.
- **The caller's version is the one saved** - A command reads the row to apply its rule, then saves under the version the caller sent, never under the one it just read.

```
internal/
├── infrastructure/postgres/tables/
│   ├── items.go                      ItemRow declares Version; ItemOfList picks one item
│   └── marks.go                      NeverChanges: the mark of a row that declares no version
├── infrastructure/postgres/rowversion/
│   └── features/  test/              the copy of the shared scenarios, on a table of notes; the steps
├── domain/entity/kept_item.go        KeptItem: an item with its id and the version a caller read
├── service/item_lists.go             Add, Items, Item answer versions; Save under the item's version
└── command/complete_item.go          the sample: read, Complete, save under the caller's version
```

# Rule

## MUST

### Declare the version on the row
Give every row of a table whose rows change the field `Version postgres.Version` with the tag `gorm:"column:xmin;->;-:migration"`, and every other row the method `NeverChanges()` of [marks.go](./examples/internal/infrastructure/postgres/tables/marks.go), copied into the application's `tables` package.

```go
type ItemRow struct {
	ID      int64            `gorm:"primaryKey"`
	ListID  string           `gorm:"not null;uniqueIndex:list_items_list_id_title_key"`
	Title   string           `gorm:"not null;uniqueIndex:list_items_list_id_title_key"`
	Done    bool             `gorm:"not null"`
	Version postgres.Version `gorm:"column:xmin;->;-:migration"`
}

func (EventRow) NeverChanges() {}
```

- Violation: the field without `->`, so GORM writes `xmin`; without `-:migration`, so the scenario of the migrations makes a column; a `version` column in a migration; a row with neither mark.
- Risk: PostgreSQL refuses a write of a system column; a column of the application's own is moved by nothing but the code, and a raw `UPDATE` slips past it.
- Fix: the scenario "Every row of the application is marked" of the shared copy fails while a row of `tables.Rows` has neither mark.

### Save under the caller's version
Read the row through the service to apply the entity's rule, set the version the caller sent on the item, and save it with the repository's `Update`; turn `postgres.ErrRowChanged` into `errorkind.ErrConcurrentChange` with the store's failure as the cause.

```go
item, err := h.lists.Item(ctx, cmd.ListID, cmd.ItemID)   // the item as it is now
if err := item.Complete(); err != nil { ... }             // the entity's rule
item.Version = cmd.Version                                // the version the caller read
saved, err := h.lists.Save(ctx, cmd.ListID, item)         // Update(&ItemRow{..., Version}); the new version on saved
```

- Violation: `item.Version` left at the value just read; a service that compares versions itself before `Update`; a retry of `Save` after `ErrConcurrentChange`.
- Risk: a save under the version just read overwrites the change the caller never saw, which is what the check prevents.
- Fix: the command scenario "The caller's version is saved, not the one read just now" fails when the read version is saved.

### Refuse a change with no version
Refuse with `errorkind.ErrInvalid` a command that changes a row and carries an empty version, before the service is asked.
- Violation: an empty version passed on, so the repository refuses it with `ErrRefused` as a defect; a version made up by the handler.
- Risk: a caller that forgot the field gets an internal error instead of being told what it missed.
- Fix: `errNoVersion` of `complete_item.go`.

### Carry the version as text through every answer
Give every item the API answers its id and its version, `string version` in the proto contract, from `AddItem`, `ListItems`, and the change's own answer.
- Violation: a version typed `uint32`; `AddItem` answering the title alone.
- Risk: the caller cannot make its first change, or the contract breaks when the engine changes.
- Fix: `entity.KeptItem` carries them from the service to the command; `itemMessage` of each entry point maps them.

### Run the shared scenarios
Name both `conformance/` folders in [conformance.copy](./conformance.copy): the example is a full copy of the PostgreSQL one, so it runs those scenarios too.
- Risk: a change of the base repository breaks the version check, and only these scenarios show it.
- Fix: `make conformance-sync` writes the copies; the steps of `row-version.feature` are in `internal/infrastructure/postgres/rowversion/test/steps_test.go`.

# Check list
- [ ] Every row of `tables.Rows` declares `Version postgres.Version` with `column:xmin;->;-:migration`, or has `NeverChanges()`.
- [ ] No file of `internal/infrastructure/postgres` was changed; no migration names a version.
- [ ] A change saves under the caller's version; `ErrRowChanged` becomes `ErrConcurrentChange` with the cause; no service retries it.
- [ ] A change with an empty version is `ErrInvalid` before the service is asked.
- [ ] Every item the API answers carries its id and its version as text.
- [ ] `make test` passes with `TEST_DATABASE_DSN` set; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports no copy changed.

# Example
A completion that another caller beat, with the example's real types:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the PostgreSQL example with the row version attached: `CompleteItem` over gRPC and HTTP, proven in `internal/command/features/complete_item.feature` and `internal/service/features/item_lists.feature`.
