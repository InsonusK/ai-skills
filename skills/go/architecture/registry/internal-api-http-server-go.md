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
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.extend`)

# Classification
Two pairings:
- `{http-api, external-integration}` — `FMN`. **F**: no Constraint between `HttpApi` (common baseline) and `ExternalIntegration` (VP2). **M**: the delta adds fields to `checkResponse` and a `switch`/`case` mapping `interfaces.ErrUnavailable` to `http.StatusBadGateway`. **N**: independent — `solution-go-http-api`'s own create supplies the route, decode/encode skeleton, and the base `checkResponse` fields; `solution-external-integration`'s delta only appends fields to that struct and a new `case` to the existing error `switch`, touching none of the lines the base create wrote.
- `{*, persistent-db}` — `FMN`. **F**: no Constraint — VP7 is independent of VP1/VP2 per the Variability Map. **M**: adds a new route (`GET /v1/links/recent`), a new response type (`historyEntry`), and a new handler (`handleRecent`) — none of which overlaps `checkResponse`, `handleCheck`, or the error-mapping `switch` that `external-integration`'s delta touches. **N**: independent — a wholly new route/handler pair added to the same `mux`, not a modification of anything `http-api` or `external-integration` already wrote.

# Ordering
- `{http-api, external-integration}`: `source: constraint` — trivial, `solution-go-http-api` must exist before anything can extend its `Server`.
- `{*, persistent-db}`: `source: constraint` — trivial, `http-api` must exist first; no ordering relative to `external-integration` matters, since the two additions are disjoint.

# Resolution
Canonical — no resolver needed for either pairing. Verified for real: `checkResponse`'s `flagged`/`reason` fields round-trip correctly over a live HTTP request, an unreachable reputation service maps to `502`, and `GET /v1/links/recent` returns correct JSON independent of and alongside `POST /v1/links/check`'s own fields.

# Architectural signal
N=3 at the deepest plateau. Unlike [[./internal-domain-services-service-go.md|internal-domain-services-service-go]]'s N≥3 case, there is no wrap/relocate risk here: every solution touching `Server` so far has added its own disjoint route and handler function, never modified another solution's handler body. The real risk to watch for on this element is not code-level conflict but **route-path collision** — two solutions independently choosing the same HTTP method+path — which this classifier's grouping-by-element pass does not catch (it groups by Go source element, not by the router's own path space). A future solution adding an HTTP route should check the existing route table (this catalog's `plateau-persistent-service--package-api-http.skill.md` structure skill) before picking a path, since nothing else in this catalog currently guards against that collision mechanically.

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `plateau-integrated-service` | 2 | First real: `solution-go-http-api` (create) + `solution-external-integration` — found retroactively during the `plateau-persistent-service` build via a catalog-wide grep for `element/internal-api-http-server-go`, not caught when this plateau was originally built | `checkResponse`'s `flagged`/`reason` round-trip over a live HTTP request; unreachable reputation service maps to `502` |
| `plateau-persistent-service` | 3 | `solution-persistent-db` joins — a wholly new, disjoint route/handler, no wrap/relocate risk | `GET /v1/links/recent` returns correct JSON independent of `POST /v1/links/check` |
