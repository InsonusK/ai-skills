---
name: solution-create
description: Define how to create new solution skills by patterns
whenToUse: when you write a solution skill
tags:
  - skill/architecture/solution/design
---

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

# Rules
MUST:
- Select the template folder that matches the target language/stack of the solution.
- Follow "# How Apply this template" rules defined in the selected template.
- Write `whenToUse` in the solution skill header as one concrete sentence naming the task types or situations that must make an agent apply this solution (e.g. "when implementing a command/write-operation handler, or adding a new feature to an existing module"). Follow the `whenToUse` baseline in [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) — an agent must be able to decide to apply the solution from that sentence alone, without reading the rest of the skill.
- Remove all `hint`, `example` and `code example` blocks from the final skill file. Do not keep them in the final skill file.
- Remove the `# How Apply this template` block from the final skill file.
- Create an `Implementation/` folder and provide concrete implementation files for every solution skill, including classification, decision, policy, or taxonomy skills. Even when the skill primarily selects between existing solutions, it must still show how each selected variant manifests in code, configuration, or project structure.
- When the skill depends on other solutions, each implementation variant or section must explicitly state which dependency solution(s) are applied and which are intentionally not applied.
- When an ADR is created, the selected variant must also be listed in `# Searched variants` and clearly marked as selected.
- The `# Selected variant` section must explicitly name and link to the selected variant from `# Searched variants`.
- When the solution relies on a term, library, technology, or pattern a reader may not already know, document it inside the solution skill's own `glossary/` folder (`solution-{Solution}.skill/glossary/`) following [documentation-for-concept.skill.md](skills/common-workflow/documentation/documentation-for-concept.skill/documentation-for-concept.skill.md), and link to it from the solution skill file instead of leaving it unexplained. Keep the glossary inside the skill folder so the solution stays self-contained and portable.
MUST NOT:
- Change other skills except the one you are building without explicit instruction in the template.
- Leave the `Implementation/` folder empty or claim "no direct mutations" unless the solution truly produces no code, configuration, or project changes.
- Add a separate `triggers` list next to `whenToUse`. Put every trigger condition into `whenToUse` itself instead of splitting it across two fields.
