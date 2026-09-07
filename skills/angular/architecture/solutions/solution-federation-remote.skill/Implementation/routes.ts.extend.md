---
description: Generic pattern for routing inside an embeddable module — mounts the root segments of the features it contains, the same way the shell mounts modules
element_kind: module
change_kind: extend
tags:
  - solution/federation-remote
  - element/remote-routes-ts
---

# How this generic file is used
This applies inside any embeddable application repository (see this solution's "embeddable application" plateau) that itself contains more than one feature. It extends the hierarchical route-ownership pattern from [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] one level down: an embeddable module mounts its own features' root segments exactly the way the platform shell mounts a feature.

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
- The module mounts only its own features' root segments — never reaches into a feature's internal path structure.
  - Risk: referencing a feature's child paths here couples the module to that feature's internals and breaks on its route refactor.
  - Fix: `loadChildren` the feature's own `ROUTES` at its root segment; let the feature own everything below.
- The module never assumes or hardcodes the segment the platform shell mounts it under.
  - Risk: a hardcoded `module1/` prefix breaks the moment the shell — or a different host reusing the module — mounts it elsewhere.
  - Fix: define every path relative to the module's own root; the router composes the full path at the mount point.

## SHOULD
- **A module hardcoding its own expected mount prefix into its internal route definitions** — Consequence: breaks if the shell (or a different host reusing this module) mounts it under a different segment — Instead: the module only ever defines paths relative to its own root; Angular's router composes the full path automatically once mounted by the parent

# Check list

- [ ] The module's routes contain no reference to the segment under which the shell mounts it
- [ ] Each feature is mounted at exactly its own root segment, with no nested paths added by the module
