---
uid:
order: 
name: skill-name
description: Describe what skill define
domain: skill
type: architecture
version: 20260610
tags:
  - skill/architecture/solution
  # any other tags
triggers:
  #What kind of task should agent do to use this solution  
  #- when skill should called
creates:
	#List of classes or project which created by this solution
  #They will have this solution as their created_by
  #Example:
  #- "[[Link]]"
extends:
  #List of classes or project which extended or effected by this solution
  #They will have this solution in their extended_by
  #Example:
  #- "[[Link]]"
depends_on:
  #List of other architecture solutions which is used by this solution
  #Example:
  #- "[[Link]]"
---
**AUTHORING RULE**: 
- Replace all ```hint```, ```example``` and ```code example``` blocks with real content. Do not keep them in the final skill file.
- header property `depends_on` couldn't have links to solution with order is greater or equal order in this solution. If it happend ask user to solve this problem.
# Goal
```hint
List of goals that are pursued by the creation of this solution.
RECOMENDATION:
- Prefer bullet list
```
```example
- Define the system-level architecture for domain events — how events are raised, persisted, and dispatched across the application
```

# Core Principals
```hint
Core principalse that a solution should follow
RECOMENDATION:
- Prefer bullet list
```
```example
- Rules define business predicates
- Entities define consistency.
```

# Requirements
```hint
List of requirements for solution appling
MUST:
- couldn't have link to solution with order number greater that order number of this solution. If it happend ask user to solve this problem.
RECOMENDATION:
- Prefer bullet list
```
```example
- definition of `interface IHasGuid` - solution [[external guid pattern solution]] define IHasGuid interface
```

# Template Skill Mutations

## App Repository (.sln)

### Structure

#### Project Structure
```hint
Define how solution EXTENDS repository structure
```
```example
/src
	/App
		/App.Host 
```

#### Directory and class skills
```hint
Define how solution EXTENDS repository directory and files
```
```example
| Directory \| file | Description        | Pattern skill    |
| ----------------- | ------------------ | ---------------- |
| /src/App          | project desciption | [[pattern link]] |
```

| Directory \| file | Description | Pattern skill |
| ----------------- | ----------- | ------------- |
|                   |             |               |

### Rules
```hint
Define how solution EXTENDS repository MUST, SHOULD, SHOULD NOT, MUST NOT rules
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
```

## {Project name} (.csproj) 
```hint
Add this block for each created or extended project in repository.
After each "{Project name} (.csproj)" add suffix
- (created) if solution create new template of project. Name of project must be added into creates property in the header of solution
- (extended) if solution extend existent template of project. Link to project must be added into extends property in the header of solution
```
```example
## {Project name} (.csproj) (created)
...
## {Project name} (.csproj) (extended)
```

### Project extension
#### Goals
```hint
Define how solution EXTENDS project goal.
MUST:
- show all added goals
RECOMENDATION:
- Prefer bullet list
```
```example
- encapsulate domain logic
```

#### Core Principals
```hint
Define how solution EXTENDS project core principals
MUST:
- show all added Core Principals
RECOMENDATION:
- Prefer bullet list
```
```example
- Entities define consistency.
```

#### Structure
##### Project Structure
```hint
Define how solution EXTENDS project structure
```
```example
/ProjectName
	/DirectoryName
		ClassesInDirectory.cs
```

##### Directory and class skills
```hint
Define how solution EXTENDS project directory and files
```
```example
| Directory \| file   | Description           | Pattern skill       |
| ------------------- | --------------------- | ------------------- |
| /DirectoryName      | Directory description | [[link to pattern]] |
| ClassInDirectory.cs | Class description     | [[link to pattern]] |
```

| Directory \| file | Description | Pattern skill |
| ----------------- | ----------- | ------------- |
|                   |             |               |

#### NuGet Packages 
```hint
Define how solution EXTENDS project nuget dependencies
```
```example
| Package   | Version constraint | Purpose                |   
| --------- | ------------------ | ---------------------- | 
| Ardalis   | >= 8.0             | SpecificationEvaluator |
```

| Package                                   | Version constraint | Purpose                               |
| ----------------------------------------- | ------------------ | ------------------------------------- |
|                                           |                    |                                       |

#### What Does NOT Belong Here
```hint
Define how solution EXTENDS project components which doesnot belong to it
RECOMENDATION:
- Prefer bullet list
```
```example
- Commands - belong to [[Other csproj skill]]
```

#### Allowed Dependencies
```hint
Define how solution EXTENDS allowed dependencies that project may have
RECOMENDATION:
- Prefer bullet list
ATTENTION:
- Solution should not change allowed dependencies. Confirm extension from user before add.
```
```example
- [[Shared]]
```

#### Rules
```hint
Define how solution EXTENDS project MUST, SHOULD, SHOULD NOT, MUST NOT rules
MUST:
- show all added Rules
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
```

#### Anti-patterns
```hint
What mean that solution applyed wrong.
```
```example
- Domain service duplicates invariant already enforced in entity setter or method
```

#### Check list
```hint
Define how solution EXTENDS project check list
RECOMENDATION:
- Prefer checkbox list
```
```example
- [ ] `int Id` with `internal set` present
```

### Class extension
#### {Class name} 
```hint
Add this block for each created or extended class in project.
After each "{Class name}" add suffix 
- (created) if solution create new template of class. Name of class must be added into creates property in the header of solution
- (extended) if solution extend existent template of class. Link of class must be added into extends property in the header of solution
```
```example
## {Class name 1} (created)
...
## {Class name 2} (extended)
```
##### Goals
```hint
Define how solution EXTENDS class goal
MUST:
- show all added Goals
RECOMENDATION:
- Prefer bullet list
```
```example
- Prevent duplicate creation via Guid uniqueness check 
```

##### Core Principals
```hint
Define how solution EXTENDS class core principals
MUST:
- show all added Core Principals
RECOMENDATION:
- Prefer bullet list
```
```example
- Entities define consistency.
```
##### Naming convention
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

##### Implementation changes
```hint
Define how solution EXTENDS class implementation
```
```example
[[Class skill]] must ...
```
```code example
public class SomeEntity: IGuidEntity{
	public int Id {get; internal set;}
	public Guid Guid {get; internal set;}
}
```

##### Rule changes
```hint
Define how solution EXTENDS class rules
MUST:
- show all added Rules
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
```

# Rules
```hint
define MUST, SHOULD, SHOULD NOT, MUST NOT rules
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
```

# Anti-patterns
```hint
What mean that soltion applyed wrong. 
RECOMENDATION:
- Prefer bullet list
```
```example
- Domain service duplicates invariant already enforced in entity setter or method
```

# Check list
```hint
what must be true before this solution is considered correctly applied?
RECOMENDATION:
- Prefer checkbox list
```
```example
- [ ] `int Id` with `internal set` present in Entity
```

# Unittest TestCases
```hint
list of unittests which must be created to test solution. 
RECOMENDATION:
- Prefer checkbox list
- Prefer integration tests
```
```example
- [ ] WHEN call command with event THEN
	- [ ] event fill domain event in entity
	- [ ] `DomainEventInterceptor` catch `SaveChanges` and add event to `outbox`
	- [ ] `OutboxDispatcher` read `outbox` and send `Notification`
```
