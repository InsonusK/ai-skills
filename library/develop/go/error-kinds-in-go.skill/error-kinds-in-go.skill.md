---
name: error-kinds-in-go
description: The error kinds in Go — sentinel errors declared once in internal/domain/entity/kinds.go, re-exported with KindOf in internal/command/pipeline/errors.go, wrapped with %w by every error a caller can act on, and answered by every entry point through one table in internal/api/rpcstatus (google.rpc.Code, HTTP status, message), with no gRPC module needed by an HTTP-only service
whenToUse: when a Go command, domain service, entity, or service returns an error a caller must act on, when a Go entry point turns an error into its answer, or when you review either
updated: 20261018
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
- **Three fixed files** - The kinds, their re-export, and the table are copied from templates and never edited by an application: every kind the table names already exists.

```
internal/domain/entity/kinds.go      ErrInvalid, ErrNotFound, ... - the sentinels
internal/domain/entity/errors.go     the application's errors, each wrapping its kind
internal/command/pipeline/errors.go  the sentinels re-exported + Kinds + KindOf
internal/api/rpcstatus/rpcstatus.go  kind → Code (google.rpc.Code) → HTTP status, message
```

# Rule

## MUST

### Copy the kinds from templates
Copy [kinds.go](./templates/internal/domain/entity/kinds.go) to `internal/domain/entity/`, and fill and copy [errors.go](./templates/internal/command/pipeline/errors.go) to `internal/command/pipeline/`, replacing `{module-path}` with the module path from `go.mod`.
- Risk: a kind declared in another package, or a second sentinel for the same kind, makes `errors.Is` miss it and the error is answered `INTERNAL`.
- Fix: one sentinel per kind in `entity`; entry points use them only through `pipeline`, so they never import the domain.

### Wrap the kind with %w
Declare each error a caller can act on as `fmt.Errorf("%w: {what happened, for the caller}", Err{Kind})` — fixed errors as package variables beside the code that returns them, errors carrying a value at the return — and wrap it again with `%w` when passing it up.
- Violation: `errors.New("not found")` that wraps no sentinel; `fmt.Errorf("plan: %v", err)` with `%v`, which drops the kind; a check `strings.Contains(err.Error(), "not found")`.
- Risk: an error that lost its kind is answered `INTERNAL` with `internal error`, and the caller cannot tell its own mistake from a crash.
- Fix: `%w` at every wrap; check a kind only with `errors.Is(err, pipeline.Err{Kind})` or `pipeline.KindOf(err)`.

### Answer through rpcstatus in every network entry point
Fill and copy [rpcstatus.go](./templates/internal/api/rpcstatus/rpcstatus.go) to `internal/api/rpcstatus/`, with the scenarios of an API variation point's example, and answer every error with `rpcstatus.Of(err)`: over HTTP as `code.HTTPStatus()` with a `google.rpc.Status` body, over gRPC as `status.Error(codes.Code(code), message)`.
- Violation: an entry point with its own `switch` or `map` of kinds; a second `rpcstatus.go` that differs from the first.
- Risk: the HTTP and gRPC forms of one service answer the same kind differently.
- Fix: the HTTP and gRPC variation points copy the same file; `bash test/check-vp-attach.sh` in this repository fails when two variation points add it differently.

### Exit by kind in a CLI entry point
Map an error to the exit code of the table in a command-line entry point: `0` for none, `1` when `pipeline.KindOf(err)` is nil or `pipeline.ErrUnavailable`, `2` otherwise.
- Risk: a script retries a request that can never succeed, or gives up on one that a retry would fix.
- Fix: follow `ExitCode` of [base-plateau-in-go](library/architecture/go/base-plateau-in-go.skill/base-plateau-in-go.skill.md)'s startup entry.

# Check list
- [ ] `internal/domain/entity/kinds.go`, `internal/command/pipeline/errors.go`, and, in a service with a network entry point, `internal/api/rpcstatus/rpcstatus.go` are filled copies of the templates.
- [ ] Every error a caller can act on wraps one sentinel of `kinds.go` with `%w`, and every wrap above it uses `%w`.
- [ ] No code outside `entity` and `pipeline` declares a kind; no code checks an error by its text.
- [ ] Every network entry point answers errors only through `rpcstatus.Of`; a CLI entry point exits by `pipeline.KindOf`.
