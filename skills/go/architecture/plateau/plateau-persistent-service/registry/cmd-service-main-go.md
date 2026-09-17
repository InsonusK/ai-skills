---
name: registry-cmd-service-main-go
description: Conflict Detection result for the `cmd-service-main-go` element
tags:
  - concern/architecture
  - stack/go
  - element/cmd-service-main-go
---

# Element
`cmd-service-main-go`

# Involved solutions
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] (`.create`)
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] (`.extend`)
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] (`.extend`)
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] (`.extend`)
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`)
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] (`.extend`)
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.extend`) — new at this plateau

# Classification
Five pairings inside this one group, read from the actual `.extend.md` files side by side rather than assumed uniform:
- `{repository-structure, app-logging, http-api}` — `FMN`, unchanged from `plateau-http-service`.
- `{http-api, grpc-api}` — `TMN`, unchanged from `plateau-dual-api-service` (`grpc-api` restructures `http-api`'s block into an `errgroup.Group`).
- `{*, external-integration}` — `FMN`, unchanged from `plateau-integrated-service`.
- `{*, cached-db}` — `FMN`, unchanged from `plateau-cached-service`.
- `{*, persistent-db}` — `FMN`. **F**: no Constraint — `solution-persistent-db`'s `depends_on` is only `solution-go-domain-ports`. **M**: inserts a `linkstore.New`+`defer store.Close()`+constructor-argument block, the exact same shape as `external-integration`'s and `cached-db`'s. **N**: independent — confirms the prediction recorded at `plateau-cached-service` exactly: `persistent-db` also inserts before `domainService := services.NewLinkCheckService(...)` and only appends its own argument (`store`) to that one call, no restructuring of the two already-applied inserts.

# Ordering
- `{repository-structure, app-logging, http-api}`: `source: ordering-only`, unchanged.
- `{http-api, grpc-api}`: `source: constraint` (`grpc-api`'s `depends_on`), unchanged.
- `{*, external-integration}`: `source: ordering-only`, unchanged.
- `{*, cached-db}`: `source: ordering-only`, unchanged.
- `{*, persistent-db}`: `source: ordering-only` — `linkstore.New` must run before `services.NewLinkCheckService`, a positional requirement stated in `persistent-db`'s own Rule; no `depends_on` edge exists between `persistent-db` and either `external-integration` or `cached-db` (VP7 is independent of VP2 and VP6 per the Variability Map — a plateau could have `persistent-db` alone, without either of the others).

# Resolution
Canonical — no resolver needed, for all five pairings. Verified by actually building and running this plateau's `example/cmd/linkcheck/main.go` against a real PostgreSQL instance, a real Redis instance, and a throwaway fake reputation server — including a full process kill-and-restart to confirm the PostgreSQL-backed data survives independently of the process, unlike the in-memory portions of the composition root itself.

# Architectural signal
N=7 at this plateau, up from N=6 at `plateau-cached-service` — every VP-realizing solution this catalog fully authored now extends this element (the catalog-wide N=7 prediction from Stage 3 is now exactly confirmed; `solution-kafka-consumer` remains the one aspirational, skeleton-only contributor not yet composed here). The pattern has now held for three VP-realizing solutions in a row (`external-integration`, `cached-db`, `persistent-db`): insert-before-construction, append-constructor-argument, no restructuring beyond `grpc-api`'s original `errgroup` conversion. This is strong, repeated evidence that `cmd-service-main-go`'s composition-root shape is stable under this catalog's whole VP set — any future VP realized the same way (a new outbound port, dialed once and passed into `NewLinkCheckService`) should compose identically; a future VP that does NOT fit this shape (e.g. one needing a second concurrent server, the way `grpc-api` did) is the one worth watching for.
