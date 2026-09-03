---
description: Generic pattern for a Playwright screenshot-regression spec under spec/, targeting a component's demo/preview page in both light and dark color schemes
project_name: "{demo-or-preview-app}"
name: "spec/{component-name}.visual"
element_kind: component
change_kind: create
tags:
  - solution/ui-testing
  - element/component-name-visual-spec-ts
---

# How this generic file is used
Created at `spec/{component-name}.visual.spec.ts` next to the component implementation. Applies identically in both catalogs (`solution-ui-testing` monolith / `solution-design-system-ui-testing` design system): navigate to the component's existing demo/preview page (design system: `projects/demo`, per [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/Implementation/demo.project.extend.md]]; platform: `apps/component-preview`, per [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/PlatformComponents/component-preview.project.create]]) and assert a [visual regression](skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/glossary/visual-regression-testing.md) screenshot match. Per [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/adr/visual-regression-approach.md|visual-regression-approach]] — no Storybook, no Chromatic.

# Goals

- Catch layout, dark-mode, contrast, and CSS-specificity regressions that a behavioral component test (jsdom/happy-dom, no layout engine) cannot detect

# Implementation changes

```typescript
// File: projects/design-system/src/lib/ds-button/spec/ds-button.visual.spec.ts
import { test, expect } from '@playwright/test';

test.describe('DsButtonComponent — visual', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/ds-button/default');
      await expect(page).toHaveScreenshot(`ds-button-default-${scheme}.png`);
    });
  }

  test('matches baseline (disabled)', async ({ page }) => {
    await page.goto('/ds-button/disabled');
    await expect(page).toHaveScreenshot('ds-button-disabled.png');
  });
});
```

# Rule changes

## MUST
- The file is created at `spec/{component-name}.visual.spec.ts`.
  - Risk: spec files next to `component.ts` clutter the component root and get mixed with source in reviews.
  - Fix: every test file for the component lives under its `spec/` folder.
- Baseline PNGs are committed under `spec/snapshot/`, via `snapshotPathTemplate` in `playwright.config.ts`.
  - Risk: baselines left in Playwright's default `__snapshots__` folder scatter across the tree and are easy to miss in review.
  - Fix: set `snapshotPathTemplate` so `toHaveScreenshot()` writes to `spec/snapshot/`; commit the PNGs.
- A visual spec navigates directly to the component's stable demo/preview URL, never through unrelated UI.
  - Risk: reaching the component via app navigation makes the screenshot test fail for reasons outside the component.
  - Fix: `page.goto('/<component>/<state>')` against the preview app's stable route.
- Tests are grouped under a `test.describe('<component-name> — visual', ...)` block.
  - Risk: a flat file gives no grouping in the Playwright report.
  - Fix: one `test.describe` per component.
- A component with both a light and a dark rendering path is screenshotted in both `emulateMedia({ colorScheme })` states.
  - Risk: a broken `light-dark()` dark branch ships because only the light screenshot is checked.
  - Fix: loop `['light','dark']` and capture a baseline per scheme.
- Every meaningfully distinct state the demo page exposes (default, disabled, error, loading) has its own baseline.
  - Risk: a regression in the disabled or error rendering is invisible if only the default state is captured.
  - Fix: one `toHaveScreenshot` per state, each with its own baseline name.
- A baseline is updated (`--update-snapshots`) only as a reviewed, intentional part of a PR that changes the component's look.
  - Risk: `--update-snapshots` run to clear a red CI silently accepts a real visual regression as the new truth.
  - Fix: inspect the diff first; update only once the change is confirmed intentional, in the same PR.
- A visual spec never asserts against a page that also renders unrelated dynamic content (a live clock, randomly-ordered data).
  - Risk: non-deterministic content produces flaky pixel diffs that erode trust in the whole suite.
  - Fix: fixed example data on the preview page; disable or complete animations before capture.
## SHOULD
- **Updating a baseline screenshot without understanding why it changed** — Consequence: silently accepts a real visual regression as the new "correct" baseline — Instead: investigate the diff first; update the baseline only once the change is confirmed intentional
- **Screenshotting a page with non-deterministic content (timestamps, random ordering, animation mid-flight)** — Consequence: flaky CI failures unrelated to any real regression, eroding trust in the whole suite — Instead: use fixed example data in the demo/preview page, and disable/complete animations before capturing (Playwright's `toHaveScreenshot` has built-in animation-handling options)

# Check list

- [ ] The file is created at `spec/{component-name}.visual.spec.ts`, not next to `component.ts`
- [ ] Baseline PNGs are committed under `spec/snapshot/`
- [ ] Every component/state with a demo/preview page has at least one committed baseline screenshot
- [ ] Both light and dark color schemes are covered for any component with a dark-mode-aware rendering path
- [ ] No baseline was updated without a reviewed, intentional visual change in the same PR
- [ ] Visual specs group tests under a `test.describe('<component-name> — visual', ...)` block

# Unittest TestCases

- [ ] WHEN a component's CSS is changed such that its rendered layout shifts THEN
  - [ ] the visual spec fails with a pixel diff against the committed baseline
- [ ] WHEN a component's `light-dark()` dark-mode branch stops applying THEN
  - [ ] the dark-scheme screenshot fails against its baseline, even though the light-scheme screenshot still passes
