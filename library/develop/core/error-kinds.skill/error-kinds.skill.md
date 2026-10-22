---
name: error-kinds
description: The kinds of error an application's commands return and how every entry point answers them — eleven kinds (invalid, not found, conflict, concurrent change, failed precondition, unauthenticated, permission denied, resource exhausted, unimplemented, unavailable, and no kind for a failure of the service), when to choose each, and one table from kind to gRPC code, HTTP status, and CLI exit code, with the canonical gRPC-to-HTTP mapping
whenToUse: when a command, domain service, entity, or service returns an error a caller must act on, when an entry point (HTTP, gRPC, CLI, a listener) turns an error into its response, when you choose between two kinds (conflict or failed precondition, invalid or not found), or when you review either
updated: 20261022
tags:
  - stack
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
---

# Goal
- **One answer per outcome** - The same outcome is answered with the same code by every entry point of every service: a missing order is `NOT_FOUND`/404 over gRPC and HTTP, in this service and the next.
- **No guessing** - The agent writing a command picks the kind from the table below by what happened, and the agent writing an entry point never chooses a status: the table does.
- **Nothing internal leaks** - A client reads the message of every kind it can act on, and never the message of a failure of the service.

# Core Principle
- **The command says what happened, the entry point says it in its protocol** - A command returns a kind (`not found`), never a status (`404`); each entry point turns the kind into its own answer through this one table.
- **A kind is about the request, not the code path** - The kind says what the caller can do about it: fix the input, pick another name, retry later, ask for a permission. Two errors the caller handles the same way share a kind.
- **No kind means the service failed** - Any error that carries no kind is the service's own failure, answered `INTERNAL` without its message.

