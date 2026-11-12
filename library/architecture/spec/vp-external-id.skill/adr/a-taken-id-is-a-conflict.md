---
name: a taken id is a conflict
description: What a create answers when the caller's id is already taken - a conflict, a success when the fields are the same, or a conflict carrying the row that has it
problem: A caller that lost the answer of a create sends it again with the same id, and the id is taken. A second caller may also send an id another row has. What does the service answer?
decision: A taken id is the kind conflict, decided by the database's unique key. A repeated request is told apart by vp-idempotency, which answers it with the first answer before the command runs.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The old catalog's `GuidResolvingBehavior` looked the id up before the handler and answered 409 with the existing row. A repeated request and a second create of one id look the same to the service: both carry a taken id.

# Selected variant
[[#A conflict; the repeated request is vp-idempotency's]]
- Chosen by the owner (2026-11-04): with no matching idempotency key, a taken id is a conflict; with one, the request is a duplicate and gets the first answer.

# Searched variants

## A conflict; the repeated request is vp-idempotency's

**Selected.**

### Description
The insert fails on the unique key; the service answers `Conflict`. `vp-idempotency`, in front of the command, answers a request whose `Idempotency-Key` it knows with the answer it stored, so that request never reaches the insert.

### Benefits
- One check, in the insert; no read first, so two creates of one id at once cannot both pass.
- No code per table: the kind is the same for every table.
- A repeated request of any command is answered the same way, not only a create.

### Costs
- A caller that repeats a create without an idempotency key gets a conflict, and reads the row to see it is its own.

## A success when the fields are the same

### Description
The service reads the row that has the id and answers success when its fields equal the request's, a conflict otherwise.

### Benefits
- A caller's repeated create succeeds with no key.

### Costs
- A comparison written for every table, and a field changed since the first create turns a repeat into a conflict.

## A conflict carrying the row that has it

### Description
409 with the existing row in the body, as the old catalog did.

### Benefits
- The caller sees the row with no second read.

### Costs
- The error of one kind carries a body that differs per table, which no entry point answers through the one table of error kinds.
