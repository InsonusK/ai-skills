---
name: intersection registry design
description: File format and placement for recording a classified delta intersection
problem: How and where should the result of classifying an intersection between two or more solutions be recorded, so it is discoverable and does not go stale?
decision: One file per conflicting element, at the catalog root's `registry/` folder (sibling to `variability-map.md` and `plateau/`), holding the current, cumulative analysis plus a growth-history table across the plateaus that touched it — every plateau that includes the element links to this one file via its own `registry:` YAML property.
tags:
  - concern/architecture
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Once [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill|delta-conflict-detection]]'s classifier assigns a code to a group of solutions sharing an `element/{element-name}` tag, the result (the code, the ordering, the resolution) needs to be recorded somewhere durable — otherwise the same classification work gets silently redone the next time someone touches one of the intersecting solutions. This repository's prior design conversation (recorded before this skill existed) left two open questions: per-element files, or one shared document per plateau/catalog; and where — attached to the catalog root, or to a specific plateau.

# Selected variant
[[#Per-element files at the catalog root, cumulative across plateaus (selected)]]

# Searched variants

## Per-element files at the catalog root, cumulative across plateaus (selected)

### Description
One file per conflicting element (`{catalog}/registry/{element-name}.md`), at the catalog root — a sibling of `variability-map.md` and `plateau/`, exactly mirroring how [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/one-map-per-catalog|variability-map.md is one file per catalog, not per plateau]] and how `plateau-repository.md` is likewise one file per catalog. The file holds the *current* (deepest-known) classification and involved-solutions list, plus a compact growth-history table (one row per plateau where the intersecting set changed: plateau name, N, what changed, how it was ground-truth verified in that plateau's own `example/`). Every plateau whose `created_by`/`parent_plateaus` includes the element links to this same file from its own `registry:` YAML property — the file is not owned by any one plateau.

### Benefits
- Consistent with the pipeline's own established precedent: this is the third artifact in the pipeline (after the Variability Map and the Plateau Repository) that answers "one shared fact, scoped to the whole catalog" — keeping registry scoped per-plateau was the one place this pipeline duplicated instead of centralizing, found during a retro on the go-web-service-plateau-map build.
- Eliminates the duplication the per-plateau variant produced in practice: a linear 5-plateau catalog produced 5 near-identical copies of the same element's classification, differing only in N and the involved-solutions list, restating the same `Constraint`/`Category`/`Kind` reasoning each time.
- A single source of truth per element removes the risk of an insight discovered at a deeper plateau (a nuance in the classification, a corrected Ordering) silently failing to propagate back into the shallower plateaus' own copies, since there are no other copies to fall out of sync.
- A catalog-wide view of every intersection is now a single folder listing (`{catalog}/registry/`), with no need for tag-based query tooling to reconstruct it — directly resolving the previous variant's own stated cost.

### Costs
- The plateau where an intersection *first became real* is no longer encoded by the file's physical location — it must be stated explicitly in the file's growth-history table instead. Mitigated: this is strictly more informative than the old variant, since the old variant only showed a snapshot at one depth per file, never the full growth arc in one place.
- A per-plateau ground-truth verification note ("verified by building this plateau's own `example/`") can no longer be a single freeform paragraph per file — it becomes one growth-history table row per plateau instead. Mitigated: the table format is arguably clearer than N nearly-identical paragraphs were.
- Existing catalogs built under the previous (per-plateau) variant needed migration — done for `dotnet`, `angular`, and `go` as part of adopting this decision; any future catalog starts directly in this shape.

## Per-element files inside the owning plateau's registry folder (previously selected, superseded)

### Description
One file per conflicting element (`registry/{element-name}.md`), placed inside the plateau where every intersecting solution is first simultaneously present in `created_by` (directly or via `parent_plateaus`) — the same placement rule `plateau-create-by-solutions` already uses for its own conflict ADRs. Listed in that plateau root skill's `registry:` YAML property, exactly mirroring how `adr:` already lists that plateau's ADR files.

### Benefits
- Reuses a convention this repository already trusts (`adr/`) instead of inventing an unrelated new one — same folder shape, same registration-in-YAML-property pattern, same "list every file, link from the body" discipline `adr-create.skill.md` already enforces for ADRs.
- One file per element keeps a single classification small and independently reviewable/updatable, the same benefit `adr-create.skill.md` states for "one ADR per decision instead of mixing several into one file."
- Placing it at the plateau where the intersection first becomes real (not at the catalog root) keeps it next to the actual composed context the classification was made against — the same reasoning `plateau-create-by-solutions` already uses to place conflict ADRs at the composing plateau rather than at each individual solution.

### Costs
- A catalog-wide view of every intersection requires reading across every plateau's `registry/` folder rather than one file — mitigated by each entry being small and tagged `element/{element-name}`, so a tag-based query can still assemble a catalog-wide view without a hand-maintained index file. In practice this mitigation was never built as real tooling, and the folder-per-plateau shape instead produced growing duplication as a catalog's plateau chain deepened (5 near-identical files for one element in a 5-plateau linear catalog) — the concrete cost that led to superseding this variant.
- Every plateau in a lineage chain that inherits (rather than changes) an intersection still needed its own copy restating the same analysis, with no mechanism to say "unchanged, see the parent's entry" other than prose.

## One shared document per plateau or catalog

### Description
A single `intersections.md` (or similar) per plateau, or one per whole catalog, listing every classified group as a row in one table.

### Benefits
- One file to open for a full picture of every intersection in scope, no need to enumerate a folder.

### Costs
- Every classification update touches the same shared file, producing merge friction as more solutions are added over time — the same problem `adr-create.skill.md` already avoids by choosing one-file-per-decision over a single decisions log.
- A single catalog-wide document does not have an obvious plateau to "belong to," reintroducing the same placement ambiguity `adr/` avoided by being scoped per plateau from the start. (This objection no longer applies to the selected variant, which is per-*element*, not per-catalog-as-one-document — it avoids this cost while still being catalog-scoped.)
