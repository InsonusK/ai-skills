---
uid:
name: skill-name
description: Describe what skill define
domain: skill
type: template
version: 20260610
tags:
  - skill/template/class
  #- tag for skill classification
triggers:
  #What kind of task should agent do to use this class
  #- create {ClassName}
  #- write {ClassName}
  #- implememt {ClassName}
created_by: #solution - the reason the class exists
extended_by:
  #List of solution which extend to this class.
  #Example:
  #- "[[link]]"
---
**AUTHORING RULE**: Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
# Goal
```hint
List of Goals that are pursued by the creation of this skill.
Formed as summary of all Goals from the solutions which create or extend this class

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this class extension. 
  It will by on path: Implementation.{Project name} (.csproj).Class extension.{Class name}
- Under class extension header take subheader Goals
- Summarize all finded Goals

MUST:
- If Goals conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Goals. Link must follow to header "Implementation.{Project name} (.csproj).Class extension.{Class name}" in solution
```
```example
- Enforce transport validation before command handler executes 
- Prevent duplicate creation via Guid uniqueness check 

__Applied solutions:__
- [[solution from created by#{Class name}]]
- [[solution from extended by#{Class name}]] 
```

# Core Principals
```hint
List of Core Principals that are pursued by the creation of this skill.
Formed as summary of all Core Principals from the solutions which create or extend this class

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this class extension. 
  It will by on path: Implementation.{Project name} (.csproj).Class extension.{Class name}
- Under class extension header take subheader Core Principal
- Summarize all finded Core Principals

MUST:
- If Core Principals conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Core Principals. Link must follow to header "Implementation.{Project name} (.csproj).Class extension.{Class name}" in solution
```
```example
- Rules define business predicates
- Entities define consistency.

__Applied solutions:__
- [[solution from created by#{Class name}]]
- [[solution from extended by#{Class name}]]
```

# Structure
## Place in csproj
Defined in `link to project skill`
```
/ProjectName
/Folder
		classByPattern.cs
	ProjectNams.csproj
```

## Naming convention
```hint
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

## Implementation
```hint
Implementation after applying all solutions

HOW TO SUMMARIZED IMPLEMENTATION:
- get all soltuins from created_by and extended_by list
- in each solution find header about this class extension. 
  It will by on path: Implementation.{Project name} (.csproj).Class extension.{Class name}
- Under class extension header take subheader Implementation changes
- Summarize all finded Implementation changes

MUST:
- If Implementation changes conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Implementation. Link must follow to header "Implementation.{Project name} (.csproj).Class extension.{Class name}" in solution

```
```example
public class SomeEntity: IGuidEntity{
	public int Id {get; internal set;}
	public Guid Guid {get; internal set;}
}

__Applied solutions:__
- [[solution link from created_by#{Class name}]]
- [[solution link from extended_by#{Class name}]]
```

# Rules
```hint
List of Rules that are pursued by the creation of this skill.
Formed as summary of all Rules from the solutions which create or extend this class

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this class extension. 
  It will by on path: Implementation.{Project name} (.csproj).Class extension.{Class name}
- Under class extension header take subheader Rules
- Summarize all finded Rules

MUST:
- If Rules conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Rules. Link must follow to header "Implementation.{Project name} (.csproj).Class extension.{Class name}" in solution
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
- [[solution link from created_by#header with rules for this class]]
- [[solution link from extended_by#header with rules for of this class]]
```

# Anti-patterns
```hint
What mean that skill applyed wrong. 
Formed as summary of all Anti-patterns from the solutions which create or extend this class

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this class extension. 
  It will by on path: Implementation.{Project name} (.csproj).Class extension.{Class name}
- Under class extension header take subheader Anti-patterns
- Summarize all finded Anti-patterns

MUST:
- If Anti-patterns conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Anti-patterns. Link must follow to header "Implementation.{Project name} (.csproj).Class extension.{Class name}" in solution
```
```example
- Domain service takes `IRepository` parameter — application layer loads, domain decides
- Domain service duplicates invariant already enforced in entity setter or method

__Applied solutions:__
- [[solution link from created_by#header with anti-patterns for this class]]
- [[solution link from extended_by#header with anti-patterns for of this class]]
```

# Check list
```hint
what must be true before this template is considered correctly applied?
Formed as summary of all Check list from the solutions which create or extend this class

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this class extension. 
  It will by on path: Implementation.{Project name} (.csproj).Class extension.{Class name}
- Under class extension header take subheader Check list
- Summarize all finded Check list

MUST:
- If Check list conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Check list. Link must follow to header "Implementation.{Project name} (.csproj).Class extension.{Class name}" in solution
```
```example
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present

__Applied solutions:__
- [[solution link from created_by#header with check list for this class]]
- [[solution link from extended_by#header with check list for of this class]]
```

# Unittest TestCases
```hint
list of unittests which must be created to test class
Formed as summary of all Check list from the solutions which create or extend this class

HOW TO SUMMARIZED LIST:
- get all soltuins from created_by and extended_by list
- in each solution find header about this class extension. 
  It will by on path: Implementation.{Project name} (.csproj).Class extension.{Class name}
- Under class extension header take subheader Check list
- Summarize all finded Check list

MUST:
- If Check list conflicted to each other as user to solve the problem

RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Check list. Link must follow to header "Implementation.{Project name} (.csproj).Class extension.{Class name}" in solution
```
```example
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present

__Applied solutions:__
- [[solution link from created_by#header with testcases list for this class]]
- [[solution link from extended_by#header with testcasess list for of this class]]
```
