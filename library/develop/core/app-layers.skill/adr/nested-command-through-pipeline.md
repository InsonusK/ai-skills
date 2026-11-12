---
name: nested command through pipeline
description: How a command runs another command — through that command's handler as the command pipeline wrapped it, or by calling the handler or its logic directly
problem: The layer table let a command call other commands "through their handlers" without saying whether that handler is the one wrapped by the command middleware; in Go and Python a handler could take the plain handler (NewAddItemHandler) and skip logging, the access check, and the unit of work, while dotnet's ISender always runs the pipeline behaviors.
decision: A command runs another command only through the handler the command pipeline built — from the command registry in Go and Python, through ISender in dotnet — so every command middleware wraps the nested command as it wraps one started by an entry point; logic two commands share moves into a domain service.
tags:
  - stack
  - concern/architecture
  - concern/coding
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A command that imports items reuses `AddItem` for each one. Taken as `NewAddItemHandler(lists)`, the nested call writes no log line, passes no access check, and opens or joins no unit of work; taken from the registry, it gets all of them. Without a rule each stack — and each handler — chose for itself.

# Selected variant
[[#Through the command pipeline]]
- Owner decision, 2026-10-26: a nested command goes through the middleware and no other way.

# Searched variants

## Through the command pipeline

**Selected.**

### Description
The outer handler takes the nested command's wrapped handler as a constructor parameter, which the command registry builds first (Go `pipeline.Handler[TCommand, TResult]`, Python `Handler[TCommand, TResult]`); dotnet sends the command through `ISender`.

### Benefits
- One operation behaves the same whoever starts it: its log line, its access check by its own mark, its unit of work (the outermost opens it, a nested one joins).
- A middleware added later reaches nested commands with no change to the handlers.

### Costs
- Every middleware runs once per nesting level, so it must be correct when nested — a unit of work joins instead of opening a second one, and the log shows the outer and the nested command as two lines.
- In Go and Python the registry builds a nested handler before the handler that uses it; a cycle between two commands cannot be built.

## Plain handler or shared logic

### Description
The outer handler builds the nested handler itself or calls a function both share; only the outer command passes the middleware.

### Benefits
- No repeated middleware work; no order in the registry.

### Costs
- The nested operation skips its access check and its log line, and in Go and Python it escapes the unit of work, so a failure leaves part of its writes.
- dotnet and the other stacks behave differently for the same design.
