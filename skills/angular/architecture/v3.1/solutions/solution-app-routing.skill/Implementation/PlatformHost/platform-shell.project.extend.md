---
description: Root routing configuration in platform-shell — mounts embeddable modules and directly-owned features at their root segments only
name: platform-shell
project_kind: application
element_kind: project
change_kind: extend
tags:
  - solution/app-routing
  - element/platform-shell-project
---

# Goals

- Mount each embeddable module and each directly-owned feature at a single root segment, without knowing what routes exist beneath that segment

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    app.routes.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| app.routes.ts | Top-level `Routes` array. Each entry is either: (a) an embeddable module's root segment, resolved at runtime via the federation remote registry (see the platform-embeddability solution), or (b) a directly-owned feature's root segment, resolved via `loadChildren` pointing at that feature's exported `Routes`. |

# Implementation changes

```typescript
// app.routes.ts
export const routes: Routes = [
  {
    path: 'module1',
    loadChildren: () =>
      remoteRegistry.loadRemoteRoutes('module1'), // see platform-embeddability solution
  },
  {
    path: 'feature1',
    loadChildren: () =>
      import('@feature1/feature').then(m => m.FEATURE1_ROUTES),
  },
];
```

# Rule changes

## MUST
- Each entry in `app.routes.ts` is exactly one root segment, mounting an embeddable module or a directly-owned feature — never a path nested inside one.
  - Risk: `path: 'orders/report'` in the shell means the shell now knows the feature's internal routes and breaks when they change.
  - Fix: mount `orders` as one `loadChildren` entry; the feature's own routes define `report` beneath it.
- The shell never imports a component from inside a feature or module into `app.routes.ts` — only the exported `Routes` / entry point.
  - Risk: a static import of a lazy feature pulls it into the initial bundle and couples the shell to that feature's build.
  - Fix: `loadChildren: () => import('@org/{feature}-feature').then(m => m.{FEATURE}_ROUTES)`.

## SHOULD
- **Adding a route in `app.routes.ts` that targets a specific page inside a feature (e.g. `path: 'feature1/page'`)** — Consequence: shell now depends on the feature's internal route structure, breaking hierarchical ownership and making `nx affected` treat the shell as touched by internal feature navigation changes — Instead: mount only `feature1` as a segment; the feature's own routes define `page` beneath it

# Check list

- [ ] Every entry in `app.routes.ts` is a single root segment with no nested path
- [ ] No component import from inside a feature or embeddable module appears in `app.routes.ts`
