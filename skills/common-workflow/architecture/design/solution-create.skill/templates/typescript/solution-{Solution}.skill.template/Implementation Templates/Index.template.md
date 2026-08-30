---
description: Create or change a package's barrel index.ts file
project_name: # The package in which the file is located
name: # Path inside the package, e.g. src or src/validators
element_kind: index
change_kind: # create | extend
# - create if solution creates a new index.ts barrel file. The path must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing index.ts barrel file, for example by adding a re-export. Link to the file must be added into the `extends` property in the header of the solution.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the barrel file path in kebab-case, no braces or dots
  # (e.g. src/index.ts -> element/src-index-ts, src/{area}/index.ts -> element/src-area-index-ts).
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
Define how solution EXTENDS index.ts rules. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
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
- Every symbol consumed outside the package must be re-exported from `index.ts`
  - Risk: consumers import internal paths, so restructuring becomes a breaking change.
  - Fix: re-export every public symbol explicitly from `index.ts`.
- Never re-export internal test helpers.
  - Risk: test-only utilities silently become public API.
  - Fix: keep test helpers outside the barrel file's re-exports.
- Never use `export * from "./internal-module"`.
  - Risk: every export of the internal module silently becomes public API, including ones never meant to be.
  - Fix: re-export named symbols explicitly.
```

## SHOULD
```example
- Group related re-exports with a short comment
```

## MAY
```example
- ...
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
