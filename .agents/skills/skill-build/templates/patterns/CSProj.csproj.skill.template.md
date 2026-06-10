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
# Who Apply this template
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.

# Goal
```hint
List of Goals that are pursued by the creation of this skill.
Formed as summary of all Goals from the solutions which create or extend this project

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this project extension. 
  It will by on path: Implementation.{Project name} (.csproj).Project extension
- Under class extension header take subheader Goals
- Summarize all finded Goals

MUST:
- If Goals conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Goals. Link must follow to header "Implementation.{Project name} (.csproj)" in solution
```
```example
- store Entity, ValueObject
- encapsulate domain logic

__Applied solutions:__
- [[solution from created by#header where define goals for this project]]
- [[solution from extended by#header where define goals for this project]] 
```

# Core Principals
```hint
List of Core Principals that are pursued by the creation of this skill.
Formed as summary of all Core Principals from the solutions which create or extend this project

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this project extension. 
  It will by on path: Implementation.{Project name} (.csproj).Project extension
- Under class extension header take subheader Core Principals
- Summarize all finded Core Principals

MUST:
- If Core Principals conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Core Principals. Link must follow to header "Implementation.{Project name} (.csproj)" in solution
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

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this project extension. 
  It will by on path: Implementation.{Project name} (.csproj).Project extension
- Under class extension header take subheader Project Structure
- Summarize all finded Project Structure

MUST:
- If Project Structure conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Project Structure. Link must follow to header "Implementation.{Project name} (.csproj)" in solution
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

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this project extension. 
  It will by on path: Implementation.{Project name} (.csproj).Project extension
- Under class extension header take subheader Directory and class skills
- Summarize all finded Directory and class skills

MUST:
- If Directory and class skills conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Directory and class skills. Link must follow to header "Implementation.{Project name} (.csproj)" in solution
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

## NuGet Packages 
```hint
Table with project nuget dependencies
Formed as summary of all nuget dependencies which was defined in solutions from created_by and extented_by

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this project extension. 
  It will by on path: Implementation.{Project name} (.csproj).Project extension
- Under class extension header take subheader NuGet Packages
- Summarize all finded NuGet Packages

MUST:
- If Directory and class skills conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this NuGet Packages. Link must follow to header "Implementation.{Project name} (.csproj)" in solution
```
```example
| Package   | Version constraint | Purpose                |   
| --------- | ------------------ | ---------------------- | 
| Ardalis   | >= 8.0             | SpecificationEvaluator |
```

| Package                                   | Version constraint | Purpose                               |
| ----------------------------------------- | ------------------ | ------------------------------------- |
|                                           |                    |                                       |

## What Does NOT Belong Here
```hint
components which doesnot belong to this project 
Formed as summary of all "What Does NOT Belong Here" from the solutions which create or extend this project

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this project extension. 
  It will by on path: Implementation.{Project name} (.csproj).Project extension
- Under class extension header take subheader "What Does NOT Belong Here"
- Summarize all finded "What Does NOT Belong Here"

MUST:
- If "What Does NOT Belong Here" conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this "What Does NOT Belong Here". Link must follow to header "Implementation.{Project name} (.csproj)" in solution
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
Formed as summary of all "Allowed dependencies" from the solutions which create or extend this project

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this project extension. 
  It will by on path: Implementation.{Project name} (.csproj).Project extension
- Under class extension header take subheader "Allowed dependencies"
- Summarize all finded "Allowed dependencies"

MUST:
- If "Allowed dependencies" conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this "Allowed dependencies". Link must follow to header "Implementation.{Project name} (.csproj)" in solution
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
Formed as summary of all Rules from the solutions which create or extend this project

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this project extension. 
  It will by on path: Implementation.{Project name} (.csproj).Project extension
- Under class extension header take subheader Rules
- Summarize all finded Rules

MUST:
- If Rules conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Rules. Link must follow to header "Implementation.{Project name} (.csproj)" in solution
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
Allowed dependencies that project may have
Formed as summary of all "Anti-patterns" from the solutions which create or extend this project

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this project extension. 
  It will by on path: Implementation.{Project name} (.csproj).Project extension
- Under class extension header take subheader "Anti-patterns"
- Summarize all finded "Anti-patterns"

MUST:
- If "Anti-patterns" conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this "Anti-patterns". Link must follow to header "Implementation.{Project name} (.csproj)" in solution
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
Formed as summary of all "Check list" from the solutions which create or extend this project

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this project extension. 
  It will by on path: Implementation.{Project name} (.csproj).Project extension
- Under class extension header take subheader "Check list"
- Summarize all finded "Check list"

MUST:
- If "Check list" conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this "Check list". Link must follow to header "Implementation.{Project name} (.csproj)" in solution
```
```example
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present

__Applied solutions:__
- [[solution link from created_by#header with check list for this project]]
- [[solution link from extended_by#header with check list for of this project]]
```
