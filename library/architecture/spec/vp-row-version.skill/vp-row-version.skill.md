---
name: vp-row-version
description: The row version variation point — a table whose rows change declares a version the database engine keeps, every read and every save answers it as an opaque text, the caller gives it back with a write, and the base repository's one write that finds another version reports a changed row, so no edit is lost silently, with no lock and no read before the write, attached to vp-postgres table by table
whenToUse: when rows of a table change after they are created, when two users or two processes may change one row, when you decide whether a table needs a version, or when you review how an edit reaches the database
updated: 20261112
tags:
  - stack
  - concern/architecture
  - app-type/service
adr:
  - adr/version-kept-by-the-engine.md
  - adr/one-check-in-the-write.md
  - adr/version-compared-by-the-base-repository.md
---

# Goal
- **No edit lost silently** - Of two writers of one row, the first is kept and the second is told, so neither overwrites the other unseen.
- **Protected from the read a person saw** - The check covers the time from the read a caller was shown to its save, also when that is minutes, not only the moment inside one command.
- **Only tables that change** - A table whose rows never change carries no version.
- **Every stack behaves the same** - The scenarios of this skill's `conformance/` folder pass in every stack.

# Core Principle
- **The engine keeps the version** - The database moves it on every write, by whatever code, so no writer can forget it.
- **One check, in the write** - The write itself compares the version, so nothing can change between a check and the save.
- **A version is an opaque value** - It is compared for equality and carried; nothing computes with it or reads a time out of it, and every contract carries it as text.
- **The table declares, the base compares** - A row that changes declares its version; the repository of [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md) compares a version a row declares, so a table that never changes is written as it always was. Decision recorded in [version-compared-by-the-base-repository](./adr/version-compared-by-the-base-repository.md).

@import "./diagrams/concept.mmd" {as="mermaid"}

# Scope
- Covers one row of one table written by one call. A table takes this variation point when [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md#classify-every-table) says so; `vp-external-id` and `vp-edit-timestamp` attach beside it in any number.
- A worker claiming rows under a lock is step 4 of the [catalog](library/architecture/README.md). A write that must check the versions of several rows the caller saw together is left to the project: the variation point checks each row it writes.
- The implementation for each stack is described in its stack-specialized extension: `vp-row-version-in-dotnet`.

# Rule

## MUST

### Mark every table
Mark every row of the application as one that changes, by declaring its version, or one that never does, and let a check fail on a row with no mark.
- Violation: a new table with no mark; a version on a table of events; a table marked as unchanging with a service that updates it.
- Risk: a table nobody decided about keeps losing edits with no failure.
- Fix: a row that changes declares the version; a check over the mapped rows fails while one has neither mark.

### Let the engine keep the version
Take the version from what the engine moves on every write, never from the application or from a clock the application reads. Decision recorded in [version-kept-by-the-engine](./adr/version-kept-by-the-engine.md).
- Violation: `updated_at` set by the application as the version; a counter the code increments.
- Risk: a write by another path leaves the version as it was, and the next save overwrites that write with no failure.
- Fix: `xmin` in PostgreSQL. A time a reader needs is `vp-edit-timestamp`, and it decides nothing here.

### Answer the version with every read and every save
Answer the version with the row, through the service and the API, to every caller that may send a change back: on a read, and the new version on the answer of a create and of an update.
- Violation: a read that answers the fields with no version; a version dropped between the service and the API; a create that answers only the id, so the caller cannot make its first edit; a version typed as a number in the contract.
- Risk: the caller has nothing to give back, so the check covers milliseconds inside one command, and two people who opened one form overwrite each other; a contract that types the version as the engine's number breaks when the engine changes.
- Fix: the API's contract carries it as text: `string version` of the message in a proto contract, `ETag` on a hand-written HTTP API; the repository puts the new version on the row it saved.

### Require the version on a write
Refuse with the kind `Invalid` a change of an existing row that comes with no version.
- Violation: a missing version read as "skip the check".
- Risk: a caller that forgot the field turns the check off and nobody notices.
- Fix: the field is required in the contract; a new row needs none.

### Check in the write, not before it
Give the caller's version to the repository's write and let that one statement compare it, with no read to compare first. Decision recorded in [one-check-in-the-write](./adr/one-check-in-the-write.md).
- Violation: a middleware that reads the current version and compares it before the handler; a service that reads the row to compare.
- Risk: a write between the read and the save passes the check, and each write costs one more query.
- Fix: the service puts the caller's version on the row and calls `Update` or `Delete`.

### Answer a changed row as a concurrent change
Turn a changed row into the kind `ConcurrentChange` in the service, so the caller reads again and decides.
- Violation: a service that retries a person's edit by itself; a conflict answered as an internal failure.
- Risk: a retry with no new read writes over the other edit, which is what the check prevents.
- Fix: only a change that does not depend on what it read may be retried by the service; `Update(change)` of a selector fails as a whole, so its service may read again and repeat it.

### Pass the shared scenarios
Run this skill's [conformance/](./conformance/) scenarios in every stack, unedited, through a copy named in the stack skill's `conformance.copy`, as [conformance-scenarios-copied-into-each-stack](library/architecture/spec/vp-logging.skill/adr/conformance-scenarios-copied-into-each-stack.md) decides.
- Risk: a stack answers a different contract under the same names.
- Fix: step definitions are the stack's own; the scenarios run on a table `notes` of the test project, which the application never ships.

### Declare the version, never write its check
Attach the variation point by declaring the version on each row that changes and carrying it through its services and API, and change no fixed file of [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md), whose repository compares a version a row declares.
- Violation: a repository of a table that compares the version itself; a plugin or a subclass of the base repository for versioned rows; a version declared on every row by a base class.
- Risk: a second check drifts from the base's and misses a path (a selector's `Update`); a version on every row promises a check the tables that never change do not need.
- Fix: the stack extension lists the files it changes; a check compares its example with the base against that list.

# Check list
- [ ] Every row is marked; a row that changes declares its version; a check fails on a row with no mark.
- [ ] The version comes from the engine; no code sets it.
- [ ] Every read, create, and update answers the version as text; a change without one is `Invalid`.
- [ ] The version is compared only in the write, by the base repository; no file of vp-postgres changed.
- [ ] A changed row is a `ConcurrentChange`; no service retries a person's edit.
- [ ] The stack skill's `conformance.copy` names this skill's `conformance/` folder, and every copy is unedited.
