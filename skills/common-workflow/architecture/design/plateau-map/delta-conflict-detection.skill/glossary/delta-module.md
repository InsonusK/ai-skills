# Delta module

A **Delta module** is a self-contained set of add/remove/modify operations layered on top of a [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/core-module|Core module]] (or on top of another delta already applied), active only when its [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/application-condition|Application condition]] holds.

## Why it exists
Without deltas, supporting a new Variant would mean maintaining a whole separate copy of the product for it. A delta captures only the *difference* a Variant introduces, so adding a Variant costs one small delta, not a new full copy.

## How it works
A delta's Application condition is checked against the chosen Variants; if true, the delta's operations (add a class, extend a method, register a DI substitution) are applied. Two deltas can land on the same artifact — see [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill|delta-conflict-detection]] for how that case is classified.

## How it is structured
In this repository, a delta is one solution's `Implementation/*.create.md` (introduces an artifact) or `*.extend.md` (modifies one already introduced by the core or another delta).

## Example
`solution-domain-rules`'s `Implementation/{ValueObject}.cs.extend.md` is a delta: it modifies a class `solution-value-objects`'s own delta already created, active only when centralized rules are selected.

## Related concepts
- [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/core-module|Core module]]
- [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/application-condition|Application condition]]
- [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/glossary/conflict-resolving-delta|Conflict-resolving delta]]

## Sources
- Schaefer, I., Bettini, L., Bono, V., Damiani, F., Tanzarella, N. (2010). "Delta-Oriented Programming of Software Product Lines." *SPLC 2010*.
