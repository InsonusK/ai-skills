---
name: variability-map-create
description: Define how to build and maintain a Variability Map — one table binding every Variation Point of a plateau/solution catalog to the solutions that realize it (Variants, Constraint, Realized by, Realization depends on, Migration)
whenToUse: when a plateau/solution catalog needs its variability made explicit as a table instead of tribal knowledge — when grouping a Feature Model's non-common features into Variation Points, or when a new optional/alternative solution is added to the catalog, or when a Variation Point shared by every backend web-service stack is admitted into the common map
updated: 20260926
tags:
  - skill/architecture/variability/design
  - stack
  - concern/architecture
adr:
  - "[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/one-map-per-catalog|One map per catalog, not per plateau]]"
  - "[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/constraint-vs-ordering-columns|Constraint vs. ordering stays column-level, not a depends_on schema change]]"
  - "[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/no-plateau-view-in-variability-map|The map binds VPs to solutions; the plateau↔VP view lives in plateau-map-create]]"
  - "[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/common-vps-inherited-by-id|Common VPs inherited by ID, not copied]]"
  - "[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/inbox-status-key-not-task-id|Inbox status by a separate status_key, not by the task id]]"
---

# Goal
- **Variability Map file** - `{catalog}/variability-map.md` produced as the binding between the catalog's Variation Points and the solutions that realize them, with every column filled.
- **Variation Point table** - One row per axis on which two teams building on this catalog could legitimately answer differently, with Variants, Constraint, Realization depends on, and Migration filled directly by this skill — so an agent deciding what a new plateau contains reads one table instead of reverse-engineering intent from `created_by`/`depends_on` lists.
- **Completed Realized by column** - The `Realized by` column filled by the [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]] sub-step, which walks each VP, authors or selects its realizing solution(s), and classifies their intersections — the map is not finished until this runs.
- **Common Variation Points carried** - For a backend web-service catalog, a `## Common Variation Points` table carrying every row of the [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|common map]] with a State, separate from the catalog's own `## Stack Variation Points`.
- **Surfaced missing edges** - Every requirement found in prose or in the Feature Model but absent from a solution's `depends_on` raised as a fix proposal, not left as an unencoded assumption.

