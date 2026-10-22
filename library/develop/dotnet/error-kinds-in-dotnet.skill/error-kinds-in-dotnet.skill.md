---
name: error-kinds-in-dotnet
description: The error kinds in dotnet — an ErrorKind enum and an ErrorKindException declared once in the Shared project, thrown through one factory method per error in each module's errors class, read by entry points only through ErrorKindException.KindOf, with exceptions rather than Result types as the one channel of errors
whenToUse: when a dotnet command, domain service, entity, or service raises an error a caller must act on, when a dotnet entry point turns an exception into its answer, or when you review either
updated: 20261021
tags:
  - stack/dotnet
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
adr:
  - adr/exceptions-carry-the-kind.md
---

# Goal
- **The error kinds in dotnet** - A dotnet application that keeps every property of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md); this skill details that skill's implementation for dotnet and restates none of its rules.

# Core Principle
- **One channel of errors** - An error a caller can act on is an exception carrying its kind, like every other failure; entry points and pipeline behaviors catch one thing. Decision recorded in [exceptions-carry-the-kind](./adr/exceptions-carry-the-kind.md).
- **Two fixed files** - The kinds and the exception are copied from templates and never edited by an application: every kind the table names already exists.

```
src/Shared/Errors/ErrorKind.cs                          Invalid, NotFound, ... - the kinds
src/Shared/Errors/ErrorKindException.cs                 the exception + KindOf
src/Modules/{Module}/{Module}.Domain/Errors/{Module}Errors.cs   the module's errors, one factory each
src/App/App.Host/Api/Core/Errors/RpcStatus.cs                kind → RpcCode (google.rpc.Code) → HTTP status, message
```

# Scope
- Covers declaring, raising, and reading a kind, and the one table network entry points answer it by; a one-shot entry's exit code is [base-plateau-in-dotnet](library/architecture/dotnet/base-plateau-in-dotnet.skill/base-plateau-in-dotnet.skill.md)'s `EntryRunner.ExitCode`.

# Rule

## MUST

### Copy the kinds from templates
Copy [Shared.csproj](./templates/src/Shared/Shared.csproj), [ErrorKind.cs](./templates/src/Shared/Errors/ErrorKind.cs), and [ErrorKindException.cs](./templates/src/Shared/Errors/ErrorKindException.cs) to `src/Shared/`, and reference the project from every `{Module}.Domain` and from `BuildingBlocks`.
- Risk: a kind declared in a module, or a second exception type for the same purpose, is missed by `KindOf` and the error is answered `INTERNAL`.
- Fix: one `ErrorKind` and one `ErrorKindException` in `Shared`; `Shared` references no package, so the domain stays free of every framework.

### One factory method per error
Declare each error a caller can act on as a static method of `{Module}Errors` in `{Module}.Domain/Errors/` returning `new ErrorKindException(ErrorKind.{Kind}, "{what happened, for the caller}")`, and raise it with `throw {Module}Errors.{Error}()`.
- Violation: `throw new ErrorKindException(ErrorKind.Invalid, "item title is empty")` written inside an entity; a `static readonly` exception instance thrown again and again; `throw new ArgumentException(...)` for a business rule.
- Risk: the same error written twice drifts in kind or text; a shared instance has its stack overwritten by every throw; an exception without a kind is answered `INTERNAL`.
- Fix: `TodoErrors.EmptyTitle()`; a service's error (a record not found) is a method of the same class or of the service's own errors class, still built on `ErrorKindException`.

### Keep the kind when wrapping
Wrap an exception only as the inner exception of the new one (`new InvalidOperationException("plan", error)`), and never catch an `ErrorKindException` to throw a new exception without it.
- Violation: `catch (ErrorKindException e) { throw new Exception(e.Message); }`; a check `error.Message.Contains("not found")`.
- Risk: an error that lost its kind is answered `INTERNAL`, and the caller cannot tell its own mistake from a crash.
- Fix: pass it as `innerException`; read a kind only with `ErrorKindException.KindOf(error)`, which walks the chain of inner exceptions.

### Entry points read the kind, never the error
Map an exception to an answer in an entry point only by `ErrorKindException.KindOf(error)`: one `switch` over `ErrorKind?` in the one place of the stack that holds the table, `null` meaning a failure of the service.
- Violation: `catch (EmptyTitleException)` in an endpoint; a second `switch` over the kinds in another entry point.
- Risk: every new rule error must be added to every entry point, and two entry points answer one kind differently.
- Fix: one table per stack (`EntryRunner.ExitCode` for exit codes; `RpcStatus` for codes and statuses).

### Answer through RpcStatus in every network entry point
Copy [RpcStatus.cs](./templates/src/App/App.Host/Api/Core/Errors/RpcStatus.cs) to `src/App/App.Host/Api/Core/Errors/` with the scenarios of an API variation point's example (`features/RpcStatus.feature` and its steps), and answer every exception with `RpcStatus.Of(error)`: over HTTP as `code.HttpStatus()` with a `google.rpc.Status` body, over gRPC as `new RpcException(new Status((StatusCode)code, message))`.
- Violation: an entry point with its own `switch` over `ErrorKind`; a second `RpcStatus.cs` that differs from the first; the message of an exception with no kind sent to a client.
- Risk: the HTTP and gRPC forms of one service answer the same kind differently; a connection string in an exception's message reaches the client.
- Fix: the HTTP and gRPC variation points copy the same file; `bash test/check-vp-attach.sh` in this repository fails when two variation points add it differently. `Of` answers an error with no kind `INTERNAL` "internal error", and a wrapped error with the message of the exception that carries the kind.

# Check list
- [ ] `src/Shared/Shared.csproj` and `src/Shared/Errors/{ErrorKind,ErrorKindException}.cs` are copies of the templates.
- [ ] Every error a caller can act on is a factory method of a `{Module}Errors` class returning an `ErrorKindException` of one kind of the table.
- [ ] No code declares another kind type, throws a shared exception instance, or checks an error by its text.
- [ ] Every wrap keeps the original as the inner exception; entry points read kinds only through `ErrorKindException.KindOf`.
- [ ] In a service with a network entry point, `src/App/App.Host/Api/Core/Errors/RpcStatus.cs` is a copy of the template, and every such entry point answers errors only through `RpcStatus.Of`.
