---
name: project-name
description: Describe which plateau Nx project (app or lib) does skill describe
domain: skill
type: template
plateau:
project_kind: # application | library
version:
tags:
  - skill/template/project
created_by:
---
# How Apply this template
- Find in all solutions from `created_by` files made by the project-level implementation file (`{project-name}.project.create.md` / `{project-name}.project.extend.md`)
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
- add to header properties `tags` tag `plateau/{plateau-name}`
- Use `project_kind: application` for anything under `apps/` and `project_kind: library` for anything under `libs/`

# Goal
```hint
Define List of Goals that are pursued by the creation of this skill. Summarize all Goals from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Goals conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class/artifact-level details.

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Encapsulate feature-level orders logic behind a narrow public API

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Core Principles
```hint
Define List of Core Principles that are pursued by the creation of this skill. Summarize all Core Principles from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Core Principles conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class/artifact-level details.

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Only the store and facade are part of the public API; components stay internal.

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Structure

## Workspace place
`Where defined place in the Nx workspace`
```
/apps or /libs
	/project-name
```

## Project Structure
```hint
Define project structure. Summarize all project structure from applied `{project-name}.project.create.md` or `{project-name}.project.extend.md` files. If find project structure in other files also applyed here.

At the end of block writes list to all used files to build block.

MUST:
- Keep only project-level content here. Do not include repository-level or class/artifact-level details.
- If solution conflicted to each other as user to solve the problem
- For project structure block:
  - use link to files which define the class/artifact
- For Applied solutions block:
	- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
```
```example
/libs/orders-feature
  /src
    /lib
      /orders-list
        [orders-list.component.ts](./classes/class-orders-list.skill.md)
      [orders.store.ts](./classes/class-orders-store.skill.md)
    index.ts

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

## Directory and class skills
```hint
Define project Table with directory and files which implement in the project. Summarize all "Directory and file skills" tables from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Directory and class skills conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class/artifact-level details. Do not list nested files inside a class/artifact skill.

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
| `Directory\|file`  | Description                                    | Pattern skill          |
| ------------------- | ---------------------------------------------- | ---------------------- |
| /src/lib/orders-list | Presentational list component                 | [[class-orders-list.skill.md\|class-orders-list.skill]] |
| index.ts            | Public API barrel — only store and facade are re-exported | — |

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

| `Directory\|file` | Description | Pattern skill |
| ---------------- | ----------- | ------------- |
|                  |             |               |

## NPM Packages
```hint
Define project Table with npm dependencies. Summarize all "NPM Packages" tables from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If entries conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class/artifact-level details.

RECOMMENDATION:
- Prefer pure copy with out changing
```
```example
| Package         | Version constraint | Purpose                    |
| --------------- | ------------------- | ---------------------------- |
| @ngrx/signals   | >= 18               | Signal Store implementation |

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
|         |                     |         |

## What Does NOT Belong Here
```hint
Define components which do not belong to this project. Summarize all "What Does NOT Belong Here" from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If entries conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class/artifact-level details.

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- HTTP client wiring - belongs to [[project-orders-data-access.skill.md|project-orders-data-access]]

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

## Allowed Dependencies
```hint
Define allowed dependencies that the project may have (in terms of Nx tags, e.g. scope:*, type:*). Summarize all "Allowed Dependencies" from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If entries conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class/artifact-level details.

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- [[project-shared-ui.skill.md|libs/shared/ui]] (tag: type:ui)

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```
MUST NOT:
- extended_by solution modify Allowed Dependencies without explicit user confirmation

# Rules
```hint
Define MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules. Summarize all Rules from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Rules conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class/artifact-level details.
- Only add a subblock for categories where at least one solution introduces a rule. If a category has no rules, skip it — do not write an empty subblock.

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```

## MUST
```example
- ...

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
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
Define what it means that this skill was applied wrong. Summarize all "Anti-patterns" from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If entries conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class/artifact-level details.

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- **Import another feature lib's internal component directly, bypassing its index.ts**
  - Consequence: breaks encapsulation, defeats affected-based builds, creates hidden coupling
  - Instead: only import through the public API barrel

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Check list
```hint
Define what must be true before this template is considered correctly applied. Summarize all "Check list" from all finded project-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If entries conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only project-level content here. Do not include repository-level or class/artifact-level details.

RECOMMENDATION:
- Prefer checkbox list
- Prefer pure copy with out changing
```
```example
- [ ] Project has Nx tags matching its `type:*`/`scope:*` role
- [ ] Public API is exposed through a single `index.ts`

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```
