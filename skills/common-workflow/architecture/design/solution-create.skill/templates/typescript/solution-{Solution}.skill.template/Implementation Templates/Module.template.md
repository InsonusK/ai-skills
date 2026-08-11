---
description: Short description what must be made while creation or change in a file that contains standalone functions
project_name: # The package in which the file is located
name: # Module name
element_kind: module
change_kind: # create | extend
# - create if solution creates a new functions module. Name of the module/file must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing functions module. Link to the module/file must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this module, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS module goal.
MUST:
- show all added Goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Provide the validation function proven by the shared Cucumber spec
```

# Core Principles
```hint
Define how solution EXTENDS module core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- Functions are pure and side-effect free
```

# Naming convention
```hint
Module and function naming convention. Fill table:
- use case - when apply naming convention
- function name pattern - mask of function name. Example: is{Rule}
- function name - example of function name. Example: isValidEmail
- file name pattern - file name pattern. Example: {kebab-name}.ts
- file name - example of file name. Example: is-valid-email.ts
```

| use case | function name pattern | function name | file name pattern | file name |
| -------- | --------------------- | ------------- | ----------------- | --------- |
|          |                       |               |                   |           |

# Implementation changes
```hint
Define how solution EXTENDS module implementation.
```
```example
[[Functions module]] must ...
```
```code example
export function isValidEmail(value: string): boolean {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(value);
}
```

# Rule changes
```hint
Define how solution EXTENDS module MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- Functions must be pure and side-effect free
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
Describe concrete wrong ways to implement this module and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Duplicate the validation rule inside a Cucumber step definition instead of calling this module**
  - Consequence: the scenario can pass even after this module's real behavior is broken
  - Instead: import and call the exported function directly from the step definition
```

# Check list
```hint
Define how solution EXTENDS module check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] Function is exported from the module and re-exported through `index.ts`
```

# Unittest TestCases
```hint
Define how solution EXTENDS module unit tests.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN called with valid input THEN returns the expected result
```
