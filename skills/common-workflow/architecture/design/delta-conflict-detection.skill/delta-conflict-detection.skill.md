---
name: delta-conflict-detection
description: Classify and resolve intersections between solutions (deltas) that touch the same element of a plateau, using a fixed three-axis code (Constraint x Category x Kind), and record the result as per-element Registry entries inside the plateau where the intersection first becomes real
whenToUse: after a plateau's Variability Map is built and its solutions selected — before, or immediately after, assembling that plateau via plateau-create-by-solutions/plateau-update-by-solutions — whenever two or more solutions in the same plateau share an element/{element-name} tag inside their Implementation/ folders
tags:
  - skill/architecture/variability/conflict-detection
  - stack
  - concern/architecture
adr:
  - "[[skills/common-workflow/architecture/design/delta-conflict-detection.skill/adr/intersection-registry-design.md|Intersection Registry: per-element files, placed at the shallowest plateau where the intersection is real]]"
---

# Goal
- Give every pair (or larger group) of solutions that touch the same element a single, fixed classification instead of an ad-hoc "stop and ask the user" every time `plateau-create-by-solutions`/`plateau-update-by-solutions` hits a merge conflict.
- Build a resolver only for the three codes that genuinely need one, and never fold a resolver into one of the original solutions it resolves.
- Make a resolver itself a participant in the next pass, since it can intersect with a further solution its author never knew about.

