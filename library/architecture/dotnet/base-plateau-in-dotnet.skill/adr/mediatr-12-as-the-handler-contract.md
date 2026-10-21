---
name: mediatr 12 as the handler contract
description: What the one handler contract of the dotnet base plateau is — MediatR 12, MediatR 13+, or an own interface
problem: Every dotnet handler must satisfy one contract that behaviors wrap and entry points call; should it be MediatR, and which version, given that MediatR 13 and later are commercial?
decision: MediatR 12.5.0 (Apache-2.0), the last version before the commercial license — IRequestHandler as the contract, IPipelineBehavior as the decorator, ISender as the way entry points reach a handler; MediatR.Contracts 2.0.1 in the Interfaces project.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The Go base plateau defines its own `Handler[TCommand, TResult]` and wraps handlers with functions. In dotnet the same role is usually played by MediatR, which the previous dotnet plateau used (12.4.1). Since version 13 (2025) MediatR is commercial with a license key; 12.5.0 is the last Apache-2.0 release.

# Selected variant
[[#MediatR 12.5.0, pinned below 13]]
- Owner decision, 2026-10-21.

# Searched variants

## MediatR 12.5.0, pinned below 13

**Selected.**

### Description
`IRequest<TResult>` on the command record, `IRequestHandler<TCommand, TResult>` on the handler, `IPipelineBehavior<TCommand, TResult>` per cross-cutting step, `AddMediatR` with an assembly scan in `// [handlers]`.

### Benefits
- What dotnet developers already know; the previous dotnet catalog's behaviors port over.
- Handlers are found by an assembly scan, so a new command needs no line in `Program.cs`.
- `MediatR.Contracts` alone lets the Interfaces project declare commands without the mediator.

### Costs
- No security fixes or new features after 12.5.0; a move to an own contract later touches every handler's base interface (a mechanical rename).
- A reflection-based dispatch per call, measured in hundreds of nanoseconds — below everything else a request does.

## MediatR 13+

### Description
The current MediatR with a license key.

### Benefits
- Maintained.

### Costs
- A commercial license per organization using the catalog; a key to manage in every service.

## Own handler interface

### Description
`IHandler<TCommand, TResult>` and decorators registered by hand, as in Go.

### Benefits
- No dependency; the same shape as the Go base plateau.

### Costs
- Decorator registration (Scrutor or hand-written wrappers per handler) is code every application carries; dotnet developers expect MediatR.
