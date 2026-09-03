---
name: plateau-embeddable-app--class-remote-routes
description: The federation-exposed module — REMOTE_ROUTES, a root-relative Routes array the host mounts at one segment; the remote never references that segment — embeddable-app plateau
domain: skill
type: template
whenToUse: when creating or editing the remote's exposed src/app/remote.routes.ts, or reviewing that it bakes in no mount prefix
plateau: embeddable-app
artifact_type: module
version: 20260903180000
tags:
  - skill/template/class
  - plateau/embeddable-app
  - stack/typescript
  - framework/native-federation
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]]"
---

> `src/app/remote.routes.ts`, declared in `federation.config.mjs` as `exposes: { './Routes': './src/app/remote.routes.ts' }`. The host's `loadChildren` reads `REMOTE_ROUTES` off the loaded module.

# Goal

- Define the remote's internal navigation entirely relative to its own root, so any host can mount it at any segment without a code change — hierarchical route ownership (from `solution-app-routing`) one level down

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/routes.ts.extend.md|routes.ts.extend]]

# Naming convention

| use case | export | file |
| -------- | ------ | ---- |
| Exposed routes | `REMOTE_ROUTES` (or `{FEATURE}_ROUTES`) | `remote.routes.ts` |

# Implementation

```typescript
// Skill: class-remote-routes
// Plateau: embeddable-app
import { Routes } from '@angular/router';
import { requirePermission } from './session/require-permission';

// The federation-exposed module. Root-relative paths ONLY — no reference to the
// segment the host mounts this remote at.
export const REMOTE_ROUTES: Routes = [
  {
    path: '',
    canActivate: [requirePermission('reports.view')],
    loadComponent: () => import('./reports/reports.component').then((m) => m.ReportsComponent),
  },
  // a multi-feature remote mounts each of its own features' root segments here,
  // exactly the way the platform shell mounts a feature — never a nested path.
];
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/routes.ts.extend.md|routes.ts.extend]]

# Rules

## MUST
- The exposed module mounts only its **own** features' root segments — never reaches into a feature's internal path structure.
- No route path references the segment the host mounts the remote at (e.g. no `reports/` prefix inside these definitions).
- The exposed module is the single federation entry (`./Routes`) — no second exposed surface the host imports.

## SHOULD
- A `loadComponent` / `loadChildren` split inside the remote is a normal per-route decision, unchanged from the monolith's `class-feature-routes` guidance.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/routes.ts.extend.md|routes.ts.extend]]

# Check list

- [ ] No route path includes the segment the shell mounts the remote at
- [ ] Each feature is mounted at exactly its own root segment, no nested paths added by the module
- [ ] `./Routes` is the only federation-exposed surface

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/routes.ts.extend.md|routes.ts.extend]]

# Unittest TestCases

- [ ] WHEN the exposed `REMOTE_ROUTES` is inspected THEN it contains no reference to the segment under which the shell mounts it
- [ ] WHEN the host mounts `./Routes` at a segment THEN Angular's router composes the full path automatically, with no change to the remote

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/solution-federation-remote.skill.md|solution-federation-remote]] - [[skills/angular/architecture/v3.1/solutions/solution-federation-remote.skill/Implementation/routes.ts.extend.md|routes.ts.extend]]
