# Core module

The **Core module** is the one concrete, fully-runnable, minimal baseline that every [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/glossary/delta-module.md|Delta module]] is layered on top of, in [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/glossary/delta-oriented-programming.md|Delta-Oriented Programming]].

## Why it exists
Deltas need a real starting point to apply to — not an abstract skeleton, but something that already runs. Choosing the core deliberately is what lets every other Variation Point's delta stay small: it only needs to state what changes relative to a known-working baseline.

## How it works
The core is built once, contains only genuine [[skills/common-workflow/architecture/design/variability-map-create.skill/glossary/program-families.md|Commonality]], and is never itself gated by an Application condition — it is always present.

## How it is structured
In this repository, a plateau's `structure/` folder before any `created_by` solution is applied plays this role — the deepest plateau in a `parent_plateaus` chain is effectively the family's core.

## Example
`plateau-stateless-non-interactive-service` in `skills/dotnet/architecture/v3` — a fixed module shape, DI container, and health-check endpoint present in every plateau built on top of it.

## Related concepts
- [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/glossary/delta-module.md|Delta module]]
- [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/glossary/delta-oriented-programming.md|Delta-Oriented Programming]]

## Sources
- Schaefer, I., Bettini, L., Bono, V., Damiani, F., Tanzarella, N. (2010). "Delta-Oriented Programming of Software Product Lines." *SPLC 2010*.
