---
name: route-ownership-location
description: Where feature routes are defined, and how routing responsibility is split between the shell, embeddable modules, and features
problem: Should feature routes be centralized in the platform shell, or defined inside each feature/module and only mounted by the level above
decision: Routes are owned hierarchically — each level (shell, embeddable module, feature) only defines paths relative to its own root segment and cannot reach above it
---

# Problem

The routing tree spans three levels that were established by earlier solutions: the platform shell (`apps/platform-shell`), an optional embeddable module mounted via Native Federation (see the "Встраиваемость платформы" solution), and individual features (`libs/{feature}/feature`, see the "Структура репозитория" solution). We need to decide where the routes for a feature are defined — centrally in the shell, or inside the feature itself — and, given the multi-level structure, how routing responsibility is split across all three levels without letting a lower level reach outside its own boundary or a higher level reach into a lower level's internals.

# Selected variant

**Selected variant:** [[#Hierarchical route ownership inside each owning project]]

Each level defines routes strictly relative to its own root segment and exposes them through its public API (`index.ts`), the same boundary mechanism already used for code in the "Структура репозитория" solution. The platform shell knows only the root segment under which an embeddable module or a feature is mounted (e.g. `module1/`); an embeddable module knows only the root segments of the features it contains (e.g. `feature1/`, `feature-map/feature2`); a feature knows only paths relative to its own root (e.g. `page`). No level can define or override a path outside the segment it owns.

# Searched variants

## Hierarchical route ownership inside each owning project

### Description

Routes are defined at the same level as the code that serves them, and each level only ever describes paths *below* its own root segment:
- `apps/platform-shell` mounts each top-level segment (an embeddable module's root, or a directly-owned feature's root) via `loadChildren`, without knowing what lies beneath that segment.
- An embeddable module (see the platform-embeddability solution) mounts the root segments of the features it contains, the same way the shell mounts modules — it does not know what lies beneath a feature's own root.
- A feature (`libs/{feature}/feature`) defines only paths relative to its own root (e.g. a component at `page`, not `module1/feature1/page`), and exports its `Routes` array through `index.ts`.

### Benefits

- Mirrors the module-boundary principle from the "Структура репозитория" solution: a project only has authority over what is below it, never over what sits above or beside it
- A feature (or embeddable module) can be freely reorganized internally — renamed sub-pages, added sub-routes — without touching any project above it, keeping `nx affected` scoped correctly
- Composes cleanly with lazy loading (`loadChildren`) at every level, since each level already only exposes a self-contained routes array through its public API
- Composes cleanly with the platform-embeddability solution: an embeddable module is, from the shell's point of view, exactly the same kind of "root segment with routes below it" as a directly-owned feature — one mounting mechanism for both
- No level can accidentally shadow or break a path that isn't its own, since a project by construction cannot express a path above its own root

### Costs

- No single file shows the entire application's full URL sitemap at a glance — understanding the complete tree requires walking each level's routes
- Coordinating a shared prefix across several sibling features (if ever desired) requires an explicit convention at the mounting level, since no feature can reach outside its own root to claim a shared prefix

## Centralized routes in the platform shell

### Description

`apps/platform-shell` contains a single `app.routes.ts` listing every path in the application, pointing `loadComponent`/`loadChildren` directly at components living inside feature libs.

### Benefits

- The complete application sitemap is visible in one file
- Centralized control over route ordering, redirects, and wildcard handling

### Costs

- Directly violates the module-boundary principle from the "Структура репозитория" solution: the shell would need to know a feature's internal component structure, not just its public API
- Any internal navigation change inside a feature requires editing `apps/platform-shell`, which makes `nx affected` treat the shell as touched by almost every change — defeating the affected-based CI benefit that motivated choosing Nx in the first place
- Does not extend cleanly to the platform-embeddability solution, where an embeddable module's internals are, by design, not visible to the shell at build time at all
- Does not scale as the number of features grows — a single ever-growing routing file becomes a bottleneck and a merge-conflict hotspot across teams
