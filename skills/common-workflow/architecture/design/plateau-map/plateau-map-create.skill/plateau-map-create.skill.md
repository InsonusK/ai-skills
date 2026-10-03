---
name: plateau-map-create
description: Define how to build and maintain a catalog's plateau repository — {catalog}/plateau/plateau-repository.md with the Plateau × VP matrix and lineage — as a derived, checkable view over a fully-filled Variability Map, updated whenever a plateau or a Variation Point changes
whenToUse: when a plateau is created or its solution set changes (via plateau-create-by-solutions/plateau-update-by-solutions), when a Variation Point is added, changed, or removed in the catalog's variability-map.md, or when reviewing whether the catalog's plateaus still cover every legitimate VP combination teams actually choose between, or when a plateau's code must be assigned or changes with its VP combination
updated: 20260926
tags:
  - skill/architecture/variability/design
  - stack
  - concern/architecture
adr:
  - "[[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/adr/plateau-code-by-combination|Plateaus are coded by their VP combination, not named by topic]]"
---

# Goal
- **Maintained repository file** - `{catalog}/plateau/plateau-repository.md` kept as a derived, checkable view over the catalog's Variability Map.
- **Complete matrix** - One row per plateau folder on disk, one column per Variation Point in [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map.md]] — no stale rows, no missing columns.
- **Cumulative lineage** - Each plateau's VP set equals its parent chain's set plus its own delta, matching `parent_plateaus`.
- **Coded plateaus** - Every plateau carrying a code `{stack}{kind}{common}.{specific}` derived from its VP combination, with its common part registered in the shared [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/registry/web-service-common-plateaus|registry/web-service-common-plateaus]] and its specific part in the catalog's own `## Stack VP combinations` table.
- **Constraint-checked rows** - Every plateau's VP set verified against every Constraint in the map; violations surfaced, never silently kept.

# Core Principle
- **The map is the source of truth** - The repository file never states VP facts (constraints, realizations, variants) the Variability Map does not state; it re-presents the map plateau-oriented and links back to it.
- **Derived, never remembered** - Every ✅/❌ is computed from the plateau's actual `created_by`/`parent_plateaus` mapped through the map's Realized-by column — not from what a plateau is "about".
- **The code is the combination** - A plateau's code names its VP combination, not its topic: the same common-VP combination has the same `{common}` number in every stack, the same stack-VP combination the same `{specific}` number within a catalog, and the descriptive name survives only as the matrix's Title column.
- **Two triggers, one owner** - Plateau changes (create/update) and VP changes (map edits) both terminate here; no other file carries the plateau↔VP matrix.
- **Starts from a finished map and existing plateaus** - This skill reads a Variability Map built by [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]] (its `Realized by` column filled by that skill's own [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]] sub-step) whose every column is already filled, and plateau folders already created by [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] or changed by [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/plateau-update-by-solutions.skill.md|plateau-update-by-solutions]]. It never authors solutions, plateaus, or the map itself — its only edit to a plateau is recoding one whose combination changed.
- **Plateaus are self-contained** - A plateau's own skill file and its `structure/` files never link to another plateau's skill file in their body — a plateau links only to its own `structure/` files, the solutions in its `created_by`, and its own `registry/`/`adr/` entries. Only the `parent_plateaus`/`created_by` YAML properties name another plateau or solution, and those are read by tooling (this skill, `plateau-create-by-solutions`), never followed as a narrative link an agent must load to understand the plateau.

# Workflow

## What the repository holds
`{catalog}/plateau/plateau-repository.md` — a human-facing index (not a skill) containing:
- **Plateau × VP matrix**: rows = plateaus as `Code | Title | {VP columns}`; one column per common VP (`VP-C###`, cell = the Variant the plateau realizes, e.g. `PostgreSQL` / `None`) and one per stack VP (✅/❌ cells); answers cumulative down the lineage (a plateau has every VP its parent has, plus its own).
- **Stack VP combinations**: `{specific}` number → the set of stack VPs it stands for; `000` = none.
- **Column legend**: one line per VP ID with its name, pointing at the map for full descriptions.
- **Lineage table**: per plateau — `standalone` flag, parent, and the new solutions its `created_by` adds on top of the parent chain, grouped by VP.
- **Shape notes** for VPs that are not plain yes/no per module: per-entity VPs (✅ means *enabled*, not used by every entity) and skeleton/draft VPs (❌ until a real consumer exists).

Worked example: [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/examples/plateau-repository.example|examples/plateau-repository.example.md]].

