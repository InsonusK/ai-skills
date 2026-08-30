# How Apply this template
1. Replace `{catalog-name}` with the catalog's own name (e.g. the plateau/solution tree's top folder name).
2. Fill one row per Variation Point, following [../variability-map-create.skill.md](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md)'s "How to build a Variability Map".
3. Fill [Plateau Map derivation](#plateau-map-derivation) from the catalog's actual plateaus.
4. Remove this `# How Apply this template` section and every `hint`/`example` block before saving as `{catalog}/variability-map.md`.

# {catalog-name} Variability Map

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
```hint
One row per Variation Point.
- ID: VP1, VP2, ... — stable once assigned, never renumbered when a VP is removed (leave a gap instead).
- VP: the question, in one line, phrased so a reader can answer it Yes/No or by naming a Variant.
- Variants: the legal answers. `Yes / No` for a boolean VP; the named list for a categorical VP.
- Constraint: a real depends_on/built_on_plateau edge or stated prose requirement gating which Variant combinations are legal. `—` if none.
- Realized by: wikilink(s) to the solution skill(s) that implement each Variant. For a categorical VP, show the mapping (e.g. `Variant A → solution-x + solution-y`).
- Realization depends on: cross-VP relationships that change code *shape* without gating legality — mandatory sub-feature / orthogonal VP / cross-feature interaction. `—` if none.
- Migration: `Yes` only if this VP's answer is known to change after a service already exists on this catalog; `No` otherwise.
```
```example
| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | HasCentralizedRules | Yes / No | Yes requires (VP-Interaction=Yes OR VP-State=Yes) | `solution-domain-rules` | — | No |
```

# Plateau Map derivation
```hint
One row per plateau already in the catalog. State the exact VP-answer combination that plateau fixes, in the same VP IDs used above. Cross-check every row against every Constraint above — a plateau whose combination violates a stated Constraint is a defect, not a footnote.
```

| Plateau | VP answers fixed |
| --- | --- |
```example
| Plateau | VP answers fixed |
| --- | --- |
| `plateau-shared-rules` | `VP1=Yes` (requires `VP-Interaction=Yes` or `VP-State=Yes`, satisfied via its parent chain) |
```
