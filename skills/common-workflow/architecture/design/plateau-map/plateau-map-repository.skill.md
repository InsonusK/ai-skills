---
name: plateau-map-repository
description: Define how to maintain a catalog's plateau-map repository — the plateau/ folder's README with the Plateau × VP matrix and lineage — as a derived, checkable view over the Variability Map, updated whenever a plateau or a Variation Point changes
whenToUse: when a plateau is created or its solution set changes (via plateau-create-by-solutions/plateau-update-by-solutions), when a Variation Point is added, changed, or removed in the catalog's variability-map.md, or when reviewing whether the catalog's named plateaus still cover every legitimate VP combination teams actually choose between
updated: 20260906
tags:
  - skill/architecture/variability/design
  - stack
  - concern/architecture
---

# Goal
Maintain `{catalog}/plateau/plateau-repository.md` as a derived, checkable view over the catalog's Variability Map. Concretely:
- **Complete matrix** - One row per plateau folder on disk, one column per Variation Point in [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map.md]] — no stale rows, no missing columns.
- **Cumulative lineage** - Each plateau's VP set equals its parent chain's set plus its own delta, matching `parent_plateaus`.
- **Constraint-checked rows** - Every plateau's VP set verified against every Constraint in the map; violations surfaced, never silently kept.

# Core Principle
- **The map is the source of truth** - The plateau-repository never states VP facts (constraints, realizations, variants) the Variability Map does not state; it re-presents the map plateau-oriented and links back to it.
- **Derived, never remembered** - Every ✅/❌ is computed from the plateau's actual `created_by`/`parent_plateaus` mapped through the map's Realized-by column — not from what a plateau is "about".
- **Two triggers, one owner** - Plateau changes (create/update) and VP changes (map edits) both terminate here; no other file carries the plateau↔VP matrix.

# What the repository holds
`{catalog}/plateau/plateau-repository.md` — a human-facing index (not a skill) containing:
- **Plateau × VP matrix**: rows = plateaus, columns = VP IDs, ✅/❌ cells; answers cumulative down the lineage (a plateau has every VP its parent has, plus its own).
- **Column legend**: one line per VP ID with its name, pointing at the map for full descriptions.
- **Lineage table**: per plateau — `standalone` flag, parent, and the new solutions its `created_by` adds on top of the parent chain, grouped by VP.
- **Shape notes** for VPs that are not plain yes/no per module: per-entity VPs (✅ means *enabled*, not used by every entity) and skeleton/draft VPs (❌ until a real consumer exists).

Worked example: [[skills/dotnet/architecture/plateau/plateau-repository.md|skills/dotnet/architecture/plateau/plateau-repository.md]].

# How to update
1. Read `{catalog}/variability-map.md` fresh: VP IDs, Constraints, Realized by.
2. Enumerate the plateau folders on disk (`{catalog}/plateau/plateau-*/`); compute each plateau's VP set from its `created_by` (plus `parent_plateaus` transitively), mapped through the map's Realized-by column.
3. Cross-check every plateau's VP set against every Constraint in the map (see [Constraint check on every update](#constraint-check-on-every-update)).
4. Rewrite the matrix, legend, and lineage from the computed data — recompute, never patch a single cell from memory.
5. Annotate special VP shapes (per-entity, skeleton/draft) so they do not read as plain ✅/❌.

# Rule

## MUST

### Derived from the map, never restating it
State no VP fact in the README that the Variability Map does not state — constraints, realizations, and variants live there; the README links back.
- Risk: two homes for one fact drift; a reader cannot tell which is authoritative.
- Fix: the legend and every note carry VP IDs and link the map; prose explains presentation, not content.

### Every plateau and every VP present
Keep one matrix row per plateau folder on disk and one column per VP in the current map — a new plateau, a removed plateau, or a changed VP ID set must all show up after the update.
- Risk: a missing row or column makes the matrix silently stale exactly where the catalog moved.
- Fix: recompute rows and columns from disk and the map on every update, never from the previous README.

### Cumulative consistency with the lineage
Make each plateau's ✅ set equal its parent's set plus its own `created_by` delta, with the parent chain taken from `parent_plateaus`.
- Risk: a row that lists a VP the parent chain contradicts (or omits one the parent already realizes) teaches a wrong model of what composing that plateau means.
- Fix: walk `parent_plateaus` transitively before filling a row; flag any plateau whose set is not a superset of its parent's for the owner.

### Constraint check on every update
Cross-check every plateau's VP set against every Constraint in the map, and treat a violation as a defect — raised as a plateau-level ADR per [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md#Recording plateau-level decisions|plateau-create-by-solutions]], never a silent exception.
- Risk: an inconsistent plateau ships unnoticed because nothing ever checked it against the table meant to make combinations explicit.
- Fix: derive every row from the plateau's actual `created_by`/`parent_plateaus` and check it against every Constraint row before saving the README.

### Both triggers covered
Update the README when a plateau is created or its solution set changes, and when a VP is added, changed, or removed in the map — the second trigger fires even though no plateau changed.
- Risk: a VP rename or removal leaves a ghost column (or a missing one) because no plateau edit happened to drag the README along.
- Fix: run the update after either event; the map's change log entry is a trigger, not only plateau work.

## SHOULD

### Legend over naked IDs
Give the column legend a one-line name per VP and a link to the map, instead of leaving readers to decode `VP7`.
- Risk: a matrix of bare IDs is readable only with the map open in a second tab — the README fails as a standalone index.
- Fix: one line per VP, name plus link; details stay in the map.

### Mark special VP shapes
Annotate per-entity VPs and skeleton/draft VPs explicitly instead of letting them read as plain ✅/❌.
- Risk: a ✅ on a per-entity VP overpromises ("every entity has it"); a ❌ on a skeleton VP underpromises ("never available") — both mislead.
- Fix: one shape note per special VP under the matrix.

## MAY

### Extra views allowed
Add sections beyond the matrix — folder contents, registry notes, counts, historical reference mappings — as long as they restate no VP facts from the map.

# Check list
- [ ] Matrix rows match the plateau folders on disk; columns match the VP IDs in the current `variability-map.md`.
- [ ] Every row's ✅ set verified against the plateau's actual `created_by` + transitive `parent_plateaus`.
- [ ] Every row cross-checked against every Constraint in the map; violations raised as plateau-level ADRs.
- [ ] The legend names every VP in one line and links the map as the source of truth.
- [ ] Per-entity and skeleton/draft VPs are annotated, not presented as plain ✅/❌.
- [ ] Facet tags follow [[skills/common-workflow/skill-design.skill/facet-vocabulary.md|facet-vocabulary]]: `concern/architecture`, bare `stack`.
