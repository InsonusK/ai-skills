---
name: registry-repo-root
description: Conflict Detection result for the `repo-root` element
tags:
  - concern/architecture
  - stack/go
  - element/repo-root
---

# Element
`repo-root`

# Involved solutions
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] (`.create`)
- [[skills/go/architecture/solutions/solution-go-conformance-testing.skill/solution-go-conformance-testing.skill.md|solution-go-conformance-testing]] (`.extend`)
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] (`.extend`)
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`)

Unchanged from `plateau-cached-service` — neither `solution-cached-db` nor `solution-persistent-db` contributes a `Repository.extend.md`; both add ordinary new packages under the `internal/infrastructure/` tree `solution-go-repository-structure` already established (see this plateau's own `structure/plateau-persistent-service--repo-persistent-service.skill.md`, which states this explicitly).

# Classification
`TDN` for the `{grpc-api, external-integration}` pair specifically, `FMN` for the rest — identical to `plateau-cached-service` and `plateau-integrated-service`, re-confirmed here rather than assumed: `solution-persistent-db`'s absence from this element's involved-solutions list means the group composition genuinely did not change between those two plateaus and this one.

# Ordering
`source: constraint` for `{grpc-api, external-integration}`, `source: ordering-only` for everything else — unchanged.

# Resolution
Canonical — no resolver needed. `make proto-gen` still regenerates both `gen/api` (now including `RecentChecks`) and `gen/reputation` correctly in this plateau's own `example/`, confirming `persistent-db`'s `.proto` extension (folded into `internal/api/grpc/server.go.extend.md`, per that solution's own file — see [[./internal-api-grpc-server-go.md|internal-api-grpc-server-go]]'s registry entry) needed no `Repository`-level or `Makefile`-level change of its own.

# Architectural signal
N=4, unchanged from `plateau-cached-service` and `plateau-integrated-service` — this element has now stayed flat across two plateaus running while every *other* tracked element in this catalog grew. This is the expected, healthy shape for `repo-root`: it should only grow when a solution introduces genuinely new repository-level scaffolding (a new top-level proto subtree, a new Makefile target family), not merely a new package nested inside scaffolding that already exists — which is exactly the distinction `cached-db` and `persistent-db` both respect.
