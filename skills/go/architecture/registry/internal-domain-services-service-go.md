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
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] (`.extend`)
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.extend`)

# Classification
Three pairings:
- `{domain-logic, external-integration}` — `FMN`. **F**: no Constraint between `DomainLogic` (common baseline) and `ExternalIntegration` (VP2). **M**: the delta adds a field to `{Service}`, a parameter to `New{Service}`, and extends `Check`'s body. **N**: independent — `solution-go-domain-logic`'s own Implementation file is a placeholder-shaped illustration this plateau never uses verbatim; `solution-external-integration`'s delta is the first real content applied.
- `{external-integration, cached-db}` — `FMN`, resolved via the wrap/relocate test, not a resolver. Read naively, `cached-db`'s delta *wraps/moves* the exact call `external-integration` contributed (`s.reputation.CheckReputation(ctx, normalized)` becomes `s.reputationWithCache(ctx, normalized)`, with the original call relocated inside the new helper) — the surface shape of a conflict. What resolves it: `solution-cached-db`'s own Implementation file states outright — "If solution-external-integration is also applied on the same plateau, this cache sits in front of *that* solution's port call specifically... not in front of every operation" — a named, explicit merge instruction inside the later-applied solution's own file, written at catalog-authoring time. This passes [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md#the-wraprelocate-footnote-fmc-vs-fmn|the wrap/relocate footnote]]'s test exactly: the author of `cached-db` (the later delta), reading only `external-integration`'s existing code, could write a complete and correct merge instruction alone, with no change needed to `external-integration` itself — so this is `FMN` with a cross-reference note, never real `FMC`. (This pairing was first found and recorded as "a third shape the classifier didn't originally anticipate" before the footnote existed; the footnote was written specifically to formalize what this pairing already demonstrated.)
- `{*, persistent-db}` — plain `FMN`, confirming the prediction made when `cached-db` was added rather than assuming it. **F**: no Constraint — `solution-persistent-db`'s `depends_on` is only `solution-go-domain-ports`; VP7 is independent of VP2 and VP6. **M**: adds a `history` field, a `New{Service}` parameter, a `history.Record` call, and a new `RecentChecks` method. **N**: independent, and *not* wrap-shaped the way `{external-integration, cached-db}` was — verified by reading `Check`'s actual body: `s.history.Record(...)` runs strictly **after** `s.reputationWithCache(...)` returns, appended at the end of the method rather than wrapping or relocating any existing call.

# Ordering
- `{domain-logic, external-integration}`: `source: constraint` — trivial, `solution-go-domain-logic` must exist before anything can extend it.
- `{external-integration, cached-db}`: no `source: constraint` (no `depends_on`) and not truly `ordering-only` either — the applicable "ordering" is "awareness": whichever of the two is applied second must read the other's `{service}.go` delta before writing its own, which `solution-cached-db`'s own prose already tells an applying agent to do.
- `{*, persistent-db}`: `source: ordering-only` — none needed in practice, since the append happens after whatever `rep` value the earlier deltas produced, regardless of whether `cached-db` is present.

# Resolution
Canonical for all three pairings — no resolver solution needed anywhere on this element. `{external-integration, cached-db}` is resolved by the later solution's own Implementation file explicitly naming the other and stating the merge rule (the wrap/relocate shape), not by the usual FMN/TMN independence reasoning — verified by actually building and running the plateau where each pairing first became real.

# Architectural signal
N=4 at the deepest plateau. The lesson generalizes across all three pairings recorded here: a new solution touching this element needs the `cached-db`-style explicit cross-reference only if it **wraps or relocates** an existing call; a solution that **reads an already-computed value and appends independent work** (recording, auditing, metrics-emission-shaped solutions, like `persistent-db`) stays plain `FMN` regardless of how many accumulate. This is a useful predictive rule for any future solution considered against this element, not just a retrospective note — and it is now written into the classifier itself as the wrap/relocate footnote, rather than living only in this entry's own history.

# Growth history
| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| `plateau-integrated-service` | 2 | First real: `solution-go-domain-logic` (create) + `solution-external-integration`, the first solution to reach N≥2 on this element | Built and run against a real (throwaway) reputation server |
| `plateau-cached-service` | 3 | `solution-cached-db` joins — the wrap-shaped pairing with `external-integration`, recorded honestly as a case the classifier's fixed codes didn't cleanly cover at the time | Built and run against a real Redis instance: cache hit skips the external call, a miss populates the cache, proven with real Redis + a real gRPC call |
| `plateau-persistent-service` | 4 | `solution-persistent-db` joins — confirmed (not assumed) to stay plain `FMN` by reading `Check`'s actual body | Built and run end-to-end: two checks recorded correctly with both `Flagged`/`Reason` and cache-aside behavior intact, unaffected by `persistent-db`'s addition |
