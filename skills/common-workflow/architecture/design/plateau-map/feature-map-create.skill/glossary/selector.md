# Selector

A **selector** is a pure selector: an entry that maps combinations of other features to an outcome but has no independent yes/no choice of its own. Its value is fully determined by which other features are selected.

## Why it exists
A row in the Features table implies an independent choice — "is this feature present, yes or no?". A selector has no such choice: ask its question and the answer is always "it depends on these other features." Giving it a row (or a diagram node) would invent variability that does not exist.

## How it works
When a candidate turns out to be a pure function of other features' selections, exclude it from the Features table and from the diagram, and instead explain it in a short prose note under the table, next to the feature(s) it maps.

## How it is structured
No table row, no diagram node — one prose note naming the selector and the features whose combination determines it.

## Example
An "API surface profile" entry that is fully determined by which API features are selected (`HttpApi`, `GrpcApi`): there is nothing to select independently, so it gets a prose note, not a row.

## Related concepts
- [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/glossary/mandatory-companion|Mandatory companion]] — the other kind of entry that gets prose instead of a row
- [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/glossary/feature-model|Feature Model]]
