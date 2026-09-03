---
name: plateau-design-system--class-component-name-visual-spec
description: Generic Playwright screenshot-regression spec for a ds-* component at spec/{component}.visual.spec.ts, targeting its projects/demo preview page in light and dark schemes — design-system plateau
domain: skill
type: template
whenToUse: when writing or reviewing a ds-* component's spec/{component}.visual.spec.ts, or updating a committed baseline
plateau: design-system
artifact_type: component
version: 20260903170000
tags:
  - skill/template/class
  - plateau/design-system
  - stack/typescript
  - framework/angular
  - concern/architecture

created_by:
  - "[[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]]"
---

> `projects/design-system/src/lib/{component}/spec/{component}.visual.spec.ts`. Runs via `playwright test` against a served `projects/demo`. Baselines commit to `spec/snapshot/` (`snapshotPathTemplate` in `playwright.config.ts`). No Storybook, no Chromatic.

# Goal

- Catch layout, dark-mode, contrast, and CSS-specificity regressions that a jsdom behavioural test cannot detect

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create.md|Testing/{component-name}.visual.spec.ts.create]]

# Implementation

```typescript
import { test, expect } from '@playwright/test';

test.describe('DsButtonComponent — visual', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/button');
      await expect(page.locator('[data-preview="default"]')).toHaveScreenshot(`button-default-${scheme}.png`);
    });
  }

  test('matches baseline (disabled)', async ({ page }) => {
    await page.goto('/button');
    await expect(page.locator('[data-preview="disabled"]')).toHaveScreenshot('button-disabled.png');
  });
});
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create.md|Testing/{component-name}.visual.spec.ts.create]]

# Rules

## MUST
- The file is `spec/{component-name}.visual.spec.ts`; baselines commit to `spec/snapshot/`.
- Navigate directly to the component's stable preview URL — never drive UI to reach it.
- Group tests under `test.describe('<component-name> — visual', ...)`.
- Screenshot both `colorScheme` states for any component with a light and a dark rendering path.
- Every meaningfully distinct state has its own baseline.
- A baseline is updated (`--update-snapshots`) only as a reviewed, intentional appearance change — never to silence an unexplained CI failure; check the paired style-snapshot diff first.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create.md|Testing/{component-name}.visual.spec.ts.create]]


- **Updating a baseline without understanding why it changed**
  - Consequence: silently accepts a real regression as the new "correct" baseline
  - Instead: investigate the diff (and the paired style-snapshot) first

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create.md|Testing/{component-name}.visual.spec.ts.create]]

# Check list

- [ ] The file is at `spec/{component-name}.visual.spec.ts`; baselines under `spec/snapshot/`
- [ ] Both light and dark schemes covered for any dark-mode-aware component
- [ ] Every state with a preview page has a committed baseline
- [ ] Tests grouped under `test.describe('<component-name> — visual', ...)`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create.md|Testing/{component-name}.visual.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN a component's CSS shifts its layout THEN the visual spec fails against the committed baseline
- [ ] WHEN a component's `light-dark()` dark branch stops applying THEN the dark screenshot fails while the light one still passes

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.visual.spec.ts.create.md|Testing/{component-name}.visual.spec.ts.create]]
