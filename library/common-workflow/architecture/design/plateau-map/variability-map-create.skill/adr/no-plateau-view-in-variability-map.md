---
name: no-plateau-view-in-variability-map
description: The plateau↔VP view (Plateau Map derivation) moves out of variability-map-create into a dedicated plateau-map-create skill owning plateau/plateau-repository.md
problem: variability-map-create carried the plateau↔VP derivation alongside its VP↔solution table — a second trigger, a second audience, and a second home for the same mapping
decision: The Variability Map binds VPs to solutions only; the plateau↔VP view lives in plateau-map-create, which owns {catalog}/plateau/plateau-repository.md and runs on plateau changes and VP changes alike
tags:
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`variability-map-create` originally included a "Plateau Map derivation" section: every named plateau derived as a fixed VP-answer combination and cross-checked against Constraints. Three tensions surfaced:

1. **Two triggers.** The map changes when VPs/solutions change; the derivation must re-run when *plateaus* change (`created_by` edits by `plateau-update-by-solutions`). One skill was answering to two different lifecycles.
2. **Unreachable check.** `plateau-create-by-solutions`/`plateau-update-by-solutions` never reference the Variability Map, so the mandated "plateau violates a Constraint = defect" check sat in a skill that never runs at plateau-assembly time.
3. **Duplicated view.** Real catalogs (dotnet v3.1) grew a Plateau × VP matrix in `plateau/plateau-repository.md` while the map's own derivation section stayed "empty by design" — two homes for one mapping, one of them already drifting.

# Selected variant
**Selected variant:** [[#Dedicated plateau-map-create skill]]
- The Variability Map keeps only the VP↔solution binding (its actual subject). The plateau↔VP view moves to the dedicated plateau-map-create skill, which owns `{catalog}/plateau/plateau-repository.md` and carries the constraint cross-check.

# Searched variants

## Dedicated plateau-map-create skill

### Description
A new skill owning `{catalog}/plateau/plateau-repository.md` (Plateau × VP matrix, lineage, shape notes), triggered by both plateau changes and VP changes; the constraint cross-check moves there.

### Benefits
- One trigger class per skill: the map answers to VP/solution changes, the repository to plateau/VP changes.
- The constraint check lives where it can actually run — at plateau-assembly and map-change time.
- One home for the plateau↔VP matrix; the README stops competing with a section in the map.
- Matches observed practice: dotnet v3.1 already grew the matrix in `plateau/plateau-repository.md`.

### Costs
- One more skill in the pipeline; the pipeline README and cross-links need updating.
- Existing maps carry a now-orphaned derivation section that must be removed (drift cleanup).

The derivation section is removed from `variability-map.md`; `plateau/README.md` in real catalogs is renamed to `plateau-repository.md` and becomes this skill's output.

## Keep the derivation inside variability-map-create (status quo)

### Description
The map keeps its "Plateau Map derivation" section and the rules mandating plateau consistency checks.

### Benefits
- One artifact to read for both VP→solution and plateau→VP questions.

### Costs
- The skill keeps two triggers and two audiences — against the one-trigger-per-skill rule.
- The plateau-consistency check stays unreachable from the skills that change plateaus.
- The duplication with `plateau/README.md` persists and keeps drifting.

## Fold the plateau view into plateau-create-by-solutions

### Description
Let the plateau-assembly skills own the matrix as part of building/updating plateaus.

### Benefits
- The check runs exactly where plateaus change.

### Costs
- Those skills are general-purpose authoring machinery reused outside this pipeline; a per-catalog index file is a different concern.
- VP-only changes (no plateau touched) would still leave the matrix stale — nobody owns that trigger.