# Scope
- Covers the errors a command returns and how entry points answer them. Codes an entry point sets by itself, with no command involved, are listed in [Codes an entry point sets itself](#codes-an-entry-point-sets-itself).
- The implementation for each stack is described in its stack-specialized extension: `error-kinds-in-go`, `error-kinds-in-dotnet`, `error-kinds-in-python`.

# Rule

## MUST

### Choose the kind by the table
Return every error a caller can act on with exactly one kind of this table, chosen by its "When" column; return any other error with no kind.

| Kind | When | gRPC | HTTP | CLI exit |
|---|---|---|---|:-:|
| invalid | the input breaks a rule whatever the state: an empty title, a malformed e-mail, two equal titles in one request | `INVALID_ARGUMENT` | 400 | 2 |
| not found | the object the input names does not exist: no order with this id | `NOT_FOUND` | 404 | 2 |
| conflict | the object to create already exists: the name is taken, the e-mail is registered | `ALREADY_EXISTS` | 409 | 2 |
| concurrent change | the object was changed by someone else between read and write; repeating the whole operation may succeed | `ABORTED` | 409 | 2 |
| failed precondition | the input is valid, but the object's state does not allow the operation: the order is already shipped, the item is already done | `FAILED_PRECONDITION` | 400 | 2 |
| unauthenticated | the caller is not identified: no token, an expired token | `UNAUTHENTICATED` | 401 | 2 |
| permission denied | the caller is identified but may not do this | `PERMISSION_DENIED` | 403 | 2 |
| resource exhausted | a quota or limit of the caller is used up: too many requests, the plan's maximum of projects | `RESOURCE_EXHAUSTED` | 429 | 2 |
| unimplemented | the operation is not offered: deprecated and switched off, or not built yet | `UNIMPLEMENTED` | 501 | 2 |
| unavailable | a dependency is down for now; the same request may succeed later | `UNAVAILABLE` | 503 | 1 |
| *(no kind)* | anything else: a bug, a broken invariant, an unexpected answer of a dependency | `INTERNAL` | 500 | 1 |

- Violation: "the order is already shipped" returned as invalid; "the name is taken" as invalid or failed precondition; a database timeout returned as unavailable when the cause is a bug in the query.
- Risk: the caller is told to fix an input that is correct, or to retry a request that can never succeed, and every service of the system answers the same outcome differently.
- Fix: ask what the caller can do: change the input → invalid; change the target → not found; pick another name → conflict; read again and retry → concurrent change; wait for the object's state to change → failed precondition; retry the same request later → unavailable.

### Tell the close pairs apart
Decide the kinds that are easy to confuse by these tests.

| Pair | Test |
|---|---|
| invalid / failed precondition | would the same request succeed on another object, or at another time? yes → failed precondition |
| invalid / not found | is the input well-formed and names something that does not exist? yes → not found |
| conflict / failed precondition | is the problem that the thing already exists? yes → conflict |
| conflict / concurrent change | would a fresh read and the same change succeed? yes → concurrent change |
| unavailable / no kind | is the dependency known to be down (refused connection, 503 from it) rather than answering wrongly? yes → unavailable |

- Risk: two agents choose different kinds for one outcome, and a client handling `409` misses half of the cases.
- Fix: apply the test; record the choice in a comment beside the error when the case is still close.

### Return the kind, never a status
Return a kind from the layer where the outcome is known — an entity for a rule, a service for a missing record, a command for a missing permission — and let only an entry point turn it into a status, a code, or an exit code.
- Violation: a command returning `404`, an HTTP status type, or a gRPC status; an entry point choosing a status by the error's text.
- Risk: the command depends on one transport, and the next entry point must translate a foreign status back into its own.
- Fix: the kind wraps the error (`not found: order 42`); every entry point answers it through the one table of its stack.

### Answer every kind through one table
Map kinds to codes in one place shared by every entry point of the application, and derive the HTTP status from the gRPC code by the canonical mapping.

| gRPC code | HTTP |
|---|---|
| `INVALID_ARGUMENT`, `FAILED_PRECONDITION`, `OUT_OF_RANGE` | 400 |
| `UNAUTHENTICATED` | 401 |
| `PERMISSION_DENIED` | 403 |
| `NOT_FOUND` | 404 |
| `ALREADY_EXISTS`, `ABORTED` | 409 |
| `RESOURCE_EXHAUSTED` | 429 |
| `CANCELLED` | 499 |
| `INTERNAL`, `UNKNOWN`, `DATA_LOSS` | 500 |
| `UNIMPLEMENTED` | 501 |
| `UNAVAILABLE` | 503 |
| `DEADLINE_EXCEEDED` | 504 |

- Violation: an HTTP entry point with its own `switch` of kinds; 409 for `FAILED_PRECONDITION`; 422 for a request that cannot be parsed.
- Risk: the HTTP and gRPC forms of one service answer one outcome differently, and a gateway in front of the service answers differently from the service itself.
- Fix: one table in code, used by every entry point; a new kind is one row there.

### Show the message of a kind, hide the rest
Send the error's message to the client for every kind, and send only `internal error` for an error with no kind, logging the full error.
- Violation: `INTERNAL` carrying `dial tcp 10.0.0.5:5432: connection refused`; a kind's message holding a secret or another user's data.
- Risk: an internal message leaks addresses, queries, and stack details; a client denied a useful message for a rejected request cannot fix it.
- Fix: write a kind's message for the caller (`item titles repeat`); log the failure in the handler's logging decorator, not in the answer.

## SHOULD

### Codes an entry point sets itself
Set `CANCELLED` (the client went away), `DEADLINE_EXCEEDED` (the call's deadline passed), and `INVALID_ARGUMENT` for a request that cannot be decoded in the entry point itself, never as a kind a command returns.
- Risk: a command that returns "cancelled" or "deadline exceeded" duplicates what the transport already knows, and the two can disagree.

# Check list
- [ ] Every error a caller can act on carries exactly one kind of the table, chosen by its "When" column and, for a close pair, by its test.
- [ ] No command, service, or entity returns a status, a code, or an exit code.
- [ ] Every entry point answers kinds through the one shared table; the HTTP status comes from the canonical mapping.
- [ ] An error with no kind is answered `INTERNAL` with `internal error`; its full text goes only to the log.
