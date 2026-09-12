---
name: plateau-update-by-solutions
description: When a solution is added to a plateau or a solution used by a plateau is updated, propagate changes to the plateau root skill and to every skill inside the plateau's structure folder.
whenToUse: when a solution is added to a plateau, when an existing solution referenced by a plateau is updated, or when a solution is removed from a plateau
updated: 20260909
tags:
  - skill/architecture/plateau/design
  - stack
  - concern/architecture
---

# Goal
- The plateau root skill (`plateau-{plateau-name}.skill.md`) with `description`, `created_by`, `Core Principles`, `Capabilities`, `Use cases`, `__Applied solutions:__`, and `version` consistent with the changed solution.
- Every `structure/**` skill affected by the solution created, extended, or cleaned, with up-to-date `created_by` and `__Applied solutions:__` and a bumped `version`.
- A plateau-level ADR for every conflict resolution, solution removal, or emptied-skill deletion, listed in the root skill's `adr:` property and recording which plateaus the change propagated to.
- Every child and grandchild plateau that composed the solution updated by the same workflow.
- No `hint`, `example`, or `# How Apply this template` block left in any rewritten skill.

# Core Principle
- **Propagate every solution change through the whole plateau** - A solution added, updated, or removed must reach the plateau root skill and every `structure/**` skill assembled from that solution, or those skills go stale.
- **An update is assembly re-run over a diff** - Discovery of affected files and name normalization use the exact rules from [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill|plateau-create-by-solutions]].
- **Reach every descendant** - A shared solution's change must follow `parent_plateaus` into every child and grandchild plateau that composed it; see [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]] for merge semantics.
- **The plateau owns update decisions** - Every conflict resolution, solution removal, and emptied-skill deletion is recorded as a plateau-level ADR following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]].
- **A Component is not a Solution** - If the input only wires in at the composition root and never touches a module, it is a Plateau Component and this workflow does not apply; see [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]].

# Workflow

Inputs:
- `{plateau-name}` — the plateau to update.
- `{solution}` — the solution skill being added, updated, or removed.
- `{stack}` — the plateau's target language/stack (`dotnet`, `python`, `typescript`, …); detect from `{solution}` or the existing plateau.
- `{output}` — the folder containing the plateau. Default `skills/{stack}/architecture/plateau/{plateau-name}`.

Before starting, read [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill|plateau-create-by-solutions]] (the same `Implementation/` file patterns, name normalization, `.create.md`/`.extend.md` semantics, and placeholder rules apply) and [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]]; check `{solution}` against [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]] first.

## Identify affected structural skills
1. Open the plateau root skill `{output}/plateau-{plateau-name}.skill.md`.
2. Scan `{output}/structure/` for skills that already reference `{solution}` in `created_by` or `__Applied solutions:__`.
3. Scan `{solution}`'s `Implementation/` folder for every file (`Repository.create.md`/`.extend.md`; .NET `{Project}.csproj.create.md`/`.extend.md` and nested `{Class}.cs.*`; Python `{App}.create.md`/`.extend.md` and nested class/functions/init files; Angular `{project}.project.create.md`/`.extend.md` plus `.federation.extend.md`/project-level `.extend.md` and every `{name}.{artifact-type}.ts`/`.ts`/`.spec.ts`/`.scss` file, flattening topic subfolders).
4. Map each discovered file to a structural skill using [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill|plateau-create-by-solutions]]'s normalization: `Repository.*` → `sln-{plateau-name}` (.NET) / `repo-{plateau-name}` (Python/Angular); project/package files → `csproj-`/`package-`/`project-{normalized}`; class/module files → `class-`/`module-{normalized}` (`artifact_type` per that skill's mapping).
5. The union of (2) and (4) is the set of skills to create or update.

## Apply the change
6. For each affected structural skill, apply the correct `.create`/`.extend` action per [Create or extend each affected structural skill](#create-or-extend-each-affected-structural-skill); add or update its goals, core principles, rules, anti-patterns, check lists, `created_by`, and `__Applied solutions:__`; bump its `version`.
7. Update the plateau root skill's `description`, `created_by`, `Core Principles`, `Capabilities`, `Use cases`, and `__Applied solutions:__`; bump its `version`.
8. On a removal, strip content and `created_by`/`__Applied solutions:__` entries that came only from `{solution}` per [Remove solution content cleanly](#remove-solution-content-cleanly).
9. Search every `plateau-*.skill.md` under `skills/{stack}/architecture/plateau/**` whose `parent_plateaus` contains `{plateau-name}`; repeat this whole workflow against each child that has `{solution}` in its own `created_by`, recursing into grandchildren.
10. Record every conflict resolution, removal, or emptied-skill deletion as a plateau-level ADR in `{output}/adr/`, and verify no `hint`/`example`/`# How Apply this template` block remains in any rewritten skill.

# Rule

## MUST

### Confirm the input is a Solution, not a Component
Confirm `{solution}` is a Solution and not a Plateau Component before running the workflow.
- Risk: a Plateau Component added to a plateau's `created_by`/`structure/` forces every user of the plateau to take an optional capability and forces a second "without it" variant.
- Fix: check it against [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]]; a Component attaches to a composed service separately, never here.

### Apply the plateau-create-by-solutions mapping
Use the `Implementation/` file patterns, name normalization, and `.create.md`/`.extend.md` semantics from [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill|plateau-create-by-solutions]] for `{stack}` when discovering and naming affected skills.
- Risk: an ad-hoc mapping produces structural skills whose names or shape do not match what assembly would have produced, so the plateau drifts from its own generator.
- Fix: normalize every discovered file through that skill's tables.

