---
name: intersection registry design
description: File format and placement for recording a classified delta intersection
problem: How and where should the result of classifying an intersection between two or more solutions be recorded, so it is discoverable and does not go stale?
decision: One file per conflicting element, in a `registry/` folder sibling to `adr/` inside the plateau where all intersecting solutions are first simultaneously present, listed in that plateau's `registry:` YAML property — mirroring the existing `adr/` convention exactly.
tags:
  - concern/architecture
  - stack
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Once [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]]'s classifier assigns a code to a group of solutions sharing an `element/{element-name}` tag, the result (the code, the ordering, the resolution) needs to be recorded somewhere durable — otherwise the same classification work gets silently redone the next time someone touches one of the intersecting solutions. This repository's prior design conversation (recorded before this skill existed) left two open questions: per-element files, or one shared document per plateau/catalog; and where — attached to the catalog root, or to a specific plateau.

# Selected variant
[[#Per-element files inside the owning plateau's registry folder (selected)]]

# Searched variants

## Per-element files inside the owning plateau's registry folder (selected)

### Description
One file per conflicting element (`registry/{element-name}.md`), placed inside the plateau where every intersecting solution is first simultaneously present in `created_by` (directly or via `parent_plateaus`) — the same placement rule `plateau-create-by-solutions` already uses for its own conflict ADRs. Listed in that plateau root skill's `registry:` YAML property, exactly mirroring how `adr:` already lists that plateau's ADR files.

### Benefits
- Reuses a convention this repository already trusts (`adr/`) instead of inventing an unrelated new one — same folder shape, same registration-in-YAML-property pattern, same "list every file, link from the body" discipline `adr-create.skill.md` already enforces for ADRs.
- One file per element keeps a single classification small and independently reviewable/updatable, the same benefit `adr-create.skill.md` states for "one ADR per decision instead of mixing several into one file."
- Placing it at the plateau where the intersection first becomes real (not at the catalog root) keeps it next to the actual composed context the classification was made against — the same reasoning `plateau-create-by-solutions` already uses to place conflict ADRs at the composing plateau rather than at each individual solution.

### Costs
- A catalog-wide view of every intersection requires reading across every plateau's `registry/` folder rather than one file — mitigated by each entry being small and tagged `element/{element-name}`, so a tag-based query can still assemble a catalog-wide view without a hand-maintained index file.

## One shared document per plateau or catalog

### Description
A single `intersections.md` (or similar) per plateau, or one per whole catalog, listing every classified group as a row in one table.

### Benefits
- One file to open for a full picture of every intersection in scope, no need to enumerate a folder.

### Costs
- Every classification update touches the same shared file, producing merge friction as more solutions are added over time — the same problem `adr-create.skill.md` already avoids by choosing one-file-per-decision over a single decisions log.
- A single catalog-wide document does not have an obvious plateau to "belong to," reintroducing the same placement ambiguity `adr/` avoided by being scoped per plateau from the start.
