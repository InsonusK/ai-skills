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
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] (`.extend`) — first solution to reach N≥2 on this element, at this plateau

# Classification
`FMN` — **F**: no Constraint between `DomainLogic` (common baseline) and `ExternalIntegration` (VP2). **M**: the delta adds a field to `{Service}`, a parameter to `New{Service}`, and extends `Check`'s body. **N**: independent in the sense that matters — `solution-go-domain-logic`'s own Implementation file is a placeholder-shaped illustration (`{Service}`/`{Method}`, zero fields) that this plateau never uses verbatim; `solution-external-integration`'s delta is the first real content applied to the concrete `LinkCheckService`. There is exactly one prior state to extend, not two competing deltas.

# Ordering
`source: constraint` — trivial: `solution-go-domain-logic` must exist before anything can extend it; no other ordering question arises with only one extending solution so far.

# Resolution
Canonical — no resolver needed. `solution-cached-db` and `solution-persistent-db` will each add their own port to the same struct/constructor starting at `plateau-cached-service`/`plateau-persistent-service` — see those plateaus' own registry entries once they exist; this entry will be revisited then, per the catalog-level prediction already recorded in `skills/go/architecture/agent/DECISIONS.md`.
