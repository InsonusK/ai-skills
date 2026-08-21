---
description: Short description what must be made while creation or change in a file that contains a class
project_name: # The package in which the file is located
name: # Class name
element_kind: class
change_kind: # create | extend
# - create if solution creates a new class template. Name of the class/file must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing class template. Link to the class/file must be added into the `extends` property in the header of the solution.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the class file name in kebab-case, no braces or dots
  # (e.g. is-email-validator.ts -> element/is-email-validator-ts, {class-name}.ts -> element/class-name-ts).
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this class/file, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS class goal.
MUST:
- show all added Goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Prevent duplicate creation via a uniqueness check
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
- Classes encapsulate behavior and state
```

# Naming convention
```hint
Class and file naming convention. Fill table:
- use case - when apply naming convention
- class name pattern - mask of class name. Example: Is{Rule}Validator
- class name - example of class name. Example: IsEmailValidator
- file name pattern - file name pattern. Example: {kebab-name}.ts
- file name - example of file name. Example: is-email-validator.ts
```

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
|          |                    |            |                   |           |

# Implementation changes
```hint
Define how solution EXTENDS class implementation.

When this solution is built on a plateau (`built_on_plateau` is set), structure the change as a delta from the plateau:
- AS IS — copy or summarize the relevant implementation from the plateau class skill.
- TO BE — show the implementation after the solution's changes.

When `built_on_plateau` is empty, describe the change directly without the AS IS/TO BE split.
```
```example
### AS IS
[[Class skill]] currently ...

### TO BE
[[Class skill]] must ...
```
```code example
### AS IS
export class SomeEntity {
  constructor(private readonly id: number) {}
}

### TO BE
export class SomeEntity {
  constructor(
    private readonly id: number,
    private readonly guid: string
  ) {}
}
```

# Rule changes
```hint
Define how solution EXTENDS class/file rules. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
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
- Class must expose a typed public interface
  - Risk: callers depend on an untyped surface, so contract violations surface at runtime.
  - Fix: declare explicit types on every public method and property.
- Never use `any` for constructor parameters.
  - Risk: type errors surface at runtime instead of compile time.
  - Fix: declare explicit parameter types.
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
Define how solution EXTENDS class/file check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] Class has a typed public API
```

# Unittest TestCases
```hint
Define how solution EXTENDS class unit tests.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN constructed with valid data THEN instance is initialized
```
