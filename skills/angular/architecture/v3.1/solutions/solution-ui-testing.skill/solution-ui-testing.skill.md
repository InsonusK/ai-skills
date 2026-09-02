---
name: solution-ui-testing
description: Three-layer UI component testing in the monolith — behavioral (Testing Library), visual regression (Playwright screenshots), and accessibility (@axe-core/playwright), each visual spec paired with a computed-style snapshot, run against a minimal apps/component-preview harness, without Storybook or Chromatic
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
  - solution/ui-testing

whenToUse: when writing a monolith UI component (feature or form component), deciding whether its test needs business-layer mocking, or reviewing whether a visual or accessibility regression could have been caught automatically
creates:
  - apps/component-preview
extends:
  - libs/{feature}/feature (component spec files under spec/)
  - libs/shared/ui (component spec files under spec/)
depends_on:
  - "[[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]]"
adr:
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/visual-regression-approach.md|visual-regression-approach]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/accessibility-testing-approach.md|accessibility-testing-approach]]"
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/style-snapshot-approach.md|style-snapshot-approach]]"
---

# Goal
- Test a monolith UI component (feature component or form component, per `solution-forms`) purely on its own `input()`/`output()`/`model()` surface — independent of whatever business logic sits around it.
- Cover the visual and accessibility regressions a behavioral test structurally cannot detect (no layout engine in jsdom/happy-dom, no WCAG rule evaluation in a DOM-structure assertion).
- Reuse the already-adopted tools (Vitest, Playwright, Testing Library) — no new tool.

# Capabilities
- Fast, ESM-native [behavioral component tests](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/behavioral-component-testing.md) via Vitest + Testing Library, no business-layer mocks.
- Automated [visual regression](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/visual-regression-testing.md) coverage (Playwright screenshots, light + dark) against `apps/component-preview`.
- [Computed-style snapshot](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/style-snapshot-testing.md) coverage paired with every visual spec — a failing pixel diff becomes a named property/value change.
- Automated [accessibility](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/accessibility-testing.md) regression coverage (`@axe-core/playwright`) for the same components.

# Core Principle
- A UI component is tested independently of the business logic around it — a signal/input in, a rendered result/event out — the test never needs a Facade, Client, or backend.
- Two independent layers, neither substitutes for the other: **behavioral** (Testing Library) catches what a test author anticipated; **visual + accessibility** (Playwright + axe-core) catches what nobody anticipated.
- Every `.visual.spec.ts` ships with a paired `.style-snapshot.spec.ts` (per the style-snapshot ADR); a failing visual baseline is never updated without checking the paired style-snapshot diff first.
- Checks run against `apps/component-preview` — a minimal harness this solution introduces — never Storybook, never Chromatic.

# Boundaries
- The **monolith** side. The identical approach applied to the design system's `ds-*` components against `projects/demo` is [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]], which reuses this solution's three ADRs and spec patterns.
- monolith `ComponentTesting` (flagged common — feature-model open question). Assumes the `solution-repository-structure` baseline + `solution-app-testing` (Vitest/Playwright tool ADRs).
- Business-layer testing (Client/Facade/Store, `HttpTestingController`, MSW) is `solution-app-testing`, not this — a component test never needs it.

# Adr
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/visual-regression-approach.md|visual-regression-approach]] — Playwright `toHaveScreenshot()` against a preview page, over Storybook + Chromatic.
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/accessibility-testing-approach.md|accessibility-testing-approach]] — `@axe-core/playwright` reusing the same Playwright pages.
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/style-snapshot-approach.md|style-snapshot-approach]] — a fixed `getComputedStyle()` property list snapshotted per state, pairing each pixel screenshot with an explanation.

# Requirements

SOLUTION:
- [[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/solution-app-testing.skill.md|solution-app-testing]]
  - reuses its Vitest ([[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/adr/test-runner-choice.md|test-runner-choice]]) and Playwright ([[skills/angular/architecture/v3.1/solutions/solution-app-testing.skill/adr/e2e-framework-choice.md|e2e-framework-choice]]) ADRs without re-arguing them
- [[skills/angular/architecture/v3.1/solutions/solution-repository-structure.skill/solution-repository-structure.skill.md|solution-repository-structure]]
  - the base Nx workspace `apps/component-preview` is added to
