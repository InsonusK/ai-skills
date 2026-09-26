# How Apply this template
1. Replace `{catalog-name}` with the catalog's own name (e.g. the plateau/solution tree's top folder name).
2. For a backend web-service catalog, fill `## Common Variation Points` with one row per row of the [web-service common map](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md), per [variability-map-create's Common Variation Points](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#common-variation-points). For any other catalog, delete that section.
3. Fill `## Stack Variation Points` with one row per Variation Point of this family's own, following [../variability-map-create.skill.md](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md)'s "How to build a Variability Map" — fill VP, Variants, Constraint, Realization depends on, and Migration directly.
4. Fill the **Realized by** column by running [delta-conflict-detection](skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md): it walks each VP, authors or selects the realizing solution(s), and classifies their intersections. The map is not finished until this runs.
5. Remove this `# How Apply this template` section and every `hint`/`example` block before saving as `{catalog}/variability-map.md`.

The plateau↔VP view (which plateau realizes which VPs) is **not** part of this map — it lives in `{catalog}/plateau/plateau-repository.md`, built by [plateau-map-create](skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/plateau-map-create.skill.md).

# {catalog-name} Variability Map

## Common Variation Points

| ID | VP | State | Stack delta | Realized by | Migration |
| --- | --- | --- | --- | --- | --- |
```hint
One row per row of the web-service common map — every one, none skipped.
- ID: the common `VP-C###` ID, linked to its concept section in the common map.
- VP: the common VP's name only — never its question, Variants, or Constraint.
- State, Stack delta, Realized by: per variability-map-create's "Give every common row a State" table.
```
```example
| ID | VP | State | Stack delta | Realized by | Migration |
| --- | --- | --- | --- | --- | --- |
| [VP-C003](skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map.md#vp-c003-examplevp) | ExampleVP | Refined | SQLite unsupported — the family's persistence ADR picks PostgreSQL only | PostgreSQL → `solution-persistent-db` | No |
```

## Stack Variation Points

| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
```hint
One row per Variation Point of this family's own — never one a common VP already answers.
- ID: VP1, VP2, ... — stable once assigned, never renumbered when a VP is removed (leave a gap instead).
- VP: the question, in one line, phrased so a reader can answer it Yes/No or by naming a Variant.
- Variants: the legal answers. `Yes / No` for a boolean VP; the named list for a categorical VP.
- Constraint: a real depends_on/built_on_plateau edge or stated prose requirement gating which Variant combinations are legal. `—` if none.
- Realized by: wikilink(s) to the solution skill(s) that implement each Variant, filled by the delta-conflict-detection step. For a categorical VP, show the mapping (e.g. `Variant A → solution-x + solution-y`).
- Realization depends on: cross-VP relationships that change code *shape* without gating legality — mandatory sub-feature / orthogonal VP / cross-feature interaction. `—` if none.
- Migration: `Yes` only if this VP's answer is known to change after a service already exists on this catalog; `No` otherwise.
```
```example
| ID | VP | Variants | Constraint | Realized by | Realization depends on | Migration |
| --- | --- | --- | --- | --- | --- | --- |
| VP1 | HasCentralizedRules | Yes / No | Yes requires (VP-Interaction=Yes OR VP-State=Yes) | `solution-domain-rules` | — | No |
```
