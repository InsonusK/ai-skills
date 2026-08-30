---
name: one map per catalog, not per plateau
description: Where a Variability Map lives relative to the plateaus it describes
problem: Should a Variability Map exist once per catalog, or once per plateau?
decision: One Variability Map per catalog, at `{catalog}/variability-map.md`, sibling to `plateau/` and `solutions/`; plateaus are derived points in the space it describes, not separate spaces of their own.
tags:
  - concern/architecture
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A plateau/solution catalog (e.g. `skills/dotnet/architecture/v3`) can have several plateaus at different composition depths (`plateau-stateless-non-interactive-service`, `plateau-statefull-service`, `plateau-v1`, ...). Where should the Variability Map — the table of Variation Points, Variants, Constraints, and Realized-by links — be stored: once for the whole catalog, or once per plateau?

# Selected variant
[[#One map per catalog (selected)]]

# Searched variants

## One map per catalog (selected)

### Description
A single `variability-map.md` at the catalog root, sibling to `plateau/` and `solutions/`. Every plateau is documented as a row in that one map's "Plateau Map derivation" section, stating the exact VP-answer combination it fixes.

### Benefits
- A Variation Point that spans multiple plateaus (most of them do, since plateaus compose cumulatively) is written exactly once, not duplicated into every plateau that happens to include it.
- The "is this plateau's combination even legal" check (against stated Constraints) has one table to check against, not N potentially-inconsistent copies.
- Matches how the underlying theory treats it: a Feature Model / OVM table describes one product *line*, not one product.

### Costs
- The map can grow long as the catalog grows; mitigated by keeping rows to genuine Variation Points only (per the skill's Core Principle), not every solution.
- A reader focused on one specific plateau has to find its row inside a larger table instead of opening a dedicated file — mitigated by the "Plateau Map derivation" section being a short, scannable table keyed by plateau name.

## One map per plateau

### Description
Each plateau folder gets its own `variability-map.md`, describing only the VPs relevant to solutions introduced at that plateau (or inherited from its `parent_plateaus`).

### Benefits
- Shorter, more locally-scoped file per plateau.
- Colocated with the plateau it describes, similar to how `adr/` is already colocated per plateau.

### Costs
- A VP realized by a solution shared across several plateaus (the common case, given plateaus compose cumulatively via `parent_plateaus`) would need to be copied into every plateau's map or cross-referenced, reintroducing exactly the duplication-drift problem `adr/`'s per-plateau placement does NOT have (an ADR is about a decision local to one plateau's own composition, not a cross-cutting VP).
- No single place to check whether the full set of named plateaus actually covers every constraint-legal combination.
