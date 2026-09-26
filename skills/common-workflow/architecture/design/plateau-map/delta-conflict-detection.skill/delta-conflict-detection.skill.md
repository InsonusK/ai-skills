---
name: delta-conflict-detection
description: Fill a Variability Map's Realized by column — walk every Variation Point, author or select the solution(s) that realize it, then classify and resolve intersections between solutions (deltas) that touch the same element, using a fixed three-axis code (Constraint x Category x Kind), recording the result as per-element Registry entries inside the plateau where the intersection first becomes real
whenToUse: when a catalog's Variability Map has VP, Variants, and Constraint filled and the Realized by column still needs populating — this skill walks each VP, authors/selects its realizing solution(s), and classifies where any two solutions share an element/{element-name} tag inside their Implementation/ folders
updated: 20260918
tags:
  - skill/architecture/variability/conflict-detection
  - stack
  - concern/architecture
adr:
  - "[[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/adr/intersection-registry-design|Intersection Registry: per-element files, placed at the shallowest plateau where the intersection is real]]"
---

# Goal
- **Filled Realized by column** - Every VP row of `{catalog}/variability-map.md` has `Realized by` pointing at the solution skill(s) that realize it — authored via [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] (draft contract when none exists yet) or reused from the catalog.
- **Classified intersections** - Every group of 2+ solutions sharing an `element/{element-name}` classified with the fixed three-axis code (Constraint x Category x Kind).
- **Resolvers where needed** - A separate resolver solution for every `TMC`/`FMC`/`FDC` group — never folded into an original solution — with the detection pass iterated to a fixed point.
- **Registry entries** - One file per intersected element, at the catalog root (`{catalog}/registry/{element-name}.md`), holding the current cumulative analysis plus a growth-history table across the plateaus that touched it — listed in every relevant plateau root skill's `registry:` property, never copied into a plateau folder.

# Core Principle
- **Completes the Variability Map** - This step runs to finish [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/variability-map-create.skill|variability-map-create]]'s table: once VP, Variants, and Constraint are set, this skill walks each VP and produces its `Realized by` entry. The map is not done until this has run over every row.
- **On existing tags** - It consumes the `element/{element-name}` tags [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] already puts on every `Implementation/` file — no new tagging convention is introduced.
- **The classifier is settled** - The three-axis classifier below (`Constraint x Category x Kind`) is fixed. Do not reword, reorder, or re-derive it — treat it as settled input, not a draft.

# The classifier
Three independent axes, one letter each, read in this order:

| Axis | Values |
| --- | --- |
| **Constraint** | `F` no constraint between the intersecting VPs · `T` a constraint exists · `-` does not affect classification |
| **Category** | `N` no shared artifact · `D` DI substitution · `M` code change · `-` not applicable |
| **Kind** | `N` independent · `C` conflicting · `-` not distinguished for this Category (applies only to `D`/`M`) |

