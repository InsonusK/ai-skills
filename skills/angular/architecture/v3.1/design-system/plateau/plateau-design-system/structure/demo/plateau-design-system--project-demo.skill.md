---
name: plateau-design-system--project-demo
description: The unpublished Angular preview app — consumes the built design-system package, applies theme.scss/custom-tokens.scss once at the root, and mounts one deep-linkable route per component preview; the navigation target for every visual / style-snapshot / a11y spec — design-system plateau
domain: skill
type: template
whenToUse: when wiring the demo app's routes to a new component preview, applying the design-system theme at the root, or checking that a preview state has a stable deep-linkable URL
plateau: design-system
project_kind: application
version: 20260903170000
tags:
  - skill/template/project
  - plateau/design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]]"
---

> `projects/demo` is the design system's self-built component preview app (Storybook was rejected, see [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/component-preview-tooling.md|component-preview-tooling]]). `solution-design-system-ui-testing` adds nothing structural — it only makes every preview page a stable target for the visual / style-snapshot / a11y specs.

# Goal

- Give a human somewhere to review every shipped component, and give the Playwright specs a stable, deep-linkable target — one set of preview pages, not two

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]

# Core Principles

- The demo consumes `design-system` as a regular built dependency — it applies `theme.scss` + `custom-tokens.scss` **once**, at the root (`styles.scss`), and defines no palette of its own
- Every meaningfully distinct component state (default, disabled, error, ...) gets its own clearly delineated preview section with a stable, directly navigable URL
- Preview components are authored in the library, under `src/lib/{component}/spec/preview/`, and imported here — never re-authored inside `projects/demo`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

# Structure

## Project Structure

```
/projects/demo
  /src
    styles.scss              <- @use 'design-system/styles/theme'; @use 'design-system/styles/custom-tokens'
    /app
      app.ts                 <- shell: a nav + <router-outlet />
      app.config.ts
      app.routes.ts          <- one lazy route per component preview (loadComponent from the lib's spec/preview/)
```

## Directory and class skills

| `Directory\|file` | Description | Pattern skill |
| ------------------ | ----------- | -------------- |
| src/styles.scss | Applies the design system's theme + `--ds-*` tokens at the root. No palette of its own. | — |
| src/app/app.routes.ts | `{ path: '{component}', loadComponent: () => import('@ds-preview/{component}').then(m => m.Ds{Component}PreviewComponent) }` per component. | — |
| src/app/app.ts | Minimal shell — component nav + `<router-outlet />`. | — |

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

## What Does NOT Belong Here

- Preview markup for a component that already has a `spec/preview/` file in the library — import it, don't duplicate it
- A second `mat.theme()` or an app-specific palette — the design system owns the one palette
- Publication config — `projects/demo` is never published (`.changeset` `ignore`)

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

# Rules

## MUST
- Every component's preview page covers, at minimum, the states its visual suite screenshots — the preview page and its visual-spec coverage never drift apart.
- Every screenshot-target route/section is a stable, deep-linkable URL — a Playwright spec navigates straight to it, without driving UI to reach it.
- Every preview component lives in `projects/design-system/src/lib/{component}/spec/preview/` and is imported here — never authored inside `projects/demo`.
- `theme.scss` and `custom-tokens.scss` are applied exactly once, here, at the root — never per-component, never a second time.
- `projects/demo` is never added to the publishable set.

## SHOULD
- Give every meaningfully distinct state its own directly navigable route/anchor, backed by the component's `spec/preview/` file — a new example with no navigable target can't be reliably screenshotted.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

# Check list

- [ ] `styles.scss` applies `design-system/styles/theme` + `.../custom-tokens` once, at the root
- [ ] Every component state with a preview page has a stable, directly navigable URL
- [ ] No preview component is authored inside `projects/demo`
- [ ] `projects/demo` is not in the publishable set

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create.md|Repository.create]]
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]

# Unittest TestCases

- [ ] WHEN a new component/state is added to a preview page THEN a corresponding visual and accessibility spec is added in the same change
- [ ] WHEN the demo is built THEN it resolves `design-system` to the built package and the theme applies at the root

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|demo.project.extend]]
