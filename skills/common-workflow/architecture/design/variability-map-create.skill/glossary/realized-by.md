# Realized by

**Realized by** is the link from a [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/variant.md|Variant]] to the actual code that implements it — in this repository, one or more solution skills. It is a reference, never a copy: the Variability Map states *which* solutions realize a Variant, not *what* those solutions do.

## Why it exists
Without an explicit Realized-by link, "which code do I need for this answer?" has to be re-derived by reading solution descriptions every time. Keeping the variability model (the table) orthogonal to the artifact model (the solutions) — connected only by this named link — is the core move of the Orthogonal Variability Model.

## How it works
Realized by holds one or more wikilinks to solution skills. A categorical VP's Variant is often realized by a *combination* of solutions (e.g. "apply both X and Y"), not just one.

## How it is structured
Written in the Variability Map's `Realized by` column, e.g. `` `solution-domain-rules` `` for a single-solution Variant, or `External Mutable → solution-entity-concurrency-change + solution-external-created-entity` for a combination.

## Example
`VP2 — HttpApi`, Variant `Yes`, is Realized by `` `solution-http-api-publication` `` — a direct pointer to the solution skill, not a restatement of what it builds.

## Related concepts
- [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/variant.md|Variant]]
- [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/variation-point.md|Variation Point]]

## Sources
- Pohl, K., Böckle, G., van der Linden, F. (2005). *Software Product Line Engineering*, chapter on the Orthogonal Variability Model.
