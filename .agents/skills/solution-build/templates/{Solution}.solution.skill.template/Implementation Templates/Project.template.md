---
description: Short description what must be made while creation or change in project
name: #Project name
element_kind: #repository | project | class
change_kind: #create | extend
#- create if solution create new template of project. Name of project must be added into creates property in the header of solution
#- extend if solution extend existent template of project. Link to project must be added into extends property in the header of solution
---
# How Apply this template
- Replace all ```hint```, ```example``` and ```code example``` blocks with real content. Do not keep them in the final skill file.
- header property `depends_on` couldn't have links to solution with order is greater or equal order in this solution. If it happend ask user to solve this problem.

# Goals
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

# Core Principals
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

# Structure
## Project Structure
```hint
Define how solution EXTENDS project structure
```
```example
/ProjectName
	/DirectoryName
		ClassesInDirectory.cs
```

## Directory and class skills
```hint
Define how solution EXTENDS project directory and files
```
```example
| Directory \| file   | Description           |
| ------------------- | --------------------- |
| /DirectoryName      | Directory description |
| ClassInDirectory.cs | Class description     |
```

| Directory \| file | Description |
| ----------------- | ----------- |
|                   |             |

# NuGet Packages 
```hint
Define how solution EXTENDS project nuget dependencies
```
```example
| Package   | Version constraint | Purpose                |   
| --------- | ------------------ | ---------------------- | 
| Ardalis   | >= 8.0             | SpecificationEvaluator |
```

| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
|         |                    |         |

# What Does NOT Belong Here
```hint
Define how solution EXTENDS project components which doesnot belong to it
RECOMENDATION:
- Prefer bullet list
```
```example
- Commands - belong to [[Other csproj skill]]
```

# Allowed Dependencies
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

# Rules
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

# Anti-patterns
```hint
What mean that solution applyed wrong.
```
```example
- Domain service duplicates invariant already enforced in entity setter or method
```

# Check list
```hint
Define how solution EXTENDS project check list
RECOMENDATION:
- Prefer checkbox list
```
```example
- [ ] `int Id` with `internal set` present
```

