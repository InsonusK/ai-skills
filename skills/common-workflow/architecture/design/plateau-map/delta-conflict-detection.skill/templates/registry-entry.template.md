# How Apply this template
1. Replace `{element-name}` with the exact `element/{element-name}` tag value shared by the intersecting `Implementation/` files.
2. Fill every section below from an actual reading of the intersecting solutions' `Implementation/*.extend.md` (and `.create.md`) files — never assume a code from the course example alone.
3. Remove this `# How Apply this template` section and every `hint`/`example` block before saving as `{plateau}/registry/{element-name}.md`.

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
Every solution whose Implementation/ file carries this element tag, as wikilinks.
```

# Classification
```hint
The exact code from delta-conflict-detection.skill.md's classifier table (e.g. `FMN`, `TD-`, `TMC`). State which axis value was chosen for Constraint/Category/Kind and why, in one line each.
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
