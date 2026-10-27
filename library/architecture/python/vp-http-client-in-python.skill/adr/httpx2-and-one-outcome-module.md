---
name: httpx2 and one outcome module
description: Which library a Python client of an HTTP API is built on - httpx2, aiohttp, or the standard library - and where the outcomes every client reports live
problem: A Python service needs an async HTTP client with a timeout, a way to answer every request inside the process for the mock, and outcomes the service over the adapter catches; which library, and one outcome module for HTTP and gRPC or one per transport?
decision: The fixed code is built on httpx2 (2.16.0), the library the catalog's HTTP API scenarios already use through FastAPI's test client; the mock is httpx2.MockTransport over the adapter's emulator function; asyncio.timeout bounds the whole call. The outcomes (ApiUnavailableError, ApiStatusError, StatusCode) live in infrastructure/client/core/outcomes.py for every transport, as dotnet's Client.Abstractions does.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The Go client is built on `net/http` and an `http.Handler` emulator; the dotnet client on `HttpClient` and an `HttpMessageHandler` emulator. Python's standard library has no async HTTP client.

# Selected variant
[[#httpx2, outcomes in one module]]
- Chosen while the owner was away (2026-10-27), for review with the PR: the catalog already pins httpx2 for the HTTP API's scenarios, so no second HTTP library enters a project that attaches both; the shared outcome module follows the owner's dotnet decision (one-outcome-project-for-every-client).

# Searched variants

## httpx2, outcomes in one module

**Selected.**

### Description
`httpx2.AsyncClient` per API with no timeout of its own, `asyncio.timeout(config.timeout)` around the request and the read; mocked, `httpx2.MockTransport(emulator)`, where the emulator is a function from `httpx2.Request` to `httpx2.Response`.

### Benefits
- One HTTP library in the catalog; `MockTransport` runs the same client code against the emulator with no server.
- The service over an adapter is the same for HTTP and gRPC.

### Costs
- A dependency of the application, where Go and dotnet need none beyond the runtime.
- Go keeps its outcomes per transport package; Python and dotnet share one.

## aiohttp

### Description
`aiohttp.ClientSession`, a test server of `aiohttp.test_utils` for the mock.

### Benefits
- Widely used, fast.

### Costs
- A second HTTP library beside httpx2; no transport to swap for the mock, so the mock needs a server.

## The standard library

### Description
`urllib.request` run in a thread with `asyncio.to_thread`.

### Benefits
- No dependency.

### Costs
- A thread per call, no connection reuse, and no transport to swap for the mock.
