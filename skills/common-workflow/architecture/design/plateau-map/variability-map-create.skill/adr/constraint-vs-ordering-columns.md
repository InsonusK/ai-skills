---
name: constraint vs ordering stays column-level
description: How to represent the difference between a hard Feature-Model constraint and a pure delta-ordering dependency, without changing depends_on
problem: A dependency between two solutions can mean either "this combination is illegal without the other" (a Feature-Model constraint) or "apply this one first, purely for a resolver to work" (ordering-only) — solution-create's depends_on field cannot distinguish the two today
decision: Do not change depends_on's shape (it stays a plain wikilink list, unchanged for every stack). Represent the distinction only in the Variability Map's own Constraint / Realization-depends-on columns and, for delta ordering specifically, in a Registry entry's Ordering field.
tags:
  - concern/architecture
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`solution-create.skill.md`'s `depends_on` field is a plain list of wikilinks to sibling solutions, shared by every stack template (.NET, Python, TypeScript, Angular). In practice a `depends_on` edge carries one of two different meanings that look identical in the YAML:
1. A **constraint** from the underlying Feature Model — the dependent solution is illegal to select without the other (e.g. `HasCentralizedRules` requires `HasInteractionValidation` or `HasState`).
2. **Ordering-only** — no Feature-Model rule requires the combination; the edge exists purely so a Delta-Oriented-Programming resolver has something deterministic to apply after.

Nothing in the existing schema tells a reader which of the two a given `depends_on` entry means, and this repository's prior design conversation (recorded before this skill existed) considered annotating each entry with a `reason: constraint | ordering-only` field.

# Selected variant
[[#Column-level distinction, depends_on unchanged (selected)]]

# Searched variants

## Column-level distinction, depends_on unchanged (selected)

### Description
`depends_on` keeps its existing plain-list shape everywhere, with no schema change. The Variability Map's `Constraint` column carries every genuine Feature-Model requirement; its `Realization depends on` column carries same-VP relationships that change code shape without gating legality. For delta ordering that exists purely for a conflict resolver (no Feature-Model constraint behind it at all), [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill|delta-conflict-detection]]'s Registry entries carry an explicit `Ordering` field stating `source: constraint` or `source: ordering-only`.

### Benefits
- `solution-create.skill.md` and every existing solution skill across every stack stay untouched — zero migration cost, zero risk of breaking a schema every stack's templates share.
- The distinction still becomes visible and queryable — just in the new artifacts built specifically to need it, not retrofitted onto a field designed for a simpler purpose.
- Keeps `depends_on` doing exactly one job (structural dependency for ordering/discovery), consistent with how `solution-plateau-hierarchy.skill.md` already treats it relative to `built_on_plateau`.

### Costs
- The two meanings are not visible from `depends_on` alone — a reader must open the Variability Map or the relevant Registry entry to know which kind a given edge is. Mitigated by making that lookup exactly this skill's and `delta-conflict-detection`'s job, rather than leaving it undocumented anywhere.

## Annotate depends_on with a reason field

### Description
Change `depends_on` from a plain wikilink list to a list of `{ link, reason }` objects, where `reason` is `constraint` or `ordering-only`.

### Benefits
- The distinction is visible directly on the solution file itself, with no need to cross-reference a separate table.

### Costs
- Changes a field defined once in `solution-create.skill.md` and consumed identically by every stack's template (.NET, Python, TypeScript, Angular) and by `plateau-create-by-solutions`/`plateau-update-by-solutions`'s own scans — every existing solution in the repository (dozens, across stacks) would need retrofitting or the field would need to support two shapes simultaneously.
- `solution-create.skill.md` is explicitly out of scope to edit for this work (per its own `depends_on` definition and this catalog's existing rule against changing other skills without explicit instruction) — this variant requires editing it anyway.
