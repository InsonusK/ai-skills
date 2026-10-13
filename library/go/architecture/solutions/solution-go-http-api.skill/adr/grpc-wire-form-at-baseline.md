---
name: grpc-wire-form-at-baseline
description: Which JSON shape, statuses, and error body the baseline HTTP API answers with before the service has a grpc contract
problem: `api-develop` fixes proto-JSON, the canonical status table, and the `google.rpc.Status` error body for the HTTP form of a grpc contract only; the baseline has no grpc contract, and gains one when VP1 is composed.
decision: The baseline writes by hand exactly what `protojson` writes later — lowerCamelCase keys, every field present, `{"code","message","details"}` errors, statuses from the canonical table.
tags:
  - solution/go-http-api
  - concern/documentation
  - concern/documentation/adr
  - stack/go
---

# Problem

A service starts at the baseline and may compose `solution-grpc-api` later. From then on its HTTP form is proto-JSON by rule. Whatever the baseline answered before is what its clients were built against.

# Selected variant

**Selected variant:** [[#The grpc wire form from the start]]
- Composing VP1 then changes no byte a client reads.

# Searched variants

## The grpc wire form from the start

**Selected.**

### Description

`errors.go` holds the `google.rpc.Code` numbers the adapter uses, their HTTP statuses from the canonical table, and a `statusBody` struct. Request and response structs carry lowerCamelCase tags without `omitempty`. Success is 201, 202, or 200 by what the method did.

### Benefits

- One feature file passes unchanged on the baseline example and on the VP1 example — verified.
- Services of one system answer alike whether or not they expose grpc.

### Costs

- A few grpc code numbers are written by hand in a service with no grpc dependency.
- The rule on JSON tags is kept by review and by the feature, not by a generator.

## A form of the baseline's own

### Description

`{"error": "…"}` bodies, HTTP statuses chosen per handler, snake_case or `omitempty` where it reads well — the catalog's form before this decision.

### Benefits

- Nothing of grpc in a service that does not serve it.

### Costs

- Composing VP1 renames keys, changes the error body, and moves statuses (502 to 503): a breaking change of a published `/v1/` with no new major.
