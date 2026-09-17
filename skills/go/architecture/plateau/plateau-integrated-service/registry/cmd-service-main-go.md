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
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`) — new at this plateau

# Classification
Three pairings inside this one group, read from the actual `.extend.md` files side by side rather than assumed uniform:
- `{repository-structure, app-logging, http-api}` — `FMN`, unchanged from `plateau-http-service`.
- `{http-api, grpc-api}` — `TMN`, unchanged from `plateau-dual-api-service` (`grpc-api` restructures `http-api`'s block into an `errgroup.Group`).
- `{*, external-integration}` — `FMN`. **F**: no Constraint — `solution-external-integration`'s `depends_on` is only `solution-go-domain-ports` (for the `internal/domain/interfaces` package), not any of the other `cmd-service-main-go` contributors. **M**: `external-integration`'s delta inserts a `Dial`+`defer Close`+constructor-argument block. **N**: independent — it inserts *before* `domainService := services.NewLinkCheckService(...)` (a position `grpc-api`'s already-`errgroup`-shaped `run()` doesn't touch), and only changes what argument that one constructor call receives, which is additive by construction (the domain-service constructor itself already accretes parameters per [[./internal-domain-services-service-go.md|internal-domain-services-service-go's own entry]]).

# Ordering
- `{repository-structure, app-logging, http-api}`: `source: ordering-only`, unchanged.
- `{http-api, grpc-api}`: `source: constraint` (`grpc-api`'s `depends_on`), unchanged.
- `{*, external-integration}`: `source: ordering-only` — `reputationclient.Dial` must run before `services.NewLinkCheckService` is called (so the client can be passed in), a positional requirement stated in `external-integration`'s own Rule, not backed by a `depends_on` edge against any of the server-wiring solutions specifically.

# Resolution
Canonical — no resolver needed, for all three pairings. Verified by actually building and running this plateau's `example/cmd/linkcheck/main.go` — HTTP+gRPC smoke-tested against a real (throwaway) reputation server, including the reputation-service-unavailable path.

# Architectural signal
N=5 at this plateau, up from N=4 at `plateau-dual-api-service`. `external-integration` did **not** need a second restructuring the way `grpc-api` did — it slots into the already-`errgroup`-shaped `run()` as a plain insertion before the domain-service construction, confirming the prediction recorded at `plateau-dual-api-service` ("no second restructuring... worth the next plateau builder confirming that assumption explicitly"). `solution-cached-db` and `solution-persistent-db` are expected to compose the same way (insert-before-construction, append-constructor-argument) — re-confirm at `plateau-cached-service`/`plateau-persistent-service` rather than assuming.

