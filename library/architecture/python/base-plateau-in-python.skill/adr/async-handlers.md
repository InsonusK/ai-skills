---
name: async handlers
description: Whether the Python base plateau's handlers and entry points are async (asyncio) or synchronous
problem: The handler contract is the one shape every entry point and decorator is written against; an HTTP server, a gRPC server, and a listener run in one process beside each other; must a handler be a coroutine or a plain function?
decision: async def handle on every handler; every entry point an asyncio task run by run_all, stopped by an asyncio.Event.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The handler contract cannot change later without rewriting every command. The entry points the catalog brings — FastAPI over uvicorn, `grpc.aio`, a Kafka consumer — must run together in one process and stop together, as the runner of the entry points requires.

# Selected variant
[[#async]]
- Owner decision, 2026-10-22.

# Searched variants

## async

**Selected.**

### Description
`async def handle(self, command) -> TResult`; `run_all` runs each entry point as an asyncio task and sets an `asyncio.Event` to stop them; a step module runs a coroutine with `asyncio.run`.

### Benefits
- FastAPI, uvicorn, and `grpc.aio` are asyncio-native: a handler is awaited by the route with no thread pool between them.
- One event loop runs every entry point; stopping them is setting one event, as Go cancels one context.
- The request id is a `contextvars.ContextVar`, copied into each task, so it follows a request with no parameter.

### Costs
- A CLI pays one `asyncio.run` for a single command.
- A blocking call (a synchronous driver) inside a handler stalls every entry point; it must be an async client or run in `asyncio.to_thread`.

## sync

### Description
`def handle(self, command) -> TResult`; servers on threads.

### Benefits
- Plain functions; no event loop in a CLI.

### Costs
- FastAPI runs every handler in a thread pool; gRPC needs the threaded server; the runner joins threads instead of awaiting tasks.
- Moving to async later rewrites every command and every step.
