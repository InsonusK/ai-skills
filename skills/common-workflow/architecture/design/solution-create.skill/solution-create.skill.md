---
name: solution-create
description: Define how to create new solution skills by patterns
whenToUse: when you write a solution skill
tags:
  - skill/architecture/solution/design
  - stack
  - concern/architecture

---

# Goal
- Produce solution skills that an agent can apply without guessing: consistent structure across stacks, concrete implementation files, no leftover template hints.

# Core Principle
- Build every solution skill from the template folder that matches the target stack, fill it with real content, and strip all authoring aids before finalizing.
- A solution skill is incomplete without its `Implementation/` folder — the rules describe what the implementation files demonstrate.

# Workflow
1. Understand 
  - goal of solution
  - capabilities which solution will give to project
  - how capabilities will be achieved
  - requirements of solution, does it depends on other solutions or packages
  - target language/stack of the project (dotnet, python, angular, etc.)
2. Analyze them and if you have any doubts, ask the user
3. When all doubts are closed start [build solution](#how-to-build-a-solution)

# How to build a solution
1. Detect the target language/stack from the task context or ask the user if it is unclear.
2. Use the template folder that matches the target language:
   - .NET: [templates/dotnet/solution-{Solution}.skill.template](./templates/dotnet/solution-{Solution}.skill.template/)
   - Python: [templates/python/solution-{Solution}.skill.template](./templates/python/solution-{Solution}.skill.template/)
   - TypeScript: [templates/typescript/solution-{Solution}.skill.template](./templates/typescript/solution-{Solution}.skill.template/)
   - Other languages can be added as separate subfolders under `templates/` when needed.
3. Fill the template with real content.
4. Follow authoring rules which are defined in each section of the template.
   - Authoring rules are defined in `hint` blocks.
   - Authoring examples are provided in `example` and `code example` blocks.
   - After filling template with real content remove all `hint`, `example` and `code example` blocks from final skill file. Do not keep them in the final skill file.
   - Remove the `# How Apply this template` block from the final skill file.

# Rule

## MUST
- Select the template folder that matches the target language/stack of the solution.
  - Risk: the solution inherits another stack's project layout, file naming, and dependency conventions, so every agent applying it produces wrong code.
  - Fix: use the `templates/{stack}/` folder matching the target stack; if none exists, ask the user before falling back to another stack's template.
- Follow "# How Apply this template" rules defined in the selected template.
  - Risk: sections get filled inconsistently, so the resulting skill does not match its siblings and agents cannot rely on the structure.
  - Fix: read the `# How Apply this template` section of the chosen template first and apply it section by section.
- Write `whenToUse` in the solution skill header as one concrete sentence naming the task types or situations that must make an agent apply this solution (e.g. "when implementing a command/write-operation handler, or adding a new feature to an existing module"). Follow the `whenToUse` baseline in [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) — an agent must be able to decide to apply the solution from that sentence alone, without reading the rest of the skill.
  - Violation: a `triggers:` list of loose keywords with no `whenToUse` sentence.
  - Risk: the agent cannot decide whether the solution applies to the current task and either skips it or applies it blindly.
  - Fix: write one `whenToUse` sentence that names the concrete tasks or situations; do not rely on a keyword list.
- Remove all `hint`, `example` and `code example` blocks, and the `# How Apply this template` block, from the final skill file.
  - Risk: the final skill is noisy and the agent cannot tell binding rules from authoring aids.
  - Fix: delete every `hint`, `example`, and `code example` fenced block and the `# How Apply this template` section before committing.
- Create an `Implementation/` folder and provide concrete implementation files for every solution skill, including classification, decision, policy, or taxonomy skills. Even when the skill primarily selects between existing solutions, it must still show how each selected variant manifests in code, configuration, or project structure.
  - Risk: the agent gets rules with no concrete shape and invents its own inconsistent implementation.
  - Fix: add one implementation file per created/extended element under `Implementation/`, following the template's naming rules.
- When the skill depends on other solutions, state in each implementation variant or section which dependency solution(s) are applied and which are intentionally not applied.
  - Risk: the agent cannot tell whether a missing piece is an oversight or a deliberate exclusion and may apply conflicting solutions together.
  - Fix: name the applied dependency solutions explicitly in the implementation file; name the intentionally skipped ones as well.
- Record every architecture decision made while building or editing the solution as an ADR following [adr-create.skill.md](skills/common-workflow/architecture/design/adr-create.skill/adr-create.skill.md).
  - Risk: the rejected alternatives and trade-offs are lost, and the same decision gets re-argued the next time someone edits the solution.
  - Fix: create the ADR immediately, register it in the solution's `adr:` YAML property, and link it from the skill body.
- Document every term, library, technology, or pattern a reader may not already know inside the solution skill's own `glossary/` folder (`solution-{Solution}.skill/glossary/`) following [documentation-for-concept.skill.md](skills/common-workflow/documentation/documentation-for-concept.skill/documentation-for-concept.skill.md), and link to it from the solution skill file instead of leaving it unexplained. Keep the glossary inside the skill folder so the solution stays self-contained and portable.
  - Risk: the agent guesses the meaning of an unfamiliar term and applies the solution wrongly.
  - Fix: add a concept page per unfamiliar term under `glossary/` and link to it where the term is used.
- Tag the solution skill and its supporting files with the facet tags required by [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md): the solution file carries `solution/{solution-name}` (the solution name without the `solution-` prefix); every file under `Implementation/` carries `solution/{solution-name}` and `element/{element-name}` (kebab-case element name, no braces or dots); every ADR carries `solution/{solution-name}`, `concern/documentation`, and `concern/documentation/adr`.
  - Risk: tag-expression queries cannot resolve which files belong to the solution, so the files are invisible to agents building a solution-scoped skillset.
  - Fix: fill the `tags:` blocks as shown in the templates when creating each file.
- Never change other skills except the one you are building without explicit instruction in the template.
  - Risk: unrelated skills drift out of sync with their own templates and the change escapes review scoped to this solution.
  - Fix: limit edits to the new solution skill folder; propose changes to other skills separately.
- Never leave the `Implementation/` folder empty or claim "no direct mutations" unless the solution truly produces no code, configuration, or project changes.
  - Risk: the skill looks applicable but gives the agent nothing concrete to execute.
  - Fix: either add the implementation files or state explicitly why the solution produces no artifacts.
- Never add a separate `triggers` list next to `whenToUse`.
  - Risk: trigger conditions split across two fields drift apart, and the agent does not know which one is authoritative.
  - Fix: put every trigger condition into `whenToUse` itself instead of splitting it across two fields.

# Check list
- [ ] The template folder matches the target stack.
- [ ] `whenToUse` is one concrete sentence; there is no separate `triggers` list.
- [ ] No `hint`, `example`, `code example` blocks and no `# How Apply this template` section remain in the final skill.
- [ ] `Implementation/` contains concrete files for every created/extended element.
- [ ] Applied and intentionally skipped dependency solutions are stated where relevant.
- [ ] Architecture decisions are recorded as ADRs and registered in the `adr:` property.
- [ ] Unfamiliar terms are documented in the solution's own `glossary/` folder.
- [ ] Facet tags are filled: `solution/{solution-name}` on the solution file, `solution/{solution-name}` + `element/{element-name}` on Implementation files, `solution/{solution-name}` + `concern/documentation` + `concern/documentation/adr` on ADRs.
