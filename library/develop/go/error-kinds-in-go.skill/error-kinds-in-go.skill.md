---
name: error-kinds-in-go
description: The error kinds in Go — package internal/shared/errorkind with the kind sentinels and *errorkind.Error (Kind, Message for the caller, Cause for the log) built by New or Wrap, re-exported with KindOf in internal/command/pipeline/errors.go, answered by every entry point through one table in internal/api/core/rpcstatus (google.rpc.Code, HTTP status, the error's Message), and logged as error and cause, with no gRPC module needed by an HTTP-only service
whenToUse: when a Go command, domain service, entity, or service returns an error a caller must act on, when a Go entry point turns an error into its answer, or when you review either
updated: 20261104
tags:
  - stack/go
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
---

# Goal
- **The error kinds in Go** - A Go application that keeps every property of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md); this skill details that skill's implementation for Go and restates none of its rules.

# Core Principle
- **Three fixed files** - The error, its re-export, and the table are copied from templates and never edited by an application: every kind the table names already exists.
- **Message for the caller, cause for the log** - `(*errorkind.Error).Error()` is its `Message` alone, so no print of the error shows its `Cause`; `Unwrap` gives `errors.Is` the kind and the cause.
- **The outermost error decides the kind** - `KindOf` takes the kind of the first `*errorkind.Error` in the chain; `errors.Is` would also see a kind inside its cause, which the service has told the caller in other terms.

```
internal/shared/errorkind/errorkind.go    ErrInvalid, ... - the kinds; Error{Kind, Message, Cause}, New, Wrap, KindOf, Of
internal/domain/entity/errors.go          the domain's errors, each errorkind.New of its kind
internal/command/pipeline/errors.go       the kinds re-exported + Kinds + KindOf
internal/api/core/rpcstatus/rpcstatus.go  kind → Code (google.rpc.Code) → HTTP status, the error's Message
```

# Rule

## MUST

### Copy the error from templates
Fill and copy [errorkind.go](./templates/internal/shared/errorkind/errorkind.go) to `internal/shared/errorkind/` and [errors.go](./templates/internal/command/pipeline/errors.go) to `internal/command/pipeline/`, replacing `{module-path}` with the module path from `go.mod`.
- Risk: a kind declared in another package, or a second sentinel for the same kind, makes `errors.Is` miss it and the error is answered `INTERNAL`.
- Fix: one sentinel per kind in `errorkind`, which every layer may import; entry points use them through `pipeline`.

### Build an error with errorkind.New or errorkind.Wrap
Declare each error a caller can act on as `errorkind.New(errorkind.Err{Kind}, "{what happened, for the caller}")`, or `errorkind.Wrap(kind, message, cause)` when a technical failure lies behind it, and wrap it again with `%w` when passing it up.

```go
var ErrEmptyTitle = errorkind.New(errorkind.ErrInvalid, "item title is empty")

if errors.Is(err, postgres.ErrUnavailable) {
	return errorkind.Wrap(errorkind.ErrUnavailable, "the lists are unavailable", err)
}
```

- Violation: `fmt.Errorf("%w: no list %q", errorkind.ErrNotFound, id)`; `fmt.Errorf("%w: the todo service: %v", errorkind.ErrUnavailable, err)`; `fmt.Errorf("plan: %v", err)` with `%v`; a check `strings.Contains(err.Error(), "not found")`.
- Risk: a kind wrapped by `fmt.Errorf` is answered with the kind's name alone, its message lost; a cause written into a message names hosts, users, and queries to the caller; an error that lost its kind is answered `INTERNAL`.
- Fix: the message says what happened in the caller's terms, the cause holds the failure; fixed errors are package variables beside the code that returns them, errors carrying a value are built at the return (`errorkind.New(errorkind.ErrNotFound, fmt.Sprintf("no list %q", id))`); a kind is checked only with `errors.Is(err, pipeline.Err{Kind})` or `pipeline.KindOf(err)`.

### Answer through rpcstatus in every network entry point
Fill and copy [rpcstatus.go](./templates/internal/api/core/rpcstatus/rpcstatus.go) to `internal/api/core/rpcstatus/`, with the scenarios of an API variation point's example, and answer every error with `rpcstatus.Of(err)`: over HTTP as `code.HTTPStatus()` with a `google.rpc.Status` body, over gRPC as `status.Error(codes.Code(code), message)`.
- Violation: an entry point with its own `switch` or `map` of kinds; a second `rpcstatus.go` that differs from the first.
- Risk: the HTTP and gRPC forms of one service answer the same kind differently.
- Fix: the HTTP and gRPC variation points copy the same file, which answers the `Message` of the `*errorkind.Error` itself, never the text of a wrapper; `bash test/check-vp-attach.sh` in this repository fails when two variation points add it differently.

### Log the error with logging.Error
Write the error of a failed call with `logging.Error(err)`, which writes the `error` attribute and, for an error with a kind and a cause, the `cause` attribute beside it.
- Violation: `slog.Any("error", err)` in the command logging middleware; the cause formatted into the message to see it in the log.
- Risk: the record of a failed call says only what the caller was told, and an outage is debugged with no trace of what failed.
- Fix: the command logging middleware of [base-plateau-in-go](library/architecture/go/base-plateau-in-go.skill/base-plateau-in-go.skill.md) writes it so; an error with no kind keeps its whole text in `error`.

### Exit by kind in a CLI entry point
Map an error to the exit code of the table in a command-line entry point: `0` for none, `1` when `pipeline.KindOf(err)` is nil or `pipeline.ErrUnavailable`, `2` otherwise.
- Risk: a script retries a request that can never succeed, or gives up on one that a retry would fix.
- Fix: follow `ExitCode` of [base-plateau-in-go](library/architecture/go/base-plateau-in-go.skill/base-plateau-in-go.skill.md)'s startup entry.

# Check list
- [ ] `internal/shared/errorkind/errorkind.go`, `internal/command/pipeline/errors.go`, and, in a service with a network entry point, `internal/api/core/rpcstatus/rpcstatus.go` are filled copies of the templates.
- [ ] Every error a caller can act on is built by `errorkind.New` or `errorkind.Wrap`, with the technical failure as its cause and never in its message; every wrap above it uses `%w`.
- [ ] No code outside `errorkind` and `pipeline` declares a kind; no code checks an error by its text.
- [ ] The error of a failed call is logged with `logging.Error`.
- [ ] Every network entry point answers errors only through `rpcstatus.Of`; a CLI entry point exits by `pipeline.KindOf`.
