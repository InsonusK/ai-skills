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
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`)
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.extend`) — new at this plateau; conditional (only present when `solution-grpc-api` is also applied)

# Classification
Two pairings:
- `{grpc-api, external-integration}` — `FMN`, unchanged from `plateau-integrated-service` (see that plateau's own [[../../plateau-integrated-service/registry/internal-api-grpc-server-go.md|registry entry]], written retroactively alongside this one).
- `{*, persistent-db}` — `FMN`. **F**: no Constraint — VP7 is independent of VP1/VP2 per the Variability Map; the conditionality on `solution-grpc-api` is structural (the package doesn't exist otherwise), not a Feature-Model `Requires` edge. **M**: adds a new RPC method (`RecentChecks`), extends the `.proto` file with new messages, and adds the entry-mapping loop — none of which overlaps `Check`, `CheckResponse`, or the error-mapping `switch` that `external-integration`'s delta touches. **N**: independent — a wholly new RPC added to the same `Server`, not a modification of anything `grpc-api` or `external-integration` already wrote.

# Ordering
- `{grpc-api, external-integration}`: `source: constraint` (trivial — `grpc-api` must exist first), unchanged.
- `{*, persistent-db}`: `source: constraint` (trivial — `grpc-api` must exist first; no ordering relative to `external-integration` matters, since the two additions are disjoint).

# Resolution
Canonical — no resolver needed. Verified for real via `grpcurl`: `RecentChecks` returns correct entries independent of and alongside `Check`'s own `flagged`/`reason` fields, in this plateau's own `example/`; the same two entries also matched the HTTP adapter's `GET /v1/links/recent` response and the raw `psql` row contents exactly.

# Architectural signal
N=3 at this plateau, up from N=2 at `plateau-integrated-service` — crosses the N≥3 threshold for the first time on this element, mirroring [[./internal-api-http-server-go.md|internal-api-http-server-go]]'s own N=3 case exactly (same three solutions, same shape of change, same absence of wrap/relocate risk). The analogous risk to watch here is **RPC-name / proto-field-number collision** rather than route-path collision — two solutions independently adding an RPC with the same name, or (more commonly, since proto field numbers are hand-assigned) two solutions independently claiming the same field number inside a shared message. This plateau's own `linkcheck.proto` currently has no such collision, but nothing in this catalog's classifier pass checks for it mechanically — a future solution extending this `.proto` file should check the existing field numbers in this plateau's own `example/proto/linkcheck/linkcheck.proto` before assigning a new one.
