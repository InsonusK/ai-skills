---
name: solid-decomposition
description: Decompose a new piece of business logic into SOLID-compliant services/functions, confirm the decomposition with the user before generating code, and produce a compact per-feature index of usecases and test cases
whenToUse: before implementing any new business logic — a new service, function, command, or class that does more than parse input or wire dependencies. Apply it before writing code, not after.
tags:
  - architecture
  - design
  - solid
  - testing
  - documentation
---

# Goal
- Force decomposition of a task into small, single-responsibility units before code is written.
- Give the user a checkpoint to confirm the decomposition before the agent generates any code.
- Make every unit's usage scenario and test cases explicit and discoverable.
- Keep a compact, per-feature index of capabilities, units, and their test cases — generated once and updated, not re-derived by reading the whole codebase.
- Prefer automated diagram rendering over agent-drawn diagrams.

# Scope
This skill governs the process of decomposing and confirming design, independent of language or stack. It does not replace stack-specific conventions:
- If the target stack has a plateau skill (for example [plateau-plateau-python-cli](skills/python/architecture/plateau/plateau-python-cli/plateau-plateau-python-cli.skill.md), which defines `module-service-service` and `module-functions-function` templates), use its module templates to shape the file/class for each confirmed unit.
- Use [workflow-unittest-testplan](skills/common-workflow/test/workflow-unittest-testplan.skill/workflow-unittest-testplan.skill.md) and its [usecases_list.md](skills/common-workflow/test/workflow-unittest-testplan.skill/templates/usecases_list.md) template to write the test case list for each unit — do not invent a new test case format.
- Use [code-coverage](skills/common-workflow/test/code-coverage.skill.md) to decide what must be covered.
- Use [test-driven-development](skills/common-workflow/test/test-driven-development.skill/test-driven-development.skill.md) to decide the order of writing tests vs. implementation for each unit (step 4-5 below): new units get full red-green-refactor, refactors of existing units need a green baseline first, and local/mechanical fixes inside an already-decomposed unit don't need test-first ceremony.
- Use [architect-validator](skills/common-workflow/test/architect-validator.skill.md) as the final gate after code is generated.
- Use [diagram-renderer](.agents/skills/diagram-renderer/SKILL.md) to render the decomposition diagram from the index document's frontmatter links — never hand-draw it.

# Core Principle
- Decompose before you code. Never generate the implementation of new business logic in the same step as deciding its shape.
- One unit (Service, Function, Command, class) has exactly one reason to change. If its responsibility sentence needs "and", split it.
- The orchestrator (Command/controller/entry point) only coordinates calls to units; it must not contain business logic itself.
- Depend on abstractions the caller defines, not on concrete implementations of collaborators (Dependency Inversion) — list what a unit depends on as roles, not classes.
- The decomposition list is a checkpoint, not documentation-after-the-fact: show it to the user and wait for confirmation before writing code.
- Test cases are attached to the unit at design time, not discovered after the code exists.
- A diagram that a human can regenerate automatically is more trustworthy than one the agent drew by hand.

# Workflow

1. **Extract responsibilities.** Read the task and list every distinct piece of behavior it requires. Each behavior becomes a candidate unit.
2. **Draft the decomposition.** For each candidate unit, write:
   - `name` and `kind` (Service | Function | Command/orchestrator | class)
   - `responsibility` — one sentence, no "and"
   - `depends_on` — the roles/abstractions it needs (not concrete classes)
   - `usage_scenario` — 1-3 sentences: who calls it, when, with what result
