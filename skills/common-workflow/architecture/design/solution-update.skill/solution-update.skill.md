---
name: solution-update
description: Define how to update an existing solution skill, record the change as an ADR, and propagate the change to dependent solutions and plateaus
whenToUse: when an existing solution skill must be changed — its rules, implementation files, dependencies, or metadata
tags:
  - skill/architecture/solution/design
  - stack
  - concern/architecture

---

# Goal
- Change an existing solution skill without breaking consistency: the change is recorded as an ADR, every solution that depends on the updated solution is updated too, and every plateau built on the changed solutions is refreshed.

# Core Principle
- A solution never changes in isolation: the update is finished only when the ADR is recorded, dependent solutions are updated, and dependent plateaus are refreshed via plateau-update-by-solutions.

# Prerequisites
- Read [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill|solution-create]] first. All authoring rules of a solution skill (template structure, `whenToUse`, `Implementation/` folder, facet tags, removal of `hint`/`example` blocks) apply unchanged to an update.
- Read [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]] — every architecture decision made during the update is recorded as an ADR.
- Read [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/plateau-update-by-solutions.skill|plateau-update-by-solutions]] — it defines how plateaus are refreshed after the solution changes.

# Input parameters
- {solution} - path to the solution skill folder being updated
- {change} - description of what must change (rules, implementation files, dependencies, metadata)

# Workflow
1. Understand the requested change: what must change in the solution and why. If anything is unclear, ask the user before editing.
2. Apply the change to the solution skill following the authoring rules of [[skills/common-workflow/architecture/design/solution-create.skill/solution-create.skill|solution-create]]:
   - Update the skill file and the `Implementation/` folder so they stay consistent with each other.
   - Keep facet tags, `whenToUse`, and the `Implementation/` rules from solution-create intact.
   - Bump `version` of the solution skill.
3. Record the decision behind the change as an ADR following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]]:
   - Create the ADR in the solution's own `adr/` folder.
   - Register it in the solution's `adr:` YAML property and link it from the skill body.
4. Find and update dependent solutions:
   - Search other solution skills for references to {solution} (in `created_by`, dependency statements, or applied-solutions lists).
   - Update each dependent solution to reflect the change, recording its own ADR per step 3 when the change alters a decision.
5. Update every plateau that applies any of the changed solutions following [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/plateau-update-by-solutions.skill|plateau-update-by-solutions]]:
   - Search `skills/*/architecture/plateau/**` for plateaus whose `created_by` or `__Applied solutions__` references a changed solution.
   - For each affected plateau, run the plateau-update-by-solutions workflow with the changed solution as input, including propagation through child plateaus via `parent_plateau`.
6. Verify the result with the [check list](#check-list).

# Rule

## MUST
- Record every architecture decision made during the update as an ADR in the solution's own `adr/` folder, following [[skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill|adr-create]], and register it in the solution's `adr:` YAML property.
  - Risk: the reason for the change is lost, and the next editor re-argues or silently reverts the decision.
  - Fix: create the ADR immediately when the change is applied, before moving on to dependent solutions and plateaus.
- Keep the solution skill file and its `Implementation/` folder consistent after the update.
  - Risk: the rules describe one behavior while the implementation files demonstrate another, and agents applying the solution produce wrong code.
  - Fix: re-scan `Implementation/` after editing the skill file and align both sides.
- Update every other solution that depends on the changed solution.
  - Risk: dependent solutions keep referencing the old behavior and produce conflicting implementations.
  - Fix: search for references to {solution} across solution skills and update each one found.
- Refresh every plateau that applies the changed solution (or a dependent solution changed in this pass) via [[skills/common-workflow/architecture/design/plateau-update-by-solutions.skill/plateau-update-by-solutions.skill|plateau-update-by-solutions]].
  - Risk: plateau root skills and structural skills go stale, so projects generated from the plateau miss the change.
  - Fix: run plateau-update-by-solutions for every affected plateau, including child plateaus linked via `parent_plateau`.
- Bump `version` of the updated solution skill and of every skill changed during propagation.

## MUST NOT
- Change a solution without creating an ADR for the decision behind the change.
- Skip the plateau refresh because the solution change "looks small" — even a small rule change makes plateau structural skills stale.
- Change solutions or plateaus that do not depend on the updated solution.

# Anti-patterns
- **Solution updated, ADR skipped**
  - Example: implementation files are rewritten but no ADR is added to `adr/`.
  - Consequence: the trade-offs behind the change are lost and the decision gets re-argued later.
  - Instead: record the ADR in the same pass, following adr-create.

- **Propagation stops at the solution itself**
  - Example: the solution is updated, but dependent solutions and plateaus still reference the old behavior.
  - Consequence: the skill set is inconsistent — two skills derived from the same solution contradict each other.
  - Instead: update dependent solutions, then run plateau-update-by-solutions for every affected plateau.

# Check list
- [ ] The solution skill file and its `Implementation/` folder are consistent after the update
- [ ] `version` of the updated solution is bumped
- [ ] An ADR for the change is created in the solution's `adr/` folder, registered in `adr:`, and linked from the skill body (per adr-create)
- [ ] Facet tags and `whenToUse` still follow solution-create rules; no `hint`/`example` blocks remain
- [ ] All solutions depending on the updated solution are found and updated (with their own ADRs when decisions change)
- [ ] plateau-update-by-solutions is executed for every plateau applying a changed solution, including child plateaus via `parent_plateau`
