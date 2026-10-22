---
name: fastapi with binding and error handlers
description: Which HTTP framework the Python HTTP variation point is built on, and how its routes decode requests and answer errors
problem: The HTTP entry point must decode each route's request into a type of its own, answer every error - of a command, of decoding, of the framework itself - as one google.rpc.Status body, log every request, serve the committed contract, and stop on the entry's event; which framework, and how much of that does it do?
decision: FastAPI, with pydantic request and response models bound by the framework, its own OpenAPI generation off, exception handlers for KindError, RequestValidationError, and the framework's HTTPException, and two ASGI layers - AnswerFailures for every other exception and RequestLog outside it - served by uvicorn on a socket the entry opens itself.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Go writes the HTTP entry point on `net/http` with one generic `Endpoint`; dotnet uses Minimal API binding and one middleware. Python needs a framework that is asyncio-native ([async-handlers](library/architecture/python/base-plateau-in-python.skill/adr/async-handlers.md)) and lets every error body be `google.rpc.Status`.

# Selected variant
[[#FastAPI with binding]]
- Owner decision, 2026-10-22: FastAPI.

# Searched variants

## FastAPI with binding

**Selected.**

### Description
A route declares a pydantic model as its body and returns a response model; FastAPI decodes, validates, and encodes. `add_error_answers` registers handlers for `KindError` (by `rpcstatus`), `RequestValidationError` (400 `INVALID_ARGUMENT`, saying whether the body is not JSON, not declared as JSON, or misses a field), and `HTTPException` (404 `NOT_FOUND`, 405 `UNIMPLEMENTED`). Starlette answers any other exception outside every middleware, so `AnswerFailures`, an ASGI layer inside `RequestLog`, answers it by its kind - a `KindError` wrapped in another exception included - and logs a failure with no kind as `http request failed`.

uvicorn serves on a socket the entry opens itself, so a port already taken is an `OSError` of the entry and not uvicorn's `sys.exit`; its signal capture is switched off, since the signals belong to `main.py`.

### Benefits
- The framework most Python teams know; binding and response models in the route's signature.
- Every answer has the same body as Go's and dotnet's, the framework's own 404 and 405 included.

### Costs
- Three exception handlers and two ASGI layers to keep in the copied pipeline.
- FastAPI's generated OpenAPI and its `/docs` are switched off; the committed contract is served instead.

## Starlette with pydantic

### Description
Routes and decoding written once by hand over Starlette, as Go's `Endpoint`.

### Benefits
- No hidden behaviour to switch off.

### Costs
- Binding, and the request types in a route's signature, written by the catalog.

## aiohttp

### Description
aiohttp's own server and routing.

### Benefits
- A mature asyncio server.

### Costs
- No binding by models; OpenAPI and Swagger UI only through plugins; less often chosen for new code.
