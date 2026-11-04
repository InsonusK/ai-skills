---
name: vp-external-id
description: The external id variation point — a table whose rows are created by a caller outside the service (a front end, another system) takes the caller's UUID v7 as its primary key, refused as invalid when it is no UUID v7, saved as given, and a taken id answered as a conflict, so the caller knows the id before the answer and can repeat a create it lost; attached to vp-postgres table by table
whenToUse: when rows of a table are created by a caller outside the service that must know the row's id before the service answers (a front end working offline, another system creating records), when you decide who sets the identity of a table's rows, or when you review how a created row gets its id
updated: 20261104
tags:
  - stack
  - concern/architecture
  - app-type/service
adr:
  - adr/uuid-v7-as-the-primary-key.md
  - adr/a-taken-id-is-a-conflict.md
---

# Goal
- **The caller knows the id first** - A caller creating a row makes its id itself, so it can refer to the row, and repeat the create, before any answer comes.
- **One id per row** - The id the caller made is the row's primary key; no second id, no lookup from one to the other.
- **Inserts stay cheap** - The id is ordered by time, so a table of millions of rows takes new ones at the end of its index.
- **Every stack behaves the same** - The scenarios of this skill's `conformance/` folder pass in every stack.

# Core Principle
- **The caller's UUID v7 is the key** - A UUID v7 starts with the time it was made in milliseconds, so ids come in order without the service handing them out. Decision recorded in [uuid-v7-as-the-primary-key](./adr/uuid-v7-as-the-primary-key.md).
- **The entry point guards the form** - An id that is no UUID v7 never reaches the service.
- **A taken id is a conflict** - The database's unique key decides; telling a repeated request from a second create is not this variation point's job. Decision recorded in [a-taken-id-is-a-conflict](./adr/a-taken-id-is-a-conflict.md).

@import "./diagrams/concept.mmd" {as="mermaid"}

# Scope
- Covers the identity of the rows of one table that a caller outside the service creates. A table takes this variation point when [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md#classify-every-table) says so; `vp-row-version` attaches beside it when the rows change, and then a create answers the row's version too.
- A table whose rows the service creates keeps the `bigint` identity of `vp-postgres`.
- Answering a repeated request with the answer of the first is `vp-idempotency`, by a key in a header.
- The implementation for each stack is described in its stack-specialized extension.

# Rule

## MUST

### Key the table by the caller's id
Give the table a primary key of type `uuid` with no default, save the id the caller gave, and give every foreign key to it the type `uuid`.
- Violation: a `bigint` key beside a unique `guid` column; a default of `gen_random_uuid()`; an id made by the service when the caller sent none.
- Risk: two ids per row cost a second index and a lookup on every call; an id the service makes is one the caller cannot know before the answer, which is the reason for the variation point.
- Fix: the row's key column is the id; a create with no id is `Invalid`.

### Accept only a UUID v7
Refuse at the entry point, with the kind `Invalid`, an id that is not a UUID of version 7 and the RFC 9562 variant in its form of 36 characters (either case), and parse it once there into the stack's UUID type, which answers it in lowercase.
- Violation: a `string` id passed to the service; a UUID v4 accepted; the time of the id checked against the clock.
- Risk: a random UUID scatters inserts over the whole index, as the measurement of [uuid-v7-as-the-primary-key](./adr/uuid-v7-as-the-primary-key.md) shows; a malformed id reaches the database and fails as an internal error.
- Fix: one parse function of the stack answers the id or an error of the kind `Invalid` naming the field; nothing checks the time in it, which belongs to the caller's clock.

### Warn when the creation time may be secret
Warn the user before choosing this variation point for a table when the time a row was created may be secret, because a UUID v7 tells it to everyone who sees the id.
- Violation: UUID v7 ids on rows whose creation time must not be disclosed (when a person did something, the order of records another party must not learn) chosen without a word to the user.
- Risk: the id, shown in a URL, a log, or an answer, discloses the time to the millisecond.
- Fix: name the risk to the user and let them decide; the alternative is a random id with its cost of inserts, or an id the service sets.

### Answer a taken id as a conflict
Turn the repository's duplicate key on the primary key into the kind `Conflict` in the service, naming the thing and the id.
- Violation: a duplicate key answered as an internal failure; a service that reads the row first to see whether the id is taken; a create that answers success for a taken id.
- Risk: a read before the insert lets two creates of one id pass the check; a success for a taken id tells a second caller its row was saved when another one is there.
- Fix: the unique key of the database decides, in the insert itself; a repeated request is answered by `vp-idempotency` before the command runs.

### Mark every table
Mark every row of the application by who sets its identity, the service or the caller, and let a check fail on a row with no mark.
- Violation: a new table with no mark; a `uuid` key with a default on a table marked as set by the caller.
- Risk: a table nobody decided about gets whatever id its author typed first.
- Fix: a check over the mapped rows fails while one has neither mark.

### Pass the shared scenarios
Run this skill's [conformance/](./conformance/) scenarios in every stack, unedited, through a copy named in the stack skill's `conformance.copy`, as [conformance-scenarios-copied-into-each-stack](library/architecture/spec/vp-logging.skill/adr/conformance-scenarios-copied-into-each-stack.md) decides.
- Risk: a stack answers a different contract under the same names.
- Fix: step definitions are the stack's own; the scenarios run on a table `drafts` of the test project, whose rows the caller creates and changes, which the application never ships.

### Attach without editing the base
Attach the variation point by changing no fixed file of [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md); only the composition root, the rows the caller creates, their migrations, the entry points' parse of the id, and the services over them.
- Risk: an id written into the base repository makes every table take the caller's id, also the ones the service creates.
- Fix: the stack extension lists the files it changes; a check compares its example with the base against that list.

# Check list
- [ ] Every row is marked by who sets its identity; a check fails on a row with no mark.
- [ ] A table the caller creates has a `uuid` key with no default, and every foreign key to it is `uuid`.
- [ ] Every entry point refuses an id that is no UUID v7 as `Invalid`; the service receives the stack's UUID type.
- [ ] The user was warned when the creation time of the table's rows may be secret.
- [ ] A taken id is a `Conflict`; no service reads before it inserts.
- [ ] The stack skill's `conformance.copy` names this skill's `conformance/` folder, and every copy is unedited.
