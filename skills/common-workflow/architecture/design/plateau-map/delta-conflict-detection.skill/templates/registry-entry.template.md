# How Apply this template
1. Replace `{element-name}` with the exact `element/{element-name}` tag value shared by the intersecting `Implementation/` files.
2. Fill every section below from an actual reading of the intersecting solutions' `Implementation/*.extend.md` (and `.create.md`) files — never assume a code from the course example alone.
3. Save as `{catalog}/registry/{element-name}.md` — at the catalog root, a sibling of `variability-map.md` and `plateau/`, never inside a specific plateau folder. Every plateau whose `created_by`/`parent_plateaus` includes this element links to this same file from its own `registry:` YAML property — do not create a second copy for a deeper plateau; update this one file and add a row to `# Growth history` instead.
4. Remove this `# How Apply this template` section and every `hint`/`example` block before saving.

---
name: registry-{element-name}
description: Conflict Detection result for the `{element-name}` element
tags:
  - concern/architecture
  - stack
  - element/{element-name}
---

# Element
`{element-name}`

# Involved solutions
```hint
Every solution whose Implementation/ file carries this element tag, as wikilinks — the CURRENT (deepest-known) set. When a new solution starts touching this element at a deeper plateau, add it here and add a row to Growth history below; never fork a second file for the new depth.
```

# Classification
```hint
The exact code from delta-conflict-detection.skill.md's classifier table (e.g. `FMN`, `TD-`, `TMC`). State which axis value was chosen for Constraint/Category/Kind and why, in one line each. Before recording `FMC`, check the wrap/relocate footnote (see delta-conflict-detection.skill.md's `## FMC resolution`) — a same-method touch that one delta's author can fully reconcile alone, without changing the earlier delta, is `FMN` with a cross-reference note, not `FMC`.
```

# Ordering
```hint
State the ordering between the involved solutions' deltas, and its source:
- `source: constraint` — the ordering is already required by an existing Constraint/depends_on edge (Feature-Model driven); name it.
- `source: ordering-only` — no Feature-Model constraint exists; the ordering exists solely so a resolver (named below) has something deterministic to build on.
```

# Resolution
```hint
One of:
- Canonical — no resolver needed, state why (per the classifier's Status column).
- Resolver — link the resolver solution, and confirm it depends_on every involved solution and is not folded into any of them.
- Core change — e.g. the FDC IEnumerable<T> collapse; state what changed in the core module.
```

# Architectural signal
```hint
Only when N≥3 solutions intersect on this element: note explicitly that this is also a reason to reconsider the involved VPs' boundaries, per delta-conflict-detection.skill.md's rule — not only a case needing one more resolver. Omit this section entirely when N<3.
```

# Growth history
```hint
One row per plateau where the intersecting set changed (a solution was added, or the classification/resolution itself changed) — not one row per plateau that merely inherits the element unchanged. Table columns: Plateau | N | What changed | Verified. "Verified" states how that plateau's own `example/` confirmed the classification held (a build/test run, a runtime smoke test) — this is the per-plateau ground-truth note that used to be a separate paragraph per file under the old, per-plateau registry design; see delta-conflict-detection.skill's own ADR for why it moved here.

| Plateau | N | What changed | Verified |
| --- | --- | --- | --- |
| {plateau-name} | 2 | First real: {solution-a} + {solution-b} | {how it was verified} |
```
