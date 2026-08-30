# Application condition

An **Application condition** is the boolean condition, stated over chosen [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/variant.md|Variants]], under which a [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/glossary/delta-module.md|Delta module]] is applied during assembly.

## Why it exists
A Variability Map states which combinations of Variants are legal; an Application condition is what turns that table row into something an assembly step can actually check — "is this delta relevant to the combination just chosen or not."

## How it works
It is the executable translation of one Variability Map row's Realized-by relationship: if the row says Variant `Yes` of `VP3` is Realized by `solution-domain-rules`, that solution's delta's Application condition is `VP3 = Yes`.

## How it is structured
Usually a conjunction over the specific VPs a delta actually depends on — never over every VP in the catalog. A [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/glossary/conflict-resolving-delta.md|conflict-resolving delta]]'s Application condition is the conjunction of every VP its resolved solutions require (e.g. `S=Yes AND E=Yes`), keeping growth local to the pair (or group) that actually intersects.

## Example
`solution-domain-rules`'s Application condition is `HasCentralizedRules = Yes`; a resolver for a State/ExternalIntegration intersection would carry `HasState = Yes AND HasExternalIntegration = Yes`.

## Related concepts
- [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/glossary/delta-module.md|Delta module]]
- [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/realized-by.md|Realized by]]

## Sources
- Schaefer, I., Bettini, L., Bono, V., Damiani, F., Tanzarella, N. (2010). "Delta-Oriented Programming of Software Product Lines." *SPLC 2010*.
