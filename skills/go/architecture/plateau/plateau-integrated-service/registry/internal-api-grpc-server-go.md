---
name: registry-internal-api-grpc-server-go
description: Conflict Detection result for the `internal-api-grpc-server-go` element
tags:
  - concern/architecture
  - stack/go
  - element/internal-api-grpc-server-go
---

# Element
`internal-api-grpc-server-go`

# Involved solutions
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] (`.create`)
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`) — first solution to reach N≥2 on this element, at this plateau; conditional (only present when `solution-grpc-api` is also applied)

# Classification
`FMN` — **F**: no Constraint between `GrpcApi` (VP1) and `ExternalIntegration` (VP2) themselves — `solution-external-integration`'s gRPC delta is conditional on `solution-grpc-api` being applied, but that conditionality is a structural precondition (the package doesn't exist otherwise), not a Feature-Model `Requires` edge between the two VPs. **M**: the delta adds fields to the generated `CheckResponse` mapping and a `switch`/`case` mapping `interfaces.ErrUnavailable` to `codes.Unavailable`. **N**: independent — `solution-grpc-api`'s own create supplies the RPC method, request/response wiring, and base error mapping; `solution-external-integration`'s delta only appends fields to the response construction and a new `case`, touching none of the lines the base create wrote.

# Ordering
`source: constraint` — trivial: `solution-grpc-api` must exist before anything can extend its `Server` (the package itself is absent otherwise); no other ordering question arises with only one extending solution so far.

# Resolution
Canonical — no resolver needed. Verified for real: `CheckResponse`'s `flagged`/`reason` fields round-trip correctly over a live gRPC call (via `grpcurl`) in this plateau's own `example/`, and an unreachable reputation service maps to `codes.Unavailable` as specified. This registry entry was written retroactively while building `plateau-persistent-service` — `solution-persistent-db` also extends this same element there (see that plateau's own `registry/internal-api-grpc-server-go.md`), and the omission at this plateau was caught by grepping every `element/internal-api-grpc-server-go` tag across the catalog's solutions, not by re-deriving the model from scratch.
