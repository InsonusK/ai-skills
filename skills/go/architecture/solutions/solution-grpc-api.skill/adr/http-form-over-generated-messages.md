---
name: http-form-over-generated-messages
description: How the HTTP form of the service's grpc contract is served once the proto file exists
problem: With a proto contract the HTTP API could be served by a generated grpc-to-HTTP gateway or by the hand-written `net/http` handlers the baseline already has.
decision: Hand-written `net/http` handlers that read and write the generated messages through `protojson`; no gateway runs.
tags:
  - solution/grpc-api
  - concern/documentation
  - concern/documentation/adr
  - stack/go
---

# Problem

`api-develop` fixes the HTTP form of a grpc contract — proto-JSON, 201/202/200 by outcome, the canonical error table, `google.rpc.Status`. Something has to serve it in a service whose baseline is a `net/http` adapter.

# Selected variant

**Selected variant:** [[#Hand-written handlers over the generated messages]]
- Decided by the catalog's owner, 2026-10-12.

# Searched variants

## Hand-written handlers over the generated messages

**Selected.**

### Description

The baseline's `Server`, its route list, and its docs routes stay. Each handler decodes into the `rpc`'s request message with `protojson`, calls the domain service, and encodes the response message. `protoc-gen-openapiv2` generates the spec from the same annotations.

### Benefits

- No new runtime dependency and no second server inside the process.
- The success status is one argument of `writeMessage`.
- The adapter keeps the shape `api-develop-in-go` describes, so its rules apply unchanged.

### Costs

- A route's method and path are typed twice — in the annotation and in `contractRoutes()`. The operations scenario exists to catch the difference.
- Query and path parameters are filled into the request message by hand.

## grpc-gateway

### Description

`protoc-gen-grpc-gateway` generates a reverse proxy; the HTTP server mounts its mux, which calls the grpc server implementation in-process.

### Benefits

- Routes, parameters, and shapes all come from the annotations; no route list and no scenario for it.

### Costs

- 201 and 202 need a response modifier per method.
- The baseline's adapter is thrown away rather than extended, and `api-develop-in-go` would need a second mode.
