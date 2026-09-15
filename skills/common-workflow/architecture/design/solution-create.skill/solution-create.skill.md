---
name: solution-create
description: Define how to create new solution skills by patterns
whenToUse: when you write a solution skill
updated: 20260909
tags:
  - skill/architecture/solution/design
  - stack
  - concern/architecture
---

# Goal
- A solution skill built from the template folder matching the target stack, filled with real content, with no `hint`, `example`, `code example`, or `# How Apply this template` block left.
- An `Implementation/` folder with a concrete file per created or extended element.
- One concrete `whenToUse` sentence and no separate `triggers` list.
- Facet tags filled on the solution file, every `Implementation/` file, and every ADR.
- Every architecture decision recorded as an ADR registered in `adr:`; every unfamiliar term a page under the solution's own `glossary/`.

# Core Principle
- **Template per stack, stripped before finalizing** - Build every solution skill from the template folder that matches the target stack, fill it with real content, and remove all authoring aids before finalizing.
- **No solution without its Implementation** - A solution skill is incomplete without its `Implementation/` folder; the rules describe what those files demonstrate.
- **Rule out a Plateau Component first** - A self-contained, optional capability that ships as its own project, wires in once at the composition root, and never touches a module's own files is a Plateau Component, not a Solution — run [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md|plateau-component-create]]'s Solution vs Plateau vs Component test before building here.

