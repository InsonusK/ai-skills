---
name: registry-internal-api-http-server-go
description: Conflict Detection result for the `internal-api-http-server-go` element
tags:
  - concern/architecture
  - stack/go
  - element/internal-api-http-server-go
---

# Element
`internal-api-http-server-go`

# Involved solutions
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] (`.create`)
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`) — first solution to reach N≥2 on this element, at this plateau

# Classification
`FMN` — **F**: no Constraint between `HttpApi` (common baseline) and `ExternalIntegration` (VP2). **M**: the delta adds fields to `checkResponse` and a `switch`/`case` mapping `interfaces.ErrUnavailable` to `http.StatusBadGateway`. **N**: independent — `solution-go-http-api`'s own create supplies the route, decode/encode skeleton, and the base `checkResponse` fields; `solution-external-integration`'s delta only appends fields to that struct and a new `case` to the existing error `switch`, touching none of the lines the base create wrote.

# Ordering
`source: constraint` — trivial: `solution-go-http-api` must exist before anything can extend its `Server`; no other ordering question arises with only one extending solution so far.

# Resolution
Canonical — no resolver needed. Verified for real: `checkResponse`'s `flagged`/`reason` fields round-trip correctly over a live HTTP request in this plateau's own `example/`, and an unreachable reputation service maps to `502` as specified. This registry entry was written retroactively while building `plateau-persistent-service` — `solution-persistent-db` also extends this same element there (see that plateau's own `registry/internal-api-http-server-go.md`), and the omission at this plateau was caught by grepping every `element/internal-api-http-server-go` tag across the catalog's solutions, not by re-deriving the model from scratch.
