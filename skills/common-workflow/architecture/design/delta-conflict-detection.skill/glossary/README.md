# Glossary

Terms used by [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]]. Shared Variability Map terms (Variation Point, Variant, Realized by, Feature Model, Orthogonal Variability Model) live in [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/README.md|variability-map-create's own glossary]] instead of being duplicated here.

| Term | Page | Covers |
| --- | --- | --- |
| Application condition | [application-condition.md](./application-condition.md) | The boolean condition under which a delta is applied |
| Conflict-resolving delta | [conflict-resolving-delta.md](./conflict-resolving-delta.md) | A delta whose only job is to fix the seam between two intersecting deltas |
| Core module | [core-module.md](./core-module.md) | The runnable baseline every delta is layered on top of |
| Delta module | [delta-module.md](./delta-module.md) | A self-contained, conditionally-applied change over the core |
| Delta-Oriented Programming | [delta-oriented-programming.md](./delta-oriented-programming.md) | Assembling one product from a core plus conditional deltas |
| Hard conflict / soft conflict | [hard-soft-conflict.md](./hard-soft-conflict.md) | Formally incompatible deltas vs. deltas that apply but disagree semantically |
