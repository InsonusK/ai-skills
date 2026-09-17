---
name: registry-internal-domain-services-service-go
description: Conflict Detection result for the `internal-domain-services-service-go` element
tags:
  - concern/architecture
  - stack/go
  - element/internal-domain-services-service-go
---

# Element
`internal-domain-services-service-go`

# Involved solutions
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] (`.create`)
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`)
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] (`.extend`) — new at this plateau

# Classification
Two pairings:
- `{domain-logic, external-integration}` — `FMN`, unchanged from `plateau-integrated-service`.
- `{external-integration, cached-db}` — **borderline, recorded honestly rather than force-fit.** **F**: no Constraint — `solution-cached-db`'s `depends_on` is only `solution-go-domain-ports`, not `solution-external-integration`; VP6/VP2 are independent per the Variability Map, and correctly so (a different product built on this catalog could cache something that has nothing to do with an external-integration port). **M**: code change. The **N vs C** question is the interesting part: read naively, `cached-db`'s delta *wraps/moves* the exact call `external-integration` contributed (`s.reputation.CheckReputation(ctx, normalized)` becomes `s.reputationWithCache(ctx, normalized)`, with the original call relocated inside the new helper) — the paradigm shape of a conflict (`FMC`), not an independent addition. What defuses it: `solution-cached-db`'s own Implementation file states outright — "If solution-external-integration is also applied on the same plateau, this cache sits in front of *that* solution's port call specifically... not in front of every operation" — a **named, explicit merge instruction inside the second-applied solution's own file**, written at catalog-authoring time, before any plateau combined them. That is not a `depends_on` edge (rightly — the dependency isn't structural) and not independence (the code is genuinely touched) — it is a third shape this catalog's classifier pass didn't originally anticipate: *conflict pre-empted by documentation*.

# Ordering
`{external-integration, cached-db}`: no `source: constraint` (no `depends_on`) and not truly `ordering-only` either (order doesn't matter here — the merge is textual/structural, not positional). The applicable "ordering" is really "awareness": whichever of the two is applied second must read the other's `{service}.go` delta before writing its own, which `solution-cached-db`'s own prose already tells an applying agent to do.

# Resolution
Canonical in outcome — no separate resolver *solution* needed — but not for the usual FMN/TMN reason (independence or constraint-ordering). Resolved because the conflicting solution's own Implementation file explicitly names the other and states the merge rule, verified correct by actually building and running this plateau's `example/` (cache hit skips the external call; a miss populates the cache; both proven with real Redis + a real gRPC call, not just unit-test stubs).

# Architectural signal
N=3 at this plateau, up from N=2 at `plateau-integrated-service` — crosses the N≥3 threshold. Combined with the classification nuance above: this element is a real candidate for the kind of VP-boundary reconsideration [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]]'s N≥3 rule warns about — not because the current three solutions conflict badly (they don't, in practice), but because *any future solution that also wants to wrap `Check`'s body* (not just add a field) will need the same kind of explicit, named cross-reference `cached-db` already carries, or it will produce a genuine `FMC` this catalog has not yet had to resolve for real. `solution-persistent-db` (next, at `plateau-persistent-service`) only *appends* (a `Record` call after computing the result), so it is not expected to need this — confirm rather than assume.
