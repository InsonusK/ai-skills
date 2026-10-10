---
version: 20261010120000
name: solid-decomposition
description: Decompose a new piece of business logic into SOLID-compliant, single-responsibility units and confirm the decomposition with the user before generating code
whenToUse: before implementing any new business logic — a new service, function, command, or class that does more than parse input or wire dependencies. Apply it before writing code, not after.
tags:
  - skill/develop
  - usecase
  - concern/architecture
  - concern/coding
  - stack

---

# Goal
- A decomposition list of small, single-responsibility units, written before any code.
- The user's confirmation of that list before the agent generates code.
- An orchestrator that only coordinates units, and units that depend on roles instead of concrete classes.

# Scope
This skill governs how new business logic is split into units, independent of language or stack. It does not replace stack-specific conventions:
- If the project is built from a plateau skill, use its module templates to shape the file/class for each confirmed unit.
- The order of writing tests and implementation for each unit is set by [test-driven-development](skills/testing/core/test-driven-development.skill/test-driven-development.skill.md).

# Core Principle
- Decompose before you code. Never generate the implementation of new business logic in the same step as deciding its shape.
- One unit (Service, Function, Command, class) has exactly one reason to change. If its responsibility sentence needs "and", split it.
- The orchestrator (Command/controller/entry point) only coordinates calls to units; it must not contain business logic itself.
- Depend on abstractions the caller defines, not on concrete implementations of collaborators (Dependency Inversion) — list what a unit depends on as roles, not classes.
- The decomposition list is a checkpoint, not documentation-after-the-fact: show it to the user and wait for confirmation before writing code.

# Workflow

1. **Extract responsibilities.** Read the task and list every distinct piece of behavior it requires. Each behavior becomes a candidate unit.
2. **Draft the decomposition.** For each candidate unit, write:
   - `name` and `kind` (Service | Function | Command/orchestrator | class)
   - `responsibility` — one sentence, no "and"
   - `depends_on` — the roles/abstractions it needs (not concrete classes)
   - `usage_scenario` — 1-3 sentences: who calls it, when, with what result
3. **Confirm with the user.** Present the draft decomposition (see [decomposition list format](#decomposition-list-format)) before writing any code. Do not proceed until the user confirms or edits it.
4. **Generate code.** Implement exactly the confirmed units, one responsibility per unit, in the test/implementation order of [test-driven-development](skills/testing/core/test-driven-development.skill/test-driven-development.skill.md).

## Decomposition list format
```
- {UnitName} ({Service|Function|Command|Class})
    - responsibility: {one sentence, no "and"}
    - depends_on: {roles/abstractions, not concrete classes}
    - usage_scenario: {1-3 sentences}
```

# Rule

## MUST
- Produce and confirm the decomposition list with the user before writing implementation code for new business logic, even for small features.
  - Violation: generating code straight from the task description without first producing and confirming the decomposition list, or skipping confirmation "to save time" because a feature seems small.
  - Risk: the agent produces a large, tangled implementation before the user had a chance to react, and control is lost until after the fact — skipping confirmation removes exactly the checkpoint that keeps the user in control.
  - Fix: always produce and confirm the decomposition list first (step 3 of the [workflow](#workflow)), regardless of feature size.
- Give every unit exactly one responsibility sentence with no "and"; never merge two responsibilities into one unit to reduce file count.
  - Violation: a `ReportService` that fetches data, formats it, and emails it.
  - Risk: nobody can tell what the service does or which cases it must handle; changes to email logic risk breaking data fetching.
  - Fix: split into `ReportDataFetcher`, `ReportFormatter`, `ReportMailer` (Functions or Services depending on state), orchestrated by a `Command`.
- Express `depends_on` as roles/abstractions the unit needs, not concrete classes it constructs itself.
- Keep the orchestrator/entry point free of business logic; it only calls units in sequence and never branches on business rules that belong to a unit.

## SHOULD
- Reuse an existing unit instead of creating a near-duplicate when one already covers the responsibility.
- Split a unit further if its `usage_scenario` requires describing more than one caller-facing outcome.

# Check list
- [ ] The decomposition list was shown to and confirmed by the user before code was written.
- [ ] Every unit has exactly one responsibility sentence with no "and".
- [ ] Every unit's `depends_on` lists roles/abstractions, not concrete classes.
- [ ] The orchestrator/entry point contains no business logic.
