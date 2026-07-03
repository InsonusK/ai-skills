---
description: Short description what must be made while creation or change in class
project_name: #The project in which the class is located
name: #Class name
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
Define how solution EXTENDS class goal
MUST:
- show all added Goals
RECOMENDATION:
- Prefer bullet list
```
```example
- Prevent duplicate creation via Guid uniqueness check 
```

# Core Principles
```hint
Define how solution EXTENDS class core principles
MUST:
- show all added Core Principles
RECOMENDATION:
- Prefer bullet list
```
```example
- Entities define consistency.
```

# Naming convention
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

# Implementation changes
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

# Rule changes
```hint
Define how solution EXTENDS class MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```
## MUST
```example
- Command must realize ICommand<Result<DTO>>
```
## SHOULD
```example
- ...
```
## MAY
```example
- ...
```
## SHOULD NOT
```example
- ...
```

## MUST NOT
```example
- ...
```

# Anti-patterns
```hint
Define how solution EXTENDS class anti-patterns
```
```example
- Duplicating this mapping in every external-created controller action
```

# Check list
```hint
Define how solution EXTENDS class check list
RECOMENDATION:
- Prefer checkbox list
```
```example
- [ ] 201 response uses `CreatedAtAction`
```

# Unittest TestCases
```hint
Define how solution EXTENDS class unittests
RECOMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN call command with event THEN
	- [ ] event fill domain event in entity
	- [ ] `DomainEventInterceptor` catch `SaveChanges` and add event to `outbox`
	- [ ] `OutboxDispatcher` read `outbox` and send `Notification`
```
