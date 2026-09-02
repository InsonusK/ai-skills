---
description: Extend the design system's existing projects/demo app to be the target for visual regression and accessibility specs, without changing why the demo app exists
project_name: demo
project_kind: application
element_kind: project
change_kind: extend
tags:
  - solution/design-system-ui-testing
  - element/design-system-repository
---

# How this file is used
This extends [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/Implementation/Repository.create|projects/demo]], already established as the design system's self-built component preview app (see [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/component-preview-tooling.md|component-preview-tooling ADR]]). No new project is created — `projects/demo` already fills the role `apps/component-preview` fills for the platform plateau; this file only adds the convention that every example page becomes a target for visual/a11y specs. The actual preview components now live next to each design-system component under `spec/preview/` and are imported by the demo app.

# Goals

- Reuse the demo app already built for manual visual review as the target for automated [visual regression](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/visual-regression-testing.md) and [accessibility](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/accessibility-testing.md) checks, per [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/visual-regression-approach.md|visual-regression-approach]] and [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/accessibility-testing-approach.md|accessibility-testing-approach]]
- Keep preview components in the component library's `spec/preview/` directory so they stay close to the component and its tests

# Structure

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| `projects/design-system/src/lib/{component-name}/spec/preview/{component-name}.preview.ts` | Preview component per design-system component, per [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]'s authoring convention. Every meaningfully distinct state a component supports (default, disabled, error, loading, etc.) gets its own route or clearly delineated section on the page. |
| `projects/demo/src/app/app.routes.ts` | Imports each preview component from the design-system library and registers one deep-linkable route per component/state. |

# Rules

## MUST
- Every component's demo page must cover, at minimum, the states the visual regression suite screenshots — a component's demo page and its visual-spec coverage must never drift apart.
- The demo app's routes/sections used as screenshot targets must be stable, deep-linkable URLs, so a Playwright spec can navigate directly to a specific component/state without simulating UI interaction first.
- Every component's preview component must live in `projects/design-system/src/lib/{component-name}/spec/preview/` and be imported by `projects/demo`, rather than being authored directly inside `projects/demo`.

- the demo app must never duplicate preview markup that already exists in the component library's `spec/preview/` directory.
## SHOULD
- **Adding a new example only to the demo page's visual markup without a corresponding route/anchor a Playwright spec can navigate to directly** — Consequence: the visual/a11y spec can't reliably target just that state, and ends up screenshotting the whole page or skipping the new state entirely — Instead: give every meaningfully distinct state its own stable, directly navigable route in the demo app, backed by the component's `spec/preview/` file
- **Authoring the preview component inside `projects/demo` instead of `projects/design-system/src/lib/{component-name}/spec/preview/`** — Consequence: the preview file drifts away from the component and its tests, and the design-system library no longer ships with its own test harness — Instead: keep the preview file in the component's `spec/preview/` directory and import it into the demo app

# Check list

- [ ] Every component's demo page state has a stable, directly navigable URL
- [ ] No component ships without at least one demo page state to screenshot and scan
- [ ] No preview component is authored directly inside `projects/demo` when it belongs in the component library's `spec/preview/`

# Unittest TestCases

- [ ] WHEN a new component/state is added to `projects/demo` THEN
  - [ ] a corresponding visual and accessibility spec is added in the same change, per [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create]] and [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create]]
