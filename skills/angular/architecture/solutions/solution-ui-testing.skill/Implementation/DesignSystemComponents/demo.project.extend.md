---
description: Extend the design system's existing projects/demo app to be the target for visual regression and accessibility specs, without changing why the demo app exists
project_name: demo
project_kind: application
element_kind: project
change_kind: extend
---

# How this file is used
This extends [[skills/angular/architecture/solutions/solution-design-system-structure.skill/Implementation/Repository.create|projects/demo]], already established as the design system's self-built component preview app (see [[skills/angular/architecture/solutions/solution-design-system-structure.skill/adr/component-preview-tooling|component-preview-tooling ADR]]). No new project is created — `projects/demo` already fills the role `apps/component-preview` fills for the platform plateau; this file only adds the convention that every example page becomes a target for visual/a11y specs.

# Goals

- Reuse the demo app already built for manual visual review as the target for automated [visual regression](../../glossary/visual-regression-testing.md) and [accessibility](../../glossary/accessibility-testing.md) checks, per [[skills/angular/architecture/solutions/solution-ui-testing.skill/adr/visual-regression-approach]] and [[skills/angular/architecture/solutions/solution-ui-testing.skill/adr/accessibility-testing-approach]]

# Structure

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| `projects/demo/src/app/{component-name}/` | Existing example page per component, per [[skills/angular/architecture/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]'s SHOULD rule. Every meaningfully distinct state a component supports (default, disabled, error, loading, etc.) gets its own route or clearly delineated section on the page. |

# Rules

## MUST
- Every component's demo page MUST cover, at minimum, the states the visual regression suite screenshots — a component's demo page and its visual-spec coverage MUST NOT drift apart.
- The demo app's routes/sections used as screenshot targets MUST be stable, deep-linkable URLs, so a Playwright spec can navigate directly to a specific component/state without simulating UI interaction first.

# Anti-patterns

- **Adding a new example only to the demo page's visual markup without a corresponding route/anchor a Playwright spec can navigate to directly**
  - Consequence: the visual/a11y spec can't reliably target just that state, and ends up screenshotting the whole page or skipping the new state entirely
  - Instead: give every meaningfully distinct state its own stable, directly navigable location on the demo page

# Check list

- [ ] Every component's demo page state has a stable, directly navigable URL
- [ ] No component ships without at least one demo page state to screenshot and scan

# Unittest TestCases

- [ ] WHEN a new component/state is added to `projects/demo` THEN
  - [ ] a corresponding visual and accessibility spec is added in the same change, per [[skills/angular/architecture/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create]] and [[skills/angular/architecture/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create]]
