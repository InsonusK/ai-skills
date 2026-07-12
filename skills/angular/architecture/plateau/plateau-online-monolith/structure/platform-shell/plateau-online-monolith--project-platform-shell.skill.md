---
name: plateau-online-monolith--project-platform-shell
description: The single deployable Angular application — composition root, top-level root-relative routing, root providers. No preloading strategy yet, no global error handler yet. — online-monolith plateau
domain: skill
type: template
plateau: online-monolith
project_kind: application
version: 20260711180000
tags:
  - skill/template/project
  - plateau/online-monolith
created_by:
  - "[[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]]"
---

> `solution-app-testing` does not extend this project directly — its content lives in the sibling [[skills/angular/architecture/plateau/plateau-online-monolith/structure/platform-shell-e2e/plateau-online-monolith--project-platform-shell-e2e.skill.md|platform-shell-e2e]] project and in each feature's own test specs. Also depends on the `design-system` npm package (see the NPM Packages table below) — plain, non-federated consumption only.

# Goal

- Be the only deployable unit at this plateau: bootstrap the application, own top-level routing, register root providers
- Contain no business logic of its own — every feature lives under `libs/{feature}` and is only routed to from here
- Mount each directly-owned feature at a single root segment, without knowing what routes exist beneath that segment

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Structure

## Project Structure

```
/apps/platform-shell
  /src
    /app
      app.config.ts
      app.routes.ts
    main.ts
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| app.config.ts | Root provider registration. | — |
| app.routes.ts | Top-level `Routes` array — one `loadChildren` entry per directly-owned feature's root segment. | — |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

## NPM Packages

| Package | Version constraint | Purpose |
| ------- | ------------------- | ------- |
| design-system | latest compatible, per [[skills/angular/architecture/plateau/plateau-design-system/plateau-design-system.skill.md|design-system]] | The [[skills/angular/architecture/plateau/plateau-design-system/plateau-design-system.skill.md|design-system]] plateau's published component library. `theme.scss` is applied once at the application root. |

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]

# Rules

## MUST
- [[skills/angular/architecture/plateau/plateau-online-monolith/structure/plateau-online-monolith--repo-online-monolith.skill#MUST|repo-online-monolith]]

## MUST NOT
- [[skills/angular/architecture/plateau/plateau-online-monolith/structure/plateau-online-monolith--repo-online-monolith.skill#MUST NOT|repo-online-monolith]]

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Anti-patterns

- **Adding a route in `app.routes.ts` that targets a specific page inside a feature (e.g. `path: 'feature1/page'`)**
  - Consequence: shell now depends on the feature's internal route structure
  - Instead: mount only `feature1` as a segment; the feature's own routes define `page` beneath it

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-app-routing.skill/solution-app-routing.skill.md|solution-app-routing]] - [[skills/angular/architecture/solutions/solution-app-routing.skill/Implementation/PlatformHost/platform-shell.project.extend|PlatformHost/platform-shell.project.extend]]

# Check list

- [ ] `apps/platform-shell` contains no HTTP calls, no business state, no feature-specific components
- [ ] Every entry in `app.routes.ts` is a single root segment with no nested path
- [ ] `design-system`'s `theme.scss` is applied exactly once, at the application root

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]] - [[skills/angular/architecture/solutions/solution-repository-structure.skill/Implementation/Repository.create|Repository.create]]
