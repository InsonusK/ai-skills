---
name: vp-postgres
description: The PostgreSQL variation point — one database behind a repository in one of two forms (rows under a service with one contract of insert, update, delete, SelectBy and a selector, in Go and Python; the domain's entities mapped and reached by the handler through Ardalis's repository, in dotnet), named specifications, every table classified by who sets its identity and whether its rows change, the outcomes (duplicate key, missing reference, row changed, unavailable) told as error kinds, DATABASE_DSN that no log prints, migrations applied by a one-shot job or behind MIGRATE_ON_START, a readiness check, and shared scenarios on the real engine at TEST_DATABASE_DSN
whenToUse: when an application built on the base plateau must keep its data between runs in PostgreSQL, when you add a table, a specification, a repository method, or a migration, when you replace an in-memory store with a database, or when you review database code
updated: 20261113
tags:
  - stack
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/test-database-from-the-project.md
  - adr/repository-selector-and-specification.md
  - adr/order-and-page-on-the-selector.md
  - adr/a-selector-is-no-snapshot.md
  - adr/two-forms-rows-and-entities.md
---

# Goal
- **Data outlives the process** - What a command stored is there after a restart and for every replica.
- **Commands do not change** - In the rows form the database replaces an in-memory store behind the service: no command, entity, or domain service changes, as [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md) promises for infrastructure. In the entities form a handler is written over the repository's contract, which no store changes; a sample of the base that kept its data in the process takes the repository when the database attaches.
- **One query, one name** - A set of rows is described once, by a specification with a name of the business, and every read, count, update, or delete of that set uses it.
- **The schema travels with the code** - Every change of a table is a migration in the repository, applied before the code that needs it starts.
- **Failures are told apart** - A taken key is a conflict, a row changed meanwhile is a concurrent change, a database that does not answer is unavailable, anything else is a defect of the service.
- **Every stack behaves the same** - The scenarios of this skill's `conformance/` folder pass in every stack, but those of `postgres-selector/` and `postgres-item-lists-service/`, which only the rows form has, pass in every stack of that form.

# Core Principle
- **A repository saves at once** - Each write finishes before it returns, so its caller sees the database's outcome, told as a kind. A command that must keep several writes together takes the unit of work variation point (`vp-unit-of-work`), which puts a transaction around them with no change to a repository.
- **Two forms, one behaviour** - In the rows form a repository speaks rows and the service over it builds entities from them; in the entities form the domain's entities are mapped, one per table, and the handler takes the repository. The outcomes, migrations, settings, and readiness are the same in both. Decision recorded in [two-forms-rows-and-entities](./adr/two-forms-rows-and-entities.md).
- **Which rows, then what to do** - A specification says which rows; in the rows form a selector says what to do with them, and in what order.
- **One application, one database** - One schema, one migration history; another service reaches this data through this service's API, never its tables.

The diagram shows the rows form; in the entities form the command takes the repository itself, and the store failures middleware stands where the service turns the outcomes.

@import "./diagrams/concept.mmd" {as="mermaid"}

# Scope
- Covers one PostgreSQL database of one application: the repository, specifications, selectors, migrations, settings, readiness, and how their scenarios get a database. A transaction across several writes is `vp-unit-of-work`; what a table adds by its kind is `vp-row-version`, `vp-external-id`, and `vp-edit-timestamp`; SQLite is `vp-sqlite`; a worker claiming rows under a lock is step 4 of the [catalog](library/architecture/README.md) (TaskBox, Inbox, Outbox).
- The implementation for each stack is described in its stack-specialized extension: `vp-postgres-in-dotnet`, `vp-postgres-in-go`, `vp-postgres-in-python`.

# Rule

## MUST

### Classify every table
Decide for every table who sets the identity of its rows and whether they change, and attach the variation points the answer asks for.

| Identity set by | Rows change | Key | `vp-row-version` | `vp-external-id` |
|---|---|---|---|---|
| The service | No | `bigint` identity | No | No |
| The caller | No | the caller's UUID v7 | No | Yes |
| The service | Yes | `bigint` identity | Yes | No |
| The caller | Yes | the caller's UUID v7 | Yes | Yes |

