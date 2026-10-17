---
name: workflow-consistency-design
description: How to design a process that changes state in more than one place — the own database, another service, a broker — so that a failure at any point cannot leave the system inconsistent — list every state change, write the inconsistency each gap between them leaves, close each gap with a known protection, and show the user what remains before any code is written
whenToUse: when you design, change, or review a process, command, or integration that writes to more than one store or system — the own database plus another service, a broker, a file, or a second database — or that creates or changes data in another service, or retries such a call
updated: 20261017
tags:
  - stack
  - concern/architecture
---

# Goal
- A consistency table for the process: every state-changing step with where its change lands, every gap after a step with the inconsistency it leaves, the protection chosen for it, and the risk that remains.
- Every gap either closed by a protection or accepted by the user by name.
- The table shown to the user before the process is implemented, and kept with the process's design.

# Core Principle
- **It can stop between any two steps** - A crash, a deploy, a timeout, or an error can end the process after any step, so every gap between two state changes is designed for, not hoped away.
- **A timeout is an unknown outcome** - A call that timed out may have done its work; the process cannot treat it as failed.
- **Only one local transaction is atomic for free** - Changes inside one database transaction commit together; every other pair of changes needs a protection.
- **The user decides what stays open** - An inconsistency nobody closed is shown to the user and accepted or rejected by them, never left unmentioned.

# Workflow
1. [List every state change](#list-every-state-change-and-where-it-lands) of the process in order.
2. [Write the inconsistency of every gap](#write-the-inconsistency-of-every-gap).
3. [Close each gap](#close-each-gap-with-a-known-protection) and write what risk remains.
4. [Show the table to the user](#show-the-table-before-implementing) and record what they accept.

# Rule

## MUST

### List every state change and where it lands
List each step of the process that changes state, in order, with the place the change lands: the own database, another service (named), a broker topic, a file.
- Violation: a design that says "create the user and save the link" without saying that the first change lands in the user service and the second in the own database.
- Risk: two changes in different places look like one step, and the gap between them is never examined.
- Fix: one row per change; a read that decides what happens next is listed too when its answer can be stale.

### Write the inconsistency of every gap
For each gap — after each step, and after the last step before the caller gets the reply — write what the system holds if the process stops there, and what happens when the caller or a retry runs the process again.
- Violation: "if saving the link fails we return an error" — silent about the user already created in the other service, and about the retry that tries to create them again.
- Risk: the gap is found in production as an orphan record, a duplicate, or a lost message.
- Fix: write it as a state: "user exists in the user service, no link in our database; a retry creates a second user".

### Close each gap with a known protection
Close every gap with the protection matching its situation, and write the risk that remains after it.

| Situation | Protection |
|---|---|
| several changes in the own database | one local transaction around them |
| a change in the own database plus a message or call to another system | transactional outbox: the message or call is saved as a task in the same transaction, sent after commit, and retried until delivered |
| a message or call that may arrive twice | inbox or idempotency key: the receiver records what it processed and ignores the repeat |
| a call that may time out | idempotency key on the call, and a status check before retrying |
| changes in several services that must all happen or none | saga: each step has a compensating step, run in reverse order when a later step fails |
| a step that cannot be undone | order it last, after every step that can fail; or reserve first and confirm last |
| a gap no protection closes at an acceptable cost | a reconciliation job that finds and repairs the inconsistency, or the user's explicit acceptance |

- Risk: an invented protection misses a case the known pattern already handles — a duplicate on retry, a message sent before a rollback.
- Fix: name the pattern in the table; a residual risk is written even when it is "none".

### Never retry without idempotency
Retry a step that changes state — a call, a message, a task — only when the receiver recognises the repeat and does not apply it twice.
- Violation: a retry loop around `POST /users` with no idempotency key.
- Risk: every timeout that actually succeeded becomes a duplicate.
- Fix: send an idempotency key the receiver stores, or check the receiver's state before retrying.

### Show the table before implementing
Show the consistency table to the user before implementing the process, and again in any change or review that adds a state change or a gap, naming each gap that is not fully closed and the inconsistency it can leave.
- Violation: implementing a command that creates a record in another service and writes locally, mentioning the gap only in a code comment.
- Risk: the user learns about the inconsistency from an incident instead of deciding about it up front.
- Fix: present the table, ask the user to accept or close each open gap, and record the decision with the process's design (an ADR or the feature's design document).

## SHOULD

### Prefer ordering and outbox over a saga
Close a gap by reordering the steps or by a transactional outbox before reaching for a saga with compensations.
- Risk: every compensation is a second process with its own gaps.
- Fix: a saga only when several services must each change state and none of the changes can be deferred to a task.

# Check list
- [ ] Every state-changing step is listed with where its change lands.
- [ ] Every gap, including the one before the reply, has its inconsistency written as a state, with the effect of a retry.
- [ ] Every gap names its protection from the table, and its remaining risk.
- [ ] No state-changing step is retried without idempotency.
- [ ] The table was shown to the user; every gap left open was accepted by them and recorded with the design.

# Example
- [examples/add-user.md](./examples/add-user.md) — the consistency table of a command that creates a user in another service and saves the link locally.