## Plateau codes
A code is `{stack}{kind}{common}.{specific}` — e.g. `GW003.000`, Go web-service, common combination 003, no stack VPs — built by [Code every plateau](#code-every-plateau) from two registers:
- **Common part** — the shared [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/registry/web-service-common-plateaus|registry/web-service-common-plateaus]] numbers each combination of 📐 common-VP Variants (from the web-service common Variability Map) and shows, per bound stack, whether a plateau with that combination exists there.
- **Specific part** — the catalog's `## Stack VP combinations` table numbers each combination of that catalog's own stack VPs.

Decision recorded in [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/adr/plateau-code-by-combination|adr/plateau-code-by-combination]]. A plateau created by `plateau-create-by-solutions` takes the code's file form as its `{plateau-name}` (`GW003.000` → `gw003-000`, folder `gw003-000/`, root `plateau-gw003-000.skill/`).

## How to update
1. Read `{catalog}/variability-map.md` fresh: VP IDs, Constraints, Realized by. Confirm every column is filled — if `Realized by` has gaps, stop and finish the map first (see [Precondition: a fully-filled map](#precondition-a-fully-filled-map)).
2. Enumerate the plateau folders on disk (`{catalog}/plateau/*/`); compute each plateau's VP set from its `created_by` (plus `parent_plateaus` transitively), mapped through the map's Realized-by column — for a common VP, the Variant its solution realizes, or the "no" Variant (`None`/`No`) when none of its solutions realizes one.
3. Cross-check every plateau's VP set against every Constraint in the map (see [Constraint check on every update](#constraint-check-on-every-update)).
4. Derive each plateau's code per [Code every plateau](#code-every-plateau): look up (or add) its common-VP combination in [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/registry/web-service-common-plateaus|registry/web-service-common-plateaus]] and its stack-VP combination in `## Stack VP combinations`; when the code differs from the plateau's current one, recode it per [Recode when the combination changes](#recode-when-the-combination-changes).
5. Update the registry's status cell for this stack per [Keep registry statuses current](#keep-registry-statuses-current).
6. Rewrite the matrix, legend, stack-VP combinations, and lineage from the computed data — recompute, never patch a single cell from memory.
7. Annotate special VP shapes (per-entity, skeleton/draft) so they do not read as plain ✅/❌.

# Rule

## MUST

### Precondition: a fully-filled map
Start only from a `{catalog}/variability-map.md` built via [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]] with every column filled — `Realized by` included — and from plateau folders already built by [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] or changed by [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/plateau-update-by-solutions.skill.md|plateau-update-by-solutions]].
- Violation: running this skill against a map with empty `Realized by` cells (a `planned — {chosen realization}` cell counts as filled: no plateau can compose that Variant until its solution exists), against a map (or a hand-written table shaped like one) that `variability-map-create` did not produce, or inventing plateau folders here.
- Risk: a matrix derived from an incomplete or improvised map has columns that cannot be mapped to solutions, so its cells are guesses; authoring plateaus here duplicates work this skill only reads.
- Fix: build or finish the map via `variability-map-create` (its `Realized by` column filled by that skill's own [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]] sub-step) and create/update plateaus via their own skills before building the repository file.

### Precondition: plateaus are self-contained, never cross-plateau links
A plateau's own skill file and its `structure/` files must never link to another plateau's skill file in their body — a plateau links only to its own `structure/` files, the solutions named in its `created_by`, and its own `registry/`/`adr/` entries. Cross-plateau relationships are expressed only through the `parent_plateaus`/`created_by` YAML properties, which tooling reads directly without needing to load the target plateau's full content.
- Violation: a Goal section reading "Everything `[[plateau-integrated-service]]` has, plus a Redis-backed cache..." or a Structure section reading "See `structure/` — everything from the parent, union'd with: ..." with a wikilink into another plateau's own skill file.
- Risk: an agent asked to build or understand one plateau follows the link into its parent, which links into its own parent, cascading through the entire lineage chain — instead of loading only the one plateau it actually needs.
- Fix: state each plateau's content cumulatively and completely in its own file — restate inherited Goal/Capabilities/Structure entries directly rather than pointing at the parent plateau to find them; reserve `parent_plateaus`/`created_by` for the YAML metadata this skill and `plateau-create-by-solutions` already read.

### Derived from the map, never restating it
State no VP fact in `plateau-repository.md` that the Variability Map does not state — constraints, realizations, and variants live there; the file links back.
- Risk: two homes for one fact drift; a reader cannot tell which is authoritative.
- Fix: the legend and every note carry VP IDs and link the map; prose explains presentation, not content.

### Every plateau and every VP present
Keep one matrix row per plateau folder on disk and one column per VP in the current map — every 📐 common VP carried by the map as well as every stack VP — — a new plateau, a removed plateau, or a changed VP ID set must all show up after the update.
- Risk: a missing row or column makes the matrix silently stale exactly where the catalog moved.
- Fix: recompute rows and columns from disk and the map on every update, never from the previous file.

### Cumulative consistency with the lineage
Make each plateau's ✅ set equal its parent's set plus its own `created_by` delta, with the parent chain taken from `parent_plateaus`.
- Risk: a row that lists a VP the parent chain contradicts (or omits one the parent already realizes) teaches a wrong model of what composing that plateau means.
- Fix: walk `parent_plateaus` transitively before filling a row; flag any plateau whose set is not a superset of its parent's for the owner.

### Constraint check on every update
Cross-check every plateau's VP set against every Constraint in the map, and treat a violation as a defect — raised as a plateau-level ADR per [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md#Record every plateau-level decision as an ADR|plateau-create-by-solutions]], never a silent exception.
- Risk: an inconsistent plateau ships unnoticed because nothing ever checked it against the table meant to make combinations explicit.
- Fix: derive every row from the plateau's actual `created_by`/`parent_plateaus` and check it against every Constraint row before saving the file.

### Code every plateau
Give every plateau the code `{stack}{kind}{common}.{specific}`, using only the registered letters below and three-digit numbers.

| Stack | Letter | | Kind | Letter |
| --- | --- | --- | --- | --- |
| dotnet | `D` | | backend web-service | `W` |
| Go | `G` | | CLI app | `C` |
| Python | `P` | | Angular app | `A` |
| TypeScript | `T` | | | |

- `{common}`: the number of the plateau's combination of 📐 common-VP Variants in [[skills/common-workflow/architecture/design/plateau-map/plateau-map-create.skill/registry/web-service-common-plateaus|registry/web-service-common-plateaus]]; `000` for a kind with no common Variability Map.
- `{specific}`: the number of the plateau's set of stack VPs in the catalog's `## Stack VP combinations` table; `000` = no stack VP.
- Violation: a new letter used without adding it to this table, two plateaus with the same combination under different numbers, or a descriptive plateau name used as the identity.
- Risk: codes stop meaning "this combination" — the same combination appears under two numbers across stacks, and a reader can no longer find the equivalent plateau in another stack.
- Fix: register a new stack or kind letter here first; look up the combination before assigning a number, and assign the next free number only for a combination not listed yet; keep the descriptive name in the matrix's Title column.

### Recode when the combination changes
Change a plateau's code in the same change that changes its VP combination — a VP admitted as common, a stack VP re-IDed to a common VP, or a solution added to or removed from its `created_by`.
- Violation: a plateau keeping `GW001.001` after its gRPC stack VP became common VP-C00x, so its code no longer states its combination.
- Risk: the code silently lies, and cross-stack equality of `{common}` numbers breaks.
- Fix: recompute the code, rename the plateau (folder, root skill, element-skill prefixes, `parent_plateaus`/`created_by` references) in the same change; never reuse a number another combination holds. A plateau created before codes existed, and not yet renamed, carries its recomputed code in the matrix's Code column beside its current folder name until it is renamed.

### Keep registry statuses current
Mark each registry row's cell for this stack ✅ when a plateau with that combination is built here with its example, and 🔸 when only another stack has built it; add a row only when the first stack builds that combination.
- Violation: listing every theoretically legal combination as a registry row, or leaving 🔸 after this stack built the plateau.
- Risk: the registry explodes with combinations nobody uses, or hides which stack is missing a plateau another stack already has.
- Fix: update the status cell for this stack on every run of this skill; unbuilt combinations stay off the registry.

### Both triggers covered
Update `plateau-repository.md` when a plateau is created or its solution set changes, and when a VP is added, changed, or removed in the map — the second trigger fires even though no plateau changed.
- Risk: a VP rename or removal leaves a ghost column (or a missing one) because no plateau edit happened to drag the file along.
- Fix: run the update after either event; the map's change log entry is a trigger, not only plateau work.

### Follow the skill-design baseline
Follow [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]]'s baseline (tags, `whenToUse`, link style, no leftover hint/example blocks) in addition to this skill's own rules.
- Risk: this skill's rules cover the repository file's content, not the mechanics every skill must follow — skipping the shared baseline produces a technically-correct workflow in a non-conforming skill file.
- Fix: apply `skill-design.skill.md` in addition to, never instead of, the rules above.

## SHOULD

### Legend over naked IDs
Give the column legend a one-line name per VP and a link to the map, instead of leaving readers to decode `VP7`.
- Risk: a matrix of bare IDs is readable only with the map open in a second tab — the file fails as a standalone index.
- Fix: one line per VP, name plus link; details stay in the map.

### Mark special VP shapes
Annotate per-entity VPs and skeleton/draft VPs explicitly instead of letting them read as plain ✅/❌.
- Risk: a ✅ on a per-entity VP overpromises ("every entity has it"); a ❌ on a skeleton VP underpromises ("never available") — both mislead.
- Fix: one shape note per special VP under the matrix.

## MAY

### Extra views allowed
Add sections beyond the matrix — folder contents, registry notes, counts, historical reference mappings — as long as they restate no VP facts from the map.

# Check list
- [ ] `{catalog}/variability-map.md` has every column filled (`Realized by` included) before this skill ran.
- [ ] No plateau skill file or `structure/` file links to another plateau's skill file in its body — cross-plateau relationships are expressed only via `parent_plateaus`/`created_by` YAML metadata.
- [ ] Matrix rows match the plateau folders on disk; columns match every 📐 common VP and every stack VP in the current `variability-map.md`.
- [ ] Every plateau has a code `{stack}{kind}{common}.{specific}` with registered letters; its `{common}` number matches its combination in the common-plateau registry, its `{specific}` number the catalog's `## Stack VP combinations` table.
- [ ] Every plateau whose combination changed was recoded in the same change; the registry's cell for this stack is ✅ / 🔸 as built.
- [ ] Every row's ✅ set verified against the plateau's actual `created_by` + transitive `parent_plateaus`.
- [ ] Every row cross-checked against every Constraint in the map; violations raised as plateau-level ADRs.
- [ ] The legend names every VP in one line and links the map as the source of truth.
- [ ] Per-entity and skeleton/draft VPs are annotated, not presented as plain ✅/❌.
- [ ] Facet tags follow [[skills/design/skill-tags.skill/skill-tags.skill.md|skill-tags]]: `concern/architecture`, bare `stack`.