### Update the plateau root skill on every change
On every add, update, or removal, bring the plateau root skill's `description`, `created_by`, `Core Principles`, `Capabilities`, `Use cases`, and `__Applied solutions:__` into line with the changed solution, and bump its `version`.
- Risk: a root skill that still describes the old solution set misleads every reader and every downstream assembly.
- Fix: edit all six fields plus `version` in the same pass as the structural skills.

### Create or extend each affected structural skill
Apply the action the solution's file kind dictates: a `.create.md` whose target skill does not exist → create it from the template; a `.create.md` whose target already exists → ask the user whether to overwrite or merge; a `.extend.md` whose target skill does not exist → stop, the base skill must exist first; a `.extend.md` whose target exists → merge the extension in.
- Violation: creating a second skill for a `.extend.md` because the base skill was not found, instead of stopping.
- Risk: a wrong action either loses the base content or splits one element across two skills.
- Fix: follow the four cases exactly; escalate the `.create`-over-existing conflict to the user.

### Maintain created_by
Add `{solution}`'s link to `created_by` of every affected skill when absent, keep the existing order (or append consistently with the other plateau skills), and never introduce duplicates.
- Risk: a missing entry hides which solution a skill's content came from; a duplicate or reordered list churns diffs and confuses merges.
- Fix: add once, in consistent position, deduplicated.

### Never remove other solutions from created_by
Never remove a solution other than `{solution}` from any `created_by` list unless the user explicitly instructs it.
- Risk: an unrequested removal silently drops another solution's contribution from the plateau.
- Fix: touch only `{solution}`'s entry; raise anything else with the user.

### Bump version on every changed skill
Bump `version` on every structural skill changed in the pass and on the plateau root skill.
- Risk: an unchanged `version` on edited content makes consumers and the validation queue trust a stale file.
- Fix: update the `version` timestamp wherever content changed.

### Propagate through child plateaus
After updating `{plateau-name}`, search for every plateau whose `parent_plateaus` contains it, repeat this workflow for `{solution}` against each child that has `{solution}` in its own `created_by`, and recurse into grandchildren until no plateau references the one just updated.
- Violation: stopping at the first plateau updated, or assuming a plateau has no children without searching `parent_plateaus`.
- Risk: a child's structural skills were assembled from the same solution and go stale when the change stops at the parent.
- Fix: follow `parent_plateaus` outward; skip a child only when its `created_by` never included `{solution}`, never because propagating looks like extra work. When a child composes `{plateau-name}` alongside another parent and the change now conflicts with that parent, stop, ask the user, and record a plateau-level ADR on the child per [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]].

### Remove solution content cleanly
On a removal, strip from the plateau root skill and every affected structural skill the content, `created_by` entries, and `__Applied solutions:__` bullets that came only from `{solution}`; when a structural skill is left with no content and no `created_by`, delete it or ask the user.
- Risk: leftover fragments of a removed solution describe behaviour the plateau no longer has.
- Fix: remove solution-only content everywhere it appears; bump `version` on every changed skill.

### Record plateau-level decisions as ADRs
Record every conflict resolution, solution removal, and decision to delete or keep an emptied structural skill as a plateau-level ADR in `{output}/adr/` following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]], list it in the plateau root skill's `adr:` property, and note in it which plateaus the change propagated to.
- Risk: the decision and its alternatives are lost once the update is merged, and the propagation trail is invisible.
- Fix: write the ADR, register it in `adr:`, record the full propagation set.

### Strip template scaffolding
Verify no `hint`, `example`, or `# How Apply this template` block remains in any skill rewritten during the update.
- Risk: leftover authoring aids make the updated skills noisy and hide the binding rules.
- Fix: remove every such block before finishing.

# Example
- [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/examples/example-add-solution|Adding a new solution to a .NET plateau]]
- [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/examples/example-update-solution|Updating an existing solution in a .NET plateau]]

# Check list
- [ ] `{solution}` was confirmed to be a Solution, not a Plateau Component, before the workflow ran.
- [ ] [[skills/common-workflow/architecture/design/plateau-create-by-solutions.skill/plateau-create-by-solutions.skill|plateau-create-by-solutions]]'s mapping rules were applied for the plateau's `{stack}`.
- [ ] The plateau root skill references `{solution}` in `created_by`, describes it in `Core Principles`/`Capabilities`, and lists it in the correct `__Applied solutions:__` list.
- [ ] Every affected structural skill was identified via `created_by`, `__Applied solutions:__`, and `{solution}`'s `Implementation/` folder.
- [ ] New skills were created for `.create.md` files with no matching skill; existing skills were extended for `.extend.md` files.
- [ ] Every affected skill's `created_by` is current, ordered consistently, and free of duplicates; no other solution was removed.
- [ ] `version` was bumped on the plateau root skill and every changed structural skill.
- [ ] On a removal, solution-only content and links were stripped everywhere and emptied skills were deleted or escalated.
- [ ] Plateaus whose `parent_plateaus` contains `{plateau-name}` were searched, and the workflow was repeated for every child and grandchild that includes `{solution}`.
- [ ] Every conflict resolution, solution exclusion, and removal decision is a plateau-level ADR in `{output}/adr/`, listed in the root skill's `adr:`.
- [ ] No `hint`, `example`, or `# How Apply this template` block remains in any rewritten skill.
