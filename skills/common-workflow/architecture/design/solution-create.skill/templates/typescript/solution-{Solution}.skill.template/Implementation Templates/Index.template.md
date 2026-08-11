---
description: Create or change a package's barrel index.ts file
project_name: # The package in which the file is located
name: # Path inside the package, e.g. src or src/validators
element_kind: index
change_kind: # create | extend
# - create if solution creates a new index.ts barrel file. The path must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing index.ts barrel file, for example by adding a re-export. Link to the file must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this `index.ts`, remove the section or add a note that no changes are introduced.

# Goals
```hint
Define how solution EXTENDS index.ts goal.
MUST:
- show all added goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Expose the package's validators as its only public API surface
```

# Core Principles
```hint
Define how solution EXTENDS index.ts core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- `index.ts` re-exports named symbols explicitly; it never uses `export *` on internal modules
```

# Naming convention
```hint
Barrel file naming convention. Fill table:
- use case - when apply naming convention
- path pattern - mask of the barrel file's path. Example: src/{area}
- path - example of path. Example: src/validators
- file name - always index.ts
```

| use case | path pattern | path | file name |
| -------- | ------------ | ---- | --------- |
|          |              |      |           |

# Implementation changes
```hint
Define how solution EXTENDS index.ts implementation.
```
```example
[[Package index]] must ...
```
```code example
// src/index.ts
export { isValidEmail } from "./is-valid-email";
export { PhoneNumberValidator } from "./phone-number-validator";
```

# Rule changes
```hint
Define how solution EXTENDS index.ts MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- Every symbol consumed outside the package must be re-exported from `index.ts`
```

## SHOULD
```example
- Group related re-exports with a short comment
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
- Re-export internal test helpers
```

# Anti-patterns
```hint
Describe concrete wrong ways to implement this index.ts and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Use `export * from "./internal-module"`**
  - Consequence: every export of the internal module silently becomes public API, including ones never meant to be
  - Instead: re-export named symbols explicitly
```

# Check list
```hint
Define how solution EXTENDS index.ts check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] `src/index.ts` exists and re-exports the intended public API only
```
