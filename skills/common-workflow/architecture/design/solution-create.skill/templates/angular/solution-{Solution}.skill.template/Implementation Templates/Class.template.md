---
description: Short description what must be made while creation or change in TypeScript file/class
project_name: # The project in which the file/class is located
name: # File or class name
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new file/class template. Name of the file/class must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing file/class template. Link to the file/class must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this file/class, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS file/class goal.
MUST:
- show all added Goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Provide reactive state for a feature
```

# Core Principles
```hint
Define how solution EXTENDS file/class core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- Services are stateless where possible
```

# Naming convention
```hint
File/class naming convention. Fill table:
- use case - when apply naming convention
- class name pattern - mask of class name. Example: Is{Rule}
- class name - example of class name. Example: IsEven
- file name pattern - file name pattern. Example: Is{Rule}.ts
- file name - example of file name. Example: IsEven.ts
```

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
|          |                    |            |                   |           |

# Implementation changes
```hint
Define how solution EXTENDS file/class implementation.
```
```example
[[Class skill]] must ...
```
```code example
@Component({
  selector: 'app-example',
  template: `<div>{{ title() }}</div>`
})
export class ExampleComponent {
  title = input<string>();
}
```

# Rule changes
```hint
Define how solution EXTENDS file/class MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- Component must use OnPush change detection
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
Describe concrete wrong ways to implement this file/class and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Use `any` for typed inputs**
  - Consequence: loses type safety and IDE support
  - Instead: define typed inputs with `input<T>()`
```

# Check list
```hint
Define how solution EXTENDS file/class check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] Component uses `ChangeDetectionStrategy.OnPush`
```

# Unittest TestCases
```hint
Define how solution EXTENDS file/class unit tests.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN input changes THEN component re-renders
```
