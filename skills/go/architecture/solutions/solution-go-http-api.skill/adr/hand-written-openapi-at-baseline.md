---
name: hand-written-openapi-at-baseline
description: Where the OpenAPI spec of the baseline HTTP API comes from while the service has no proto contract
problem: `api-develop` requires a published spec for every HTTP endpoint; the baseline service has `net/http` handlers and no `.proto` file, and `net/http` holds no types a spec could be built from.
decision: The baseline's spec is a hand-written `docs/integration/api/{contract-name}/openapi.json`; `solution-grpc-api` replaces it by a proto file and the spec generated from it.
tags:
  - solution/go-http-api
  - concern/documentation
  - concern/documentation/adr
  - stack/go
---

# Problem

Every plateau of this catalog serves HTTP, and only those with VP1 `GrpcApi` have a proto file to generate a spec from. The baseline needs a spec too.

# Selected variant

**Selected variant:** [[#Hand-written spec at the baseline]]
- Decided by the catalog's owner, 2026-10-12.

# Searched variants

## Hand-written spec at the baseline

**Selected.**

### Description

`solution-go-http-api` creates `openapi.json` in OpenAPI 3 beside `embed.go`. The handlers use their own request and response structs. When `solution-grpc-api` is composed, the proto file takes the spec's place and the handlers move to the generated messages.

### Benefits

- An HTTP-only service needs no protobuf toolchain: no `buf`, no generators, no `gen/`.
- The baseline stays what a reader expects of a plain `net/http` service.

### Costs

- Nothing compares the structs with the spec: the operations scenario holds methods and paths only.
- Composing VP1 is no longer purely additive — it rewrites `internal/api/http/` onto the generated messages and deletes `openapi.json`.

## Contract-first from the baseline

### Description

The proto file exists in the first plateau; the HTTP handlers use `protojson` over the generated messages from the start, and VP1 only adds the grpc server.

### Benefits

- Request and response shapes cannot drift from the spec in any plateau.
- VP1 adds files and changes none.

### Costs

- Every HTTP-only service carries `buf`, three generators, and committed generated code for a transport it does not serve.
