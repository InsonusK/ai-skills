---
description: Register a custom selective PreloadingStrategy in platform-shell and mark which top-level mounted segments should be preloaded
name: platform-shell
project_kind: application
element_kind: project
change_kind: extend
---

# Goals

- Preload only the top-level segments (features or embeddable modules) explicitly marked as worth warming up in the background, leaving everything else purely on-demand

# Structure

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| /preloading/selective-preloading.strategy.ts | Custom `PreloadingStrategy` implementation that preloads a route only if `route.data?.['preload'] === true` |
| app.config.ts | Registers the router with `withPreloading(SelectivePreloadingStrategy)` |

# Implementation changes

```code example
// selective-preloading.strategy.ts
@Injectable({ providedIn: 'root' })
export class SelectivePreloadingStrategy implements PreloadingStrategy {
  preload(route: Route, load: () => Observable<unknown>): Observable<unknown> {
    return route.data?.['preload'] === true ? load() : of(null);
  }
}
```

```code example
// app.routes.ts (mounting point — see App routing solution)
export const routes: Routes = [
  {
    path: 'feature1',
    data: { preload: true }, // this feature is common enough to warm up
    loadChildren: () => import('@feature1/feature').then(m => m.FEATURE1_ROUTES),
  },
  {
    path: 'module1',
    // no preload flag: this embeddable module's remote chunk is fetched only on demand
    loadChildren: () => remoteRegistry.loadRemoteRoutes('module1'),
  },
];
```

```code example
// app.config.ts
provideRouter(routes, withPreloading(SelectivePreloadingStrategy));
```

# Rule changes

## MUST
- The router MUST be configured with `withPreloading(SelectivePreloadingStrategy)`, not `withPreloading(PreloadAllModules)` and not left with the default `NoPreloading`.
- Marking a top-level segment with `data: { preload: true }` MUST happen in `app.routes.ts`, at the mounting point — never inside the mounted feature's or module's own routes (see [[../Repository.extend.md#MUST NOT|Repository.extend]]).

# Anti-patterns

- **Marking every top-level segment `preload: true` "to be safe"**
  - Consequence: degenerates into the equivalent of `PreloadAllModules`, including unconditionally prefetching federated embeddable modules' remote chunks — the exact outcome this ADR chose selective preloading to avoid
  - Instead: mark only the small number of genuinely high-traffic segments; leave the rest on-demand

# Check list

- [ ] `app.config.ts` registers `withPreloading(SelectivePreloadingStrategy)`
- [ ] Only a deliberately reviewed subset of top-level segments carry `data: { preload: true }`

# Unittest TestCases

- [ ] WHEN a route has `data: { preload: true }` THEN
  - [ ] `SelectivePreloadingStrategy.preload` invokes `load()`
- [ ] WHEN a route has no `preload` flag (or `false`) THEN
  - [ ] `SelectivePreloadingStrategy.preload` does not invoke `load()`, returning `of(null)` instead
