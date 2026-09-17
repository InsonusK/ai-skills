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
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`) — new at this plateau; the predicted collision point from `plateau-dual-api-service`'s own note

# Classification
`TDN` for the `{grpc-api, external-integration}` pair specifically, `FMN` for the rest. **T**: `solution-external-integration`'s own Rule explicitly requires it — "If `Makefile` already has a `proto-gen` target (from `solution-grpc-api`), add this generation as a second `buf generate` line inside the existing target — never a second `proto-gen:` target declaration" — a real, stated ordering dependency, confirmed by reading both `Repository.extend.md` files side by side, not assumed. **D**: this is DI-substitution-shaped, not a code-logic change — `external-integration` extends the *recipe body* of an existing target rather than changing what the target conceptually does. **N**: independent — the two `buf generate` lines inside `proto-gen` operate on disjoint `proto/`/`gen/` subtrees (`proto/linkcheck`→`gen/api`, `proto/reputation`→`gen/reputation`) and neither reads the other's output. `solution-go-conformance-testing` stays `FMN` against all three others — its `Makefile` targets and `report-template/` remain untouched by this plateau's change.

# Ordering
`source: constraint` for `{grpc-api, external-integration}` — the ordering is stated in `external-integration`'s own Rule (quoted above), not merely a convention. `source: ordering-only` for everything else, unchanged from `plateau-dual-api-service`.

# Resolution
Canonical — no resolver needed. Verified for real: `make proto-gen` runs both `buf generate` lines in the merged target and produces both `gen/api/*.go` and `gen/reputation/*.go` correctly in this plateau's own `example/`.

# Architectural signal
N=4 at this plateau, up from N=3 at `plateau-dual-api-service` — the predicted collision (see that plateau's own note) happened exactly as anticipated, and resolved exactly as `external-integration`'s own Rule already specified: no surprise, no resolver, because the ordering was stated explicitly in advance rather than discovered by trial and error. This is a good example of the harness paying off — the N≥3 flag on the *previous* plateau caused this plateau's build to check the prediction deliberately instead of re-deriving it.
