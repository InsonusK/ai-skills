# Web-service common Variability Map

The Variation Points every backend web-service catalog shares, whatever its stack. Each one is defined here once — question, Variants, Constraint, Realization depends on, and the concept behind them — and **inherited** by every bound stack map, which adds only its State, its narrowing, and its `Realized by`. Rules: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#common-variation-points|variability-map-create — Common Variation Points]]. Derived from, and kept consistent with, [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/templates/web-service-common-features/web-service-common-features|web-service-common-features]].

A VP enters this map only through [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md#how-to-admit-a-common-variation-point|admission]]: discussed with the owner and designed on every bound stack in the same change. The map starts empty and grows one VP at a time.

## Common Variation Points

| ID | VP | Variants | Constraint | Realization depends on |
| --- | --- | --- | --- | --- |

## Bound stack maps

Every map below carries every row of the table above in its own `## Common Variation Points` table. Plain paths, not links — this file belongs to a stack-agnostic skill.

- `skills/go/architecture/variability-map.md`
- `skills/dotnet/architecture/variability-map.md`
