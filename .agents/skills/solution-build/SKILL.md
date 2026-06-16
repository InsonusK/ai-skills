---
name: create-solution-build
description: Define how create new solution by patterns
whenToUse: when you write a solution skill
---

# How to build a skill
1. Define type of skill and use template acourding to the type. 
  - All solution skill templates you can find in [templates](./templates) folder.
  - Tempaletes name has pattern *.{type}.skill.template.md. Example: class.skill.template.md
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
MUST NOT:
- Change other skills except the one you are building without explicit instruction in the template.