---
name: plateau-async-monolith--project-platform-shell
description: The single deployable Angular application — composition root, top-level root-relative routing, root providers, a SelectivePreloadingStrategy, and error-level bundle budgets on the production build. No global error handler yet. — async-monolith plateau
domain: skill
type: template
whenToUse: when editing apps/platform-shell — app.config.ts, app.routes.ts, main.ts, project.json, the service worker — or checking the composition root holds no business logic and mounts each feature at one root segment
plateau: async-monolith
project_kind: application
version: 20260902160000
tags:
  - skill/template/project
  - plateau/async-monolith
  - stack/typescript
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-global-store.skill/solution-global-store.skill.md|solution-global-store]]"

> `solution-app-testing` does not extend this project directly — its content lives in the sibling [[skills/angular/architecture/v3.1/monolith/plateau/plateau-async-monolith/structure/platform-shell-e2e/plateau-async-monolith--project-platform-shell-e2e.skill.md|platform-shell-e2e]] project and in each feature's own test specs. Also depends on the `design-system` npm package (see the NPM Packages table below) — plain, non-federated consumption only.

# Goal

- Be the only deployable unit at this plateau: bootstrap the application, own top-level routing, register root providers
- Contain no business logic of its own — every feature lives under `libs/{feature}` and is only routed to from here
- Mount each directly-owned feature at a single root segment, without knowing what routes exist beneath that segment

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    /app
      /preloading
        selective-preloading.strategy.ts      <- new (VP1)
        selective-preloading.strategy.spec.ts
      app.config.ts
      app.routes.ts
    main.ts
  project.json                                 <- production build gains error-level `budgets` (VP1)
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| app.config.ts | Root provider registration. VP1: `provideRouter(appRoutes, withPreloading(SelectivePreloadingStrategy))`. | — |
| app.routes.ts | Top-level `Routes` array — one `loadChildren` entry per directly-owned feature's root segment. VP1: a reviewed subset carries `data: { preload: true }`. | — |
| preloading/selective-preloading.strategy.ts | Custom `PreloadingStrategy` — preloads a route's chunk only when `route.data['preload'] === true`. | [[skills/angular/architecture/v3.1/monolith/plateau/plateau-async-monolith/structure/platform-shell/classes/plateau-async-monolith--class-selective-preloading-strategy.skill.md\|class-selective-preloading-strategy]] |
| project.json | VP1: `build.configurations.production.budgets` — an `initial` and an `anyScript` budget, each with a `maximumError`. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| design-system | latest compatible, per [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/plateau-design-system.skill.md|design-system]] | The [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/plateau-design-system.skill/plateau-design-system.skill.md|design-system]] plateau's published component library. `theme.scss` is applied once at the application root. |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]

# Rules

## MUST
- [[skills/angular/architecture/v3.1/monolith/plateau/plateau-async-monolith/structure/plateau-async-monolith--repo-async-monolith.skill#must|repo-async-monolith]]

- [[skills/angular/architecture/v3.1/monolith/plateau/plateau-async-monolith/structure/plateau-async-monolith--repo-async-monolith.skill#must never|repo-async-monolith]]
- `app.config.ts` must register the router with `withPreloading(SelectivePreloadingStrategy)`.
- `data: { preload: true }` is set only here, on a top-level `loadChildren` entry — never passed down into or set by a feature's own routes.
- `project.json`'s production build must carry an `initial` and a per-script bundle budget, each with a `maximumError` (not only `maximumWarning`).
- Never mark a top-level segment `preload: true` without a deliberate review that it is high-traffic — preloading every segment degenerates into `PreloadAllModules` and prefetches remote chunks the shell never intended to warm up.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]


- **Adding a route in `app.routes.ts` that targets a specific page inside a feature (e.g. `path: 'feature1/page'`)**
  - Consequence: shell now depends on the feature's internal route structure
  - Instead: mount only `feature1` as a segment; the feature's own routes define `page` beneath it

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]

# Check list

- [ ] `apps/platform-shell` contains no HTTP calls, no business state, no feature-specific components
- [ ] Every entry in `app.routes.ts` is a single root segment with no nested path
- [ ] `design-system`'s `theme.scss` is applied exactly once, at the application root
- [ ] `app.config.ts` registers `withPreloading(SelectivePreloadingStrategy)`
- [ ] Only a deliberately reviewed subset of top-level segments carry `data: { preload: true }`
- [ ] The production build declares `initial` + `anyScript` budgets with `maximumError` set

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/solution-performance-tuned-routing.skill.md|solution-performance-tuned-routing]] - [[skills/angular/architecture/v3.1/solutions/solution-performance-tuned-routing.skill/Implementation/PlatformHost/platform-shell.project.extend.md|PlatformHost/platform-shell.project.extend]]
