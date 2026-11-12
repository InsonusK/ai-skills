---
name: untyped middleware with typed adapter
description: How a list of command middleware is written in Go, where a generic function cannot stand in a slice
problem: The command registry wraps every handler with one list of middleware given in main.go. A handler is a generic Handler[TCommand, TResult]; a middleware written as a generic function over it cannot be put into a []Middleware, because Go has no generic function values. How is a middleware written so it stands in the list and still wraps typed handlers?
decision: A middleware is the non-generic pipeline.Middleware, func(ctx, name, cmd any, next Next) (any, error); pipeline.Decorate[TCommand, TResult] builds the chain once per handler and is the only place that asserts the command and the result back to their types.
tags:
  - stack/go
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`WithLogging[TCommand, TResult](name, logger, next Handler[TCommand, TResult]) Handler[TCommand, TResult]` wraps one handler at a time, so `main.go` had one line per command and decorator. A list given once to the registry needs one type for every middleware, whatever command it wraps.

# Selected variant
[[#Untyped middleware, typed adapter]]
- Owner decision, 2026-10-24: the command registry with decorator lists for every stack; in Go the decorator is untyped and a typed adapter hides the assertions.
- Owner decision, 2026-10-25: the decorators are named middleware, one package each.

# Searched variants

## Untyped middleware, typed adapter

**Selected.**

### Description
`type Middleware func(ctx context.Context, name string, cmd any, next Next) (any, error)`. `Decorate("PlanItems", handler, middlewares)` builds the chain from the last middleware to the first around the handler and returns a `Handler[TCommand, TResult]`; a middleware that passes on a value of another type gets an error instead of a panic.

### Benefits
- One list in `main.go`; a middleware is a plain function, written and tested without type parameters.
- A filtered middleware asks for a mark with `cmd.(Restricted)`, the way Go selects by behaviour.
- Entry points and handlers stay fully typed.

### Costs
- A middleware sees the command and the result as `any`; changing the command's type is caught at run time, not at build time.
- One closure per middleware per handler, built once at start.

## A generic middleware per command in main.go

### Description
Keep `WithLogging[TCommand, TResult]` and wrap each handler in `main.go`.

### Benefits
- Fully typed; no adapter.

### Costs
- One line per command and middleware; a middleware variation point edits every command's lines, and a new command repeats them all.

## A middleware interface with a generic method

### Description
`type Middleware interface { Wrap[TCommand, TResult any](Handler[TCommand, TResult]) Handler[TCommand, TResult] }`.

### Benefits
- Typed inside the middleware.

### Costs
- Not valid Go: a method cannot have type parameters.
