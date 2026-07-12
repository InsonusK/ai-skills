---
description: Generic pattern for routing inside an embeddable module — mounts the root segments of the features it contains, the same way the shell mounts modules
element_kind: module
change_kind: extend
---

# How this generic file is used
This applies inside any embeddable application repository (see this solution's "embeddable application" plateau) that itself contains more than one feature. It extends the hierarchical route-ownership pattern from [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|App routing (база)]] one level down: an embeddable module mounts its own features' root segments exactly the way the platform shell mounts a feature.

# Structure

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| {module-root}/routes.ts | The module's own root-relative `Routes` array, exposed as (or alongside) its federation-exposed entry point. Mounts each of its own features' root segments (e.g. `feature-map/feature2`), without knowing what lies beneath a feature's own root. |

# Implementation changes

```typescript
// exposed as part of the module's federation entry point
export const MODULE_ROUTES: Routes = [
  {
    path: 'feature-map/feature2',
    loadChildren: () =>
      import('@feature2/feature').then(m => m.FEATURE2_ROUTES),
  },
];
```

# Rule changes

## MUST
- The module MUST only mount its own features' root segments — it MUST NOT reach into a feature's internal path structure.
- The module MUST NOT assume or hardcode the segment under which the platform shell will mount it (e.g. it must not reference `module1/` inside its own route definitions) — that prefix is assigned by the shell at the mounting point.

# Anti-patterns

- **A module hardcoding its own expected mount prefix into its internal route definitions**
  - Consequence: breaks if the shell (or a different host reusing this module) mounts it under a different segment
  - Instead: the module only ever defines paths relative to its own root; Angular's router composes the full path automatically once mounted by the parent

# Check list

- [ ] The module's routes contain no reference to the segment under which the shell mounts it
- [ ] Each feature is mounted at exactly its own root segment, with no nested paths added by the module