| Code | Meaning | Example | Status |
| --- | --- | --- | --- |
| `-N-` | No shared artifact, any constraint state | Two solutions each add their own, unrelated classes | Canonical — nothing further needed |
| *(none)* | Two solutions both `.create` the same artifact from scratch | — | Design error, outside the grid — not resolved by a resolver; one of the two must `.extend`, not `.create` |
| `FDN` | No constraint, DI substitution, independent | An independently-pluggable component (e.g. logging) wired at the composition root | Canonical — DI at the composition root, the deltas never know about each other |
| `FDC` | No constraint, DI substitution, conflicting | 2+ solutions compete for one DI slot but are meant to work simultaneously (e.g. publishing to two channels at once) | Needs resolution — see [FDC resolution](#fdc-resolution) |
| `FMN` | No constraint, code change, independent | One solution changes a method, another changes the constructor of the same class | Canonical — several independent changes; the default when nothing more specific applies |
| `FMC` | No constraint, code change, conflicting | 2+ solutions with no constraint between them change the same method | Needs resolution — check the [wrap/relocate footnote](#the-wraprelocate-footnote-fmc-vs-fmn) first, then see [FMC resolution](#fmc-resolution) |
| `TDN` | Constraint present, DI substitution, independent | The dependent solution's delta extends DI settings the base solution's delta did not touch | Canonical — the dependent delta extends DI |
| `TDC`/`TD-` | Constraint present, DI substitution (Kind not distinguished) | `IRuleSource`: the base VP registers `LocalRuleSource`; the dependent VP re-registers it to `CentralizedRuleSource` | Canonical — the dependent delta re-registers the interface without touching the base solution's code; order is guaranteed by the constraint. Kind is not distinguished here: with a constraint present, DI substitution always resolves the same way regardless of "independent" vs "conflicting" |
| `TMN` | Constraint present, code change, independent | The constraint (`VP3 requires N x VP`) reduces which combinations are reachable, but the reachable ones never touch the same method | Canonical — several independent changes |
| `TMC` | Constraint present, code change, conflicting | `VP3 requires N x VP`; 2+ of the required solutions change the same method/function | Needs a resolver per legal combination — see [TMC handling](#tmc-handling) |

Only `TMC`, `FMC`, `FDC` are flagged for resolution — every other code is canonical and needs no further action. All three are formulated for **2+** intersecting solutions from the start, never assumed to be exactly a pair.

## FDN never forks the plateau tree
An `FDN`-classified VP — DI substitution, no constraint, independent — never earns its own plateau, no matter how many Variants it has: swapping one Variant for another only changes which concrete adapter is wired at the composition root (`main.go`/`Program.cs`/...), never the plateau's own structural shape. Two shapes of this: (1) a cross-cutting capability that touches no module-internal file at all — build it as a [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|Plateau Component]], attached outside `created_by` entirely; (2) an alternative *dispatch target* behind an already-existing port — e.g. an outbox's drained events going out via a Kafka producer vs a webhook caller — each target is its own small solution implementing the same port, wired at the composition root, and none of them earns a separate plateau. Only an *additive* capability — one that changes what the plateau's own files actually contain, not just which adapter is plugged into an existing port — belongs to a new point in the plateau lattice.
- Violation: creating `plateau-outbox-kafka` and `plateau-outbox-webhook` as sibling plateaus that are structurally identical except for one adapter file.
- Risk: N interchangeable dispatch targets multiply into N sibling plateaus that duplicate the same structure and drift independently from each other, when the actual decision is a one-line composition-root wiring choice.
- Fix: keep the port and its owning plateau singular; add each dispatch target as its own solution implementing that port, selected at composition-root wiring time — never as a plateau fork. See [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] for where this decision is actually made.

## FDC resolution
Preferred: adapt the shared slot to accept a collection (e.g. .NET's `IEnumerable<T>`) so every contributor registers independently and a dispatcher invokes all of them — this collapses the case into `FDN`, and no separate resolver solution is needed at all. Fall back to a Composite-pattern resolver only when the slot genuinely cannot become a collection. Either way, the resolver — if one is built — is never folded into either original solution; both must stay self-sufficient on their own.

## TMC handling
When the constraint defines a finite number of legal combinations (`N x required VP`), assemble the plateau context for each legal combination in advance, and write the delta module stating explicitly which combination from that set it assumes — do not write one delta that silently branches its own behavior per combination.

## FMC resolution
No Constraint exists between the intersecting VPs, so — unlike `TD-`/`TMN`, where the Constraint itself guarantees an order — nothing dictates which delta's change to the shared method "wins" or applies first. Two shapes, in preference order:
1. **Preferred: collapse to independent** — restructure the shared method into an ordered sequence of steps (a hook, a chain, an extension point) that each intersecting solution's delta attaches to independently, the same spirit as [FDC resolution](#fdc-resolution)'s preference for a collection over a Composite resolver. This turns the conflict into ordinary `FMN`/`FDN` and needs no resolver solution at all.
2. **Fallback: a dedicated resolver solution** — when the method genuinely cannot be decomposed into independent steps (the intersecting deltas want the *same* decision to work two incompatible ways, not two different things to both happen), build a resolver solution whose own `Implementation/*.extend.md` on that element is the single source of truth for the shared region — not a further edit layered on top of either original solution's own version of it. The resolver's `depends_on` names every solution it resolves, per [Resolvers are separate solutions](#resolvers-are-separate-solutions); an applying agent composing both intersecting solutions together uses the resolver's Implementation for that element instead of either original's.

Before recording `FMC` at all, check [the wrap/relocate footnote](#the-wraprelocate-footnote-fmc-vs-fmn) — a same-method touch is not automatically `FMC` just because no Constraint exists between the VPs.

## The wrap/relocate footnote: FMC vs FMN
A same-method touch with no Constraint between the VPs is not `FMC` merely because it looks conflicting on the surface — check first whether the later-authored delta is a **mechanical wrap or relocation** of the earlier delta's existing call, with exactly one sensible way to combine them (e.g. extracting an existing call into a helper and adding logic around it, never touching the extracted call's own behavior). The test: can the author of the later delta, reading only the earlier delta's existing code, write a complete and correct merge instruction on their own — with no further change needed to the earlier delta? If yes, this is `FMN`, resolved by an explicit, named cross-reference inside the later delta's own Implementation file stating the merge rule — never a separate resolver solution. If the merge genuinely requires changing *both* deltas, or there is no well-defined "later" (either could reasonably be authored first, and the result would differ), it is real `FMC` — go to [FMC resolution](#fmc-resolution). Contrast [example-dotnet-registry-entry](./examples/example-dotnet-registry-entry.md): that case genuinely fails this test — two properties both need to be "first," so no author, reading only the other's code, can resolve it alone; it stays real `FMC`.

## The `TD-`/degenerate footnote
A solution that "looks different depending on which VP called it" is never a new row in this table — check first whether only the DI substitution differs (then it is ordinary `TD-`/`FDN`, the solution stays one) or whether the code's own structure differs (then two solutions were mistakenly bundled under one name and must be honestly split, each with its own realization — an `FMN`/`TMN` case). Never write "if called from VP1 do X, if from VP2 do Y" conditional logic inside one delta as a substitute for this split.

# Workflow

## The 5-step workflow
1. **Core module** — the catalog's shared baseline, the same starting point [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] later assembles from.
2. **Unconstrained deltas** — for every VP with no Constraint against another VP in the Variability Map, author or reuse the realizing solution as an ordinary, independent one via [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] (a draft contract when none exists yet), and write its wikilink into that VP's `Realized by` cell.
3. **Constrained deltas** — for every VP with a Constraint, author the realizing solution via [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] accounting for it: DI substitution (`TD-`) or independent code change (`TMN`), both canonical, ordering already guaranteed by the constraint itself. When the intersection is conflicting and the constraint defines a finite combination set, follow [TMC handling](#tmc-handling). Write each solution's wikilink into its VP's `Realized by` cell.
4. **Conflict Detection pass** — group the plateau's active `Implementation/` files by their existing `element/{element-name}` tag (see [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]]'s tagging rule):
   - Two or more `.create` files on one element → design error outside the grid; fix by turning one into `.extend`, never by writing a resolver.
   - Classify every remaining group using [The classifier](#the-classifier) above.
   - Build a resolver only for `TMC`, `FMC`, `FDC`.
5. **Fixed-point iteration** — every resolver built in step 4 is itself added to the pool grouped by `element/{element-name}`. Repeat the grouping and classification pass until no new group appears. Record the intersection in a Registry entry (see [Where a Registry entry lives](#where-a-registry-entry-lives)) for every group found, canonical or not.

Finish with two outputs: every VP row of `{catalog}/variability-map.md` now has its `Realized by` cell filled (including any resolver solutions built in steps 4–5); and a summary of one row per intersecting group, its classification code, and its resolution (canonical / resolver link / core change) — the latter is the content of the catalog's `registry/` folder (see [Where a Registry entry lives](#where-a-registry-entry-lives)), not a separate document.

## Where a Registry entry lives
Record every intersection at the **catalog root**: one file per element, at `{catalog}/registry/{element-name}.md` — a sibling of `variability-map.md` and `plateau/`, never inside a specific plateau's own folder — using [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/templates/registry-entry.template|templates/registry-entry.template.md]]. The file holds the *current* (deepest-known) classification plus a `# Growth history` table — one row per plateau where the intersecting set actually changed (a solution joined, or the classification/resolution itself changed), not one row per plateau that merely inherits the element unchanged. List the one file in the `registry:` YAML property of **every** plateau root skill whose `created_by`/`parent_plateaus` includes the element — the same file, linked from several plateaus, never copied. See [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/adr/intersection-registry-design|adr/intersection-registry-design]] for why this format was chosen (superseding an earlier per-plateau design that produced growing duplication down a plateau chain).

A Registry entry's **Ordering** field states whether the ordering it records comes from a real Feature-Model constraint (`source: constraint` — already free, since `depends_on` had to carry it anyway) or exists purely so a resolver has something deterministic to build on (`source: ordering-only` — the resolver's own `depends_on` is the *only* place this ordering is recorded at all). See [[skills/common-workflow/architecture/design/plateau-map/variability-map-create.skill/adr/constraint-vs-ordering-columns|variability-map-create's ADR]] for why this distinction never becomes a `depends_on` schema change.

See [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/examples/example-dotnet-registry-entry|examples/example-dotnet-registry-entry.md]] for a worked entry against a real catalog.

# Rule

## MUST

### Every VP gets a Realized by entry
Walk every VP row of the Variability Map and give each one a `Realized by` wikilink to a real solution skill before starting the Conflict Detection pass — author a draft contract via [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] when no solution exists yet.
- Risk: leaving a VP with no realizing solution means the map still describes intent in prose, and the classifier has nothing to group for that VP.
- Fix: treat steps 2–3 as complete only when every VP row's `Realized by` cell holds a wikilink.

### The classifier is fixed input
Use the classifier exactly as defined in [The classifier](#the-classifier) — never reword, reorder, merge, or split its axes or codes.
- Risk: re-deriving the taxonomy independently each time it is applied produces inconsistent codes across plateaus and silently redoes settled design work.
- Fix: apply the fixed table; if a real case does not fit, raise it as a question rather than inventing a new code informally.

### Only three codes need a resolver
Build a resolver only for `TMC`, `FMC`, `FDC`; treat every other code as canonical and take no further action beyond recording it.
- Risk: writing unnecessary resolvers for canonical cases adds indirection nothing needed.
- Fix: check the code against [The classifier](#the-classifier) before deciding a resolver is needed at all.

### Resolvers are separate solutions
Never fold a resolver's logic into one of the original intersecting solutions — keep the resolver as its own, separate solution, `depends_on` naming every solution it resolves. This applies to real `FMC`/`TMC`/`FDC` only — a case reclassified as `FMN` by the [wrap/relocate footnote](#the-wraprelocate-footnote-fmc-vs-fmn) is not a resolver at all and correctly lives inside the later-authored solution's own file.
- Risk: folding the fix into one original solution makes that solution silently aware of, and dependent on, the other — breaking the guarantee that each stays self-sufficient on its own, and hiding the resolution from anyone reading only the original solution's file.
- Fix: create a distinct resolver solution per [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]], with `depends_on` listing every intersecting solution it resolves; for a wrap/relocate case, write the cross-reference note inside the later solution's own Implementation file instead — check [the footnote's test](#the-wraprelocate-footnote-fmc-vs-fmn) before choosing between the two.

### Iterate to a fixed point
Re-run the grouping-by-`element/{element-name}` pass after building any resolver, treating the resolver as a normal participant, until a pass produces no new group.
- Risk: stopping after one pass misses a conflict the resolver itself introduces with a further solution.
- Fix: iterate to a fixed point per [step 5](#the-5-step-workflow).

### One file at the catalog root, never one per plateau
Place every Registry entry at `{catalog}/registry/{element-name}.md`, and update that one file (adding a `# Growth history` row) when the intersecting set changes at a deeper plateau — never create a second copy inside a plateau folder, per [Where a Registry entry lives](#where-a-registry-entry-lives).
- Risk: a copy per plateau restates the same Constraint/Category/Kind reasoning at every depth, drifts the moment an insight discovered at one depth fails to propagate to the others, and has no single place a catalog-wide reader can check for the current state of an element.
- Fix: check whether `{catalog}/registry/{element-name}.md` already exists before creating one; if it does, extend it and add a `# Growth history` row instead of writing a new file. Every plateau whose `created_by`/`parent_plateaus` includes the element lists the same file in its own `registry:` YAML property.

### Double-create is a design error
Flag two or more `.create` files landing on the same element as a design error and fix it by converting one to `.extend` — never attempt to resolve it with a conflict resolver.
- Risk: treating this as an ordinary conflict produces a resolver papering over what is actually a modeling mistake (two solutions both claiming to originate the same artifact).
- Fix: identify which solution should really be extending the artifact the other creates, and correct its `Implementation/` files accordingly.

### N≥3 carries an architectural-signal note
Record an architectural-signal note on any Registry entry whose group reaches N≥3 intersecting solutions (`TMC`/`FMC`/`FDC`), stating that this is also a reason to reconsider the involved VPs' boundaries, not only a case needing one more resolver.
- Risk: treating N≥3 as "just a bigger version of the same case" hides a real signal that the variability decomposition at that point may need rethinking.
- Fix: add the note explicitly in the Registry entry, per [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/templates/registry-entry.template|templates/registry-entry.template.md]].

### Follow the skill-design baseline
Follow [[skills/design/skill-design.skill/skill-design.skill.md|skill-design]]'s baseline (tags, `whenToUse`, link style, no leftover hint/example blocks) in addition to this skill's own rules.
- Risk: this skill's rules cover conflict detection's content, not the mechanics every skill must follow — skipping the shared baseline produces a technically-correct workflow in a non-conforming skill file.
- Fix: apply `skill-design.skill.md` in addition to, never instead of, the rules above.

## SHOULD

### Prefer collections over Composite for FDC
Prefer the `IEnumerable<T>`-style collection fix for `FDC` over a Composite resolver whenever the shared slot can reasonably become a collection.

### Cross-check degenerate TD- cases
Cross-check a `TD-`/degenerate-looking case against [the footnote](#the-td-degenerate-footnote) before accepting it as ordinary DI substitution.

## MAY

### Skipping -N- entries
Skip writing a Registry entry for an `-N-` group when the catalog's scale makes tracking every non-intersection impractical — this code needs no action either way, so the entry is a convenience, not a requirement.

# Check list
- [ ] Every VP row of `{catalog}/variability-map.md` has its `Realized by` cell filled with a wikilink to a real solution skill (draft contract counts).
- [ ] Every intersecting group found by grouping on `element/{element-name}` was classified using the fixed table in [The classifier](#the-classifier), with no reworded or invented codes.
- [ ] A resolver was built only for `TMC`, `FMC`, or `FDC` groups.
- [ ] Every resolver is its own solution with `depends_on` naming every solution it resolves — none folded into an original solution.
- [ ] The grouping pass was repeated after adding any resolver, until no new group appeared.
- [ ] Every Registry entry lives at `{catalog}/registry/{element-name}.md`, never duplicated inside a plateau folder; every plateau that includes the element links to that one file, and a deeper plateau that changes the intersecting set added a `# Growth history` row instead of a new file.
- [ ] Every `FMC` classification was checked against the [wrap/relocate footnote](#the-wraprelocate-footnote-fmc-vs-fmn) before being recorded as `FMC` rather than `FMN`.
- [ ] No `FDN`-classified VP (a Plateau Component, or interchangeable adapters behind one existing port) was given its own plateau — see [FDN never forks the plateau tree](#fdn-never-forks-the-plateau-tree).
- [ ] Any group reaching N≥3 carries the architectural-signal note.
- [ ] Two-or-more-`.create`-on-one-element cases were fixed by converting one to `.extend`, never by writing a resolver.
- [ ] The plateau root skill's `registry:` YAML property lists every Registry file created for that plateau.
- [ ] Facet tags follow [[skills/design/skill-tags.skill/skill-tags.skill.md|skill-tags]]: `concern/architecture`, bare `stack`.
