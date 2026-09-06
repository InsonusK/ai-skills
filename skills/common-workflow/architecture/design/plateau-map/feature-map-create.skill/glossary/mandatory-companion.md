# Mandatory companion

A **mandatory companion** is an entry that is always applied together with another feature (its host) and never independently — nothing in the family applies one without the other.

## Why it exists
A row in the Features table implies an independent yes/no choice. A mandatory companion has no such choice: its host's verdict already decides it. Modeling it as a separate feature would imply a freedom the family does not have.

## How it works
When a candidate is always present exactly when its host is present, exclude it from the Features table and instead explain it in a short prose note under the table, next to the host feature. Unlike a bundled child (drawn nested inside its parent's block with a `Mandatory` relation), a companion is not decomposed under the host at all — it is an aspect of the host, not a sub-feature of it.

## How it is structured
No table row — one prose note naming the companion and its host feature.

## Example
`skills/dotnet/architecture/v3.1/feature/feature-model.md`: the Cecil architecture tests (structural build-time guarantees over the rule mechanism) are a mandatory companion of `CentralizedRules` — the catalog never applies one without the other, so they are a prose note, not a feature row.

## Related concepts
- [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/glossary/selector|Selector]] — the other kind of entry that gets prose instead of a row
- [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/feature-model|Feature Model]]
