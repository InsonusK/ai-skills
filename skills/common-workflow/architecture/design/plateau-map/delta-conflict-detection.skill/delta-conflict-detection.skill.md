---
name: delta-conflict-detection
description: Fill a Variability Map's Realized by column — walk every Variation Point, author or select the solution(s) that realize it, then classify and resolve intersections between solutions (deltas) that touch the same element, using a fixed three-axis code (Constraint x Category x Kind), recording the result as per-element Registry entries inside the plateau where the intersection first becomes real
whenToUse: when a catalog's Variability Map has VP, Variants, and Constraint filled and the Realized by column still needs populating — this skill walks each VP, authors/selects its realizing solution(s), and classifies where any two solutions share an element/{element-name} tag inside their Implementation/ folders
updated: 20260906
tags:
  - skill/architecture/variability/conflict-detection
  - stack
  - concern/architecture
adr:
  - "[[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/adr/intersection-registry-design|Intersection Registry: per-element files, placed at the shallowest plateau where the intersection is real]]"
---

# Goal
Fill the `Realized by` column of the catalog's Variability Map and record the classification of every solution intersection it produces. Concretely:
- **Filled Realized by column** - Every VP row of `{catalog}/variability-map.md` has `Realized by` pointing at the solution skill(s) that realize it — authored via [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] (draft contract when none exists yet) or reused from the catalog.
- **Classified intersections** - Every group of 2+ solutions sharing an `element/{element-name}` classified with the fixed three-axis code (Constraint x Category x Kind).
- **Resolvers where needed** - A separate resolver solution for every `TMC`/`FMC`/`FDC` group — never folded into an original solution — with the detection pass iterated to a fixed point.
- **Registry entries** - One file per intersected element, placed in the shallowest plateau where the intersection is real and listed in that plateau root skill's `registry:` property.

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
| `FMC` | No constraint, code change, conflicting | 2+ solutions with no constraint between them change the same method | Needs a resolver `depends_on` naming every conflicting solution |
| `TDN` | Constraint present, DI substitution, independent | The dependent solution's delta extends DI settings the base solution's delta did not touch | Canonical — the dependent delta extends DI |
| `TDC`/`TD-` | Constraint present, DI substitution (Kind not distinguished) | `IRuleSource`: the base VP registers `LocalRuleSource`; the dependent VP re-registers it to `CentralizedRuleSource` | Canonical — the dependent delta re-registers the interface without touching the base solution's code; order is guaranteed by the constraint. Kind is not distinguished here: with a constraint present, DI substitution always resolves the same way regardless of "independent" vs "conflicting" |
| `TMN` | Constraint present, code change, independent | The constraint (`VP3 requires N x VP`) reduces which combinations are reachable, but the reachable ones never touch the same method | Canonical — several independent changes |
| `TMC` | Constraint present, code change, conflicting | `VP3 requires N x VP`; 2+ of the required solutions change the same method/function | Needs a resolver per legal combination — see [TMC handling](#tmc-handling) |

Only `TMC`, `FMC`, `FDC` are flagged for resolution — every other code is canonical and needs no further action. All three are formulated for **2+** intersecting solutions from the start, never assumed to be exactly a pair.

## FDC resolution
Preferred: adapt the shared slot to accept a collection (e.g. .NET's `IEnumerable<T>`) so every contributor registers independently and a dispatcher invokes all of them — this collapses the case into `FDN`, and no separate resolver solution is needed at all. Fall back to a Composite-pattern resolver only when the slot genuinely cannot become a collection. Either way, the resolver — if one is built — is never folded into either original solution; both must stay self-sufficient on their own.

## TMC handling
When the constraint defines a finite number of legal combinations (`N x required VP`), assemble the plateau context for each legal combination in advance, and write the delta module stating explicitly which combination from that set it assumes — do not write one delta that silently branches its own behavior per combination.

## The `TD-`/degenerate footnote
A solution that "looks different depending on which VP called it" is never a new row in this table — check first whether only the DI substitution differs (then it is ordinary `TD-`/`FDN`, the solution stays one) or whether the code's own structure differs (then two solutions were mistakenly bundled under one name and must be honestly split, each with its own realization — an `FMN`/`TMN` case). Never write "if called from VP1 do X, if from VP2 do Y" conditional logic inside one delta as a substitute for this split.

# The 5-step workflow
1. **Core module** — the catalog's shared baseline, the same starting point [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] later assembles from.
2. **Unconstrained deltas** — for every VP with no Constraint against another VP in the Variability Map, author or reuse the realizing solution as an ordinary, independent one via [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] (a draft contract when none exists yet), and write its wikilink into that VP's `Realized by` cell.
3. **Constrained deltas** — for every VP with a Constraint, author the realizing solution via [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] accounting for it: DI substitution (`TD-`) or independent code change (`TMN`), both canonical, ordering already guaranteed by the constraint itself. When the intersection is conflicting and the constraint defines a finite combination set, follow [TMC handling](#tmc-handling). Write each solution's wikilink into its VP's `Realized by` cell.
4. **Conflict Detection pass** — group the plateau's active `Implementation/` files by their existing `element/{element-name}` tag (see [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]]'s tagging rule):
   - Two or more `.create` files on one element → design error outside the grid; fix by turning one into `.extend`, never by writing a resolver.
   - Classify every remaining group using [The classifier](#the-classifier) above.
   - Build a resolver only for `TMC`, `FMC`, `FDC`.
5. **Fixed-point iteration** — every resolver built in step 4 is itself added to the pool grouped by `element/{element-name}`. Repeat the grouping and classification pass until no new group appears. Record the intersection in a Registry entry (see [Where a Registry entry lives](#where-a-registry-entry-lives)) for every group found, canonical or not.

Finish with two outputs: every VP row of `{catalog}/variability-map.md` now has its `Realized by` cell filled (including any resolver solutions built in steps 4–5); and a summary of one row per intersecting group, its classification code, and its resolution (canonical / resolver link / core change) — the latter is the content of the plateau's `registry/` folder, not a separate document.

# Where a Registry entry lives
Record an intersection at the **shallowest plateau** where every intersecting solution is simultaneously present in `created_by` (directly, or transitively via `parent_plateaus`) — the same placement logic [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md#Recording plateau-level decisions|plateau-create-by-solutions already uses for conflict ADRs]]. One file per element, in a `registry/` folder sibling to that plateau's `adr/` and `structure/`, using [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/templates/registry-entry.template|templates/registry-entry.template.md]]. List every registry file in the plateau root skill's `registry:` YAML property, mirroring how `adr:` is already listed. See [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/adr/intersection-registry-design|adr/intersection-registry-design]] for why this format was chosen over one shared document.

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
Never fold a resolver's logic into one of the original intersecting solutions — keep the resolver as its own, separate solution, `depends_on` naming every solution it resolves.
- Risk: folding the fix into one original solution makes that solution silently aware of, and dependent on, the other — breaking the guarantee that each stays self-sufficient on its own, and hiding the resolution from anyone reading only the original solution's file.
- Fix: create a distinct resolver solution per [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]], with `depends_on` listing every intersecting solution it resolves.

### Iterate to a fixed point
Re-run the grouping-by-`element/{element-name}` pass after building any resolver, treating the resolver as a normal participant, until a pass produces no new group.
- Risk: stopping after one pass misses a conflict the resolver itself introduces with a further solution.
- Fix: iterate to a fixed point per [step 5](#the-5-step-workflow).

### Shallowest plateau hosts the entry
Place every Registry entry at the shallowest plateau where all intersecting solutions are simultaneously present in `created_by` (directly or via `parent_plateaus`), per [Where a Registry entry lives](#where-a-registry-entry-lives).
- Risk: recording the intersection at the wrong depth either misses the plateau where it first becomes real, or duplicates the same entry into every deeper plateau that inherits it.
- Fix: check `created_by`/`parent_plateaus` transitively before placing the file.

### Double-create is a design error
Flag two or more `.create` files landing on the same element as a design error and fix it by converting one to `.extend` — never attempt to resolve it with a conflict resolver.
- Risk: treating this as an ordinary conflict produces a resolver papering over what is actually a modeling mistake (two solutions both claiming to originate the same artifact).
- Fix: identify which solution should really be extending the artifact the other creates, and correct its `Implementation/` files accordingly.

### N≥3 carries an architectural-signal note
Record an architectural-signal note on any Registry entry whose group reaches N≥3 intersecting solutions (`TMC`/`FMC`/`FDC`), stating that this is also a reason to reconsider the involved VPs' boundaries, not only a case needing one more resolver.
- Risk: treating N≥3 as "just a bigger version of the same case" hides a real signal that the variability decomposition at that point may need rethinking.
- Fix: add the note explicitly in the Registry entry, per [[skills/common-workflow/architecture/design/plateau-map/delta-conflict-detection.skill/templates/registry-entry.template|templates/registry-entry.template.md]].

### Follow the skill-design baseline
Follow [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]'s baseline (tags, `whenToUse`, link style, no leftover hint/example blocks) in addition to this skill's own rules.
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
- [ ] Every Registry entry is placed at the shallowest plateau where all intersecting solutions are simultaneously present in `created_by`.
- [ ] Any group reaching N≥3 carries the architectural-signal note.
- [ ] Two-or-more-`.create`-on-one-element cases were fixed by converting one to `.extend`, never by writing a resolver.
- [ ] The plateau root skill's `registry:` YAML property lists every Registry file created for that plateau.
- [ ] Facet tags follow [[skills/common-workflow/skill-tags.skill/skill-tags.skill.md|skill-tags]]: `concern/architecture`, bare `stack`.
