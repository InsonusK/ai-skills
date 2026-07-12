---
description: A minimal Nx application that renders platform components (feature components, forms, shared/ui) in isolation with static example inputs — the platform plateau's counterpart to the design system's projects/demo, and the target for visual/a11y specs
name: component-preview
project_kind: application
element_kind: project
change_kind: create
---

# Goals

- Give the platform plateau a place to render a component with representative inputs/states, independent of routing, a real backend, or a real Signal Store — the same practical role `projects/demo` plays for the design system
- Be the target Playwright screenshots and axe-core scans run against, per [[skills/angular/architecture/solutions/solution-ui-testing.skill/adr/visual-regression-approach]] and [[skills/angular/architecture/solutions/solution-ui-testing.skill/adr/accessibility-testing-approach]]

# Structure

## Project Structure

```
/apps/component-preview
  /src
    /app
      /{feature}
        {component-name}.preview.ts     <- one route per previewed component/state
    app.routes.ts
```

## Directory and file skills

| Directory/file | Description |
| --------------- | ----------- |
| `/{feature}/{component-name}.preview.ts` | Renders one component with a fixed, representative set of inputs/mocked dependencies (if any) — no live backend, no real routing context beyond this app's own. One route per meaningfully distinct state (e.g. `empty`, `loading`, `with-data`). |
| `app.routes.ts` | One top-level route per previewed component/state, e.g. `/orders-list/with-data`, `/orders-list/loading`. |

# NPM Packages

No new packages — this is a plain Nx `type:app` project using the workspace's existing Angular/Vitest/Playwright tooling.

# Rules

## MUST
- `apps/component-preview` MUST be tagged `type:preview`, `scope:platform`.
- A previewed component MUST be rendered with static, hardcoded example data — it MUST NOT call a real Facade, real HTTP endpoint, or real Signal Store wired to a backend.
- Every component covered by a visual regression or accessibility spec (per [[skills/angular/architecture/solutions/solution-ui-testing.skill/adr/visual-regression-approach]]) MUST have a corresponding route in this project.

## MUST NOT
- This project MUST NOT be included in the production deployment of `apps/platform-shell` — it is a development/CI-only harness, built and served separately.
- This project MUST NOT contain business logic, HTTP calls, or routing guards — only static rendering of components with example inputs.

# Anti-patterns

- **Wiring a previewed component to the real Facade/Store "to keep the preview realistic"**
  - Consequence: reintroduces exactly the backend dependency this harness exists to avoid, making preview pages flaky and slow, and defeating their purpose as a stable screenshot target
  - Instead: always use static, hardcoded example data for every previewed state

# Check list

- [ ] `apps/component-preview` is tagged `type:preview`, `scope:platform`, and excluded from the production deploy
- [ ] Every visually/accessibility-tested component has a corresponding preview route
- [ ] No preview route calls a real backend or a real Facade/Store

# Unittest TestCases

- [ ] WHEN `apps/component-preview` is served THEN
  - [ ] every route renders without any network request being made
