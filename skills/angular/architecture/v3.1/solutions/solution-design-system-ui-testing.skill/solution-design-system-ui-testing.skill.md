---
name: solution-design-system-ui-testing
description: The same three-layer UI component testing discipline as solution-ui-testing — behavioral, visual regression, accessibility, plus a paired computed-style snapshot — applied to the design system's ds-* components against projects/demo
domain: skill
type: architecture
version: 20260902000000
tags:
  - skill/architecture/solution
  - stack/typescript
  - concern/testing
  - ui
  - accessibility
  - visual-regression
  - framework/angular
  - concern/architecture
  - solution/design-system-ui-testing

whenToUse: when authoring or reviewing tests for a design-system ds-* component (behavioral, visual, style-snapshot, a11y), or setting up projects/demo as the visual/a11y target
creates: []
extends:
  - projects/design-system (component spec files under spec/)
  - projects/demo (visual/a11y spec target via spec/preview/)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]"
adr: []
---

# Goal
- Apply `solution-ui-testing`'s exact approach — behavioral (Testing Library) + visual (Playwright screenshots) + accessibility (`@axe-core/playwright`) + a paired computed-style snapshot — to the design system's `ds-*` components.
- Use `projects/demo` (already established by `solution-design-system-structure`) as the visual/a11y target, rather than a new harness.
- Change nothing about the testing method — only where the preview pages physically live.

# Capabilities
- Every `ds-*` component ships the same four test layers a monolith feature component does.
- `projects/demo` doubles as the human-review preview and the machine visual/a11y target — one set of preview pages.

# Core Principle
- The testing method, tool ADRs, spec patterns, and rules are `solution-ui-testing`'s — this solution does not restate or re-argue them.
- The **only** difference from the monolith side: the project prefix is `projects/design-system/src/lib/{component}/` and the preview app is `projects/demo`, not `apps/component-preview`.
- What gets tested here is the `ds-*` authoring convention from `solution-design-system-components` — signal-based API, full Material encapsulation — at all three layers.
- No Storybook, no Chromatic — the visual-regression ADR builds directly on `solution-design-system-structure`'s `component-preview-tooling` decision (the demo app, not Storybook).

# Boundaries
- The **design-system** side. The monolith side is [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]].
- design-system `ComponentTesting` (common). Assumes `solution-design-system-structure` (the `projects/demo` app) + `solution-design-system-components` (the `ds-*` convention under test) + `solution-app-testing` (Vitest/Playwright tool ADRs).
- Adds no new ADR, no new spec pattern, no new tool — it is the design-system application of an existing solution.

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]
  - provides the three ADRs, the four spec patterns, `read-visual-style-properties.ts`, and every rule — reused verbatim
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/solution-design-system-structure.skill.md|solution-design-system-structure]]
  - `projects/demo`, already established there, is reused as the visual/a11y target ([[skills/angular/architecture/v3.1/solutions/solution-design-system-structure.skill/adr/component-preview-tooling.md|component-preview-tooling]])
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-components.skill/solution-design-system-components.skill.md|solution-design-system-components]]
  - the `ds-*` authoring convention this tests
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]
  - the Vitest/Playwright tool ADRs

# Template Skill Mutations

PROJECT:
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md|projects/demo]] - extend - now also the visual/a11y target, one preview section per `ds-*` component state

Artifact-level (generic patterns): reused from `solution-ui-testing` — `{component-name}.component.spec.ts` / `.visual.spec.ts` / `.style-snapshot.spec.ts` / `.a11y.spec.ts` and `read-visual-style-properties.ts`, applied under `projects/design-system/src/lib/{component}/spec/`.

# Directory layout

Identical to `solution-ui-testing`'s, with the design-system project prefix:

```text
projects/design-system/src/lib/{component-name}/
  ds-{component-name}.component.ts / .html / .scss
  spec/
    {component-name}.component.spec.ts
    {component-name}.visual.spec.ts
    {component-name}.style-snapshot.spec.ts
    {component-name}.a11y.spec.ts
    snapshot/   (committed baselines)
    preview/    ({component-name}.preview.ts — consumed by projects/demo)
```

The shared `readVisualStyleProperties` helper lives in `projects/design-system/testing/`.

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md#MUST|demo.project.extend]]
- Apply `solution-ui-testing`'s rules unchanged — the four spec layers ship together, no business-layer mocks, no baseline update without checking the style-snapshot diff.
  - Risk: the design system's tests drift from the monolith's discipline and a `ds-*` regression slips through.
  - Fix: the method is `solution-ui-testing`'s; only the target path differs.

## SHOULD
- Avoid adding a design-system-specific property to `read-visual-style-properties`'s list — keep the shared list identical across both catalogs so a snapshot means the same thing.

# Check list
- [ ] Every `ds-*` component has the four spec layers under `projects/design-system/src/lib/{component}/spec/`.
- [ ] `projects/demo` has one preview section per `ds-*` component state, doubling as the visual/a11y target.
- [ ] The spec patterns, ADRs, and `read-visual-style-properties` list are `solution-ui-testing`'s, unchanged.
- [ ] No Storybook or Chromatic in the design-system repo's `package.json`.
