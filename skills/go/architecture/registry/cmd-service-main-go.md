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
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.extend`)

# Classification
Five pairings inside this one group, read from the actual `.extend.md` files side by side rather than assumed uniform:
- `{repository-structure, app-logging, http-api}` — `FMN`. **F**: no Constraint between these three (all common-baseline). **M**: each changes `run()`'s body. **N**: independent — `app-logging` only inserts one call at a stated position; `http-api` only adds the single-server construct/serve/shutdown block after it.
- `{http-api, grpc-api}` — `TMN`. **T**: a real constraint — `solution-grpc-api`'s frontmatter `depends_on: solution-go-http-api`, because `grpc-api`'s delta does not just append: it **restructures** `http-api`'s single-server block into one `g.Go` closure inside a new `errgroup.Group`. **M**: code change. **N**: independent in the sense the classifier means — the resulting `run()` is fully specified by `grpc-api`'s own Implementation file, so applying it is deterministic, not an ambiguous merge.
- `{*, external-integration}` — `FMN`. **F**: no Constraint — `solution-external-integration`'s `depends_on` is only `solution-go-domain-ports`. **M**: inserts a `Dial`+`defer Close`+constructor-argument block. **N**: independent — inserts before `domainService := services.NewLinkCheckService(...)`, only changes what argument that one constructor call receives.
- `{*, cached-db}` — `FMN`. **F**: no Constraint — `solution-cached-db`'s `depends_on` is only `solution-go-domain-ports`. **M**: inserts a `New`+`defer Close`+constructor-argument block, the same shape as `external-integration`'s. **N**: independent — also inserts before `domainService := services.NewLinkCheckService(...)`, only appends its own argument, no restructuring.
- `{*, persistent-db}` — `FMN`. **F**: no Constraint — `solution-persistent-db`'s `depends_on` is only `solution-go-domain-ports`. **M**: inserts a `linkstore.New`+`defer store.Close()`+constructor-argument block, the exact same shape as `external-integration`'s and `cached-db`'s. **N**: independent — also inserts before construction, no restructuring of the two already-applied inserts.

# Ordering
- `{repository-structure, app-logging, http-api}`: `source: ordering-only` — `logging.Init` must precede anything that logs; stated in `app-logging`'s own Rule, not backed by a `depends_on` edge.
- `{http-api, grpc-api}`: `source: constraint` — `solution-grpc-api`'s own `depends_on: solution-go-http-api` frontmatter edge is the ordering's source.
- `{*, external-integration}`: `source: ordering-only` — `reputationclient.Dial` must run before `services.NewLinkCheckService` is called, a positional requirement stated in `external-integration`'s own Rule.
- `{*, cached-db}`: `source: ordering-only` — `reputationcache.New` (and `reputationclient.Dial`) must both run before `services.NewLinkCheckService`; no `depends_on` edge exists between `cached-db` and `external-integration` (VP6/VP2 are independent).
- `{*, persistent-db}`: `source: ordering-only` — `linkstore.New` must run before `services.NewLinkCheckService`; no `depends_on` edge exists between `persistent-db` and either `external-integration` or `cached-db` (VP7 is independent of VP2 and VP6 per the Variability Map).

# Resolution
Canonical — no resolver needed, for all five pairings. Verified at every plateau by actually building and running that plateau's own `example/cmd/linkcheck/main.go` (`go build`/`go vet`, real HTTP+gRPC smoke tests) — see Growth history below for what each plateau specifically verified.

# Architectural signal
N=7 at the deepest plateau — every VP-realizing solution this catalog fully authored now extends this element (`solution-kafka-consumer` remains the one aspirational, skeleton-only contributor not yet composed here). The pattern has held for three VP-realizing solutions in a row (`external-integration`, `cached-db`, `persistent-db`): insert-before-construction, append-constructor-argument, no restructuring beyond `grpc-api`'s original `errgroup` conversion. This is strong, repeated evidence that `cmd-service-main-go`'s composition-root shape is stable under this catalog's whole VP set — any future VP realized the same way (a new outbound port, dialed once and passed into `NewLinkCheckService`) should compose identically; a future VP that does NOT fit this shape (e.g. one needing a second concurrent server, the way `grpc-api` did) is the one worth watching for.

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `plateau-http-service` | 3 | First real: `solution-go-repository-structure` (create) + `solution-go-app-logging` + `solution-go-http-api` | `go build`/`go vet` clean; base HTTP server smoke-tested |
| `plateau-dual-api-service` | 4 | `solution-grpc-api` joins — first restructuring: single-server `run()` converted to an `errgroup.Group` | `go build`/`go vet` clean; HTTP+gRPC both smoke-tested |
| `plateau-integrated-service` | 5 | `solution-external-integration` joins — plain insertion before construction, no second restructuring, confirming the prediction from `plateau-dual-api-service` | HTTP+gRPC smoke-tested against a real (throwaway) reputation server, including the reputation-service-unavailable path |
| `plateau-cached-service` | 6 | `solution-cached-db` joins — same insert-before-construction shape | Smoke-tested against a real Redis instance and a throwaway fake reputation server |
| `plateau-persistent-service` | 7 | `solution-persistent-db` joins — same shape a third time, catalog-wide N=7 prediction confirmed exactly | Smoke-tested against a real PostgreSQL instance, a real Redis instance, and a throwaway fake reputation server, including a full process kill-and-restart confirming the PostgreSQL-backed data survives independently of the process |
