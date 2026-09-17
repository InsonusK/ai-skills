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
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] (`.extend`) — new at this plateau

# Classification
Four pairings inside this one group, read from the actual `.extend.md` files side by side rather than assumed uniform:
- `{repository-structure, app-logging, http-api}` — `FMN`, unchanged from `plateau-http-service`.
- `{http-api, grpc-api}` — `TMN`, unchanged from `plateau-dual-api-service` (`grpc-api` restructures `http-api`'s block into an `errgroup.Group`).
- `{*, external-integration}` — `FMN`, unchanged from `plateau-integrated-service`.
- `{*, cached-db}` — `FMN`. **F**: no Constraint — `solution-cached-db`'s `depends_on` is only `solution-go-domain-ports`. **M**: inserts a `New`+`defer Close`+constructor-argument block, the same shape as `external-integration`'s. **N**: independent — confirms the prediction recorded at `plateau-integrated-service` exactly: `cached-db` also inserts before `domainService := services.NewLinkCheckService(...)` and only appends its own argument to that one call, no restructuring.

# Ordering
- `{repository-structure, app-logging, http-api}`: `source: ordering-only`, unchanged.
- `{http-api, grpc-api}`: `source: constraint` (`grpc-api`'s `depends_on`), unchanged.
- `{*, external-integration}`: `source: ordering-only`, unchanged.
- `{*, cached-db}`: `source: ordering-only` — `reputationcache.New` (and `reputationclient.Dial`) must both run before `services.NewLinkCheckService`, a positional requirement stated in `cached-db`'s own Rule; no `depends_on` edge exists between `cached-db` and `external-integration` (VP6/VP2 are independent — a plateau could have `cached-db` without `external-integration`, in which case `NewLinkCheckService` would take only the cache port).

# Resolution
Canonical — no resolver needed, for all four pairings. Verified by actually building and running this plateau's `example/cmd/linkcheck/main.go` against a real Redis instance and a throwaway fake reputation server.

# Architectural signal
N=6 at this plateau, up from N=5 at `plateau-integrated-service` — every VP-realizing solution this catalog defines now extends this element (matches the catalog-wide N=7 prediction from Stage 3, with `solution-kafka-consumer` the one remaining, aspirational, contributor). The pattern has now held for two VP-realizing solutions in a row (`external-integration`, `cached-db`): insert-before-construction, append-constructor-argument, no restructuring beyond `grpc-api`'s original `errgroup` conversion. `solution-persistent-db` is expected to compose the same way at `plateau-persistent-service` — re-confirm rather than assume, per the same discipline applied here.