- [[skills/angular/architecture/v3.1/solutions/solution-forms.skill/solution-forms.skill.md|solution-forms]]
  - a form component is the clearest case tested purely at the UI level — its Signal Forms state is entirely internal

NPM:
- `@testing-library/angular`, `@testing-library/user-event` — DOM-level component testing.
- `@axe-core/playwright` — accessibility scanning inside a Playwright page.

# Template Skill Mutations

PROJECT:
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create.md|apps/component-preview]] - create - a minimal harness app rendering components in isolation

Artifact-level (generic patterns):
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md|{component-name}.component.spec.ts]] - create - behavioral test via Testing Library
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create.md|{component-name}.visual.spec.ts]] - create - Playwright screenshot-regression spec
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create.md|{component-name}.style-snapshot.spec.ts]] - create - computed-style snapshot, paired with the visual spec
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create.md|read-visual-style-properties.ts]] - create - shared helper + curated property list
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create.md|{component-name}.a11y.spec.ts]] - create - `@axe-core/playwright` scan

# Directory layout

All test files live under a `spec/` directory next to the component:

```text
libs/{feature}/feature/src/lib/{feature}/{component-name}/
  {component-name}.component.ts / .html / .scss
  spec/
    {component-name}.component.spec.ts
    {component-name}.visual.spec.ts
    {component-name}.style-snapshot.spec.ts
    {component-name}.a11y.spec.ts
    snapshot/   (committed baselines: *.png, *.styles.txt)
    preview/    ({component-name}.preview.ts — consumed by apps/component-preview)
```

The shared `readVisualStyleProperties` helper lives in `libs/{feature}/feature/testing/`, not duplicated per component.

# Workflow

## Adding a UI component, fully tested (happy path)

1. Build the component (per `solution-forms` for a form component) with a signal-based API.
2. `spec/{component-name}.component.spec.ts` (Testing Library) — rendered behavior per distinct state.
3. `spec/preview/{component-name}.preview.ts`, registered in `apps/component-preview`, one section per state.
4. `spec/{component-name}.visual.spec.ts` — screenshot each state, both color schemes.
5. Paired `spec/{component-name}.style-snapshot.spec.ts` via `read-visual-style-properties`.
6. `spec/{component-name}.a11y.spec.ts` — axe scan of the same states.

## A visual regression slips past behavioral tests (the gap this closes)

1. A CSS change breaks a component's dark-mode branch.
2. The behavioral spec (jsdom, no paint engine) still passes.
3. The dark-scheme `.visual.spec.ts` screenshot fails against its baseline.
4. The paired `.style-snapshot.spec.ts` names which properties (`color`, `backgroundColor`) no longer resolve to their dark values — confirming a real regression.

# Rules

## MUST
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.component.spec.ts.create.md#MUST|{component-name}.component.spec.ts]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create.md#MUST|{component-name}.visual.spec.ts]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create.md#MUST|{component-name}.style-snapshot.spec.ts]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create.md#MUST|read-visual-style-properties.ts]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.a11y.spec.ts.create.md#MUST|{component-name}.a11y.spec.ts]]
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create.md#MUST|apps/component-preview]]
- Never update a failing visual baseline without first checking the paired style-snapshot diff.
  - Risk: a real regression is baked into the baseline as "expected".
  - Fix: empty style-snapshot diff → rendering noise, safe; a named property change → confirm intentional first.
- Never wire a previewed component to a real Facade/Store/backend "to keep it realistic".
  - Risk: the preview stops being deterministic and the visual spec becomes flaky.
  - Fix: the preview passes fixed inputs only.

## SHOULD
- Avoid asserting against `fixture.componentInstance` instead of the rendered DOM.
- Avoid disabling axe-core entirely instead of scoping an exception to one rule.
- Avoid reaching for `HttpTestingController` or a faked Facade/Client to test a component — that is `solution-app-testing`.

# Check list
- [ ] Every UI component has a behavioral, visual, style-snapshot, and a11y spec — the four ship together.
- [ ] No component test fakes anything beyond the component's own immediate injected dependency.
- [ ] Every preview state has a committed baseline screenshot, a committed style-snapshot, and a passing axe scan.
- [ ] No visual baseline was updated without checking its paired style-snapshot diff.
- [ ] No Storybook or Chromatic dependency in `package.json`.
