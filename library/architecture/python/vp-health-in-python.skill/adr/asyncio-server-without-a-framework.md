---
name: asyncio server without a framework
description: What the Python management entry point serves its three health endpoints on - asyncio's own streams, or the HTTP variation point's Starlette and uvicorn
problem: The management entry point answers three GET endpoints on a port of its own and attaches to any Python service - one with FastAPI, a gRPC-only one, one with no API. Go serves it on net/http and dotnet on its web host, both part of the platform; Python's standard library has no asyncio HTTP server. Does the entry point bring a web framework, or answer on asyncio alone?
decision: asyncio alone (owner decision, 2026-10-26) - asyncio.start_server on one dual-stack socket, one request per connection, the request line and the headers read with a timeout, a JSON answer with Connection close; the server closed and its requests in flight awaited up to 10 seconds on stop.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The health endpoints are part of every long-running service of the catalog, whatever its entry points. Attaching them must not change the service's dependencies more than the endpoints need.

# Selected variant
[[#asyncio streams]]
- Owner decision, 2026-10-26.

# Searched variants

## asyncio streams

**Selected.**

### Description
`Server.start()` opens one socket for IPv6 and IPv4 and serves it with `asyncio.start_server`. A connection is one request: the request line, the headers read and dropped (a probe sends no body), the answer - `/health/live`, `/health/ready`, `/health` for `GET` and `HEAD`, 404 and 405 otherwise, 400 for a request line that is not HTTP - and the connection closed. On stop the server closes, waits for the connections in flight, and aborts them after 10 seconds.

### Benefits
- No dependency: `attach.allow` names only `main.py` and the readme, as in Go; a gRPC-only service does not install a web framework for its probes.
- About a hundred lines, the same in every service, covered by scenarios through real sockets and checked with `curl`.

### Costs
- An HTTP parser of the template's own: it serves probes and people with `curl`, not keep-alive, chunked bodies, or HTTP/2 - none of which a probe uses.

## Starlette and uvicorn

### Description
The stack of the HTTP variation point, as a second application on the management port.

### Benefits
- A tested HTTP implementation; the same graceful stop as the HTTP API.

### Costs
- Two dependencies for every service, a change of `pyproject.toml` on attach, and their versions bound to the HTTP variation point's when both are attached.
