---
description: Generic pattern for a feature's own routes — defined and exported relative to the feature's own root only, applies to any {Feature}/feature project
project_name: "{Feature}"
name: "{feature}"
element_kind: module
change_kind: create
tags:
  - solution/app-routing
  - element/feature-routes-ts
---

# How this generic file is used
This is not tied to one concrete feature. Any solution that creates a new `libs/{feature}/feature` project follows this pattern for that feature's own routes, substituting `{Feature}`/`{feature}` with the real feature name.

# Goals

- Define the feature's internal navigation entirely relative to its own root, so the feature can be mounted anywhere by any parent (shell or embeddable module) without modification

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | -------------- | -------------------- | --------- |
| Feature routes | {FEATURE}_ROUTES | ORDERS_ROUTES | {feature}.routes.ts | orders.routes.ts |

# Implementation changes

```typescript
// orders.routes.ts
export const ORDERS_ROUTES: Routes = [
  { path: '', component: OrdersListComponent },
  { path: ':id', component: OrderDetailComponent },
];
```

```typescript
// index.ts
export { ORDERS_ROUTES } from './lib/orders.routes';
```

# Rule changes

## MUST
- `{feature}.routes.ts` paths MUST be defined relative to the feature's own root only (e.g. `''`, `':id'`) — never including the feature's own name or any assumed mount prefix.
- `{FEATURE}_ROUTES` MUST be exported from `index.ts` — it is part of the feature's public API, alongside its Signal Store.

## SHOULD
- **Baking the feature's expected mount segment into its own route paths (e.g. `path: 'orders/:id'`)** — Consequence: breaks the moment the feature is mounted under a different segment or nested inside an embeddable module instead of directly under the shell — Instead: paths are always relative to the feature's own root; the parent assigns the segment at the mounting point

# Check list

- [ ] No path inside `{feature}.routes.ts` includes the feature's own name or an assumed prefix
- [ ] `{FEATURE}_ROUTES` is exported from the feature's `index.ts`
