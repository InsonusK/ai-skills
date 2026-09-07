# Delta-Oriented Programming (DOP)

**Delta-Oriented Programming** is a way of physically assembling one specific product out of a shared base plus a set of independently-written changes ("deltas"), each active only when its condition holds. Introduced by Schaefer, Bettini, Bono, Damiani, and Tanzarella (SPLC 2010).

## Why it exists
An [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/orthogonal-variability-model|Orthogonal Variability Model]] table says *which* combinations of Variants are legal, but not *how to build the code* for a chosen combination. DOP answers that: start from one real, running [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/core-module|Core module]], and apply every [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/delta-module|Delta module]] whose [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/application-condition|Application condition]] holds for the chosen Variants.

## How it works
Assembly = Core module, plus every Delta module whose Application condition is true, applied in dependency order. Adding a new Variation Point is one new delta with its own condition — existing deltas are never rewritten to accommodate it.

## How it is structured
- **Core module** — one concrete, runnable baseline.
- **Delta module** — add/remove/modify operations layered on the core, gated by an Application condition.
- Two deltas can land on the same artifact — see [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill|delta-conflict-detection]] for how that is classified and, when needed, resolved.

## Example
In `skills/dotnet/architecture/v3`, a plateau's `structure/` is the core, and each `created_by` solution's `Implementation/*.create.md`/`*.extend.md` files are its deltas — this repository already builds plateaus this way; DOP is the name for the mechanism it already uses.

## Related concepts
- [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/core-module|Core module]]
- [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/delta-module|Delta module]]
- [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/application-condition|Application condition]]
- [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/orthogonal-variability-model|Orthogonal Variability Model]]

## Sources
- Schaefer, I., Bettini, L., Bono, V., Damiani, F., Tanzarella, N. (2010). "Delta-Oriented Programming of Software Product Lines." *SPLC 2010*.
