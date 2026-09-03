---
name: plateau-async-monolith--project-component-preview
description: A minimal Nx application that renders platform components (feature components, forms, shared/ui) in isolation with static example inputs — the platform plateau's counterpart to the design system's projects/demo, and the target for visual/a11y specs — async-monolith plateau
domain: skill
type: template
whenToUse: when adding a component preview route to apps/component-preview, or wiring a visual/a11y spec target
plateau: async-monolith
project_kind: application
version: 20260902160000
tags:
  - skill/template/project
  - plateau/async-monolith
  - stack/typescript
  - framework/angular
  - concern/architecture
created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]"
---

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create.md|component-preview.project.create]]


# Goals

- Give the platform plateau a place to render a component with representative inputs/states, independent of routing, a real backend, or a real Signal Store — the same practical role `projects/demo` plays for the design system
- Be the target [visual regression](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/visual-regression-testing.md) Playwright screenshots and [accessibility](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/accessibility-testing.md) axe-core scans run against, per [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/visual-regression-approach.md|visual-regression-approach]] and [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/accessibility-testing-approach.md|accessibility-testing-approach]]

# Structure

## Project Structure

The preview app is a thin shell. The actual preview components live inside each component library under `spec/preview/` so they stay close to the component and its tests.

```text
/apps/component-preview
  /src
    /app
      app.routes.ts    <- imports preview components from libraries and registers routes

libs/{feature}/feature/src/lib/{feature}/{component-name}/
  spec/
    preview/
      {component-name}.preview.ts   <- renders the component in one or more states
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| `libs/{feature}/feature/src/lib/{feature}/{component-name}/spec/preview/{component-name}.preview.ts` | A small preview component that renders the real component with a fixed, representative set of inputs and mocked immediate dependencies (if any) — no live backend, no real routing context beyond the preview app. One route per meaningfully distinct state (e.g. `empty`, `loading`, `with-data`). |
| `apps/component-preview/src/app/app.routes.ts` | Imports each preview component from the corresponding library path and registers one deep-linkable route per state, e.g. `/orders-list/with-data`, `/orders-list/loading`. |

# NPM Packages

No new packages — this is a plain Nx `type:app` project using the workspace's existing Angular/Vitest/Playwright tooling.

# Rules

## MUST
- `apps/component-preview` must be tagged `type:preview`, `scope:platform`.
- A previewed component must be rendered with static, hardcoded example data — it must never call a real Facade, real HTTP endpoint, or real Signal Store wired to a backend.
- Every component covered by a visual regression or accessibility spec (per [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/visual-regression-approach.md|visual-regression-approach]]) must have its `spec/preview/{component-name}.preview.ts` registered as a route in this app.
- The preview app must import preview components from the component library's `spec/preview/` path; it must never duplicate preview markup inside the app.

- Never be included in the production deployment of `apps/platform-shell` — it is a development/CI-only harness, built and served separately.
- Never contain business logic, HTTP calls, or routing guards — only route registration and static rendering of preview components.
## SHOULD
- **Wiring a previewed component to the real Facade/Store "to keep the preview realistic"** — Consequence: reintroduces exactly the backend dependency this harness exists to avoid, making preview pages flaky and slow, and defeating their purpose as a stable screenshot target — Instead: always use static, hardcoded example data for every previewed state
- **Putting the preview component directly inside `apps/component-preview/src/app/` instead of `spec/preview/` in the component library** — Consequence: the preview file drifts away from the component and its tests, and the component library no longer ships with its own test harness — Instead: keep the preview file in the component's `spec/preview/` directory and import it from the preview app

# Check list

- [ ] `apps/component-preview` is tagged `type:preview`, `scope:platform`, and excluded from the production deploy
- [ ] Every visually/accessibility-tested component has a `spec/preview/{component-name}.preview.ts` registered as a route in this app
- [ ] No preview route calls a real backend or a real Facade/Store

# Unittest TestCases

- [ ] WHEN `apps/component-preview` is served THEN
  - [ ] every route renders without any network request being made
