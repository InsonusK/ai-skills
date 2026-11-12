---
name: minimal api with binding and one error middleware
description: How a dotnet HTTP route is written — Minimal API with binding and TypedResults, errors answered by one middleware; or a generic adapter like Go's Endpoint
problem: Every route turns a request into a command and the result or the error into a response; should a dotnet route be written the Minimal API way, with binding, or through a generic adapter with decode and encode functions as the Go HTTP variation point does?
decision: Minimal API organized like controllers — one {Resource}Handler class per resource mapping its group; a route binds its request record, sends one command, returns TypedResults; ErrorAnswers in Pipeline/ answers every exception and every body-less error status as google.rpc.Status; ThrowOnBadRequest makes a body that cannot be bound one of them.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Go has no binding: its `Endpoint(handler, decode, encode, status)` decodes, calls, and answers errors in one generic function. ASP.NET Core binds a request record and serializes a result by itself, and answers a binding failure, an unknown route, or a wrong content type with its own empty 400/404/415 — which would break "every error body is `google.rpc.Status`".

# Selected variant
[[#Binding and one error middleware]]
- Owner decision, 2026-10-21: controllers-like classes over Minimal API (decided with B1), routes written the Minimal API way.

# Searched variants

## Binding and one error middleware

**Selected.**

### Description
`items.MapPost("/plan", PlanItemsAsync)` with `Task<Ok<PlanItemsResponse>> PlanItemsAsync(PlanItemsRequest body, CancellationToken ct)`. `ErrorAnswers` catches `BadHttpRequestException` (400 INVALID_ARGUMENT), `ErrorKindException` (the table), anything else (500 "internal error", logged with its stack), and fills the body of an error status ASP.NET set itself (404, 405; 415 becomes 400).

### Benefits
- A route is three lines, read by any dotnet developer as usual Minimal API code.
- The error answers live in one copied file; no route can answer differently.
- `TypedResults` give each route its success status in its signature.

### Costs
- A route can still, by mistake, catch an exception or return `Results.BadRequest`; the skill forbids it, no compiler stops it.
- A binding failure's message is ASP.NET's ("Failed to read parameter ..."), mapped to a short text only for invalid JSON and a missing body.

## Generic adapter as in Go

### Description
`HttpEndpoint.Handle(sender, decode, encode, successStatus)`; `decode` reads the body itself.

### Benefits
- The same shape as the Go variation point; errors handled inside the adapter.

### Costs
- Bypasses binding and `TypedResults`, which every dotnet developer expects; more code per route (a decode and an encode function).
- ASP.NET's own 404/405/415 still need a middleware for the `google.rpc.Status` body.
