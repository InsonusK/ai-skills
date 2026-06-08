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
List of goals that are pursued by the creation of this skill.
Formed as summary of all goals from the solutions which create or extend this class
RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

and then Applied solutions with links to all solutions which effect to this goals
```
```example
- Enforce transport validation before command handler executes 
- Prevent duplicate creation via Guid uniqueness check 

__Applied solutions:__
- [[solution from created by#header where define goals for this class]]
- [[solution from extended by#header where define goals for this class]] 
```

# Core Principles
```hint
Core principalse that a class should follow
Formed as summary of all principals from the solutions which create or extend this class
RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
  
and then Applied solutions with links to all solutions which effect to this core principasl
```
```example
- Rules define business predicates
- Entities define consistency.

__Applied solutions:__
- [[solution from created by#header where define core principal for this class]]
- [[solution from extended by#header where define core principal for this class]]
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
Implementation after applying all solutions (created_by and extended_by)

and then Applied solutions with links to all applied solutions for result implementation
```
```example
public class SomeEntity: IGuidEntity{
	public int Id {get; internal set;}
	public Guid Guid {get; internal set;}
}

__Applied solutions:__
- [[solution link from created_by#header with implementation of this class]]
- [[solution link from extended_by#header with implementation of this class]]
```

# Rules
```hint
define MUST, SHOULD, SHOULD NOT, MUST NOT rules
Formed as summary of all rules for this class from the solutions which create or extend this class
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
- [[solution link from created_by#header with rules for this class]]
- [[solution link from extended_by#header with rules for of this class]]
```

# Anti-patterns
```hint
What mean that skill applyed wrong. 
Formed as summary of all anti-patterns for this class from the solutions which create or extend this class
RECOMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

and then Applied solutions with links to all applied solutions where get anti patterns marks
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
Formed as summary of all check lists for this class from the solutions which create or extend this class
RECOMENDATION:
- Prefer checkbox list
- Prefer pure copy with out changing

and then Applied solutions with links to all applied solutions from where we get check list points
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
Formed as summary of all testcases list for this class from the solutions which create or extend this class
RECOMENDATION:
- Prefer checkbox list
- Prefer pure copy with out changing
  
and then Applied solutions with links to all applied solutions from where we get test case list points
```
```example
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present

__Applied solutions:__
- [[solution link from created_by#header with testcases list for this class]]
- [[solution link from extended_by#header with testcasess list for of this class]]
```
