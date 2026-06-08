---
uid:
name: skill-name
description: Describe what skill define
domain: skill
type: template
version: 20260610
tags:
  - skill/template/csproj
  #- tag for skill classification
triggers:
  #What kind of task should agent do to use this project  
  #- create {Project}
  #- implement {Project}
created_by: #solution - the reason the project exists
extended_by:
  #List of solution which extend this project.
  #Example:
  #- "[[link]]"
---
**AUTHORING RULE**: Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
# Goal
```hint
List of goals that are pursued by the creation of this project.
Formed as summary of all goals from the solutions which create or extend this project
RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

and then Applied solutions with links to all solutions which effect to this goals
```
```example
- store Entity, ValueObject
- encapsulate domain logic

__Applied solutions:__
- [[solution from created by#header where define goals for this project]]
- [[solution from extended by#header where define goals for this project]] 
```

# Core Principles
```hint
Core principalse that a project should follow
Formed as summary of all principals from the solutions which create or extend this project
RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
  
and then Applied solutions with links to all solutions which effect to this core principasl
```
```example
- Rules define business predicates
- Entities define consistency.

__Applied solutions:__
- [[solution from created by#header where define core principal for this project]]
- [[solution from extended by#header where define core principal for this project]]
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
What is project structure
Formed as summary of all directory and files which was defined in solutions from created_by and extented_by

and then Applied solutions with links to all solutions which effect to project
```
```example
/ProjectName
	/DirectoryName
		ClassesInDirectory.cs
	ProjectName.csproj
```
```example
__Applied solutions:__
- [[solution from created by#header where define structure for this project]]
- [[solution from extended by#header where define structure for this project]]
```

## Directory and class skills
```hint
Table with directory and files which implement in project
Formed as summary of all directory and files which was defined in solutions from created_by and extented_by
  
and then Applied solutions with links to all solutions which effect to project
```
```example
| `Directory|file`  | Description                                    | Pattern skill          |
| ------------------- | ---------------------------------------------- | ---------------------- |
| /DirectoryName      | Directory description                          | [[link to folder pattern]] |
| ClassInDirectory.cs | Description of class inside of directory above | [[link to file patter]]    |

__Applied solutions:__
- [[solution from created by#header where define dir and files for this project]]
- [[solution from extended by#header where define dir and files for this project]]
```

| `Directory|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
|                  |             |               |

## What Does NOT Belong Here
```hint
components which doesnot bolong to this project 
Formed as summary of 'does not belog list' from the solutions which create or extend this project
RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
  
and then Applied solutions with links to all solutions which effect to this core principasl
```
```example
- Command Handler - belong to [[Other csproj skill]]
- Commands - belong to [[Other csproj skill]]

__Applied solutions:__
- [[solution from created by#header where define core principal for this project]]
- [[solution from extended by#header where define core principal for this project]]
```

## Allowed Dependencies
```hint
Allowed dependencies that project may have
Formed as summary of allowed dependency from the solutions which create or extend this project
RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
ATTENTION:
- It should not be changed by extended_by solution. Confirm extension from user  
  
and then Applied solutions with links to all solutions which effect to this core principasl
```
```example
- [[Shared]]
- [[BuildingBlocks]]

__Applied solutions:__
- [[solution from created by#header where define core principal for this project]]
- [[solution from extended by#header where define core principal for this project]]
```

# Rules
```hint
define MUST, SHOULD, SHOULD NOT, MUST NOT rules
Formed as summary of all rules for this project from the solutions which create or extend this project
RECOMENDATION: 
- prefer pure copy from solution

and then Applied solutions with links to all applied solutions where get rules
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
- [[solution link from created_by#header with rules for this project]]
- [[solution link from extended_by#header with rules for of this project]]
```
MUST NOT:
- extended_by solution modify Allowed Dependencies without explicit user confirmation

# Anti-patterns
```hint
What mean that skill applyed wrong. 
Formed as summary of all anti-patterns for this project from the solutions which create or extend this project
RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

and then Applied solutions with links to all applied solutions where get anti patterns marks
```
```example
- Domain service takes `IRepository` parameter — application layer loads, domain decides
- Domain service duplicates invariant already enforced in entity setter or method

__Applied solutions:__
- [[solution link from created_by#header with anti-patterns for this project]]
- [[solution link from extended_by#header with anti-patterns for of this project]]
```

# Check list
```hint
what must be true before this template is considered correctly applied?
Formed as summary of all check lists for this project from the solutions which create or extend this project
RECOMENDATION:
- Prefer checkbox list
- Prefer pure copy with out changing

and then Applied solutions with links to all applied solutions from where we get check list points
```
```example
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present

__Applied solutions:__
- [[solution link from created_by#header with check list for this project]]
- [[solution link from extended_by#header with check list for of this project]]
```
