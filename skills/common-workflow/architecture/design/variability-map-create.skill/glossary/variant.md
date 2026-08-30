# Variant

A **Variant** is one legal answer to a [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/variation-point.md|Variation Point]]. A boolean VP has two Variants, `Yes` and `No`; a categorical VP has one Variant per named option.

## Why it exists
A VP states that a question exists; a Variant states one specific answer to it. Separating the two lets a Constraint or a [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/realized-by.md|Realized by]] link attach to one specific answer without having to restate the whole question.

## How it works
Each Variant of a VP gets its own [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/realized-by.md|Realized by]] entry — the solution(s) that make that specific answer real in code. Two Variants of the same VP are always mutually exclusive for one instance of whatever the VP describes (a module cannot be both "Internal Immutable" and "External Mutable" at once).

## How it is structured
Listed in the Variability Map's `Variants` column, one VP row wide, e.g. `Internal Immutable / External Immutable / Internal Mutable / External Mutable`.

## Example
For `VP1 — EntityKind`, `External Mutable` is one Variant, realized by applying both `solution-entity-concurrency-change` and `solution-external-created-entity` together.

## Related concepts
- [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/variation-point.md|Variation Point]]
- [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/realized-by.md|Realized by]]

## Sources
- Pohl, K., Böckle, G., van der Linden, F. (2005). *Software Product Line Engineering*, chapter on the Orthogonal Variability Model.
