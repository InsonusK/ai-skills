---
name: plateau-persisted-state-monolith--class-selective-preloading-strategy
description: The custom router PreloadingStrategy that preloads a lazy route's chunk only when its mounting route carries data.preload === true — persisted-state-monolith plateau
domain: skill
type: template
whenToUse: when creating or editing apps/platform-shell/src/app/preloading/selective-preloading.strategy.ts (VP1)
plateau: persisted-state-monolith
artifact_type: service
version: 20260903190000
tags:
  - skill/template/class
  - plateau/persisted-state-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]]"

> Lives in `apps/platform-shell/src/app/preloading/`. Registered once, in `app.config.ts`, via `provideRouter(appRoutes, withPreloading(SelectivePreloadingStrategy))`.

# Goal

- Warm up, in the background, only the lazy chunks the shell has explicitly marked as high-traffic — leaving every other feature and every federated remote chunk purely on-demand

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]

# Core Principles

- Apply ONE plateau template per class/artifact
- The strategy reads a route-data flag only — it holds no list of feature names, so adding a feature never touches this file
- The preload decision belongs to whoever mounts a segment; this class only executes that decision

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/adr/preloading-strategy.md|Preloading Strategy ADR]]

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------- | ---------- | -------------------- | --------- |
| Router preloading strategy | `SelectivePreloadingStrategy` | `SelectivePreloadingStrategy` | `selective-preloading.strategy.ts` | `selective-preloading.strategy.ts` |
| Its spec | — | — | `selective-preloading.strategy.spec.ts` | `selective-preloading.strategy.spec.ts` |

# Implementation

```typescript
// Skill: class-selective-preloading-strategy
// Plateau: persisted-state-monolith
// Version: 20260902160000

import { Injectable } from '@angular/core';
import { PreloadingStrategy, Route } from '@angular/router';
import { Observable, of } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class SelectivePreloadingStrategy implements PreloadingStrategy {
  preload(route: Route, load: () => Observable<unknown>): Observable<unknown> {
    return route.data?.['preload'] === true ? load() : of(null);
  }
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]

# Rules

## MUST
- The strategy must call `load()` only when `route.data?.['preload'] === true`, and return `of(null)` for everything else.
- It must be provided at the composition root through `withPreloading(...)` — never referenced or invoked by a feature.
- Never apply several plateau templates per class/artifact.
- Never let this class hold a hard-coded list of routes/features to preload — the `data.preload` flag on the route is the only input.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]

# Check list

- [ ] `preload()` returns `load()` for `data.preload === true` and `of(null)` otherwise
- [ ] The class is registered only via `withPreloading(SelectivePreloadingStrategy)` in `app.config.ts`
- [ ] No feature or embeddable module imports this class

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]

# Unittest TestCases

- [ ] WHEN a route has `data: { preload: true }` THEN
  - [ ] `preload()` invokes `load()` and emits its value
- [ ] WHEN a route has no `preload` flag (or `preload: false`) THEN
  - [ ] `preload()` does not invoke `load()` and emits `null`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
