---
name: unit of work at the command
description: Who owns the database transaction of an operation that changes data in several places
problem: One command can create a record, run nested commands, and trigger notification handlers that write more records and enqueue an outbox task; all of it must commit together or not at all. Who opens and commits the transaction?
decision: A unit of work opened around the outermost command owns the transaction; nested commands and notification handlers join it; services write through it and never commit. It is optional — available once the application has a database — and once any command is complex, every command is wrapped.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`AddUserCmd` creates a user, then `UserAddedNotification` is handled by several handlers that write records and enqueue an outbox task for Kafka. These writes come from code that does not know about each other, yet must be atomic. The dotnet catalog solves it with a unit of work: the first command opens it, everything below writes into it, and only the outermost command commits.

# Selected variant
[[#Unit of work around the outermost command]]
- Owner decision, 2026-10-17: optional — not needed by a simple service; once commands become complex, every command is wrapped so no command commits partway by accident. Irreversible changes in other systems are a matter of workflow design (outbox, compensation), not of the unit of work.

# Searched variants

## Unit of work around the outermost command

**Selected.**

### Description
A decorator (a pipeline behavior in dotnet, a handler decorator in Go) opens the unit of work around a command, unless one is already open. Nested commands and notification handlers join it. Storage services write through it. It commits once, after the outermost command returns successfully, and rolls back otherwise. In dotnet, EF collects the changes in memory and writes them in one short transaction at the end; in Go, the transaction travels in the context and is opened on the first write.

### Benefits
- Code that writes does not need to know who else writes in the same operation.
- An outbox task enqueued in the unit of work is atomic with the data for free.
- One pattern for every command.

### Costs
- In Go, every storage method takes its query executor from the context.
- In Go, a transaction opened at the first write stays open until the command ends, including across external calls made after it.

## Each service method owns its transaction

### Description
A storage-service method opens and commits its own transaction; atomic changes across several places need one service method that does them all.

### Benefits
- The transaction is exactly as long as the database work.

### Costs
- Nested commands and notification handlers each commit on their own; atomicity across them needs one large method, which breaks the decomposition into handlers.

## Unit of work mandatory for every application

### Description
Every application wraps every command from the start.

### Benefits
- No decision to make later.

### Costs
- A simple service with no complex commands — or no database — carries a pattern it does not use.
