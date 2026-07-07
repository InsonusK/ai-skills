---
description: Short description what must be made while creation or change in this Nx project
name: # Nx project name (e.g. "shared-ui", "platform-shell", "orders-feature")
project_kind: # application | library
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new Nx project template. Name of the project must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing Nx project template. Link to the project must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this project, remove the section or add a note that no changes are introduced.
- Use `project_kind: application` for anything under `apps/` (deployable, has its own build/serve target) and `project_kind: library` for anything under `libs/` (consumed by other projects, no standalone deploy target).

# Goals
```hint
Define how solution EXTENDS project goal.
MUST:
- show all added goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Encapsulate feature-level orders logic behind a narrow public API
```

# Core Principles
```hint
Define how solution EXTENDS project core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- Only the store and facade are part of the public API; components stay internal.
```

# Structure

## Project Structure
```hint
Define how solution EXTENDS project structure.
```
```example
/libs/orders-feature
  /src
    /lib
      /orders-list
        orders-list.component.ts
      orders.store.ts
    index.ts
```

## Directory and file skills
```hint
Define how solution EXTENDS project directory and files.
```
```example
| Directory/file | Description |
| --------------- | ----------- |
| /src/lib/orders-list | Presentational list component |
| index.ts | Public API barrel — only store and facade are re-exported |
```

| Directory/file | Description |
| --------------- | ----------- |
|                 |             |

# NPM Packages
```hint
Define how solution EXTENDS project npm dependencies.
```
```example
| Package         | Version constraint | Purpose                    |
| --------------- | ------------------- | --------------------------- |
| @ngrx/signals   | >= 18               | Signal Store implementation |
```

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
|         |                     |         |

# What Does NOT Belong Here
```hint
Define how solution EXTENDS project components which do not belong to it.
RECOMMENDATION:
- Prefer bullet list
```
```example
- HTTP client wiring - belongs to [[libs-orders-data-access skill]]
```

# Allowed Dependencies
```hint
Define how solution EXTENDS allowed dependencies that project may have (in terms of Nx tags, e.g. scope:*, type:*).
RECOMMENDATION:
- Prefer bullet list
ATTENTION:
- Solution should not change allowed dependencies. Confirm extension from user before adding.
```
```example
- [[libs-shared-ui]] (tag: type:ui)
- [[libs-orders-data-access]] (tag: type:data-access, scope:orders)
```

# Rules
```hint
Define how solution EXTENDS project MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- ...
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
Describe concrete wrong ways to apply the solution to this project and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Import another feature lib's internal component directly, bypassing its index.ts**
  - Consequence: breaks encapsulation, defeats affected-based builds, creates hidden coupling
  - Instead: only import through the public API barrel, or move shared code into a `shared`/`util` lib
```

# Check list
```hint
Define how solution EXTENDS project check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] Project has Nx tags matching its `type:*`/`scope:*` role
- [ ] Public API is exposed through a single `index.ts`
```
