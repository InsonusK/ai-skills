---
name: plateau-{plateau-name}--class-{name}
description: Describe which class skill define
whenToUse: One concrete sentence — which task must make the agent open this skill
  # MUST name a concrete situation: creating or editing this exact class, or creating a new class that plays the same role. MUST NOT be vague ("when relevant").
  # Example: "when creating or editing {ClassName}, or creating another class that plays the same role in a different module"
domain: skill
type: template
plateau:
version:
tags:
  - skill/template/class
created_by:
---
# How Apply this template
- Fill `whenToUse` with the concrete class-level situations that require this skill (creating/editing this class, or creating another class with the same role). See [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) for the baseline rules.
- Find in all solutions from `created_by` files made by Class.template.md
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
- add to header properties `tags` tag `plateau/{plateau-name}`

# Goal
```hint
Define List of Goals that are pursued by the creation of this skill. Summarize all Goals from all finded Class.template.md.

At the end of block writes list to all used templates to build block.

MUST:
- If Goals conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Class.template.md link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class-level content here. Do not include repository-level or project-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Goals. Link must follow to header "Implementation.{Project name} (.csproj).Class extension.{Class name}" in solution
```
```example
- Enforce transport validation before command handler executes 
- Prevent duplicate creation via Guid uniqueness check 

__Applied solutions:__
- [[Solution link]] - [[Class.template.md link]]
```

# Core Principles
```hint
Define List of Core Principles that are pursued by the creation of this skill. Summarize all Core Principles from all finded Class.template.md.

At the end of block writes list to all used templates to build block.

MUST:
- If Core Principles conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Class.template.md link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class-level content here. Do not include repository-level or project-level details.
- Add Core principle `Apply ONE plateau template per class`

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Apply ONE plateau template per class
- Rules define business predicates
- Entities define consistency.

__Applied solutions:__
- [[Solution link]] - [[Class.template.md link]]
```

# Naming convention
```hint
Define Naming convention. Summarize all "Naming convention" from all finded Class.template.md.

At the end of block writes list to all used templates to build block.

class naming convension. Fill table
- use case - when apply naming convesion
- class name oattern - mask of class name. Example: Is{Rule}
- class name - example of class name. Example: IsEvent
- file name pattern - file name pattern. Example: Is{Rule}.cs
- file name - example of file name. Example: IsEven.cs
```

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
|          |                    |            |                   |           |

# Implementation
```hint
Define Implementaion of class. Summarize all "Implementation changes" from all finded Class.template.md.

At the end of block writes list to all used templates to build block.

MUST:
- Write comment at the top of created class with information from applied skill properties
  - name
  - plateau
  - version

- If Implementation changes conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Class.template.md link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class-level content here. Do not include repository-level or project-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
//Skill: class-i-guid-entity 
//Plateau: default
//Version: 20260628

public class SomeEntity: IGuidEntity{
	public int Id {get; internal set;}
	public Guid Guid {get; internal set;}
}

__Applied solutions:__
- [[Solution link]] - [[Class.template.md link]]
```

# Rules
```hint
Define Rules of class. Summarize all "Rule changes" from all finded Class.template.md.

At the end of block writes list to all used templates to build block.

MUST:
- If Rules conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Class.template.md link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class-level content here. Do not include repository-level or project-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
MUST:
	- ...
SHOULD:
	- ...
SHOULD NOT:
	- ...
MUST NOT:
	- ...
	  
__Applied solutions:__
- [[Solution link]] - [[Class.template.md link]]
```

# Anti-patterns
```hint
Define What mean that skill applyed wrong. Summarize all "Anti-patterns" from all finded Class.template.md.

At the end of block writes list to all used templates to build block. 

MUST:
- If "Anti-patterns" conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Class.template.md link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class-level content here. Do not include repository-level or project-level details.
- Add antipattern `Apply SEVERAL plateau template per class`

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Apply SEVERAL plateau template per class
- Domain service takes `IRepository` parameter — application layer loads, domain decides
- Domain service duplicates invariant already enforced in entity setter or method

__Applied solutions:__
- [[Solution link]] - [[Class.template.md link]]
```

# Check list
```hint
Define what must be true before this template is considered correctly applied?. Summarize all "Check list" from all finded Class.template.md.

At the end of block writes list to all used templates to build block. 

MUST:
- If "Check list" conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Class.template.md link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class-level content here. Do not include repository-level or project-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present

__Applied solutions:__
- [[Solution link]] - [[Class.template.md link]]
```

# Unittest TestCases
```hint
Define list of unittests which must be created to test class. Summarize all "Unittest TestCases" from all finded Class.template.md.

At the end of block writes list to all used templates to build block.

MUST:
- If Check list conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Class.template.md link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class-level content here. Do not include repository-level or project-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present

__Applied solutions:__
- [[Solution link]] - [[Class.template.md link]]
```
