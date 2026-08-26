---
name: plateau-{plateau-name}--class-{name}
description: Describe which class skill define
whenToUse: One concrete sentence — which task must make the agent open this skill
  # MUST name a concrete situation: creating or editing this exact component/service/store/etc., or creating a new artifact that plays the same role. MUST NOT be vague ("when relevant").
  # Example: "when creating or editing {ClassName}, or creating another {artifact_type} that plays the same role in a different feature"
domain: skill
type: template
plateau:
artifact_type: # component | service | directive | pipe | guard | interceptor | resolver | store | module
version:
tags:
  - skill/template/class
created_by:
---
# How Apply this template
- Fill `whenToUse` with the concrete class/artifact-level situations that require this skill (creating/editing this artifact, or creating another artifact with the same role). See [skill-design](skills/common-workflow/skill-design.skill/skill-design.skill.md) for the baseline rules.
- Find in all solutions from `created_by` files made by the artifact-level implementation file (`{name}.{artifact-type}.ts.create.md` / `{name}.{artifact-type}.ts.extend.md`)
- Replace all ```hint``` and ```example``` blocks with real content. Do not keep them in the final skill file.
- add to header properties `tags` tag `plateau/{plateau-name}`
- This template covers any TypeScript building block of an Angular app — component, service, directive, pipe, guard, interceptor, resolver, Signal Store, NgRx feature, etc. Pick the `artifact_type` that fits.

# Goal
```hint
Define List of Goals that are pursued by the creation of this skill. Summarize all Goals from all finded artifact-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Goals conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class/artifact-level content here. Do not include repository-level or project-level details.

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing

After summarized list add Applied solutions list with links to all solutions which effect to this Goals.
```
```example
- Prevent duplicate submit via a pending-request guard in the store

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Core Principles
```hint
Define List of Core Principles that are pursued by the creation of this skill. Summarize all Core Principles from all finded artifact-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Core Principles conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class/artifact-level content here. Do not include repository-level or project-level details.
- Add Core principle `Apply ONE plateau template per class/artifact`

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Apply ONE plateau template per class/artifact
- Component stays presentational; all decisions are delegated to the store.

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Naming convention
```hint
Define Naming convention. Summarize all "Naming convention" tables from all finded artifact-level implementation files.

At the end of block writes list to all used templates to build block.

Artifact naming convention. Fill table:
- use case - when apply naming convention
- class name pattern - mask of the exported TS class/const/function name. Example: {Feature}Store
- class name - example of the exported name. Example: OrdersStore
- file name pattern - file name pattern, following Angular's dot-suffix convention. Example: {feature}.store.ts
- file name - example of file name. Example: orders.store.ts
```

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
|          |                     |            |                       |           |

# Implementation
```hint
Define Implementation of the class/artifact. Summarize all "Implementation changes" from all finded artifact-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- Write a comment at the top of the created file with information from applied skill properties
  - name
  - plateau
  - version

- If Implementation changes conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class/artifact-level content here. Do not include repository-level or project-level details.

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```code example
// Skill: class-orders-store
// Plateau: default
// Version: 20260628

@Injectable()
export class OrdersStore extends signalStore(
  withState<OrdersState>({ orders: [], loading: false }),
) {}
```
```example
__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Rules
```hint
Define MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules of the class/artifact. Summarize all "Rule changes" from all finded artifact-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If Rules conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class/artifact-level content here. Do not include repository-level or project-level details.
- Only add a subblock for categories where at least one solution introduces a rule. If a category has no rules, skip it — do not write an empty subblock.

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```

## MUST
```example
- Store must expose state as readonly signals, never as a mutable public field

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
Define what it means that this skill was applied wrong. Summarize all "Anti-patterns" from all finded artifact-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If entries conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class/artifact-level content here. Do not include repository-level or project-level details.
- Add antipattern `Apply SEVERAL plateau templates per class/artifact`

RECOMMENDATION:
- Prefer bullet list
- Prefer pure copy with out changing
```
```example
- Apply SEVERAL plateau templates per class/artifact
- Inject HttpClient directly into a component
  - Consequence: business logic and transport concerns leak into presentation
  - Instead: go through a facade/store that owns the HTTP call

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Check list
```hint
Define what must be true before this template is considered correctly applied. Summarize all "Check list" from all finded artifact-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If entries conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class/artifact-level content here. Do not include repository-level or project-level details.

RECOMMENDATION:
- Prefer checkbox list
- Prefer pure copy with out changing
```
```example
- [ ] Component uses `ChangeDetectionStrategy.OnPush`

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```

# Unittest TestCases
```hint
Define list of unit tests which must be created to test the class/artifact. Summarize all "Unittest TestCases" from all finded artifact-level implementation files.

At the end of block writes list to all used templates to build block.

MUST:
- If entries conflicted to each other as user to solve the problem
- Each bullet must be `<solution skill link> - <implementation file link>` (see plateau-build SKILL.md "Applied solutions list format")
- Keep only class/artifact-level content here. Do not include repository-level or project-level details.

RECOMMENDATION:
- Prefer checkbox list
- Prefer pure copy with out changing
```
```example
- [ ] WHEN store.load() is called THEN
  - [ ] loading signal becomes true
  - [ ] HTTP call is issued through the facade

__Applied solutions:__
- [[Solution link]] - [[implementation file link]]
```
