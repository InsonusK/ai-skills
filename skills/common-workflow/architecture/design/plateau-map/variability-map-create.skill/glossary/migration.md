# Migration

**Migration** is the last column of a [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variation-point|Variation Point]] row. It answers one yes/no question: *can a service that already runs in production, and already picked an answer to this VP, later switch to a different answer in place — with live users and real data — or is the answer fixed once the service is designed?*

- **`No`** — the answer is **design-time bound**. A team chooses it when the service is first built; changing it afterwards is not a supported in-place evolution, it is effectively a rewrite. This is the default.
- **`Yes`** — a real service on this catalog has been observed making this transition in production. The solution that realizes the VP must therefore carry a migration path (schema migration, data backfill, feature flag, staged rollout, temporary backward compatibility), not only a from-scratch story.

## Why it exists
Every other column of the map describes the **design space** — which answers exist, which are legal together, what realizes them. Migration describes something orthogonal: the **binding time** of the choice. A VP with no constraints at all can still be impossible to change on a running system; a tightly constrained VP can still be trivial to migrate. Recording this once, in the map, tells whoever writes the realizing solution whether an in-place transition is a scenario they must design for.

## How it works
Set `Yes` **only on a transition that has actually been observed** for a real service on this catalog — never speculatively ("a team could probably add this later" is not evidence). If `Yes` were allowed on speculation, almost every VP would earn it and the column would carry no signal. When no such transition has ever happened, the value is `No`.

`Migration` is an analysis field: it is read by a human or an agent when authoring a solution or assembling a plateau. No downstream step consumes it automatically.

## How it is structured
A single `Yes` / `No` in the `Migration` column. A `Yes` should be accompanied by a short note naming the observed transition; a `No` needs no note.

## Example (`Migration = No`)
Consider a VP **"PrimaryDatastore"** with Variants `relational` / `document`.

A service picks one answer when it is designed. Once it is in production with real data, flipping the answer means migrating every stored record into a different data model, rewriting every query and mapping layer, and running both stores in parallel during cutover — the effort and risk of building a new service, not of evolving the existing one. No team on the catalog has ever done this to a live service.

So the row's `Migration` is `No`: the choice is bound at design time. This does **not** mean the VP is unimportant or that both Variants are not fully supported — only that a given service commits to its answer up front and is not expected to change it in place.

## Related concepts
- [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variation-point|Variation Point]]
- [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variant|Variant]]
- [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/realized-by|Realized by]]

## Sources
- Pohl, K., Böckle, G., van der Linden, F. (2005). *Software Product Line Engineering* — binding time as a property of a variability decision.
