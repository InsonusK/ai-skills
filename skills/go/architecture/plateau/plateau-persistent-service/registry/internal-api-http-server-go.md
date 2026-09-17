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
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`)
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.extend`) — new at this plateau

# Classification
Two pairings:
- `{http-api, external-integration}` — `FMN`, unchanged from `plateau-integrated-service` (see that plateau's own [[../../plateau-integrated-service/registry/internal-api-http-server-go.md|registry entry]], written retroactively alongside this one).
- `{*, persistent-db}` — `FMN`. **F**: no Constraint — VP7 is independent of VP1/VP2 per the Variability Map. **M**: adds a new route (`GET /v1/links/recent`), a new response type (`historyEntry`), and a new handler (`handleRecent`) — none of which overlaps `checkResponse`, `handleCheck`, or the error-mapping `switch` that `external-integration`'s delta touches. **N**: independent — a wholly new route/handler pair added to the same `mux`, not a modification of anything `http-api` or `external-integration` already wrote.

# Ordering
- `{http-api, external-integration}`: `source: constraint` (trivial — `http-api` must exist first), unchanged.
- `{*, persistent-db}`: `source: constraint` (trivial — `http-api` must exist first; no ordering relative to `external-integration` matters, since the two additions are disjoint).

# Resolution
Canonical — no resolver needed. Verified for real: `GET /v1/links/recent` returns correct JSON independent of and alongside `POST /v1/links/check`'s own `flagged`/`reason` fields, in this plateau's own `example/`.

# Architectural signal
N=3 at this plateau, up from N=2 at `plateau-integrated-service` — crosses the N≥3 threshold for the first time on this element. Unlike [[./internal-domain-services-service-go.md|internal-domain-services-service-go]]'s N≥3 case, there is no wrap/relocate risk here: every solution touching `Server` so far has added its own disjoint route and handler function, never modified another solution's handler body. The real risk to watch for on this element is not code-level conflict but **route-path collision** — two solutions independently choosing the same HTTP method+path — which this classifier's grouping-by-element pass does not catch (it groups by Go source element, not by the router's own path space). A future solution adding an HTTP route should check the existing route table in this plateau's own `structure/plateau-persistent-service--package-api-http.skill.md` before picking a path, since nothing else in this catalog currently guards against that collision mechanically.
