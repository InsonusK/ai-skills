# Error kind

An **error kind** is one sentinel error that a whole family of errors carries, so a caller asks "is this a *kind* of error?" instead of "is this *exactly* that error?". The application has the eleven kinds of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md) — `ErrInvalid`, `ErrNotFound`, `ErrConflict`, … — and "no kind" for a failure of the service.

## Why it exists

An entry point has to answer differently to "your input is wrong", "this does not exist", and "something broke": exit code `2` vs `1` for the startup command, HTTP `400` / `404` / `500`, gRPC `INVALID_ARGUMENT` / `NOT_FOUND` / `INTERNAL`. But an entry point must not import the domain, and must not list every rule error (`ErrEmptyTitle`, `ErrDuplicateTitle`, ...) — that list would change with every new rule.

With kinds, every rule error carries one marker, and the entry point checks only the marker. The message is the error's own, for the caller; what broke inside the service stays apart, as the cause, for the log.

## How it works

1. `internal/shared/errorkind/errorkind.go` declares one sentinel per kind: `var ErrInvalid = errors.New("invalid")`, `var ErrNotFound = errors.New("not found")`, ... — and the error a caller can act on, `*errorkind.Error`, with three fields: `Kind`, `Message` for the caller, and `Cause`, the technical failure behind it, for the log.
2. Each rule error is built with `errorkind.New`: `ErrEmptyTitle = errorkind.New(errorkind.ErrInvalid, "item title is empty")`. A service that tells a dependency's failure in the application's terms keeps that failure as the cause: `errorkind.Wrap(errorkind.ErrUnavailable, "the todo service is unavailable", err)`.
3. `Error()` is the message alone, so a print of the error never shows its cause. `Unwrap` gives the kind and the cause, so `errors.Is(err, errorkind.ErrInvalid)` is true for every rule error of that kind — and for one wrapped again later with `%w`.
4. The `pipeline` package re-exports the kinds — `pipeline.ErrInvalid = errorkind.ErrInvalid`, ... — and offers `KindOf(err)`, so an entry point checks a kind through the package it already imports.

@import "./diagrams/error-kind.mmd" {as="mermaid"}

## How it is structured

| Part | Where |
|---|---|
| one sentinel per kind, `Error`, `New`, `Wrap`, `KindOf`, `Of` | `internal/shared/errorkind/errorkind.go` (template of `error-kinds-in-go`) |
| one error per business rule, built by `errorkind.New` | `internal/domain/entity/errors.go` |
| the kinds re-exported, `Kinds`, `KindOf` | `internal/command/pipeline/errors.go` (template of `error-kinds-in-go`) |
| mapping kind → answer | the exit code: `entry.ExitCode`; HTTP and gRPC: `internal/api/core/rpcstatus`, which answers the `Message` |
| the error in the log: `error` and `cause` | `logging.Error` in `internal/logging` |

## Example

```go
// internal/api/core/entry/entry.go
switch kind := pipeline.KindOf(err); {
case err == nil:
    return ExitOK
case kind == nil, kind == pipeline.ErrUnavailable:
    return ExitFailure // 1: a retry may change the outcome
default:
    return ExitRejected // 2: the request was rejected
}
```

## Related concepts

- [Entry runner](./entry-runner.md) — maps the kind to an exit code.
- [Handler contract](./handler-contract.md) — handlers return these errors.

## Sources

- `errors.Is`: https://pkg.go.dev/errors#Is
- Go blog, working with errors in Go 1.13 (wrapping with `%w`): https://go.dev/blog/go1.13-errors
- Google API design guide, errors (the codes of the kinds): https://cloud.google.com/apis/design/errors
