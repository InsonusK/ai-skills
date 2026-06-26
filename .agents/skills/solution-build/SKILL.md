---
name: solution-build
description: Define how create new solution by patterns
whenToUse: when you write a solution skill
---

# How to build a skill
1. Define type of skill and use template acourding to the type. 
  - All solution skill templates you can find in [templates](./templates) folder.
  - Tempaletes name has pattern {type}-*.{type}.skill.template.md. Example: class-Class.skill.template.md
  - If you don't find template for your skill type, Ask user what should you do.
2. Fill skill template with real content.
3. Follow authoring rules which are defined in each section of template. 
  - Authoring rules are defined in ```hint``` blocks. 
  - Authoring examples are provided in ```example``` blocks.
  - After filling template with real content remove all ```hint``` and ```example``` blocks from final skill file. Do not keep them in the final skill file.

# Rules
MUST:
- Follow "# How Apply this template" rules defined in template.
- Remove all ```hint``` and ```example``` blocks from final skill file. Do not keep them in the final skill file. Remove block "# How Apply this template"
- Create an `Implementation/` folder and provide concrete implementation files for every solution skill, including classification, decision, policy, or taxonomy skills. Even when the skill primarily selects between existing solutions, it must still show how each selected variant manifests in code, configuration, or project structure.
- When the skill depends on other solutions, each implementation variant or section must explicitly state which dependency solution(s) are applied and which are intentionally not applied.
MUST NOT:
- Change other skills except the one you are building without explicit instruction in the template.
- Leave the `Implementation/` folder empty or claim "no direct mutations" unless the solution truly produces no code, configuration, or project changes.