# Workflow
1. Understand the solution's goal, the capabilities it gives the project, how those capabilities are achieved, its requirements (dependency solutions or packages), and the target language/stack.
2. Analyze them and ask the user about any doubt.
3. Once every doubt is closed, follow [How to build a solution](#how-to-build-a-solution).

## How to build a solution
1. Detect the target language/stack from the task context, or ask the user if it is unclear.
2. Use the template folder that matches the target language:
   - .NET: [templates/dotnet/solution-{Solution}.skill.template](./templates/dotnet/solution-{Solution}.skill.template/)
   - Python: [templates/python/solution-{Solution}.skill.template](./templates/python/solution-{Solution}.skill.template/)
   - TypeScript: [templates/typescript/solution-{Solution}.skill.template](./templates/typescript/solution-{Solution}.skill.template/)
   - Other languages can be added as separate subfolders under `templates/` when needed.
3. Fill the template with real content.
4. Follow the authoring rules in each section of the template (`hint` blocks are rules; `example`/`code example` blocks are examples), then remove every `hint`, `example`, and `code example` block and the `# How Apply this template` block from the final skill file.

# Rule

## MUST

### Confirm the candidate is a Solution, not a Component
Confirm the candidate unit is a Solution, not a Plateau Component, before building it here.
- Risk: a self-contained, composition-root-only, optional capability (e.g. logging) built as a Solution is either forced into some plateau's `created_by` — forcing every user of that plateau to take it and forcing a second "without it" variant — or left dangling with nothing to compose it into.
- Fix: run [[skills/common-workflow/architecture/design/plateau-component-create.skill/plateau-component-create.skill.md#Solution vs Plateau vs Component|plateau-component-create's test]] first; build a Component there instead if it applies.

### Select the matching stack template
Select the `templates/{stack}/` folder that matches the target language/stack of the solution.
- Risk: the solution inherits another stack's project layout, file naming, and dependency conventions, so every agent applying it produces wrong code.
- Fix: use the folder matching the target stack; if none exists, ask the user before falling back to another stack's template.

### Follow the template's How-Apply rules
Follow the `# How Apply this template` rules defined in the selected template, section by section.
- Risk: sections get filled inconsistently, so the resulting skill does not match its siblings and agents cannot rely on the structure.
- Fix: read the template's `# How Apply this template` section first and apply it as you fill each section.

### Write one concrete whenToUse sentence
Write `whenToUse` as one concrete sentence naming the task types or situations that must make an agent apply this solution, per the [skill-design](skills/design/skill-design.skill/skill-design.skill.md) baseline — decidable from that sentence alone.
- Violation: a `triggers:` list of loose keywords with no `whenToUse` sentence.
- Risk: the agent cannot decide whether the solution applies and either skips it or applies it blindly.
- Fix: write one `whenToUse` sentence naming the concrete tasks (e.g. "when implementing a command/write-operation handler, or adding a new feature to an existing module").

### Strip template scaffolding
Remove all `hint`, `example`, and `code example` blocks, and the `# How Apply this template` block, from the final skill file.
- Risk: the final skill is noisy and the agent cannot tell binding rules from authoring aids.
- Fix: delete every such fenced block and the `# How Apply this template` section before committing.

### Always provide an Implementation folder
Create an `Implementation/` folder with concrete implementation files for every solution skill — including classification, decision, policy, or taxonomy skills — showing how each selected variant manifests in code, configuration, or project structure.
- Risk: the agent gets rules with no concrete shape and invents its own inconsistent implementation.
- Fix: add one implementation file per created or extended element under `Implementation/`, following the template's naming rules.

### State applied and skipped dependency solutions
When the skill depends on other solutions, state in each implementation variant or section which dependency solution(s) are applied and which are intentionally not applied.
- Risk: the agent cannot tell whether a missing piece is an oversight or a deliberate exclusion and may apply conflicting solutions together.
- Fix: name the applied dependency solutions and the intentionally skipped ones explicitly in the implementation file.

### Declare a whole-plateau assumption via built_on_plateau
When the solution assumes an entire plateau already exists rather than a handful of sibling solutions, declare it via a single `built_on_plateau`, not by listing that plateau's individual solutions in `depends_on`.
- Risk: listing the plateau's individual solutions in `depends_on` hides the real shape of the dependency and drifts out of sync the moment that plateau's composition changes.
- Fix: follow [[skills/common-workflow/architecture/design/solution-plateau-hierarchy.skill.md|solution-plateau-hierarchy]]; give the solution at most one `built_on_plateau`.

### Show AS IS and TO BE when built_on_plateau is set
When `built_on_plateau` is set, every `# Implementation changes` section must show the AS IS state inherited from the plateau and the TO BE state after applying the solution.
- Risk: agents cannot tell which code already exists because of the plateau and which the solution adds, so they duplicate or overwrite existing plateau code.
- Fix: in each implementation file that touches a plateau element, describe the AS IS implementation (copied or summarized from the plateau skill) and the TO BE implementation after the solution's changes.

### State unimplemented assumptions in Boundaries
When a Rule assumes behavior this solution does not itself implement and does not require via a named `depends_on` solution, state that assumption in `# Boundaries` instead of adding a `depends_on` link for it.
- Risk: a rule reads as self-contained while silently relying on an unstated external guarantee, and reviewers cannot tell a genuine dependency from a loose assumption.
- Fix: add a `# Boundaries` bullet naming the gap; mention a solution that closes it today only informationally, never as a `depends_on` requirement.

### Record decisions as ADRs
Record every architecture decision made while building or editing the solution as an ADR following [adr-create](skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill.md), registered in the solution's `adr:` property and linked from the skill body.
- Risk: the rejected alternatives and trade-offs are lost, and the decision gets re-argued the next time someone edits the solution.
- Fix: create the ADR immediately, register it in `adr:`, link it from the body.

### Document unfamiliar terms in glossary
Document every term, library, technology, or pattern a reader may not know inside the solution skill's own `glossary/` folder following [documentation-for-concept](skills/common-workflow/documentation/documentation-for-concept.skill/documentation-for-concept.skill.md), and link to it from the skill file.
- Risk: the agent guesses the meaning of an unfamiliar term and applies the solution wrongly.
- Fix: add a concept page per unfamiliar term under `glossary/` and link it where the term is used; keep the glossary inside the skill folder so the solution stays portable.

### Fill the facet tags
Tag the solution skill and its supporting files with the facet tags required by [skill-design](skills/design/skill-design.skill/skill-design.skill.md): the solution file carries `solution/{solution-name}`; every `Implementation/` file carries `solution/{solution-name}` and `element/{element-name}`; every ADR carries `solution/{solution-name}`, `concern/documentation`, and `concern/documentation/adr`.
- Risk: tag-expression queries cannot resolve which files belong to the solution, so they are invisible to agents building a solution-scoped skillset.
- Fix: fill the `tags:` blocks as shown in the templates when creating each file.

### Never change other skills
Never change any skill other than the one you are building without explicit instruction in the template.
- Risk: unrelated skills drift out of sync with their own templates and the change escapes review scoped to this solution.
- Fix: limit edits to the new solution skill folder; propose changes to other skills separately.

### Never leave the Implementation folder empty
Never leave `Implementation/` empty or claim "no direct mutations" unless the solution truly produces no code, configuration, or project changes.
- Risk: the skill looks applicable but gives the agent nothing concrete to execute.
- Fix: add the implementation files, or state explicitly why the solution produces no artifacts.

### Never add a triggers list
Never add a separate `triggers` list next to `whenToUse`.
- Risk: trigger conditions split across two fields drift apart, and the agent does not know which is authoritative.
- Fix: put every trigger condition into the `whenToUse` sentence itself.

# Check list
- [ ] The candidate was checked against `plateau-component-create`'s Solution vs Plateau vs Component test before being built as a Solution.
- [ ] The template folder matches the target stack.
- [ ] `whenToUse` is one concrete sentence; there is no separate `triggers` list.
- [ ] No `hint`, `example`, `code example` blocks and no `# How Apply this template` section remain in the final skill.
- [ ] `Implementation/` contains concrete files for every created/extended element.
- [ ] Applied and intentionally skipped dependency solutions are stated where relevant.
- [ ] If this solution assumes an entire plateau exists, `built_on_plateau` names it (at most one) instead of `depends_on` listing that plateau's individual solutions.
- [ ] When `built_on_plateau` is set, every `# Implementation changes` section shows AS IS (plateau state) and TO BE (after solution).
- [ ] Every assumption a Rule relies on but does not implement or require via `depends_on` is stated in `# Boundaries`.
- [ ] Architecture decisions are recorded as ADRs and registered in the `adr:` property.
- [ ] Unfamiliar terms are documented in the solution's own `glossary/` folder.
- [ ] Facet tags are filled: `solution/{solution-name}` on the solution file, `solution/{solution-name}` + `element/{element-name}` on Implementation files, `solution/{solution-name}` + `concern/documentation` + `concern/documentation/adr` on ADRs.
