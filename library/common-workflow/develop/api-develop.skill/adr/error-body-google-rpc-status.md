---
name: error-body-google-rpc-status
description: Decide the body format of every HTTP error answered by the HTTP form of a grpc contract.
problem: Services returned `{"detail": ...}`, `{"error": ...}`, and framework defaults. Which single error body does the HTTP form use?
decision: google.rpc.Status in proto-JSON.
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Two services of one system returned different error bodies, so clients needed one parser per service and could not tell apart two outcomes sharing an HTTP status.

# Selected variant
**Selected variant:** [[#google.rpc.Status]]
- It is the body grpc-gateway already emits and carries the grpc code, so the HTTP and grpc forms report the same error.

# Searched variants

## google.rpc.Status

**Selected.**

### Description
Every error body is `{"code": <grpc code>, "message": "...", "details": []}`, referenced as `.google.rpc.Status` in the OpenAPI spec.

### Benefits
- grpc-gateway emits it by default; hand-written servers have one documented shape to copy.
- The grpc code distinguishes outcomes sharing an HTTP status (`INVALID_ARGUMENT` vs `FAILED_PRECONDITION`, both 400).
- `details` carries typed error details (`BadRequest`, `ErrorInfo`) shared with grpc clients.

### Costs
- Framework servers must override their default error handler.
- Less familiar to plain-HTTP consumers than RFC 9457.

## RFC 9457 problem+json

### Description
`application/problem+json` with `type`, `title`, `status`, `detail`.

### Benefits
- An HTTP standard, familiar to REST consumers; some frameworks support it natively.

### Costs
- Carries no grpc code; the HTTP and grpc forms of one error diverge, and grpc-gateway needs a custom error handler.

## Service-specific body

### Description
Each service defines its own error message.

### Benefits
- No constraint on the service.

### Costs
- Clients need one error parser per service, and bodies drift as services are added.
