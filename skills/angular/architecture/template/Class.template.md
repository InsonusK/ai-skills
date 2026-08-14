---
description: Short description what must be made while creation or change in this artifact
project_name: # The Nx project (app or lib) in which the artifact is located
name: # Artifact name, without suffix (e.g. "button", "auth", "confirm-navigation")
artifact_type: # component | service | directive | pipe | guard | interceptor | resolver | store | module | pipe
change_kind: # create | extend
# - create if solution creates a new artifact template. Name of the artifact must be added into the `creates` property in the header of the solution.
# - extend if solution extends an existing artifact template. Link to the artifact must be added into the `extends` property in the header of the solution.
tags:
  - solution/{solution-name}
  - element/{element-name}
  # solution/{solution-name}: the owning solution name without the `solution-` prefix, kebab-case.
  # element/{element-name}: the artifact file name in kebab-case, no braces or dots
  # (e.g. orders.store.ts -> element/orders-store-ts, button.component.ts -> element/button-component-ts).
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
Define how solution EXTENDS artifact rules. Follow the Rule-section baseline in [[skills/common-workflow/skill-design.skill/skill-design.skill.md|skill-design]]:
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
- Store must expose state as readonly signals, never as a mutable public field
  - Risk: any caller can mutate state directly, bypassing the store's invariants.
  - Fix: expose computed readonly signals and mutate state only through store methods.
- Never inject HttpClient directly into a component.
  - Risk: business logic and transport concerns leak into presentation, component becomes untestable in isolation.
  - Fix: go through a facade/store that owns the HTTP call.
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
