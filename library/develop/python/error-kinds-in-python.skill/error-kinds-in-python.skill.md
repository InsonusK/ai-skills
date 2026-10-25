---
name: error-kinds-in-python
description: The error kinds in Python — the Kind enum and the KindError exception declared once in src/{package}/domain/entity/kinds.py, re-exported with kind_of in src/{package}/command/pipeline/errors.py, one KindError subclass per error a caller can act on, the kind kept through raise ... from, and an exit code chosen by kind
whenToUse: when a Python command, domain service, entity, or service raises an error a caller must act on, when a Python entry point turns an error into its answer, or when you review either
updated: 20261022
tags:
  - stack/python
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
---

# Goal
- **The error kinds in Python** - A Python application that keeps every property of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md); this skill details that skill's implementation for Python and restates none of its rules.

# Core Principle
- **An error is an exception with a kind** - Python signals errors by raising; an error a caller can act on is a `KindError` carrying one `Kind`, and any other exception is a failure of the service.
- **Two fixed modules** - The kinds and their re-export are copied from templates and never edited by an application: every kind the table names already exists.

```
src/{package}/domain/entity/kinds.py       Kind, KindError, kind_of
src/{package}/domain/entity/errors.py      the application's errors, one KindError subclass each
src/{package}/command/pipeline/errors.py   the kinds re-exported for the entry points
src/{package}/api/core/rpcstatus/          kind → google.rpc.Code → HTTP status, message
```

# Rule

## MUST

### Copy the kinds from templates
Copy [kinds.py](./templates/src/{package}/domain/entity/kinds.py) to `src/{package}/domain/entity/` and [errors.py](./templates/src/{package}/command/pipeline/errors.py) to `src/{package}/command/pipeline/`, replacing `{package}` with the import name of the application's package.
- Risk: a second `Kind` or a `KindError` declared elsewhere is not the one `kind_of` looks for, and the error is answered `INTERNAL`.
- Fix: one `Kind` and one `KindError` in `entity`; entry points use them only through `command.pipeline`, so they never import the domain.

### One subclass per error
Declare each error a caller can act on as a subclass of `KindError` in `domain/entity/errors.py` — or in the errors module of the layer that raises it — whose `__init__` passes its kind and its message for the caller, and raise an instance of it.

```python
class DuplicateTitleError(KindError):
    def __init__(self) -> None:
        super().__init__(Kind.INVALID, "item titles repeat")
```

- Violation: `raise KindError(Kind.INVALID, "item titles repeat")` at the place of the rule; `raise ValueError("not found")`; a check `"not found" in str(error)`.
- Risk: the same error is written with two messages or two kinds in two places, and a test can tell it from another only by its text; a `ValueError` is answered `INTERNAL`.
- Fix: the subclass holds the kind and the message once; a test checks `isinstance(error, DuplicateTitleError)`, an entry point `kind_of(error)`.

### Keep the kind when wrapping
Wrap an error passed up as `raise {Other}Error(...) from error`, and read a kind only with `kind_of(error)`, which follows `__cause__`.
- Violation: `raise RuntimeError("plan failed")` inside `except KindError`, which leaves the kind only in the implicit `__context__`.
- Risk: the kind is lost on the way up, and the caller's mistake is answered as a crash.
- Fix: `from error` at every wrap, or let the `KindError` propagate.

### Answer through rpcstatus in every network entry point
Copy [api/core/rpcstatus/](./templates/src/{package}/api/core/rpcstatus/) with the first network entry point, and answer every error with `rpcstatus.of(error)`: over HTTP as `http_status(code)` with a `google.rpc.Status` body, over gRPC as the same code with the message.
- Violation: an entry point with its own `dict` of kinds; a second `rpcstatus` that differs from the first.
- Risk: the HTTP and gRPC forms of one service answer the same kind differently; the message of a wrapper is sent instead of the error's own.
- Fix: the HTTP and gRPC variation points copy the same package; `of` answers with the message of the `KindError` itself.

### Exit by kind in a CLI entry point
Map an error to the exit code of the table in a command-line entry point: `0` for none, `1` when `kind_of(error)` is `None` or `Kind.UNAVAILABLE`, `2` otherwise.
- Risk: a script retries a request that can never succeed, or gives up on one that a retry would fix.
- Fix: follow `exit_code` of [base-plateau-in-python](library/architecture/python/base-plateau-in-python.skill/base-plateau-in-python.skill.md)'s runner of the entry points.

# Check list
- [ ] `src/{package}/domain/entity/kinds.py`, `src/{package}/command/pipeline/errors.py`, and, in a service with a network entry point, `src/{package}/api/core/rpcstatus/` are filled copies of the templates.
- [ ] Every error a caller can act on is a `KindError` subclass with its kind and message, raised as an instance.
- [ ] Every wrap uses `raise ... from error`; no code checks an error by its text.
- [ ] A CLI entry point exits by `kind_of`.
