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

### How to store skill
- Small skills, without appended files, must be "Flat skill" - skill does not have own directory it stored in logic folder with name `Skill-name.skill.md`
- Complex skills, skill with appending files, must be "Directory skill" - skill has own directory with name `Skill-name`, directory contain file `Skill-name.skill.md` with skill description.

## Links
- Links to files inside skill (templates, scripts) must be in
	- relative format. Example `[template name](./template/template name.md`
- All links to another skills must be in 
	- Obsidian format. Example `[[skill-name]]`
	- uid link format `[skill_name](skill uid:{{ file property uid }})`
  
## Skill types
Use 3 skill types
- workflow skill - define how do task analisis, which skill use next
- architecture skill - define architecture rules and constraints
- developing skill - how to make concrete developing patterns


# Work steps
1. Define SKILL description.
**Expected output:** filled formatter and goal
2. Confirm with user how you understand skill
**Expected output:** user confirm that you understanding is correct.
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
