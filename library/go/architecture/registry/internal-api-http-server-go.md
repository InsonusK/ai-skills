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
- [[library/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] (`.create`)
- [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] (`.extend`, since 2026-10-12)
- [[library/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`)
- [[library/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.extend`)

# Classification
Four pairings:
- `{http-api, grpc-api}` — `FMN`. **F**: no Constraint between `HttpApi` (common baseline) and VP1. **M**: the delta replaces the body of every contract handler — its own structs and `encoding/json` give way to the generated messages and `readMessage`/`writeMessage` — and deletes `errors.go` for `protojson.go`. **N**: one dependent delta over the base's create; `Server`, `Options`, `contractRoutes()`, `Handler()` and the health handler are untouched.
- `{grpc-api, external-integration}` and `{grpc-api, persistent-db}` — `FMC`, resolved inside the extending solutions (see Resolution). **F**: no Constraint between VP1 and VP-C005 or VP-C001. **M**/**C**: each pair changes the same thing — `handleCheck`'s response for external-integration, the shape of a new handler for persistent-db — in a form that depends on whether the handlers use generated messages.
- `{http-api, external-integration}` — `FMN`. **F**: no Constraint between `HttpApi` (common baseline) and `ExternalIntegration` (VP-C005). **M**: the delta adds fields to `checkResponse` and a `case` mapping `interfaces.ErrUnavailable` to `UNAVAILABLE` (503; it was `http.StatusBadGateway` before 2026-10-12). **N**: independent — `solution-go-http-api`'s own create supplies the route, decode/encode skeleton, and the base `checkResponse` fields; `solution-external-integration`'s delta only appends fields to that struct and a new `case` to the existing error `switch`, touching none of the lines the base create wrote.
- `{*, persistent-db}` — `FMN`. **F**: no Constraint — VP-C001 is independent of VP1/VP-C005 per the Variability Map. **M**: adds a new route (`GET /v1/links/recent`), a new response type (`historyEntry`), and a new handler (`handleRecent`) — none of which overlaps `checkResponse`, `handleCheck`, or the error-mapping `switch` that `external-integration`'s delta touches. **N**: independent — a wholly new route/handler pair added to the same `mux`, not a modification of anything `http-api` or `external-integration` already wrote.

# Ordering
- `{http-api, external-integration}`: `source: constraint` — trivial, `solution-go-http-api` must exist before anything can extend its `Server`.
- `{*, persistent-db}`: `source: constraint` — trivial, `http-api` must exist first; no ordering relative to `external-integration` matters, since the two additions are disjoint.

# Resolution
Canonical for `{http-api, external-integration}`, `{*, persistent-db}`, and `{http-api, grpc-api}`.

The two `FMC` pairings have no resolver solution: `solution-external-integration`'s and `solution-persistent-db`'s own `server.go.extend.md` each state their delta in two forms — with `solution-grpc-api` applied, and without. This follows the catalog's existing precedent for a delta conditional on VP1 (their `internal/api/grpc/server.go.extend.md`, "applies only when `solution-grpc-api` is also applied") and departs from `delta-conflict-detection`'s rule that a resolver is a separate solution — recorded as an open fork in `agent/DECISIONS.md`. Only the with-VP1 form is verified: no plateau composes VP-C005 or VP-C001 without VP1.

Before 2026-10-12: Verified for real: `checkResponse`'s `flagged`/`reason` fields round-trip correctly over a live HTTP request, an unreachable reputation service maps to `502` (503 since the adapter follows the canonical table), and `GET /v1/links/recent` returns correct JSON independent of and alongside `POST /v1/links/check`'s own fields.

# Architectural signal
N=4 at the deepest plateau since `solution-grpc-api` joined; the paragraph below was written at N=3 and its "never modified another solution's handler body" no longer holds for `grpc-api`, which rewrites every contract handler by design. The route-path collision it warns of is now caught mechanically: every contract route is in `contractRoutes()`, and `http.ServeMux` panics on a duplicate pattern when `Handler()` runs, which the HTTP feature does.

At N=3: Unlike [[./internal-domain-services-service-go.md|internal-domain-services-service-go]]'s N≥3 case, there is no wrap/relocate risk here: every solution touching `Server` so far has added its own disjoint route and handler function, never modified another solution's handler body. The real risk to watch for on this element is not code-level conflict but **route-path collision** — two solutions independently choosing the same HTTP method+path — which this classifier's grouping-by-element pass does not catch (it groups by Go source element, not by the router's own path space). A future solution adding an HTTP route should check the existing route table (this catalog's `plateau-persistent-service--package-api-http.skill.md` structure skill) before picking a path, since nothing else in this catalog currently guards against that collision mechanically.

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `plateau-integrated-service` | 2 | First real: `solution-go-http-api` (create) + `solution-external-integration` — found retroactively during the `plateau-persistent-service` build via a catalog-wide grep for `element/internal-api-http-server-go`, not caught when this plateau was originally built | `checkResponse`'s `flagged`/`reason` round-trip over a live HTTP request; unreachable reputation service maps to `502` |
| `plateau-persistent-service` | 3 | `solution-persistent-db` joins — a wholly new, disjoint route/handler, no wrap/relocate risk | `GET /v1/links/recent` returns correct JSON independent of `POST /v1/links/check` |
| `plateau-dual-api-service` and every later plateau (2026-10-12, issue #206) | +1 | `solution-grpc-api` joins: the contract handlers move onto the generated messages; `external-integration`'s and `persistent-db`'s deltas take their with-VP1 form | `internal/api/http/features/http_api.feature` green in all five plateaus with VP1; the same feature passes on `plateau-http-service`, so the wire form did not change with VP1 |