# Core Principle
- **Two teams, two answers** - A [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/variation-point|Variation Point]] exists only where two teams building on this catalog could legitimately answer differently. If every path through the catalog includes a solution, it is shared core (see [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/glossary/program-families|Program Families]]) — it does not get a row.
- **Shared concept, stack realization** - A VP every web-service stack shares is defined once, in the common map; each stack map inherits it by ID and contributes only its State, its narrowing, and its `Realized by` — a stack never re-cuts a shared question.
- **The table is the artifact** - A plateau never re-describes its own variability separately from the map. [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/realized-by|Realized by]] always points at solutions that exist — this skill never re-authors solution content.
- **Runs after the Feature Model** - This skill starts from a catalog Feature Model built by [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/feature-map-create.skill.md|feature-map-create]], whose non-common features feed this skill its candidate pool (see [Precondition: a finished Feature Model](#precondition-a-finished-feature-model)).
- **Realized by is a sub-step, not a later stage** - Filling `Realized by` — authoring each VP's realizing solution(s) and classifying where two of them touch the same code element — is done by [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]], run to complete this map. Assembling plateaus from the finished map is a separate stage this skill does not describe.

# Workflow

## Where the map lives
One Variability Map per catalog, at `{catalog}/variability-map.md` — a sibling of the catalog's `plateau/` and `solutions/` folders (e.g. `skills/dotnet/architecture/v3/variability-map.md`). Not one per plateau: a plateau is one *point* in the combination space the map describes, not a separate space of its own — splitting the map per plateau would duplicate the same VP row into every plateau that happens to touch it. See [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/one-map-per-catalog|adr/one-map-per-catalog]].

A backend web-service catalog's map holds two tables: `## Common Variation Points` (every common VP, inherited — see [Common Variation Points](#common-variation-points)) and `## Stack Variation Points` (this family's own VPs, full column set).

## Common Variation Points
The [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/web-service-common-variability-map/web-service-common-variability-map|web-service common map]] defines each VP shared by every backend web-service stack — question, Variants, Constraint, Realization depends on, and a concept section — under a `VP-C###` ID, lists candidate VPs not yet agreed, and lists the bound stack maps. A bound stack map carries each agreed common VP as one row: ID (linking the common concept section), VP name, Status, State, Stack delta, Realized by, Migration — governed by [Track VP status](#track-vp-status) through [Common IDs are permanent](#common-ids-are-permanent). Decision recorded in [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/common-vps-inherited-by-id|adr/common-vps-inherited-by-id]]. Decisions made inside the common map's contracts are recorded here too — e.g. why an Inbox task's status is read by a separate `status_key` rather than its `id`: [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/inbox-status-key-not-task-id|adr/inbox-status-key-not-task-id]].

## How to admit a common Variation Point
A common VP moves through three stages, each its own change:
1. **Candidate (💡)** — add a row to the common map's `## Candidate Variation Points` table: the feature group it covers and the open question. No ID yet.
2. **Concept (📐)** — agree the question, Variants, Constraint, Realization depends on, and the boundary with neighbouring VPs with the owner. Move the candidate into `## Common Variation Points` under the next free `VP-C###` ID with a `### VP-C### {Name}` concept section, referencing only 📐 common VPs, and add a `⏳` row for it to every bound stack map.
3. **Stack detail (⏳ → ✅)** — per bound stack, while the concept is fresh: decide the State, the concrete realization of every supported Variant (library or own implementation, and why), and what is narrowed. `Realized by` links an existing solution or records `planned — {chosen realization}`; a planned Variant gets its solution when a plateau "existing base plateau + this VP" is built. Re-ID the stack VP the common VP covers in the same change, per [Common IDs are permanent](#common-ids-are-permanent).

## How to build a Variability Map
1. Identify {catalog} — the folder holding the plateau/solution tree (e.g. `skills/dotnet/architecture/v3/`).
2. Read `{catalog}/feature/feature-model.md`, built by `feature-map-create` (see [Precondition: a finished Feature Model](#precondition-a-finished-feature-model)): its non-common features are the primary candidate list. Merge it with every solution reachable through any plateau's `created_by` (directly or via `parent_plateaus`) across the whole catalog — the union is the candidate pool. For a backend web-service catalog, add the map's path to the common map's `## Bound stack maps` list, carry every common VP per [Carry every common VP](#carry-every-common-vp), and remove from the candidate pool every candidate a common VP already answers.
3. For each candidate (or tight group of candidates answering one question), apply the Core Principle's test: would two teams legitimately answer differently? Discard candidates that appear on every existing and every reasonable future path — they are core, not variability.
4. Decide each VP's Variant shape per [Alternatives share a VP, combinables split](#alternatives-share-a-vp-combinables-split); use [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/variability-map.template|templates/variability-map.template.md]]'s worked entity-kind row as the model for a categorical VP.
5. Fill **Constraint** from evidence, not invention (see [Constraints from evidence only](#constraints-from-evidence-only)). When prose states a requirement absent from the solution's `depends_on`, treat this as a defect: raise a proposal to add the missing `depends_on` entry to that solution, rather than only noting the gap in the table.
6. Fill **Realization depends on** using the three relations from [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/orthogonal-variability-model|Orthogonal Variability Model]]: mandatory sub-feature, orthogonal VP, or cross-feature interaction — this is where a same-VP dependency that changes code *shape* (not just whether the VP is allowed) belongs, distinct from Constraint.
7. Fill **[[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/glossary/migration|Migration]]**: `Yes` only for a VP whose answer is known to have changed on a service that already runs on this catalog (a binding-time question, not a design-time one); `No` otherwise, and `No` is the default — see the glossary for when a VP is design-time bound.
8. Fill **Realized by** by running [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]] — with VP, Variants, and Constraint set, that skill walks each VP, authors or selects the solution skill(s) that realize it, and classifies where two solutions touch the same code element. The map is not finished until this runs. This skill never authors solution content itself; the column always holds wikilinks to real solution files.
9. Confirm every column is filled for every row. A downstream stage assembles plateaus from the finished map's Realized-by combinations; that stage is not part of this skill.

Use [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/templates/variability-map.template|templates/variability-map.template.md]] for the full column set and worked structure. See [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/examples/example-dotnet-variability-map|examples/example-dotnet-variability-map.md]] for a walkthrough against a real catalog.

# Rule

## MUST

### Precondition: a finished Feature Model
Start only from a `{catalog}/feature/feature-model.md` built via [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/feature-map-create.skill.md|feature-map-create]], with every candidate's `IsCommon` verdict decided. When a catalog genuinely has no Feature Model yet (an existing catalog being retrofitted, never a new one), treat that absence as a decision to confirm with the catalog's owner before proceeding — never a default to fall into silently.
- Violation: building a Variability Map straight from a solution list or from prose, with no Feature Model behind it and no owner confirmation that skipping one is deliberate.
- Risk: without a Feature Model, "would two teams answer differently" gets judged against whatever the current solution set happens to contain rather than against a deliberately reasoned common/variable split, so the map inherits any gap the Feature Model step exists to catch.
- Fix: build the Feature Model via `feature-map-create` first for a new catalog; for an existing catalog with none, get the owner to confirm proceeding without one before treating the map as final.

### A row only for a real decision
Create a stack VP row only for an axis where two teams could legitimately answer differently; never materialize a row for something every path through the catalog already includes — a common VP is the exception, carried even when this family never varies on it (see [Carry every common VP](#carry-every-common-vp)).
- Risk: the map balloons with rows that carry no real decision, burying the axes that actually matter under ones that don't.
- Fix: apply the Core Principle's test before adding a row; if every existing plateau answers it the same way and no stated future need suggests otherwise, leave it as core.

### Realized by links, never copies
Point **Realized by** at existing solution skills via wikilink; never copy or re-summarize a solution's own content into the table.
- Risk: the table and the solution skill drift apart the moment either one is edited, and a reader cannot tell which is authoritative.
- Fix: link the solution skill file; keep the table itself to VP/Variant/Constraint bookkeeping.

### Realized by is filled by the delta-conflict-detection step
Do not populate **Realized by** ad hoc; run [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/delta-conflict-detection.skill.md|delta-conflict-detection]] once the other columns are set, and let it author/select each VP's solutions and classify their intersections.
- Risk: solutions chosen without the classifier miss constrained-delta conflicts, so the map links a combination that cannot actually coexist in one plateau.
- Fix: treat the `Realized by` step as a required sub-step — the map is incomplete until `delta-conflict-detection` has run over every VP row.

### Constraints from evidence only
Derive every **Constraint** entry from real evidence — an existing `depends_on`/`built_on_plateau` edge, a requirement already stated in a solution's own prose, or an owner-confirmed `Requires` edge in the catalog's Feature Model — never invent one.
- Risk: a fabricated constraint blocks a legitimate combination a team actually needs, or hides a real one a team needed protection from.
- Fix: check the actual solution files (and the Feature Model, when the catalog has one) before writing a Constraint; when prose states a requirement `depends_on` does not encode, flag and fix the solution instead of only noting the gap.

### depends_on stays a plain list
Keep `depends_on`'s shape unchanged on every solution skill (plain wikilink list, no per-entry annotation) — represent the Constraint-vs-Realization-depends-on distinction only in this table's own columns.
- Risk: annotating `depends_on` entries (e.g. adding a `reason:` field) changes a schema every stack's `solution-create` shares, rippling across every existing solution in the repository for a distinction only this table needs.
- Fix: keep `depends_on` a plain list everywhere; put the "why" — constraint vs. pure ordering — only in this table's Constraint / Realization depends on columns. See [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/constraint-vs-ordering-columns|adr/constraint-vs-ordering-columns]].

### No plateau view in the map
Do not derive a plateau↔VP matrix or plateau consistency checks inside `variability-map.md` — the map binds VPs to solutions and ends there. The plateau↔VP view is built by `plateau-map-create` in `{catalog}/plateau/plateau-repository.md`.
- Risk: a plateau-oriented section gives the map a second audience and a second trigger (plateau changes), and duplicates whatever the plateau stage maintains.
- Fix: keep the artifact to the VP↔solution binding; the decision and where the view went are recorded in [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/no-plateau-view-in-variability-map|adr/no-plateau-view-in-variability-map]].

### Track VP status
Mark every common VP with its status in the common map, and every carried row with its detail status in the stack map, using only these icons.

| Where | Status | Meaning |
| --- | --- | --- |
| Common map | 💡 Candidate | A feature group is identified as a VP; nothing about it is agreed yet. No ID. |
| Common map | 📐 Concept | The common definition is agreed with the owner. |
| Common map | ⛔ Retired | No longer a VP; the row and its ID stay, never reused. |
| Stack map | ⏳ Pending | The stack carries the 📐 VP but has not decided its realization. |
| Stack map | ✅ Detailed | State, realization of every supported Variant, and narrowing are decided and recorded. |

- Violation: a stack row marked ✅ whose `Realized by` names neither a solution nor a planned realization, or a candidate given a `VP-C###` ID.
- Risk: the owner cannot see which VPs are only ideas, which are agreed, and which each stack has actually thought through.
- Fix: set the status in the same change that moves a VP between stages, per [How to admit a common Variation Point](#how-to-admit-a-common-variation-point).

### Carry every common VP
Carry every 📐 row of the common map, by its `VP-C###` ID, in a bound catalog's `## Common Variation Points` table — including rows this stack adds nothing to or never varies on.
- Violation: a stack map omitting a 📐 common VP because "this stack has nothing to add", or because every member of the family answers it the same way.
- Risk: an absent row cannot be told apart from a forgotten one, so the stack silently drifts from the shared definition.
- Fix: add the row as `⏳` until the stack decides, then detail it with the State that fits — `Fixed: {Variant}` when the family never varies.

### Give every common row a State
Give each ✅ row in a stack map exactly one State from this table; a ⏳ row carries `—` in State, Stack delta, and Realized by.

| State | Meaning | Stack delta | Realized by |
| --- | --- | --- | --- |
| `Inherited` | Taken as the common map defines it. | `—` | Per Variant: solution link, or `planned — {chosen realization}` |
| `Refined` | Taken with stack-evidenced narrowing: a Variant unsupported, an extra Constraint, a stack-specific realization note. | What is narrowed, and why | Per supported Variant: solution link, or `planned — {chosen realization}` |
| `Fixed: {Variant}` | This family's Feature Model makes the answer non-optional; `Fixed: No` = the family never has it. | The reason, with its Feature Model reference | Baseline solution for `Fixed: Yes`; `—` for `Fixed: No` |

- Violation: a `Refined` row with an empty delta, `Fixed: No` used for "no solution written yet", or `planned` without the chosen realization.
- Risk: a reader cannot tell a deliberate stack decision from a gap, and the realization decided while the concept was fresh is lost.
- Fix: state the narrowing or the Feature Model reason in the delta cell; record the chosen library or implementation after `planned —`.

### Restate nothing the common map owns
Keep a stack's common row to ID, VP name, Status, State, Stack delta, Realized by, and Migration; never copy the question, Variants, Constraint, or Realization depends on into the stack map.
- Violation: a stack row repeating the common question with a slightly different wording, or its own Constraint cell.
- Risk: the copy diverges from the common definition while still looking authoritative — the drift the common map exists to stop.
- Fix: link the ID to the common concept section; put only the stack's own narrowing in the delta cell.

### Narrow, never widen
Mark a common Variant unsupported or add a stack-evidenced Constraint in a stack's delta; never add a Variant or loosen a Constraint there.
- Violation: a stack delta adding a third store kind to a common VP that defines two.
- Risk: the stack answers a question the common map says has no such answer, and other stacks never learn the Variant exists.
- Fix: add a Variant that is not stack-specific to the common map first, through [How to admit a common Variation Point](#how-to-admit-a-common-variation-point); a truly stack-specific axis is a stack VP.

### Admit a common VP through the common map
Make a question common only through [How to admit a common Variation Point](#how-to-admit-a-common-variation-point) — owner-agreed concept, with a row in every bound stack map in the same change; never answer a shared question with a stack-local VP.
- Violation: a 📐 row committed without the bound stacks' ⏳ rows, or a new stack VP for a question the common map already answers or lists as a 💡 candidate.
- Risk: the stack maps fail to carry the new VP, or the shared question is re-cut per stack again.
- Fix: add every bound stack's ⏳ row with the 📐 row; raise a shared question as a 💡 candidate instead of a stack VP.

### Common IDs are permanent
Assign a common VP the next free `VP-C###` ID when it reaches 📐, never renumber or reuse one, and when a stack details a common VP that covers one of its stack VPs, re-ID that VP to the common ID in the stack map and in every reference in that stack's tree.
- Violation: renumbering the common map after a VP is retired, or leaving a stack's plateaus citing `VP4` after `VP4` became a common VP.
- Risk: references across stack trees silently point at the wrong question.
- Fix: mark a retired common VP `⛔ Retired` and keep its row, drop its row from every stack map, and never hand its ID out again; leave a re-IDed stack VP's old number as a gap.

### Follow the skill-design baseline
Follow [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]]'s baseline (tags, `whenToUse`, link style, no leftover hint/example blocks) in addition to this skill's own rules.
- Risk: this skill's rules cover the Variability Map's content, not the mechanics every skill must follow — skipping the shared baseline produces a technically-correct map workflow in a non-conforming skill file.
- Fix: apply `skill-design.skill.md` in addition to, never instead of, the rules above.

## SHOULD

### Alternatives share a VP, combinables split
Group solutions into one VP with several Variants when they are mutually-exclusive alternative answers to the same question (the entity-kind shape); keep them as separate boolean VPs when they are freely, independently combinable (an "Or" group in Feature-Model terms, not an "Alternative" group). Never pre-enumerate combinations of independent VPs as separate rows — each stays its own row regardless of how many other VPs it can combine with.
- Risk: enumerating combinations doubles the table every time a new independent answer appears, and the rows stop being questions a team answers.
- Fix: one row per question; a categorical VP only for genuinely mutually-exclusive answers (see the [[skills/common-workflow/architecture/design/plateau-map/feature-map-create.skill/glossary/feature-model|Feature model]] shapes).

### Reuse a near-duplicate VP first
Prefer reusing an existing VP's Variant set — a common VP before a stack VP — over introducing a near-duplicate VP when a new solution answers almost the same question an existing VP already covers.

## MAY

### Migration defaults to No
Leave **Migration** at `No` for a VP that has never yet needed to change after a service was already composed on this catalog.

# Check list
- [ ] `{catalog}/feature/feature-model.md` exists and is built via `feature-map-create` before this skill runs — or its absence was confirmed with the catalog's owner as deliberate.
- [ ] Every stack VP row passed the "would two teams legitimately answer differently" test before being added.
- [ ] The candidate pool included the Feature Model's non-common features.
- [ ] A backend web-service catalog is listed in the common map's `## Bound stack maps` and carries every 📐 common VP, by `VP-C###` ID, in `## Common Variation Points`.
- [ ] Every common VP has a status (💡 / 📐 / ⛔) and every carried row a detail status (⏳ / ✅); only 📐 and ⛔ VPs have an ID.
- [ ] Every ✅ row has exactly one State; `Refined`/`Fixed` rows state their narrowing or Feature Model reason; every `planned` names its chosen realization; no common row restates the question, Variants, or Constraint.
- [ ] No stack delta adds a Variant or loosens a common Constraint.
- [ ] A newly 📐 common VP has a ⏳ row in every bound stack map; every stack VP a ✅ row covers is re-IDed across that stack's tree.
- [ ] The **Realized by** column was filled by running `delta-conflict-detection`, not ad hoc; every entry is a wikilink to an existing solution skill (a draft-marked skeleton counts) — or, on a common row only, `planned — {chosen realization}`, or `—` for a ⏳ row or `Fixed: No` — never inlined content.
- [ ] Every **Constraint** entry is traceable to a real `depends_on`/`built_on_plateau` edge, a solution's own stated prose requirement, or an owner-confirmed Feature-Model `Requires` edge.
- [ ] No solution skill's `depends_on` field was changed in shape to carry a constraint/ordering annotation.
- [ ] The map contains no plateau↔VP derivation — that view belongs to `plateau-map-create`.
- [ ] All `hint`/`example` blocks removed from the final `variability-map.md` (none should have been copied from the template in the first place).
- [ ] Facet tags follow [[skills/design/skill-tags.skill/skill-tags.skill.md|skill-tags]]: `concern/architecture`, bare `stack`.
