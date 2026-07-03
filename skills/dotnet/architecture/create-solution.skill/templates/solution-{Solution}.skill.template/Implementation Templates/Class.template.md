---
description: Short description what must be made while creation or change in class
project_name: # The project in which the class is located
name: # Class name
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new class template. Name of the class must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing class template. Link to the class must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this class, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS class goal.
MUST:
- show all added Goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Prevent duplicate creation via Guid uniqueness check
```

# Core Principles
```hint
Define how solution EXTENDS class core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- Entities define consistency.
```

# Naming convention
```hint
Class naming convention. Fill table:
- use case - when apply naming convention
- class name pattern - mask of class name. Example: Is{Rule}
- class name - example of class name. Example: IsEven
- file name pattern - file name pattern. Example: Is{Rule}.cs
- file name - example of file name. Example: IsEven.cs
```

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
|          |                    |            |                   |           |

# Implementation changes
```hint
Define how solution EXTENDS class implementation.
```
```example
[[Class skill]] must ...
```
```code example
public class SomeEntity : IGuidEntity
{
    public int Id { get; internal set; }
    public Guid Guid { get; internal set; }
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
Describe concrete wrong ways to implement this class and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Use public setters for every property**
  - Consequence: invariants can be violated by any caller
  - Instead: expose domain methods that enforce rules and use `internal set` or private setters
```

# Check list
```hint
Define how solution EXTENDS class check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] 201 response uses `CreatedAtAction`
```

# Unittest TestCases
```hint
Define how solution EXTENDS class unit tests.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN call command with event THEN
  - [ ] event fill domain event in entity
  - [ ] `DomainEventInterceptor` catch `SaveChanges` and add event to `outbox`
  - [ ] `OutboxDispatcher` read `outbox` and send `Notification`
```
