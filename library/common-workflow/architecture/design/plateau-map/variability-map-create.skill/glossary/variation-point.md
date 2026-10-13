# Variation Point (VP)

A **Variation Point** is one question about a system that different teams could legitimately answer differently — for example "does this module expose an HTTP API?". It is the central concept of the Orthogonal Variability Model (Pohl, Böckle, van der Linden, 2005).

## Why it exists
A [[library/common-workflow/architecture/design/plateau-map/feature-map-create.skill/glossary/feature-model|Feature Model]] draws *what* the questions are, as a tree. It does not say *where the code lives* that answers each question — writing that link directly into the tree makes the tree bloat and tangle with implementation detail. A Variation Point is the OVM answer: keep the question as a row in a table, kept **orthogonal** (separate) from the code, and connect the two only through an explicit [[library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/realized-by|Realized by]] link.

## How it works
Each VP is one row of a Variability Map: an ID, the question itself, its legal [[library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variant|Variants]], any Constraint gating which Variants are legal together with other VPs, and the Realized-by link(s) to the code that implements each Variant.

## How it is structured
A VP never appears alone — it always has:
- One or more [[library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variant|Variants]] (the legal answers).
- Zero or more Constraints against other VPs.
- A [[library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/realized-by|Realized by]] link per Variant.

## Example
`VP1 — EntityKind` in `library/dotnet/architecture/v3/variability-map.md`: the question is "what kind of entity is this?", the Variants are the four entity kinds, and each Variant's Realized-by link names the combination of solutions that implements it.

## Related concepts
- [[library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variant|Variant]]
- [[library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/realized-by|Realized by]]
- [[library/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/orthogonal-variability-model|Orthogonal Variability Model]]

## Sources
- Pohl, K., Böckle, G., van der Linden, F. (2005). *Software Product Line Engineering*, chapter on the Orthogonal Variability Model.
