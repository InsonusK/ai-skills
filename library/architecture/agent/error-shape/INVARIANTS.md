# One shape of error in every stack — invariants

The anchor document for giving the error a caller can act on one shape in Go, Python, and dotnet (per [[library/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Owner's decisions of 2026-11-04 are folded in and logged in `DECISIONS.md`; the plan is in `STATUS.md`; `check.sh` checks what can be checked.

**Problem being fixed.** A Go error with a kind is one string, and `rpcstatus.Of` answers the whole string. A service therefore cannot keep the technical cause, which would reach the caller: `remote_planner.go` sends the transport's text to the client today. In dotnet and Python the cause is kept (`InnerException`, `__cause__`), but no stack writes it to the log for an error with a kind.

## 1. The error

Every error a caller can act on holds three things.

| Field | Meaning | Reaches |
|---|---|---|
| kind | one row of the table of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md) | the caller, as the code |
| message | what happened, for the caller | the caller, as the text |
| cause | the technical failure behind it, optional | the log only |

| | kind | message | cause | created as |
|---|---|---|---|---|
| Go | `Kind` (a sentinel `errorkind.ErrInvalid`, …) | `Message` | `Cause` | `errorkind.New(kind, message)`, `errorkind.Wrap(kind, message, cause)`, type `*errorkind.Error` |
| Python | `kind` (`Kind.INVALID`, …) | `message` | `__cause__` | a `KindError` subclass, `raise ...Error(...) from cause` |
| dotnet | `Kind` (`ErrorKind.Invalid`, …) | `Message` | `InnerException` | a factory of the module's errors class, `new ErrorKindException(kind, message, inner)` |

- Go's `(*errorkind.Error).Error()` returns `Message` alone, so no print of the error leaks its cause. `Unwrap()` returns the kind and the cause, so `errors.Is` sees both.
- Python and dotnet keep their types as they are: the language holds the cause.
- No Go code wraps a kind with `fmt.Errorf("%w: ...", kind)` any more. `check.sh` fails on it.

## 2. Where it lives

In a shared area, which every layer may import, as dotnet's `Shared` project.

| | Before | After |
|---|---|---|
| Go | `internal/domain/entity/kinds.go` | `internal/shared/errorkind/errorkind.go`: the kinds, `Error`, `New`, `Wrap`, `Kinds`, `KindOf` |
| Python | `src/{package}/domain/entity/kinds.py` | `src/{package}/shared/errors/kinds.py`: `Kind`, `KindError`, `kind_of` |
| dotnet | `src/Shared/Errors/` | unchanged |

- `command/pipeline` keeps re-exporting them under their present names, so an entry point still imports only the pipeline.
- The domain's own errors (`EmptyTitle`, `AlreadyDone`) stay in `domain/entity/errors`, built over the shared types.
- `domain/entity` no longer exports a kind. `check.sh` fails on `domain/entity/kinds.*` and on an import of a kind from `domain.entity`.

## 3. The answer to the caller

- An entry point answers the code of the kind and the message of the kinded error itself, never the text of a wrapper.
  - Go: `rpcstatus.Of` finds the `*errorkind.Error` with `errors.As` and answers its `Message`. The outermost `*errorkind.Error` decides the kind: a kind in its cause is not the error's. A kind returned bare is answered with its name.
  - The CLI prints the same message: `Error: {message}`.
  - Python: `rpcstatus.of` answers `message` of the `KindError` in the chain, as today.
  - dotnet: `RpcStatus` answers `Message`, as today.
- The cause never reaches the caller.
- An error with no kind is answered `INTERNAL` with `internal error`, as today.

## 4. The log

One JSON line per record. Every stack writes the same fields for an error:

| Field | An error with a kind | An error with no kind |
|---|---|---|
| `error` | its message (in Go, with the context wrapped around it on the way up) | its text |
| `cause` | the text of its cause chain, when it has a cause | — |
| `exception` | — | the stack, as today |

- Only the record that ends a failed command writes `cause`; the other records of an error (startup, readiness, a retry of a client's outcome) log errors with no kind, as their text.
- The cause chain is written as text, the messages joined by `: `: Go `Cause.Error()`, Python `str()` along `__cause__`, dotnet `Message` along `InnerException`.
- No stack is written for an error with a kind: a cause names what failed, and a stack of a rejected request is noise.
- The logging variation point writes the same fields: it changes how the payload is written, not the error.

## 5. Scope

| In | Out |
|---|---|
| `library/develop/{core,go,python,dotnet}/error-kinds*` | — (the DB variation points joined in wave 5, once #281–#284 merged) |
| `base-plateau-in-*`, `base-plateau-extension-in-*` | |
| `vp-{http,grpc,cli}-api-in-*`, `vp-health-in-*`, `vp-logging-in-*` | |
| `vp-{http,grpc}-client-in-*`, `vp-client-retry-in-*` | |
| `vp-postgres-in-*`, `vp-unit-of-work-in-*` (wave 5) | |

- A file the dotnet DB examples share with their base gets the same new content as the base, and nothing else in them changes. Otherwise `check-vp-attach.sh` fails on the copy.

## 6. Per file

- **A shared copy** (the same content in several examples): edit it once in the lowest example, then write the new content over every copy that had the old one. `check-vp-attach.sh` proves the copies agree.
- **A template** gets the same change as the example it is compared with. `check-templates.sh` proves they agree.
- **A skill file** that names a moved path, a log field, or the way to build an error states the new one, and restates nothing [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md) says.
- **Scenarios:**
  - Each stack's `rpcstatus` gets one: an error with a cause answers its message alone.
  - Each stack's logging gets one: such an error logs `error` and `cause`, and no `exception`.
  - In Go, every call that builds an error with a kind is `errorkind.New` or `errorkind.Wrap`.

## 7. Done when

- `check.sh` passes: it bundles the repository's checks and the checks of this migration.
- Every example in scope passes its tests and its type check: `go test ./...` and `go vet`, `make test` and `make lint`, `dotnet test`.
- `test/test.sh` and `library/testing/agent/check.sh` pass.
