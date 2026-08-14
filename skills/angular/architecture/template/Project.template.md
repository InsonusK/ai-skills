---
description: Short description what must be made while creation or change in this Nx project
name: # Nx project name (e.g. "shared-ui", "platform-shell", "orders-feature")
project_kind: # application | library
element_kind: # repository | project | class
change_kind: # create | extend
# - create if solution creates a new Nx project template. Name of the project must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing Nx project template. Link to the project must be added into the `extends` property in the header of the solution.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the project path in kebab-case, no braces or dots
  # (e.g. libs/shared-ui -> element/libs-shared-ui, apps/platform-shell -> element/apps-platform-shell).
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
Define how solution EXTENDS project rules. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
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
- Never import another feature lib's internal component directly, bypassing its index.ts.
  - Risk: breaks encapsulation, defeats affected-based builds, creates hidden coupling.
  - Fix: only import through the public API barrel, or move shared code into a `shared`/`util` lib.
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
Define how solution EXTENDS project check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] Project has Nx tags matching its `type:*`/`scope:*` role
- [ ] Public API is exposed through a single `index.ts`
```
