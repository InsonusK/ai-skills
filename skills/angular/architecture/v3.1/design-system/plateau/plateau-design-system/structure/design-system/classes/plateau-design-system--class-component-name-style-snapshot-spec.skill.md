---
name: plateau-design-system--class-component-name-style-snapshot-spec
description: Generic computed-CSS-property snapshot spec for a ds-* component at spec/{component}.style-snapshot.spec.ts — run paired with the visual spec so a failing pixel diff can be explained, not just observed — design-system plateau
domain: skill
type: template
whenToUse: when writing or reviewing a ds-* component's spec/{component}.style-snapshot.spec.ts, or before running --update-snapshots on a failing visual baseline
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

> `projects/design-system/src/lib/{component}/spec/{component}.style-snapshot.spec.ts`. Navigates to the same preview page as `.visual.spec.ts`, reads a computed-style snapshot via the shared [[skills/angular/architecture/v3.1/design-system/plateau/plateau-design-system/structure/design-system/classes/plateau-design-system--class-read-visual-style-properties.skill.md|read-visual-style-properties]] helper. Text baselines commit to `spec/snapshot/`.

# Goal

- Turn a failing pixel screenshot into a readable list of which CSS properties changed, and from what to what — checked *before* accepting a new visual baseline

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create.md|Testing/{component-name}.style-snapshot.spec.ts.create]]

# Implementation

```typescript
import { test, expect } from '@playwright/test';
import { readVisualStyleProperties } from '../../../../testing/read-visual-style-properties';

test.describe('DsButtonComponent — style snapshot', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`computed style matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/button');
      const styles = await readVisualStyleProperties(
        page.locator('[data-preview="default"] [data-testid="ds-button"]'),
      );
      expect(JSON.stringify(styles, null, 2)).toMatchSnapshot(`button-default-${scheme}.styles.txt`);
    });
  }
});
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create.md|Testing/{component-name}.style-snapshot.spec.ts.create]]

# Rules

## MUST
- The file is `spec/{component-name}.style-snapshot.spec.ts`; text baselines (`*.styles.txt`) commit to `spec/snapshot/`.
- It covers exactly the same states and colour schemes as the component's `.visual.spec.ts` — the two specs stay paired.
- Group tests under `test.describe('<component-name> — style snapshot', ...)`.
- Read properties only through the shared `readVisualStyleProperties` helper — never a component-specific ad hoc list.
- Before `--update-snapshots` on a failing visual baseline, inspect this diff first: an empty diff = rendering noise (safe); a non-empty diff names the property change to confirm intentional.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create.md|Testing/{component-name}.style-snapshot.spec.ts.create]]


- **A component-specific property list instead of the shared helper**
  - Consequence: snapshots become incomparable across components
  - Instead: extend the one shared `VISUAL_STYLE_PROPERTIES` list if a real gap is found

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create.md|Testing/{component-name}.style-snapshot.spec.ts.create]]

# Check list

- [ ] The file is at `spec/{component-name}.style-snapshot.spec.ts`; text baselines under `spec/snapshot/`
- [ ] Every `.visual.spec.ts` state has a paired style-snapshot covering the identical states/schemes
- [ ] Properties read only through the shared helper
- [ ] Tests grouped under `test.describe('<component-name> — style snapshot', ...)`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create.md|Testing/{component-name}.style-snapshot.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN a CSS change alters a component's resolved `color`/`backgroundColor` THEN the style-snapshot fails with a diff naming the old and new value
- [ ] WHEN a pixel screenshot fails on anti-aliasing only THEN the paired style-snapshot still passes, signalling rendering noise
- [ ] WHEN a `light-dark()` dark branch stops applying THEN the dark-scheme style-snapshot fails, naming the properties

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/{component-name}.style-snapshot.spec.ts.create.md|Testing/{component-name}.style-snapshot.spec.ts.create]]
