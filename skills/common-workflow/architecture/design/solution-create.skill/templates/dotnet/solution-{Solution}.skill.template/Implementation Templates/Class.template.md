---
description: Short description what must be made while creation or change in class
project_name: # The project in which the class is located
name: # Class name
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new class template. Name of the class must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing class template. Link to the class must be added into the `extends` property in the header of the solution.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the class name in kebab-case, no braces or dots
  # (e.g. ValidationBehavior.cs -> element/validationbehavior-cs, {Entity}.cs -> element/entity-cs).
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
Define how solution EXTENDS class rules. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
- Use only ## MUST, ## SHOULD, ## MAY subblocks — never ## MUST NOT/## SHOULD NOT headings.
- Express a prohibition as a negatively-phrased bullet ("Never ...", "Do not ...") inside ## MUST or ## SHOULD, at whichever strength it actually carries.
- Never add a separate # Anti-patterns section: convert each would-be anti-pattern into a negative bullet with nested `Risk:` (the consequence) and `Fix:` (the correct alternative).
- Every ## MUST bullet carries a nested `Risk:` and `Fix:` (`Violation:` is optional); ## SHOULD bullets carry the elaboration only when the rule is non-obvious; ## MAY bullets never carry it.
- Only add a subblock for categories where this solution introduces new rules.
- If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- Command must realize ICommand<Result<DTO>>
  - Risk: the mediator pipeline cannot infer the result type, so validation and result mapping do not apply.
  - Fix: implement ICommand<Result<DTO>> on the command record.
- Never use public setters for every property.
  - Risk: invariants can be violated by any caller.
  - Fix: expose domain methods that enforce rules and use `internal set` or private setters.
```

## SHOULD
```example
- ...
```

## MAY
```example
- ...
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
