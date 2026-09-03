---
name: plateau-multiuser-monolith--class-component-name-visual-spec
description: Generic pattern for a Playwright screenshot-regression spec under spec/, targeting a component's demo/preview page in both light and dark color schemes — multiuser-monolith plateau
domain: skill
type: template
whenToUse: when creating or editing this class in the multiuser-monolith plateau, or another artifact that plays the same role
plateau: multiuser-monolith
artifact_type: spec
version: 20260903150000
tags:
  - skill/template/class
  - plateau/multiuser-monolith
  - stack/typescript
  - framework/angular
  - concern/architecture
created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]]"
---

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/solution-ui-testing.skill.md|solution-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create.md|{component-name}.visual.spec.ts.create]]


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
- The file must be created at `spec/{component-name}.visual.spec.ts` so all test files live under `spec/` and do not clutter the component directory root.
- Baseline PNG files must be committed under `spec/snapshot/` next to the spec. Configure `snapshotPathTemplate` in `playwright.config.ts` so that `toHaveScreenshot()` stores baselines in `spec/snapshot/` rather than the default `__snapshots__` folder.
- A visual spec must navigate directly to the component's stable demo/preview URL — it must never drive the UI through unrelated navigation to reach it.
- Tests in a visual spec must be grouped under a `test.describe('<component-name> — visual', () => { ... })` block.
- Every component that ships both a light and a dark rendering path must be screenshotted in both `page.emulateMedia({ colorScheme })` states.
- Every meaningfully distinct state the component's demo/preview page exposes (default, disabled, error, loading, etc.) must have its own baseline screenshot.
- A baseline screenshot must only be updated (`--update-snapshots`) as a deliberate, reviewed part of a PR that intentionally changes the component's appearance — never to silence an unexplained CI failure.

- a visual spec must never assert against a page that also renders unrelated, unrelated-to-the-test dynamic content (e.g. a live clock, randomly-ordered data) that would make the screenshot inherently flaky.
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
