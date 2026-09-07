---
name: plateau-{plateau-name}--csproj-{name}
description: Describe which plateau csproj does skill describe
whenToUse: One concrete sentence — which task must make the agent open this skill
  # MUST name a concrete situation: creating or editing a file inside this project, deciding whether new code belongs here, or checking its allowed dependencies/NuGet packages. MUST NOT be vague ("when relevant").
  # Example: "when adding or editing a class in {ProjectName}, or deciding whether a new class belongs in this project"
domain: skill
type: template
plateau:
version:
tags:
  - skill/template/csproj
created_by:
---
# How Apply this template
- Fill `whenToUse` with the concrete project-level situations that require this skill (adding/editing a class inside `{ProjectName}`, deciding whether new code belongs here, checking allowed dependencies). See [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) for the baseline rules.
- Find in all solutions from created_by files made by Project.template.md
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
- add to header properties `tags` tag `plateau/{plateau-name}`

# Goal
```hint
Define List of Goals that are pursued by the creation of this skill. Summarize all Goals from all finded Project.template.md.

At the end of block writes list to all used templates to build block.

MUST:
- If Goals conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Project.template.md link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- store Entity, ValueObject
- encapsulate domain logic

__Applied solutions:__
- [[Solution link]] - [[Project.template.md link]]
```

# Core Principles
```hint
Define List of Core Principles that are pursued by the creation of this skill. Summarize all Core Principles from all finded Project.template.md.

At the end of block writes list to all used templates to build block.

MUST:
- If Core Principles conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Project.template.md link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Rules define business predicates
- Entities define consistency.

__Applied solutions:__
- [[Solution link]] - [[Project.template.md link]]
```

# Structure
## Solution place
`Where defined place in solution`
```
Where does it store in solution
/src
	/ProjectName
```
## Project Structure
```hint
Define project structure. Summarize all project structure from applied `{ProjectName}.csproj.created` or `{ProjectName}.csproj.extended` files. If find project structure in other files also applyed here.

At the end of block writes list to all used files to build block.

MUST:
- Keep only project-level content here. Do not include repository-level or class-level details.
- If solution conflicted to each other as user to solve the problem
- For project structure block:
  - use link to files which define class
- For Applied solutions block:
	- Each bullet must be `<solution skill link> - <Project.template.md link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
```
```example
- /ProjectName
	- /DirectoryName
		- [ClassesInDirectory.cs](./classes/classes-in-directory.skill.md)
	ProjectName.csproj
```
```example
__Applied solutions:__
- [[Solution link]] - [[Project.template.md link]]
```

## Directory and class skills
```hint
Define project Table with directory and files which implement in project. Summarize all "Table with directory and files which implement in project" from all finded Project.template.md.

At the end of block writes list to all used templates to build block.

MUST:
- If Directory and class skills conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Project.template.md link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
| `Directory|file`  | Description                                    | Pattern skill          |
| ------------------- | ---------------------------------------------- | ---------------------- |
| /DirectoryName      | Directory description                          | [[link to folder pattern]] |
| ClassInDirectory.cs | Description of class inside of directory above | [[link to file patter]]    |

__Applied solutions:__
- [[Solution link]] - [[Project.template.md link]]
```

| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
|                  |             |               |

## NuGet Packages 
```hint
Define project Table with project nuget dependencies. Summarize all "Table with project nuget dependencies" from all finded Project.template.md.

At the end of block writes list to all used templates to build block.

MUST:
- If Directory and class skills conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Project.template.md link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class-level details.

RECOMENDATION:
- Prefer pure copy with out changing
```
```example
| Package   | Version constraint | Purpose                |   
| --------- | ------------------ | ---------------------- | 
| Ardalis   | >= 8.0             | SpecificationEvaluator |

__Applied solutions:__
- [[Solution link]] - [[Project.template.md link]]
```

| Package                                   | Version constraint | Purpose                               |
| ----------------------------------------- | ------------------ | ------------------------------------- |
|                                           |                    |                                       |

## What Does NOT Belong Here
```hint
Define components which doesnot belong to this project . Summarize all "components which doesnot belong to this project " from all finded Project.template.md.

At the end of block writes list to all used templates to build block.

MUST:
- If "What Does NOT Belong Here" conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Project.template.md link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Command Handler - belong to [[Other csproj skill]]
- Commands - belong to [[Other csproj skill]]

__Applied solutions:__
- [[Solution link]] - [[Project.template.md link]]
```

## Allowed Dependencies
```hint
Define Allowed dependencies that project may have . Summarize all "Allowed dependencies that project may have" from all finded Project.template.md.

At the end of block writes list to all used templates to build block.

MUST:
- If "Allowed dependencies" conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Project.template.md link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- [[Shared]]
- [[BuildingBlocks]]

__Applied solutions:__
- [[Solution link]] - [[Project.template.md link]]
```

# Rules
```hint
Define MUST, SHOULD, MAY rules only — never `## MUST NOT`/`## SHOULD NOT` headings and never a separate `# Anti-patterns` section (see [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md)). Summarize all "Rules"/"Anti-patterns" from all finded Project.template.md, phrasing every prohibition as a negatively-worded bullet ("Never...") inside `MUST`/`SHOULD` at whichever strength it carries, and fold any anti-pattern's worked "wrong way" example into the same bullet instead of keeping a separate section.

At the end of block writes list to all used templates to build block. 

MUST:
- If Rules conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Project.template.md link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
MUST:
	- ...
	- Never ... (a prohibition, phrased positively as "never X", not as a separate MUST NOT heading)
SHOULD:
	- ...
	- Never ... (a softer prohibition, phrased positively inside SHOULD)
	  
__Applied solutions:__
- [[Solution link]] - [[Project.template.md link]]
```
MUST:
- Never let an `extended_by` solution modify Allowed Dependencies without explicit user confirmation

# Check list
```hint
Define what must be true before this template is considered correctly applied?. Summarize all "Check list" from all finded Project.template.md.

At the end of block writes list to all used templates to build block. 

MUST:
- If "Check list" conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <Project.template.md link>` (see plateau-create-by-solutions.skill.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class-level details.

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present

__Applied solutions:__
- [[Solution link]] - [[Project.template.md link]]
```