- Violation: a table whose rows two users may change at once with no version; a version on a table of events that never change; a choice made per module.
- Risk: with no version both saves succeed and the first edit is lost with no failure; an idle version on a table that never changes promises a check nothing needs.
- Fix: each variation point attaches on its own, in any number, table by table, per [vp-row-version](library/architecture/spec/vp-row-version.skill/vp-row-version.skill.md) and [vp-external-id](library/architecture/spec/vp-external-id.skill/vp-external-id.skill.md); `vp-edit-timestamp` is a separate choice, for a table users create or edit, whatever its row of this table.

### Take the form of the stack
Build the variation point in the form its stack extension names, and apply the rules marked for a form only in that form.

| | Rows form | Entities form |
|---|---|---|
| Stacks | Go, Python | dotnet |
| What is mapped | a row per table, below the service | the domain's entities, one per table, configured in their module |
| Caller of the repository | a service per table, which builds entities from rows | the handler, which loads a root, calls its methods, and saves it |
| Contract | [the one contract](#give-every-repository-the-one-contract) and its selector | the stack's library (dotnet: Ardalis's `IRepositoryBase<T>`) |
| Outcomes become kinds | in the service | in one command middleware |
| Shared scenarios | every folder of `conformance/` | `conformance/` but `postgres-selector/` and `postgres-item-lists-service/` |

- Violation: a stack mixing the forms per module; a service over the repository in the entities form; a handler calling the repository in the rows form.
- Risk: two ways to reach one table in one application, and scenarios that prove neither.
- Fix: the stack extension states its form once.

### Reach the database only through a repository
Put every statement in a repository of the infrastructure layer, behind an interface, with a service as its only caller in the rows form and a handler in the entities form, per [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md).
- Violation: a command running a query; a service holding the driver's connection; a repository method named `AddItemToList` that takes an entity.
- Risk: the technique spreads upward, so changing a query or the engine edits commands.
- Fix: in the rows form the service's `Add(listId, item)` builds a row and calls the repository's `Insert(row)`; in the entities form the handler checks the title, builds the item, and adds it through `IRepository<Item>`.

### Give every repository the one contract
In the rows form, implement this contract for every table, under these names in the stack's spelling. Decision recorded in [repository-selector-and-specification](./adr/repository-selector-and-specification.md).

| Object | Method | Answers |
|---|---|---|
| Repository | `Insert(row)`, `Update(row)`, `Delete(row)` | One row, saved at once; `Update` and `Delete` by its key, and by the version the row declares, if any (the new one put back on the row) |
| Repository | `SelectBy(specification)` | A selector; no statement runs yet |
| Selector | `All()` | A list of every picked row, as row objects; an empty list when none is picked |
| Selector | `Single()` | The one picked row as an object, or none; more than one is a failure with no kind |
| Selector | `Count()`, `Any()` | How many rows are picked, whether any is |
| Selector | `Update(change)` | Applies `change` to every picked row in one save; answers how many rows it changed |
| Selector | `Delete()` | Deletes every picked row; answers how many |
| Selector | `OrderBy(key)`, `OrderByDescending(key)` | A new ordered selector; the first one is unchanged |
| Ordered selector | everything above, and `ThenBy(key)` | A new ordered selector with one more key |
| Ordered selector | `First()` | The first row as an object, or none |
| Ordered selector | `Take(n)` | A list of up to `n` rows, every row when fewer exist; `n` starts at 1 |
| Ordered selector | `Page(page, size)` | A list of the rows of one page of `size`, numbered from 1; an empty list past the end |

- Violation: a table with methods of its own for what the contract gives; `First(n)`; a selector with no ordered form, so `First` runs with no order.
- Risk: every table reads differently, the scenarios of [Pass the shared scenarios](#pass-the-shared-scenarios) cannot run, and a stack drifts from the others.
- Fix: a new table needs a row and its mapping; it gets the contract from one generic repository.

### Pick rows with a named specification
Describe which rows a call wants as a specification class named in the business's terms, holding the conditions, the joins along the relations of the rows, and a projection, with no statement of its own.
- Violation: a specification that opens a connection; a condition inlined in a service; a specification named `TasksQuery2`.
- Risk: the same condition is written twice and the copies drift; a query the service cannot name cannot be reused for a count and an update.
- Fix: `UnclosedTasks(owner)`, handed to `SelectBy` (rows form) or to the library's list, count, or delete (entities form); the repository runs it.

### Order and page only on the selector
In the rows form, refuse a specification that holds an order or a page, and ask for them of the selector; in the entities form, refuse a specification with a page for a count, an existence check, or a delete. Decision recorded in [order-and-page-on-the-selector](./adr/order-and-page-on-the-selector.md).
- Violation: a specification with `Take(20)`, then `Delete()` on its selector; `First` on an unordered selector.
- Risk: a count, an update, or a delete that honours the page and another that ignores it work on different rows; a delete of one page removed every row of the specification in a prototype; `First` with no order answers a row chosen by the engine.
- Fix: `SelectBy` fails on such a specification; `First`, `Take`, and `Page` exist only on the ordered selector, where the language allows that check; `Single` takes the case of a unique key, where no order is needed.

### Stop a selector at the service
In the rows form, keep a selector inside the service that made it, and give it no way to add a condition.
- Violation: a service method returning a selector; `selector.Where(...)`.
- Risk: the technique reaches commands, conditions leave the specifications, and a selector outlives the connection scope it reads through.
- Fix: the service answers entities or values; a new condition is a new specification.

### Take the outcome of a write from the write
In the rows form, treat every call of a selector as a new question to the database; in both forms, take the number of rows a write touched from that write's answer. Decision recorded in [a-selector-is-no-snapshot](./adr/a-selector-is-no-snapshot.md).
- Violation: `Count()`, then `Update(change)`, then a report of the count as the number closed.
- Risk: a row arriving between the calls makes the two numbers differ, also inside a unit of work; after `Update`, a selector over `UnclosedTasks` counts 0.
- Fix: report the answer of `Update` (rows form) or of the library's write (entities form: Ardalis's `UpdateRangeAsync`, `DeleteRangeAsync`); no lock is taken for a read.

### Report a repository's outcomes in its own terms
Tell the outcomes a caller must tell apart, each as its kind, and let every other failure pass with no kind: in the rows form the repository reports outcomes of its own and the service turns them; in the entities form the repository reports nothing of its own and one command middleware turns the store's failures for every command.

| Outcome | Kind the caller is answered |
|---|---|
| Duplicate key | `Conflict` |
| Missing reference: a foreign key broken | `NotFound`, or what the business says |
| Row changed: gone, or changed meanwhile (`vp-row-version`) | `ConcurrentChange` |
| No rows (rows form: `Single`, `First` none, `All`, `Take`, `Page` empty; entities form: no root) | `NotFound`, when the service or the handler needs them |
| Unavailable: no answer, or a failure a retry may cure | `Unavailable` |

- Violation: a service or a handler reading a PostgreSQL error code; a service catching every failure of the repository as conflict; a failed write left in the session, so the next save sends it again.
- Risk: the engine's codes leak above the repository, a broken statement is answered as the caller's mistake, or one handled failure fails every later save of the command.
- Fix: one translation under every write and read, statements past the repository included; a broken statement stays `INTERNAL` and shows in the log; a failed write leaves nothing pending. A deadlock and a serialization failure are unavailable: the retry is the same.

### Go past the contract with a method of its own
Add a method to a table's own interface, which extends the repository's, for a statement the contract cannot express, passing it through the translation of [Report a repository's outcomes in its own terms](#report-a-repositorys-outcomes-in-its-own-terms) and into the transaction of a begun unit of work.
- Violation: a raw statement in a service; a statement that passes neither.
- Risk: outside the unit of work's transaction, such a statement stays after a rollback; a duplicate key in it arrives unrecognised and breaks an open transaction.
- Fix: an upsert, a bulk copy, a mass update of more rows than memory should hold, a read with a join of no relation, a grouping, or a window function; add no shortcut beside `SelectBy` for what a specification expresses; the stack extension names the one call they go through.

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
Bring a named check that the database answers a query, through the contract [vp-health](library/architecture/spec/vp-health.skill/vp-health.skill.md) reads, so readiness holds traffic back during an outage, and keep this variation point building and running without the health one.
- Risk: a service that cannot reach its database keeps receiving traffic and answers every call unavailable; a type of the health entry point in the database code makes one optional variation point require the other.
- Fix: the stack extension names the contract; liveness stays free of the check.

### Test on the real engine
Run the scenarios of the repository and of migrations on a real PostgreSQL the project provides, by `TEST_DATABASE_DSN` or a server running in its dev container, each scenario on a database it creates and drops, and let a scenario fail, never skip, when none is there. Decision recorded in [test-database-from-the-project](./adr/test-database-from-the-project.md).
- Violation: a repository tested on an in-memory provider or SQLite; a scenario skipped without the DSN; scenarios sharing rows.
- Risk: a unique index, a transaction, or an error code behaves differently on another engine; a skipped scenario reads as a pass in CI; scenarios depend on their order.
- Fix: the helper of the example creates the database; the DSN needs the right to create databases.

### Pass the shared scenarios
Run this skill's [conformance/](./conformance/) scenarios in every stack, those of [postgres-selector/](./conformance/postgres-selector/) and [postgres-item-lists-service/](./conformance/postgres-item-lists-service/) only in a stack of the rows form, unedited, through a copy named in the stack skill's `conformance.copy`, as [conformance-scenarios-copied-into-each-stack](library/architecture/spec/vp-logging.skill/adr/conformance-scenarios-copied-into-each-stack.md) decides.
- Violation: a stack rewriting a scenario to fit its code; a stack skill with no `conformance.copy`.
- Risk: a stack answers a different contract under the same names, and a gap found in one stack stays open in the others.
- Fix: step definitions are the stack's own, and one that compares rows reads each field the scenario's table names from the row object, never from a joined text; the scenarios of `repository`, `selector`, `specification`, and `outcomes` run on the tables `owners` and `tasks` of the test project, which the application never ships; what only one stack has goes in files of its own beside the copies.

### Attach without editing commands
Attach the variation point by changing no command, entity, or domain service; only the composition root, the service over the repository (rows form), the build files, and, in the entities form, the base's samples that kept their data in the process.
- Risk: a repository that reaches into commands cannot be replaced by another, or removed.
- Fix: the stack extension lists the files it changes; a check compares its example with the base against that list.

# Check list
- [ ] Every table is classified, and takes the variation points its row of the table asks for.
- [ ] The stack is in one [form](#take-the-form-of-the-stack), for every module.
- [ ] Every statement is in a repository; its caller is a service (rows form) or a handler (entities form).
- [ ] Rows form: every table gets the [contract](#give-every-repository-the-one-contract) from one generic repository; a specification holds no order, no page, and no statement; no selector leaves its service.
- [ ] Entities form: a specification with a page serves a list only; each module configures its own tables.
- [ ] The number of rows written comes from the write.
- [ ] Every outcome of the [table](#report-a-repositorys-outcomes-in-its-own-terms) is turned into its kind: by the service (rows form) or by one command middleware (entities form).
- [ ] `DATABASE_DSN` is required; no failure and no log repeats its value.
- [ ] Migrations run in exactly one mode; the job exits 0 on an up-to-date database.
- [ ] The database brings its named readiness check with no type of the health variation point.
- [ ] Scenarios run on a real PostgreSQL, each on its own database, and fail without one.
- [ ] The stack skill's `conformance.copy` names every folder of this skill's `conformance/` its form runs; every copy is unedited.
