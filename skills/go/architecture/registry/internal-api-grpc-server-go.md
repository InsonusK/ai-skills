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
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`) — conditional (only present when `solution-grpc-api` is also applied)
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.extend`) — conditional (only present when `solution-grpc-api` is also applied)

# Classification
Two pairings:
- `{grpc-api, external-integration}` — `FMN`. **F**: no Constraint between `GrpcApi` (VP1) and `ExternalIntegration` (VP2) themselves — `solution-external-integration`'s gRPC delta is conditional on `solution-grpc-api` being applied, but that conditionality is a structural precondition (the package doesn't exist otherwise), not a Feature-Model `Requires` edge between the two VPs. **M**: the delta adds fields to the generated `CheckResponse` mapping and a `switch`/`case` mapping `interfaces.ErrUnavailable` to `codes.Unavailable`. **N**: independent — `solution-grpc-api`'s own create supplies the RPC method, request/response wiring, and base error mapping; `solution-external-integration`'s delta only appends fields to the response construction and a new `case`, touching none of the lines the base create wrote.
- `{*, persistent-db}` — `FMN`. **F**: no Constraint — VP7 is independent of VP1/VP2 per the Variability Map; the conditionality on `solution-grpc-api` is structural, not a Feature-Model `Requires` edge. **M**: adds a new RPC method (`RecentChecks`), extends the `.proto` file with new messages, and adds the entry-mapping loop — none of which overlaps `Check`, `CheckResponse`, or the error-mapping `switch` that `external-integration`'s delta touches. **N**: independent — a wholly new RPC added to the same `Server`, not a modification of anything `grpc-api` or `external-integration` already wrote.

# Ordering
- `{grpc-api, external-integration}`: `source: constraint` — trivial, `grpc-api` must exist first (the package itself is absent otherwise).
- `{*, persistent-db}`: `source: constraint` — trivial, `grpc-api` must exist first; no ordering relative to `external-integration` matters, since the two additions are disjoint.

# Resolution
Canonical — no resolver needed for either pairing. Verified for real via `grpcurl`: `CheckResponse`'s `flagged`/`reason` fields round-trip correctly, an unreachable reputation service maps to `codes.Unavailable`, and `RecentChecks` returns correct entries independent of and alongside `Check`'s own fields — the same entries also matched the HTTP adapter's `GET /v1/links/recent` response and the raw `psql` row contents exactly.

# Architectural signal
N=3 at the deepest plateau, mirroring [[./internal-api-http-server-go.md|internal-api-http-server-go]]'s own N=3 case exactly (same three solutions, same shape of change, same absence of wrap/relocate risk). The analogous risk to watch here is **RPC-name / proto-field-number collision** rather than route-path collision — two solutions independently adding an RPC with the same name, or (more commonly, since proto field numbers are hand-assigned) two solutions independently claiming the same field number inside a shared message. This catalog's own `linkcheck.proto` currently has no such collision, but nothing in this catalog's classifier pass checks for it mechanically — a future solution extending this `.proto` file should check the existing field numbers first.

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `plateau-integrated-service` | 2 | First real: `solution-grpc-api` (create) + `solution-external-integration` — found retroactively during the `plateau-persistent-service` build via a catalog-wide grep for `element/internal-api-grpc-server-go`, not caught when this plateau was originally built | `CheckResponse`'s `flagged`/`reason` round-trip over a live gRPC call via `grpcurl`; unreachable reputation service maps to `codes.Unavailable` |
| `plateau-persistent-service` | 3 | `solution-persistent-db` joins — a wholly new, disjoint RPC, no wrap/relocate risk | `RecentChecks` via `grpcurl` matches the HTTP adapter's response and the raw `psql` rows exactly |
