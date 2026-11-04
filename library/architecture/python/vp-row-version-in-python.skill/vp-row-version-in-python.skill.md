---
name: vp-row-version-in-python
description: The row version variation point in Python — a row that changes declares version_column() (xmin, a system column read as text), the base repository of vp-postgres-in-python compares it in update, delete, and a selector's update and puts the new one on the row, a row that never changes derives from tables.NeverChanges, the service saves under the version the caller read and raises a concurrent change from RowChangedError, and every item the API answers carries its id and its version as text
whenToUse: when a Python application with vp-postgres-in-python has a table whose rows change after they are created, when you add a command that changes such a row, or when you review how a Python service saves a change
updated: 20261104
tags:
  - stack/python
  - concern/architecture
  - app-type/service
---

# Goal
- **The row version variation point in Python** - A Python application that keeps every property of [vp-row-version](library/architecture/spec/vp-row-version.skill/vp-row-version.skill.md); this skill details its implementation for Python and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only what [attach.allow](./attach.allow) names, which the repository's attach check verifies against [vp-postgres-in-python](library/architecture/python/vp-postgres-in-python.skill/vp-postgres-in-python.skill.md)'s example: no file of `infrastructure/postgres` but `tables/` changes.

# Core Principle
- **One column declares it** - `version: Mapped[str | None] = version_column()` makes the base repository compare the version on every write of the row; nothing else of the infrastructure changes.
- **The caller's version is the one saved** - A command reads the row to apply its rule, then saves under the version the caller sent, never under the one it just read.

```
src/{package}/
├── infrastructure/postgres/tables/
│   ├── items.py                      ItemRow declares version_column(); ItemOfList picks one item
│   └── marks.py                      NeverChanges: the mark of a row that declares no version
├── infrastructure/postgres/rowversion/
│   └── features/  test/              the copy of the shared scenarios, on a table of notes; the steps
├── domain/entity/kept_item.py        KeptItem: an item with its id and the version a caller read
├── service/item_lists.py             add, items, item answer versions; save under the item's version
└── command/complete_item.py          the sample: read, complete, save under the caller's version
```

# Rule

## MUST

### Declare the version on the row
Give every row of a table whose rows change the column `version: Mapped[str | None] = version_column()`, and make every other row derive from `NeverChanges` of [marks.py](./examples/src/todo/infrastructure/postgres/tables/marks.py), copied into the application's `tables` package.

```python
class ItemRow(Row):
    __tablename__ = "list_items"

    id: Mapped[int] = mapped_column(Identity(), primary_key=True)
    list_id: Mapped[str]
    title: Mapped[str]
    done: Mapped[bool]
    version: Mapped[str | None] = version_column()


class EventRow(Row, NeverChanges): ...
```

- Violation: `mapped_column("xmin")` written by hand, so SQLAlchemy writes it, a migration makes it, and it compares as a number; a `version` column in a migration; a row with neither mark.
- Risk: PostgreSQL refuses a write of a system column; a column of the application's own is moved by nothing but the code, and a raw `UPDATE` slips past it.
- Fix: the scenario "Every row of the application is marked" of the shared copy fails while a mapped row of `Row` has neither mark; `make test` runs `alembic check`, which leaves system columns out.

### Save under the caller's version
Read the row through the service to apply the entity's rule, set the version the caller sent on the kept item, and save it with the repository's `update`; raise the kind `CONCURRENT_CHANGE` from `RowChangedError`.

```python
kept = await self._lists.item(command.list_id, command.item_id)   # the item as it is now
kept.item.complete()                                              # the entity's rule
kept.version = command.version                                    # the version the caller read
saved = await self._lists.save(command.list_id, kept)             # update(ItemRow(..., version)); the new version on saved
```

- Violation: `kept.version` left at the value just read; a service that compares versions itself before `update`; a retry of `save` after the concurrent change.
- Risk: a save under the version just read overwrites the change the caller never saw, which is what the check prevents.
- Fix: the command scenario "The caller's version is saved, not the one read just now" fails when the read version is saved.

### Refuse a change with no version
Raise a `KindError` of `Kind.INVALID` for a command that changes a row and carries an empty version, before the service is asked.
- Violation: an empty version passed on, so the repository refuses it with `RefusedError` as a defect; a version made up by the handler.
- Risk: a caller that forgot the field gets an internal error instead of being told what it missed.
- Fix: `NoVersionError` of `complete_item.py`.

### Carry the version as text through every answer
Give every item the API answers its id and its version, `string version` in the proto contract, from `AddItem`, `ListItems`, and the change's own answer.
- Violation: a version typed `int`; `AddItem` answering the title alone.
- Risk: the caller cannot make its first change, or the contract breaks when the engine changes.
- Fix: `KeptItem` carries them from the service to the command; `item_message` of each entry point maps them.

### Run the shared scenarios
Name both `conformance/` folders in [conformance.copy](./conformance.copy): the example is a full copy of the PostgreSQL one, so it runs those scenarios too.
- Risk: a change of the base repository breaks the version check, and only these scenarios show it.
- Fix: `make conformance-sync` writes the copies; the steps of `row-version.feature` are in `src/todo/infrastructure/postgres/rowversion/test/row_version_steps_test.py`, on a table of notes mapped apart from `Row`.

# Check list
- [ ] Every mapped row of `Row` declares `version_column()` or derives from `NeverChanges`.
- [ ] No fixed file of `infrastructure/postgres` was changed; no migration names a version; `alembic check` passes.
- [ ] A change saves under the caller's version; `RowChangedError` becomes a concurrent change raised from it; no service retries it.
- [ ] A change with an empty version is `Kind.INVALID` before the service is asked.
- [ ] Every item the API answers carries its id and its version as text.
- [ ] `make test` and `make lint` pass with `TEST_DATABASE_DSN` set; `bash test/check-vp-attach.sh` in this repository reports no file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports no copy changed.

# Example
A completion that another caller beat, with the example's real types:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the PostgreSQL example with the row version attached: `CompleteItem` over gRPC and HTTP, proven in `src/todo/command/features/complete_item.feature` and `src/todo/service/features/item_lists.feature`.
