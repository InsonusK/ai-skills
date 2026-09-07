# Conflict-resolving delta

A **conflict-resolving delta** is a delta whose only purpose is to make the result of applying two (or more) intersecting deltas independent of the order they were applied in — it does not implement a Variant of its own, only the seam between two that already exist. Term from Clarke, Helvensteijn, and Schaefer's Abstract Delta Modeling (2010).

## Why it exists
When two deltas both touch the same artifact, applying "delta A then B" can give a different result than "B then A" — or worse, neither order gives a semantically correct result at all (see [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/hard-soft-conflict|hard/soft conflict]]). A conflict-resolving delta is a third, explicit delta that fixes the seam, so the combined result is correct and no longer depends on which of the two original deltas happened to apply first.

## How it works
It `depends_on` every delta (solution) it resolves, is applied after all of them, and is never folded into any one of the originals — each original solution must stay usable on its own, without the resolver, when the other is not selected.

## How it is structured
In this repository's [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill|delta-conflict-detection]] workflow, this is exactly what gets built for a `TMC`, `FMC`, or `FDC` classification — never for a canonical code.

## Example
A resolver named `solution-handler-state-and-external-integration`, `depends_on` on `solution-handler-state` and `solution-handler-external-integration`, fixing the one line where the second delta used a stale ID the first delta's addition made obsolete.

## Related concepts
- [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/hard-soft-conflict|Hard conflict / soft conflict]]
- [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/delta-module|Delta module]]

## Sources
- Clarke, D., Helvensteijn, M., Schaefer, I. (2010). "Abstract Delta Modeling." *GPCE 2010*.
