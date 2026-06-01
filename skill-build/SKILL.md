---
name: skill-build
description: Describe how to write skills
metadata:
  domain: skill
  tags:
    - skill
---

# Goal 
Defaine how agent should build skills

# Input data
## Required
- **Pain/Problem:** What is user try to solve
- **Goal:** What goal does he try to achive

## Optional
- **Context:** Where does skill applyed
- **Constraints:** Does skill has constains
- **Examples:** Do we have examples

# Rules

## Communication
- If you are not sure ask user to define exactly, what user need
- Confirm your understanding before start

## Quality
- Skill must have checklist
- It should have examples

## Format
- Skill must follow [Skill template](./templates/SKILL_template.md)
- All headeres must be filled

# Work steps
1. Define SKILL description.
**Expected output:** filled formatter and goal
2. Confirm with user how you understand skill
**Expected output:** user confirm that you understanding is correct
3. Create new skill using [Skill template](./templates/SKILL_template.md)
**Expected output:** you have create SKILL.md
4. Self validation
**Expected output:** you have passe [check list](#check-list)

# Check list
- [ ] All headers of [Skill template](./templates/SKILL_template.md) is filled
- [ ] Formatter is filled
- [ ] Goal is measurable
- [ ] Work steps has expectedoutput or example of output
- [ ] Examples or expected output contain real code or contant
- [ ] 