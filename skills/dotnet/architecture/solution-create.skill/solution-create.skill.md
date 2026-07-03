---
name: solution-create
description: Define how create new solution by patterns
whenToUse: when you write a solution skill
---

# Workflow
1. Understand 
  - goal of solution
  - capabilities which solution will give to project
  - how capabilities will be achieved
  - requirements of solution, does it depends on other solutions or packages
2. Analyze them and if you have any doubts, ask the user
3. When all doubts will be closed start [build solution](#how-to-build-a-solution)

# How to build a solution
1. Use template from [templates](./templates/solution-{Solution}.skill.template/) folder.
  - Template name has pattern `{type}-*.{type}.skill.template.md`. Example: `class-Class.skill.template.md`.
2. Fill skill template with real content.
3. Follow authoring rules which are defined in each section of template.
  - Authoring rules are defined in `hint` blocks.
  - Authoring examples are provided in `example` and `code example` blocks.
  - After filling template with real content remove all `hint`, `example` and `code example` blocks from final skill file. Do not keep them in the final skill file.
  - Remove the `# How Apply this template` block from the final skill file.

# Rules
MUST:
- Follow "# How Apply this template" rules defined in the template.
- Remove all `hint`, `example` and `code example` blocks from the final skill file. Do not keep them in the final skill file.
- Remove the `# How Apply this template` block from the final skill file.
- Create an `Implementation/` folder and provide concrete implementation files for every solution skill, including classification, decision, policy, or taxonomy skills. Even when the skill primarily selects between existing solutions, it must still show how each selected variant manifests in code, configuration, or project structure.
- When the skill depends on other solutions, each implementation variant or section must explicitly state which dependency solution(s) are applied and which are intentionally not applied.
- When an ADR is created, the selected variant must also be listed in `# Searched variants` and clearly marked as selected.
- The `# Selected variant` section must explicitly name and link to the selected variant from `# Searched variants`.
MUST NOT:
- Change other skills except the one you are building without explicit instruction in the template.
- Leave the `Implementation/` folder empty or claim "no direct mutations" unless the solution truly produces no code, configuration, or project changes.
