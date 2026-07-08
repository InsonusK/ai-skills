---
description: Short description what must be made while creation or change in the repository (Nx workspace)
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new repository-level template.
# - extend if solution extends an existing repository-level template.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for the repository, remove the section or add a note that no changes are introduced.
- "Repository" here means the Nx workspace root — everything above the level of an individual app/lib project.

# Structure

## Workspace Structure
```hint
Define how solution EXTENDS Nx workspace structure (top-level apps/libs layout).
```
```example
/apps
  /platform-shell
/libs
  /shared
    /ui
    /util
  /orders
    /feature
    /data-access
```

## Directory and project skills
```hint
Define how solution EXTENDS repository top-level directories and the Nx projects that live in them.
```
```example
| Directory | Description |
| ---------- | ----------- |
| /apps/platform-shell | Host application, composition root, routing shell |
| /libs/shared | Cross-feature reusable code (UI wrappers, utils) — no feature-specific logic |
| /libs/orders | All orders-related feature and data-access libs |
```

| Directory | Description |
| ---------- | ----------- |
|           |             |

# Rules
```hint
Define how solution EXTENDS repository MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- Every Nx project must declare tags matching the `type:*`/`scope:*` taxonomy
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
Describe concrete wrong ways to apply the solution at repository level and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Place a new feature directly under /apps instead of a routed lib under /libs**
  - Consequence: feature can no longer be reused or lazy-loaded independently, and affected-based builds treat the whole app as changed
  - Instead: scaffold the feature as a lib under /libs and route to it lazily from the shell app
```

# Unittest TestCases
```hint
Define how solution EXTENDS repository-level integration/e2e checks.

RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN `nx run-many -t lint` is executed THEN
  - [ ] `@nx/enforce-module-boundaries` reports no violations
- [ ] WHEN a commit only touches `/libs/orders/feature` THEN
  - [ ] `nx affected -t test` runs tests only for `orders-feature` and its dependents
```