3. **Confirm with the user.** Present the draft decomposition (see [decomposition list format](#decomposition-list-format)) before writing any code. Do not proceed until the user confirms or edits it.
4. **Attach test cases.** For each confirmed unit, write its test case list using the [usecases_list.md](../../../test/workflow-unittest-testplan.skill/templates/usecases_list.md) format, following [code-coverage](../../../test/code-coverage.skill.md) rules for what to cover.
5. **Generate code.** Implement exactly the confirmed units, one responsibility per unit, following [test-driven-development](../../../test/test-driven-development.skill/test-driven-development.skill.md) for the test/implementation order: a brand-new unit gets full red-green-refactor per test case; a unit created by refactoring existing code needs a green baseline before restructuring. Apply the stack's plateau/module skill if one exists for the unit's kind.
6. **Validate.** Run [architect-validator](../../../test/architect-validator.skill.md) against the generated files.
7. **Update the feature index.** Create or update `docs/features/{feature}.md` from [feature-index.template.md](./templates/feature-index.template.md): capabilities, units, links to their test case lists, and frontmatter `depends_on` links to every unit touched.
8. **Render the diagram.** Run the `diagram-renderer` CLI against `docs/features/{feature}.md` (or the `docs/features/diagrams.yaml` config if one exists) to produce the `.canvas` diagram. Do not draw the diagram by hand.

## Decomposition list format
```
- {UnitName} ({Service|Function|Command|Class})
    - responsibility: {one sentence, no "and"}
    - depends_on: {roles/abstractions, not concrete classes}
    - usage_scenario: {1-3 sentences}
```

# Rule

## MUST
- Produce and confirm the decomposition list with the user before writing implementation code for new business logic.
- Give every unit exactly one responsibility sentence with no "and".
- Express `depends_on` as roles/abstractions the unit needs, not concrete classes it constructs itself.
- Attach a test case list (via [usecases_list.md](../../../test/workflow-unittest-testplan.skill/templates/usecases_list.md)) to every confirmed unit before or immediately after generating its code.
- Keep the orchestrator/entry point free of business logic; it only calls units in sequence.
- Create or update `docs/features/{feature}.md` for every feature that added or changed units.
- Render feature diagrams with `diagram-renderer`; never hand-draw them as mermaid/ASCII in the index document.
- Run [architect-validator](../../../test/architect-validator.skill.md) after generating or changing units belonging to a plateau.

## SHOULD
- Reuse an existing unit instead of creating a near-duplicate when one already covers the responsibility.
- Keep `docs/features/{feature}.md` short: links and one-line summaries, not copies of code or full test bodies.
- Split a unit further if its `usage_scenario` requires describing more than one caller-facing outcome.

## SHOULD NOT
- Skip the confirmation step for small features "to save time" — this is exactly the step that keeps the user in control.

## MUST NOT
- Write implementation code before the decomposition list is confirmed.
- Merge two responsibilities into one unit to reduce file count.
- Let the orchestrator branch on business rules that belong to a unit.

# Anti-patterns
- **One service does several things**
  - Example: a `ReportService` that fetches data, formats it, and emails it.
  - Consequence: nobody can tell what the service does or which cases it must handle; changes to email logic risk breaking data fetching.
  - Instead: split into `ReportDataFetcher`, `ReportFormatter`, `ReportMailer` (Functions or Services depending on state), orchestrated by a `Command`.

- **No clear test cases per unit**
  - Consequence: nobody knows whether current behavior is correct or which cases are missing; regressions go unnoticed.
  - Instead: attach a [usecases_list.md](../../../test/workflow-unittest-testplan.skill/templates/usecases_list.md)-formatted list to every unit at design time.

- **No documentation of how a complex process decomposes**
  - Consequence: nobody can see what a feature is built from without re-reading all the code.
  - Instead: maintain `docs/features/{feature}.md` with links to units and their test cases, and an auto-rendered diagram via `diagram-renderer`.

- **Generating code straight from the task description**
  - Consequence: the agent produces a large, tangled implementation before the user had a chance to react; control is lost until after the fact.
  - Instead: always produce and confirm the decomposition list first (step 3 of the [workflow](#workflow)).

- **Hand-drawn diagrams in the index document**
  - Consequence: the diagram silently drifts from the real code and nobody notices.
  - Instead: derive the diagram from `depends_on` frontmatter links via `diagram-renderer`.

# Check list
- [ ] The decomposition list was shown to and confirmed by the user before code was written.
- [ ] Every unit has exactly one responsibility sentence with no "and".
- [ ] Every unit's `depends_on` lists roles/abstractions, not concrete classes.
- [ ] Every confirmed unit has a test case list in `usecases_list.md` format.
- [ ] The orchestrator/entry point contains no business logic.
- [ ] `docs/features/{feature}.md` exists and links every touched unit and its test cases.
- [ ] The feature diagram was produced by `diagram-renderer`, not drawn by hand.
- [ ] `architect-validator` was run after code generation for units belonging to a plateau.
