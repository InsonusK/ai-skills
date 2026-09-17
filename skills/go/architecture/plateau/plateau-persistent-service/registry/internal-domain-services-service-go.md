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
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] (`.extend`) — new at this plateau

# Classification
Three pairings:
- `{domain-logic, external-integration}` — `FMN`, unchanged from `plateau-integrated-service`.
- `{external-integration, cached-db}` — borderline `FMC`, defused by documentation, unchanged from `plateau-cached-service`; see that plateau's own [[../../plateau-cached-service/registry/internal-domain-services-service-go.md|registry entry]] for the full analysis, still accurate at this plateau.
- `{*, persistent-db}` — plain `FMN`, **confirming the prediction made at `plateau-cached-service`'s own registry entry rather than assuming it.** **F**: no Constraint — `solution-persistent-db`'s `depends_on` is only `solution-go-domain-ports`; VP7 is independent of VP2 and VP6 per the Variability Map. **M**: code change — adds a `history` field, a `New{Service}` parameter, a `history.Record` call, and a new `RecentChecks` method. **N**: independent, and *not* borderline the way `{external-integration, cached-db}` was — verified by reading `Check`'s actual body: `s.history.Record(...)` runs strictly **after** `s.reputationWithCache(...)` returns, appended at the end of the method rather than wrapping or relocating any existing call. `persistent-db`'s delta never touches the line `rep, err := s.reputationWithCache(ctx, normalized)` that `cached-db` introduced, nor the `reputation.CheckReputation` call inside it — it only reads the already-computed `rep` value afterward. This is textbook `FMN`: two solutions changing different, non-overlapping regions of the same method body.

# Ordering
- `{domain-logic, external-integration}`: `source: constraint`, unchanged.
- `{external-integration, cached-db}`: no `depends_on`, "awareness"-shaped as previously recorded, unchanged.
- `{*, persistent-db}`: `source: ordering-only` — none needed in practice, since the append happens after whatever `rep` value the earlier deltas produced, regardless of whether `cached-db` is present. Listed last in application order purely for readability.

# Resolution
Canonical for all three pairings — no resolver needed. Verified by actually building and running this plateau's `example/` end-to-end: two checks recorded correctly with both `Flagged`/`Reason` (from `external-integration`) and cache-aside behavior (from `cached-db`) both intact and unaffected by `persistent-db`'s addition.

# Architectural signal
N=4 at this plateau, up from N=3 at `plateau-cached-service` — crosses further past the N≥3 threshold, but the signal from `plateau-cached-service`'s entry does **not** repeat here: `persistent-db` is exactly the kind of solution that entry predicted would need no special cross-reference ("`solution-persistent-db`... only appends... not expected to need this"), and that prediction held. The lesson generalizes: a new solution touching this element needs the `cached-db`-style explicit cross-reference only if it **wraps or relocates** an existing call; a solution that **reads an already-computed value and appends independent work** (recording, auditing, metrics-emission-shaped solutions) stays plain `FMN` no matter how many of them accumulate. This is a useful predictive rule for any future solution considered against this element, not just a retrospective note.
