---
name: project-platform-shell
description: The single deployable Angular application — composition root, top-level root-relative routing, selective preloading, root providers
domain: skill
type: template
plateau: authenticated
project_kind: application
version: 20260711150000
tags:
  - skill/template/project
  - plateau/authenticated
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]]"
  - "[[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]]"
---

> Unchanged since [[skills/angular/architecture/plateau/navigable/plateau-navigable.skill.md|navigable]] — neither `solution-forms` nor `solution-api-http-layer` touch `apps/platform-shell`.
>
> **Deferred scope:** carried over from navigable — the embeddable-module/federation-based mounting mechanism (`remoteRegistry.loadRemoteRoutes(...)`) remains deferred to a future "platform" plateau. `app.routes.ts` only mounts directly-owned features via static `loadChildren` imports.

# Goal

- Be the only deployable unit at this plateau: bootstrap the application, own top-level routing, register root providers
- Contain no business logic of its own — every feature lives under `libs/{feature}` and is only routed to from here
- Mount each directly-owned feature at a single root segment, without knowing what routes exist beneath that segment
- Preload only the top-level features explicitly marked as worth warming up in the background, leaving everything else purely on-demand

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    /app
      /preloading
        selective-preloading.strategy.ts
      app.config.ts
      app.routes.ts
    main.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| app.config.ts | Root provider registration; registers the router with `withPreloading(SelectivePreloadingStrategy)` | — |
| app.routes.ts | Top-level `Routes` array — one `loadChildren` entry per directly-owned feature's root segment, some carrying `data: { preload: true }` | — |
| /preloading/selective-preloading.strategy.ts | Custom `PreloadingStrategy` implementation that preloads a route only if `route.data?.['preload'] === true` | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Rules

## MUST
- [[../repo-authenticated.skill.md#MUST|repo-authenticated]]

## MUST NOT
- [[../repo-authenticated.skill.md#MUST NOT|repo-authenticated]]

# Anti-patterns

- **Adding a route in `app.routes.ts` that targets a specific page inside a feature (e.g. `path: 'feature1/page'`)**
  - Consequence: shell now depends on the feature's internal route structure
  - Instead: mount only `feature1` as a segment; the feature's own routes define `page` beneath it
- **Marking every top-level segment `preload: true` "to be safe"**
  - Consequence: degenerates into the equivalent of `PreloadAllModules`
  - Instead: mark only the small number of genuinely high-traffic segments

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Check list

- [ ] `apps/platform-shell` contains no HTTP calls, no business state, no feature-specific components
- [ ] Every entry in `app.routes.ts` is a single root segment with no nested path
- [ ] No component import from inside a feature appears in `app.routes.ts`
- [ ] `app.config.ts` registers `withPreloading(SelectivePreloadingStrategy)`
- [ ] Only a deliberately reviewed subset of top-level segments carry `data: { preload: true }`

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Unittest TestCases

- [ ] WHEN a route has `data: { preload: true }` THEN
  - [ ] `SelectivePreloadingStrategy.preload` invokes `load()`
- [ ] WHEN a route has no `preload` flag (or `false`) THEN
  - [ ] `SelectivePreloadingStrategy.preload` does not invoke `load()`, returning `of(null)` instead

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/solution-lazy-loading-routing.skill|solution-lazy-loading-routing]] - [[skills/angular/architecture/solutions/solution-lazy-loading-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]
