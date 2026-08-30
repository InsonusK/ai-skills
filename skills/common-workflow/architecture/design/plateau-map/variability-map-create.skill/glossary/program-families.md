# Program Families

A **Program Family** is a set of programs similar enough that it pays to design them together, up front, rather than build each one from scratch. David Parnas named the idea in 1976: before building anything, split every requirement into **Commonality** (what every member of the family has, unconditionally) and **Variability** (what differs from one member to the next).

## Why it exists
Without this split, a second, similar service usually gets built by cloning the first one and tweaking it. Commonality and variability end up tangled inside the clone, so a change to the shared part (e.g. how logging works) has to be repeated by hand in every clone. Naming the split up front is what makes a catalog of plateaus and solutions possible at all — it is the precondition every later technique (Feature Model, OVM, Delta-Oriented Programming) builds on.

## How it works
For a given family, write two lists: "this is the same in every member" (Commonality) and "this differs between members" (Variability). Only the second list needs any further modeling — Commonality is simply built once and never revisited per member.

## How it is structured
- **Commonality** — content that appears in every plateau in a catalog, regardless of which solutions were added. It never becomes a Variation Point row.
- **Variability** — content that differs between at least two legitimate members of the family. Every [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variation-point|Variation Point]] in a Variability Map exists because it was placed on this list.

## Example
In `skills/dotnet/architecture/v3`, every plateau shares a fixed module shape, a DI container, and a health-check endpoint — Commonality, never a VP row. Whether a module exposes an HTTP API is Variability — different modules on the same catalog answer it differently, so it becomes VP2 (`HttpApi`) in the Variability Map.

## Related concepts
- [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variation-point|Variation Point]]
- [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/feature-model|Feature Model]]

## Sources
- Parnas, D. L. (1976). "On the Design and Development of Program Families." *IEEE Transactions on Software Engineering*.
