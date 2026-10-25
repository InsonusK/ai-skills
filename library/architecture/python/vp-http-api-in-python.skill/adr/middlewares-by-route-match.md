---
name: middlewares by route match
description: How the application's middlewares wrap the contract's routes of the Python HTTP entry point, and only them
problem: An API middleware - authentication, a rate limit - must wrap every request of the contract's routes, inside the request log and the error answers, and leave the docs open; FastAPI has no public hook that wraps the routes of included routers, and since 0.161 it includes a router lazily, so its routes are not in the application's route list to be wrapped one by one. Where do the middlewares go?
decision: One ASGI layer, ContractMiddlewares, inside AnswerFailuresMiddleware and RequestLogMiddleware, that matches the request against the contract routers' routes (BaseRoute.matches) and runs a matching request through the middlewares, each a Starlette BaseHTTPMiddleware dispatch function (request, call_next); any other request goes past them.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The base-plateau-extension skill puts the API middleware around the contract's routes only, inside the fixed request log and error answers. In Go every route is wrapped as it is registered on the `ServeMux`. In FastAPI the routes belong to `APIRouter`s that the resources build, and the application includes them; the framework offers middlewares for the whole application, dependencies for routes, and a custom route class chosen when a router is created.

# Selected variant
[[#One ASGI layer matching the contract's routes]]
- Owner decision, 2026-10-24: entry decorators as an ordered list given to each API's entry, shown on an authentication example; the docs stay open.

# Searched variants

## One ASGI layer matching the contract's routes

**Selected.**

### Description
`ContractMiddlewares(app, routes, middlewares)` is added with `app.add_middleware` before `AnswerFailuresMiddleware`, so it stands inside it. It builds the chain once — the last middleware wraps the application first — and for each HTTP request asks every contract route `route.matches(scope)`; a match (full, or the path with another method) runs the chain, anything else the application alone.

### Benefits
- Public APIs only: `add_middleware`, `BaseRoute.matches`, `BaseHTTPMiddleware`.
- A middleware has the familiar shape of `@app.middleware("http")`, and raising a `KindError` is answered by `AnswerFailuresMiddleware` as `google.rpc.Status` and logged by the request log.
- A `ContextVar` set before `call_next` (the caller's identity) is seen by the route and its command.

### Costs
- Every request is matched against the contract's routes once more than the router does.
- `BaseHTTPMiddleware` runs the rest of the request in a task of its own; a `ContextVar` set by the route is not seen by the middleware after `call_next`.

## Replace each route's ASGI application

### Description
After including the routers, wrap `route.app` of every `APIRoute` of the application.

### Benefits
- No matching; each route carries its own chain.

### Costs
- Since FastAPI 0.161 an included router is an `_IncludedRouter`, not its routes, so the application's route list holds none to wrap; the attribute is the framework's internal.

## Router dependencies

### Description
Give the contract's routers `dependencies=[Depends(authenticate)]`.

### Benefits
- FastAPI's own way of running code before a route.

### Costs
- A dependency runs before the route but cannot act after it, so it is no decorator: a step that needs the response (a metric, a header) has no place.
- The resources build their routers; adding a dependency to them edits every resource.

## Middlewares around the whole application

### Description
Add each middleware with `app.add_middleware`.

### Benefits
- The framework's own way, no matching.

### Costs
- The docs and an unknown path go through authentication too; Swagger UI cannot load without a token.
