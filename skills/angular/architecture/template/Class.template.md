---
description: Short description what must be made while creation or change in this artifact
project_name: # The Nx project (app or lib) in which the artifact is located
name: # Artifact name, without suffix (e.g. "button", "auth", "confirm-navigation")
artifact_type: # component | service | directive | pipe | guard | interceptor | resolver | store | module | pipe
change_kind: # create | extend
# - create if solution creates a new artifact template. Name of the artifact must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing artifact template. Link to the artifact must be added into the `extends` property in the header of the solution.
---

# How Apply this template
- Replace all `hint`, `example` and `code example` blocks with real content. Do not keep them in the final skill file.
- If a section does not introduce any changes for this artifact, remove the section or add a note that no changes are introduced.
- This template covers any TypeScript building block of an Angular app — component, service, directive, pipe, guard, interceptor, resolver, Signal Store, NgRx feature (actions/reducer/effects/selectors), etc. Pick the `artifact_type` that fits and remove naming-convention rows that do not apply.

# Goals
```hint
Define how solution EXTENDS artifact goal.
MUST:
- show all added Goals
RECOMMENDATION:
- Prefer bullet list
```
```example
- Prevent duplicate submit via a pending-request guard in the store
```

# Core Principles
```hint
Define how solution EXTENDS artifact core principles.
MUST:
- show all added Core Principles
RECOMMENDATION:
- Prefer bullet list
```
```example
- Component stays presentational; all decisions are delegated to the store.
```

# Naming convention
```hint
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

# Implementation changes
```hint
Define how solution EXTENDS artifact implementation.
```
```example
[[Artifact skill]] must expose loading state as a computed signal.
```
```code example
@Injectable()
export class OrdersStore extends signalStore(
  withState<OrdersState>({ orders: [], loading: false }),
) {}
```

# Rule changes
```hint
Define how solution EXTENDS artifact MUST, SHOULD, MAY, SHOULD NOT, MUST NOT rules.
Only add a subblock for categories where this solution introduces new rules.
If a category has no new rules, skip it — do not write an empty subblock.

MUST:
- show all added Rules
```

## MUST
```example
- Store must expose state as readonly signals, never as a mutable public field
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
Describe concrete wrong ways to implement this artifact and their consequences.
Each item must tell the agent what NOT to do, why it is harmful, and what to do instead.

Format:
- **{What NOT to do}**
  - Consequence: {negative consequence}
  - Instead: {correct alternative}
```
```example
- **Inject HttpClient directly into a component**
  - Consequence: business logic and transport concerns leak into presentation, component becomes untestable in isolation
  - Instead: go through a facade/store that owns the HTTP call
```

# Check list
```hint
Define how solution EXTENDS artifact check list.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] Component uses `ChangeDetectionStrategy.OnPush`
```

# Unittest TestCases
```hint
Define how solution EXTENDS artifact unit tests.
RECOMMENDATION:
- Prefer checkbox list
```
```example
- [ ] WHEN store.load() is called THEN
  - [ ] loading signal becomes true
  - [ ] HTTP call is issued through the facade
  - [ ] on success, orders signal is populated and loading becomes false
```
