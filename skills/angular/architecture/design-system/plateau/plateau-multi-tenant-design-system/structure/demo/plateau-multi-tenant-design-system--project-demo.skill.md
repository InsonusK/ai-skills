---
name: plateau-multi-tenant-design-system--project-demo
description: The unpublished Angular preview app — consumes the built design-system package, applies theme.scss/custom-tokens.scss/tenants.scss once at the root, mounts one deep-linkable route per component preview, and carries a tenant switcher that sets document.documentElement.dataset.tenant; the navigation target for every visual / style-snapshot / a11y spec including the per-tenant snapshots — multi-tenant-design-system plateau
domain: skill
type: template
whenToUse: when wiring the demo app's routes to a new component preview, applying the design-system theme / tenants at the root, editing the tenant switcher, or checking that a preview state has a stable deep-linkable URL
plateau: multi-tenant-design-system
project_kind: application
version: 20260903200000
tags:
  - skill/template/project
  - plateau/multi-tenant-design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]]"
---

> `projects/demo` is the design system's self-built component preview app (Storybook was rejected, see [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/component-preview-tooling.md|component-preview-tooling]]). `solution-design-system-ui-testing` adds nothing structural — it only makes every preview page a stable target for the visual / style-snapshot / a11y specs.

# Goal

- Give a human somewhere to review every shipped component, and give the Playwright specs a stable, deep-linkable target — one set of preview pages, not two

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]

# Core Principles

- The demo consumes `design-system` as a regular built dependency — it applies `theme.scss` + `custom-tokens.scss` + `tenants.scss` **once**, at the root (`styles.scss`), in that order (`tenants` after `theme`), and defines no palette of its own
- Every meaningfully distinct component state (default, disabled, error, ...) gets its own clearly delineated preview section with a stable, directly navigable URL
- Preview components are authored in the library, under `src/lib/{component}/spec/preview/`, and imported here — never re-authored inside `projects/demo`
- **VP1** — the demo carries a tenant `<select data-testid="tenant-select">` (options: `(base brand)` + every `DS_TENANTS` id) that sets / clears `document.documentElement.dataset.tenant`. This models the consuming-app responsibility; the design system ships no such control. Per-tenant style-snapshot specs drive it with `selectOption(...)`.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

# Structure

## Project Structure

```
/projects/demo
  /src
    styles.scss              <- @use 'design-system/styles/theme'; @use '.../custom-tokens'; @use '.../tenants'  (tenants LAST)
    /app
      app.ts                 <- shell: a nav + tenant <select> + <router-outlet />
      app.config.ts
      app.routes.ts          <- one lazy route per component preview (loadComponent from the lib's spec/preview/)
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| src/styles.scss | Applies the design system's theme + `--ds-*` tokens + tenant palettes at the root (`tenants` after `theme`). No palette of its own. | — |
| src/app/app.routes.ts | `{ path: '{component}', loadComponent: () => import('@ds-preview/{component}').then(m => m.Ds{Component}PreviewComponent) }` per component. | — |
| src/app/app.ts | Minimal shell — component nav + a tenant `<select data-testid="tenant-select">` (sets `document.documentElement.dataset.tenant`) + `<router-outlet />`. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

## What Does NOT Belong Here

- Preview markup for a component that already has a `spec/preview/` file in the library — import it, don't duplicate it
- A second `mat.theme()` or an app-specific palette — the design system owns the palettes
- Tenant *resolution* logic beyond the demo switcher (reading a real claim / API) — that is what a production consuming app does; the demo only exercises the mechanism
- Publication config — `projects/demo` is never published (`.changeset` `ignore`)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

# Rules

## MUST
- Every component's preview page covers, at minimum, the states its visual suite screenshots — the preview page and its visual-spec coverage never drift apart.
- Every screenshot-target route/section is a stable, deep-linkable URL — a Playwright spec navigates straight to it, without driving UI to reach it.
- Every preview component lives in `projects/design-system/src/lib/{component}/spec/preview/` and is imported here — never authored inside `projects/demo`.
- `theme.scss`, `custom-tokens.scss` and `tenants.scss` are applied exactly once, here, at the root — `tenants` after `theme` — never per-component, never a second time.
- `projects/demo` is never added to the publishable set.
- The tenant switcher only reads / writes `document.documentElement.dataset.tenant` — it never sets a `--mat-sys-*` / `--ds-*` value directly.

## SHOULD
- Give every meaningfully distinct state its own directly navigable route/anchor, backed by the component's `spec/preview/` file — a new example with no navigable target can't be reliably screenshotted.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

# Check list

- [ ] `styles.scss` applies `design-system/styles/theme` + `.../custom-tokens` + `.../tenants` once, at the root, `tenants` last
- [ ] Every component state with a preview page has a stable, directly navigable URL
- [ ] No preview component is authored inside `projects/demo`
- [ ] `projects/demo` is not in the publishable set
- [ ] The tenant switcher's options are `(base brand)` + every `DS_TENANTS` id, and it only sets/clears `data-tenant`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/solution-design-system-multi-tenant-theming.skill.md|solution-design-system-multi-tenant-theming]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-multi-tenant-theming.skill/Implementation/Repository.extend.md|Repository.extend]]

# Unittest TestCases

- [ ] WHEN a new component/state is added to a preview page THEN a corresponding visual and accessibility spec is added in the same change
- [ ] WHEN the demo is built THEN it resolves `design-system` to the built package and the theme applies at the root
- [ ] WHEN the tenant switcher is set to `globex` THEN `document.documentElement` gains `data-tenant="globex"` and a component using `--mat-sys-primary` re-renders with the globex colour
- [ ] WHEN the tenant switcher is set back to `(base brand)` THEN `data-tenant` is removed and the base palette applies

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]
