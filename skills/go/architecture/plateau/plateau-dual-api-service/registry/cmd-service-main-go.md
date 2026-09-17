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
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] (`.extend`) — new at this plateau

# Classification
Two different pairings inside this one group, read from the actual `.extend.md` files side by side rather than assumed uniform:
- `{repository-structure, app-logging, http-api}` — `FMN`. **F**: no Constraint between these three (all common-baseline). **M**: each changes `run()`'s body. **N**: independent — `app-logging` only inserts one call at a stated position; `http-api` only adds the single-server construct/serve/shutdown block after it. Unchanged from `plateau-http-service`.
- `{http-api, grpc-api}` — `TMN`. **T**: a real constraint — `solution-grpc-api`'s frontmatter `depends_on: solution-go-http-api` (not merely ordering-only convention, an actual dependency edge), because `grpc-api`'s delta does not just append: it **restructures** `http-api`'s single-server block into one `g.Go` closure inside a new `errgroup.Group`, and moves the shutdown goroutine into a second closure. **M**: code change. **N**: independent in the sense the classifier means — the resulting `run()` is fully specified by `grpc-api`'s own Implementation file (it shows the complete post-transformation function), so applying it is deterministic, not an ambiguous merge.

# Ordering
Mixed, matching the two pairings above:
- `{repository-structure, app-logging, http-api}`: `source: ordering-only` — `logging.Init` must precede anything that logs; stated in `app-logging`'s own Rule, not backed by a `depends_on` edge.
- `{http-api, grpc-api}`: `source: constraint` — `solution-grpc-api`'s own `depends_on: solution-go-http-api` frontmatter edge is the ordering's source; `grpc-api` is applied only after `http-api`'s single-server `run()` already exists.

# Resolution
Canonical — no resolver needed, for both pairings. The four deltas are read together and hand-merged into one `run()` at plateau-assembly time (this plateau's own `example/cmd/linkcheck/main.go`, and the summary in [[../structure/plateau-dual-api-service--file-cmd-service-main.skill.md|the file-tier skill]]) — verified by actually building and running it (`go build`/`go vet` clean, HTTP+gRPC both smoke-tested), not merely composed on paper.

# Architectural signal
N=4 at this plateau, up from N=3 at `plateau-http-service`; every further VP-realizing solution this catalog defines also extends this same element (catalog-wide N=7, per `skills/go/architecture/agent/DECISIONS.md`). Confirmed real, not just a count: `grpc-api` is the first solution to actually **restructure** rather than purely append to this element (converting single-server `run()` to an `errgroup.Group`), which is exactly the kind of growing-intersection risk the catalog-level note anticipated. Any future solution adding a *third* concurrent server (`solution-kafka-consumer`) only needs to add one more `g.Go` closure to an already-`errgroup`-shaped `run()` — no second restructuring — so the risk does not compound further, but it is worth the next plateau builder confirming that assumption explicitly rather than inheriting it silently.

