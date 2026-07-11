---
name: project-feature-feature
description: Generic template for any {feature}/feature lib — routed, presentational + container components for one feature, plus its feature-level Signal Store
domain: skill
type: template
plateau: foundation
project_kind: library
version: 20260711120000
tags:
  - skill/template/project
  - plateau/foundation
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]]"
---

> Generic pattern, not tied to one concrete feature — every business feature added to the workspace gets its own `libs/{feature}/feature` project following this template, substituting `{Feature}`/`{feature}` with the real feature name.

# Goal

- Colocate a feature's state with its components inside `libs/{feature}/feature`, keeping feature-level state encapsulated behind the workspace's module boundaries
- Avoid classical NgRx action/reducer/effect boilerplate for state owned by exactly one feature

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]

# Structure

## Project Structure

```
/libs/{feature}
  /feature
    /src
      /lib
        [{feature}.store.ts](./classes/class-feature-store.skill.md)
      index.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| `{feature}.store.ts` | Feature-level NgRx Signal Store. Owns all state and derived data specific to this feature. | [[classes/class-feature-store.skill.md\|class-feature-store.skill]] |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]

## What Does NOT Belong Here

- HTTP client wiring / DTO mapping — belongs in this feature's own `data-access` lib
- Cross-cutting state (auth, notifications, offline-sync) — belongs in `libs/shared/state`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

## Allowed Dependencies

- `libs/{feature}/data-access` (same scope)
- `libs/shared/ui` (tag: `type:ui`, `scope:shared`)
- `libs/shared/util` (tag: `type:util`, `scope:shared`)
- `libs/shared/state` (tag: `type:store`, `scope:shared`)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- The feature Signal Store MUST live inside `libs/{feature}/feature`, not in a shared location.
- The feature Signal Store MUST be the only place that feature's components read/write feature-owned state.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]

# Anti-patterns

- **Placing a feature Signal Store in `libs/shared/state` or another feature's lib**
  - Consequence: feature state leaks across module boundaries and violates the workspace's enforced module boundaries
  - Instead: keep the store colocated with the feature it belongs to

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]

# Check list

- [ ] `libs/{feature}/feature` contains exactly one feature-level Signal Store for that feature
- [ ] The store is exported from the lib's `index.ts`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-state-management.skill/solution-state-management.skill|solution-state-management]] - [[skills/angular/architecture/solutions/solution-state-management.skill/Implementation/FeatureStore/{Feature}.project.extend|FeatureStore/{Feature}.project.extend]]
