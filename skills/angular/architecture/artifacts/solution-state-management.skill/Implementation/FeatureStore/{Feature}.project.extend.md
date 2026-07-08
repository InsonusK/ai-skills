---
description: Extend a feature's libs/{feature}/feature project with a feature-level NgRx Signal Store
name: "{Feature}"
project_kind: library
element_kind: project
change_kind: extend
---

# Goals

- Colocate a feature's state with its components inside `libs/{feature}/feature`, keeping feature-level state encapsulated behind the module boundary defined by the repository-structure solution
- Avoid the classical NgRx action/reducer/effect boilerplate for state that is owned by exactly one feature

# Structure

## Project Structure

```
/libs/{feature}
  /feature
    /src
      /lib
        {feature}.store.ts
      index.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| `{feature}.store.ts` | Feature-level NgRx Signal Store, created by [[./{Feature}.project.extend/{feature}.store.ts.create.md]]. Owns all state and derived data specific to this feature. |

# Rule changes

## MUST
- The feature Signal Store MUST live inside `libs/{feature}/feature`, not in a shared location.
- The feature Signal Store MUST be the only place that feature's components read/write feature-owned state.

# Anti-patterns

- **Placing a feature Signal Store in `libs/shared/state` or another feature's lib**
  - Consequence: feature state leaks across module boundaries and violates the workspace's enforced module boundaries
  - Instead: keep the store colocated with the feature it belongs to

# Check list

- [ ] `libs/{feature}/feature` contains exactly one feature-level Signal Store for that feature
- [ ] The store is exported from the lib's `index.ts`
