---
name: vp-unit-of-work
description: The unit of work variation point — a command middleware around the outermost command that keeps every write of that command, its nested commands, and its notification handlers in one transaction, opened on the first write and ended by the middleware alone, attached to a DB variation point with no change to a store, a service, or a handler
whenToUse: when an application with a DB variation point has a command that writes in several places at once (nested commands, notification handlers, an outbox task), when you decide whether an application needs a unit of work, or when you review one
updated: 20261031
tags:
  - stack
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **All or nothing** - Every write of one command and of the commands it runs is kept, or none is, so a failure halfway leaves nothing to clean up.
- **Attached, not edited** - A store keeps saving as it does in [vp-postgres](library/architecture/spec/vp-postgres.skill/vp-postgres.skill.md): the variation point only puts a transaction around its saves, so no store, service, or handler changes.
- **Reads hold nothing** - A command that writes nothing opens no transaction.

# Core Principle
- **The outermost command owns the unit** - It begins and ends it; nested commands and notification handlers join; a service never commits ([app-layers](library/develop/core/app-layers.skill/app-layers.skill.md)).
- **Open on the first write** - The transaction spans the command from its first write to its end, so what a command does before that write holds no lock.

@import "./diagrams/concept.mmd" {as="mermaid"}

# Scope
- Covers the local transaction of one command over one database. A write to another system is not rolled back by it: that is [workflow-consistency-design](library/common-workflow/architecture/core/workflow-consistency-design.skill/workflow-consistency-design.skill.md) and the outbox variation point.
- The implementation for each stack is described in its stack-specialized extension: `vp-unit-of-work-in-dotnet`.

# Rule

## MUST

### Take it for every command or for none
Attach the unit of work once any command changes data in several places at once, and let it wrap every command; an application where every command makes one write may go without it.
- Violation: a mark that wraps only the commands someone remembered.
- Risk: a nested command joins nothing when its parent has no unit, and the failure leaves half the writes.
- Fix: one middleware in the list, around every command.

### End the unit only in the middleware
Commit when the outermost command returned and roll back when it failed, in the middleware alone; let a nested command join and end nothing.
- Violation: a service or handler that commits; a nested command that rolls back.
- Risk: the first nested command to finish ends the transaction of all the others.
- Fix: the handler reports a failure by its error; the outermost command decides.

### Let the caller's cancellation stop the work, not the commit
Commit and roll back whatever the caller's cancellation says, once the handler has ended.
- Violation: the caller's deadline passed to the commit.
- Risk: work that is done is undone because the caller left at the last moment, or a rollback never runs and holds its locks.
- Fix: a commit has the database's own timeout.

### Tell a failed commit as unavailable
Report a commit the database did not answer with the kind unavailable, and treat its outcome as unknown: the commit may have reached the database.
- Risk: a retry of a command whose commit did reach the database does its work twice.
- Fix: a command a caller may retry after unavailable is idempotent, by a unique key of its own.

### Keep other services' calls before the first write
Call other services before the command's first write, or after it through an outbox.
- Violation: a handler that inserts a row, calls an HTTP API for ten seconds, and inserts another.
- Risk: the transaction holds its locks and a connection for as long as the other service takes, and a rollback does not undo that service's change.
- Fix: read first, write last; show the user the consistency table.

### Attach without editing
Attach the variation point by changing no store, service, or handler; only the composition root and the build files.
- Risk: a unit of work written into the stores cannot be removed, and a store written for it fails without it.
- Fix: the stack extension lists the files it changes; a check compares its example with the DB example against that list.

# Check list
- [ ] Every command runs through the middleware; nested commands and notification handlers join its unit.
- [ ] Only the middleware commits or rolls back; a command that only reads opens no transaction.
- [ ] The caller's cancellation does not reach the commit or the rollback.
- [ ] A failed commit is told as unavailable.
- [ ] No other service is called between the first write and the end of a command.
- [ ] No store, service, or handler changed to attach it.
