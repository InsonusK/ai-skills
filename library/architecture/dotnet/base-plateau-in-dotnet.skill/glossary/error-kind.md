# Error kind

An **error kind** (`ErrorKind` in `src/Shared/Errors`) tells an entry point what the caller can do about an error — fix the input, pick another name, retry later — without the entry point knowing the error itself. The meaning of each kind and its answer per transport is the table of skill `error-kinds`; the dotnet rules are `error-kinds-in-dotnet`.

## How it works

```
Item ctor ── throw TodoErrors.EmptyTitle() ──► ErrorKindException(Invalid, "item title is empty")
   ▲                                                   │ passes up unchanged (or as an inner exception)
PlanItemsHandler ◄── LoggingMiddleware ◄── ISender ◄── entry point
                                                       │
                                ErrorKindException.KindOf(error) → Invalid → exit 2 / HTTP 400
```

| Piece | Role |
|---|---|
| `ErrorKind` | the ten kinds; `null` (no kind) is a failure of the service |
| `ErrorKindException` | the exception carrying a kind and the caller's message |
| `{Module}Errors` | one factory method per error of the module: kind and text written once |
| `ErrorKindException.KindOf` | the kind of an exception or of its inner exceptions |

## Example

```csharp
public static ErrorKindException AlreadyDone() =>
    new(ErrorKind.FailedPrecondition, "item is already done");

// in the entity
if (Done) throw TodoErrors.AlreadyDone();
```

## Related concepts

- [Entry runner](./entry-runner.md) — `ExitCode` maps the kind to `0`/`1`/`2`.

## Sources

- Best practices for exceptions: https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions
- google.rpc.Code: https://github.com/googleapis/googleapis/blob/master/google/rpc/code.proto