# Core Principle
- This is the step that runs after [[skills/common-workflow/architecture/design/variability-map-create.skill/variability-map-create.skill.md|variability-map-create]] has produced a Realized-by combination for a plateau, and consumes the `element/{element-name}` tags [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]] already puts on every `Implementation/` file — no new tagging convention is introduced.
- The three-axis classifier below (`Constraint x Category x Kind`) is fixed. Do not reword, reorder, or re-derive it — it was arrived at after several rounds of revision in the design conversation this skill formalizes; treat it as settled input, not a draft.
- Only three codes (`TMC`, `FMC`, `FDC`) ever require building a resolver. Every other code is canonical: record it, do nothing further.
- A resolver is itself a delta and can itself intersect with a solution its author did not know about — the detection pass repeats to a fixed point, not once.

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
1. **Core module** — the plateau's starting point, built the same way [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md|plateau-create-by-solutions]] already assembles it.
2. **Unconstrained deltas** — for every VP with no Constraint against another VP in the plateau's Variability Map, build the delta as an ordinary, independent solution.
3. **Constrained deltas** — for every VP with a Constraint, write the delta accounting for it: DI substitution (`TD-`) or independent code change (`TMN`), both canonical, ordering already guaranteed by the constraint itself. When the intersection is conflicting and the constraint defines a finite combination set, follow [TMC handling](#tmc-handling).
4. **Conflict Detection pass** — group the plateau's active `Implementation/` files by their existing `element/{element-name}` tag (see [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]]'s tagging rule):
   - Two or more `.create` files on one element → design error outside the grid; fix by turning one into `.extend`, never by writing a resolver.
   - Classify every remaining group using [The classifier](#the-classifier) above.
   - Build a resolver only for `TMC`, `FMC`, `FDC`.
5. **Fixed-point iteration** — every resolver built in step 4 is itself added to the pool grouped by `element/{element-name}`. Repeat the grouping and classification pass until no new group appears. Record the intersection in a Registry entry (see [Where a Registry entry lives](#where-a-registry-entry-lives)) for every group found, canonical or not.

Finish with a summary: one row per intersecting group, its classification code, and its resolution (canonical / resolver link / core change) — this is the content of the plateau's `registry/` folder, not a separate document.

# Where a Registry entry lives
Record an intersection at the **shallowest plateau** where every intersecting solution is simultaneously present in `created_by` (directly, or transitively via `parent_plateaus`) — the same placement logic [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill.md#Recording plateau-level decisions|plateau-create-by-solutions already uses for conflict ADRs]]. One file per element, in a `registry/` folder sibling to that plateau's `adr/` and `structure/`, using [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/templates/registry-entry.template.md|templates/registry-entry.template.md]]. List every registry file in the plateau root skill's `registry:` YAML property, mirroring how `adr:` is already listed. See [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/adr/intersection-registry-design.md|adr/intersection-registry-design]] for why this format was chosen over one shared document.

A Registry entry's **Ordering** field states whether the ordering it records comes from a real Feature-Model constraint (`source: constraint` — already free, since `depends_on` had to carry it anyway) or exists purely so a resolver has something deterministic to build on (`source: ordering-only` — the resolver's own `depends_on` is the *only* place this ordering is recorded at all). See [[skills/common-workflow/architecture/design/variability-map-create.skill/adr/constraint-vs-ordering-columns.md|variability-map-create's ADR]] for why this distinction never becomes a `depends_on` schema change.

See [[skills/common-workflow/architecture/design/delta-conflict-detection.skill/examples/example-dotnet-registry-entry.md|examples/example-dotnet-registry-entry.md]] for a worked entry against a real catalog.

# Rule

## MUST
- Use the classifier exactly as defined in [The classifier](#the-classifier) — never reword, reorder, merge, or split its axes or codes.
  - Risk: re-deriving the taxonomy independently each time it is applied produces inconsistent codes across plateaus and silently redoes settled design work.
  - Fix: apply the fixed table; if a real case does not fit, raise it as a question rather than inventing a new code informally.
- Build a resolver only for `TMC`, `FMC`, `FDC`; treat every other code as canonical and take no further action beyond recording it.
  - Risk: writing unnecessary resolvers for canonical cases adds indirection nothing needed.
  - Fix: check the code against [The classifier](#the-classifier) before deciding a resolver is needed at all.
- Never fold a resolver's logic into one of the original intersecting solutions — keep the resolver as its own, separate solution, `depends_on` naming every solution it resolves.
  - Risk: folding the fix into one original solution makes that solution silently aware of, and dependent on, the other — breaking the guarantee that each stays self-sufficient on its own, and hiding the resolution from anyone reading only the original solution's file.
  - Fix: create a distinct resolver solution per [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill.md|solution-create]], with `depends_on` listing every intersecting solution it resolves.
- Re-run the grouping-by-`element/{element-name}` pass after building any resolver, treating the resolver as a normal participant, until a pass produces no new group.
  - Risk: stopping after one pass misses a conflict the resolver itself introduces with a further solution.
  - Fix: iterate to a fixed point per [step 5](#the-5-step-workflow).
- Place every Registry entry at the shallowest plateau where all intersecting solutions are simultaneously present in `created_by` (directly or via `parent_plateaus`), per [Where a Registry entry lives](#where-a-registry-entry-lives).
  - Risk: recording the intersection at the wrong depth either misses the plateau where it first becomes real, or duplicates the same entry into every deeper plateau that inherits it.
  - Fix: check `created_by`/`parent_plateaus` transitively before placing the file.
- Flag two or more `.create` files landing on the same element as a design error and fix it by converting one to `.extend` — never attempt to resolve it with a conflict resolver.
  - Risk: treating this as an ordinary conflict produces a resolver papering over what is actually a modeling mistake (two solutions both claiming to originate the same artifact).
  - Fix: identify which solution should really be extending the artifact the other creates, and correct its `Implementation/` files accordingly.
- Record an architectural-signal note on any Registry entry whose group reaches N≥3 intersecting solutions (`TMC`/`FMC`/`FDC`), stating that this is also a reason to reconsider the involved VPs' boundaries, not only a case needing one more resolver.
  - Risk: treating N≥3 as "just a bigger version of the same case" hides a real signal that the variability decomposition at that point may need rethinking.
  - Fix: add the note explicitly in the Registry entry, per [templates/registry-entry.template.md](./templates/registry-entry.template.md).
- Follow [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]'s baseline (tags, `whenToUse`, link style, no leftover hint/example blocks) in addition to this skill's own rules.

## SHOULD
- Prefer the `IEnumerable<T>`-style collection fix for `FDC` over a Composite resolver whenever the shared slot can reasonably become a collection.
- Cross-check a `TD-`/degenerate-looking case against [the footnote](#the-td--degenerate-footnote) before accepting it as ordinary DI substitution.

## MAY
- Skip writing a Registry entry for an `-N-` group when the catalog's scale makes tracking every non-intersection impractical — this code needs no action either way, so the entry is a convenience, not a requirement.

# Check list
- [ ] Every intersecting group found by grouping on `element/{element-name}` was classified using the fixed table in [The classifier](#the-classifier), with no reworded or invented codes.
- [ ] A resolver was built only for `TMC`, `FMC`, or `FDC` groups.
- [ ] Every resolver is its own solution with `depends_on` naming every solution it resolves — none folded into an original solution.
- [ ] The grouping pass was repeated after adding any resolver, until no new group appeared.
- [ ] Every Registry entry is placed at the shallowest plateau where all intersecting solutions are simultaneously present in `created_by`.
- [ ] Any group reaching N≥3 carries the architectural-signal note.
- [ ] Two-or-more-`.create`-on-one-element cases were fixed by converting one to `.extend`, never by writing a resolver.
- [ ] The plateau root skill's `registry:` YAML property lists every Registry file created for that plateau.
- [ ] Facet tags follow [[skills/common-workflow/skill-design.skill/facet-vocabulary.md|facet-vocabulary]]: `concern/architecture`, bare `stack`.
