---
name: plateau-design-system--class-read-visual-style-properties
description: Shared helper reading a fixed, curated set of visually meaningful computed CSS properties from a page element — one per plateau, imported by every spec/{component}.style-snapshot.spec.ts — design-system plateau
domain: skill
type: template
whenToUse: when creating projects/design-system/testing/read-visual-style-properties.ts, or extending the shared VISUAL_STYLE_PROPERTIES list
plateau: design-system
artifact_type: module
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

> `projects/design-system/testing/read-visual-style-properties.ts` — one shared helper for the whole plateau, imported by every `spec/{component}.style-snapshot.spec.ts`. The method, list, and ADR are `solution-ui-testing`'s; only the location differs.

# Goal

- Read `getComputedStyle()` values — resolved property values, never class names — restricted to one shared list of visually meaningful properties, kept in exactly one place

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create.md|Testing/read-visual-style-properties.ts.create]]

# Implementation

```typescript
import type { Locator } from '@playwright/test';

export const VISUAL_STYLE_PROPERTIES = [
  'color', 'backgroundColor', 'borderColor', 'borderWidth', 'borderRadius', 'boxShadow',
  'padding', 'margin', 'fontSize', 'fontWeight', 'lineHeight', 'opacity', 'display', 'transform',
] as const;

export type VisualStyleSnapshot = Record<(typeof VISUAL_STYLE_PROPERTIES)[number], string>;

export async function readVisualStyleProperties(locator: Locator): Promise<VisualStyleSnapshot> {
  return locator.evaluate((el, properties) => {
    const computed = getComputedStyle(el as Element);
    return Object.fromEntries(
      properties.map((p) => [p, computed[p as keyof CSSStyleDeclaration] as string]),
    ) as Record<string, string>;
  }, VISUAL_STYLE_PROPERTIES) as Promise<VisualStyleSnapshot>;
}
```

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create.md|Testing/read-visual-style-properties.ts.create]]

# Rules

## MUST
- `VISUAL_STYLE_PROPERTIES` stays a single shared list imported by every style-snapshot spec — never redefined per component.
- The helper lives in `projects/design-system/testing/`, never duplicated inside a component's `spec/` folder.
- It reads computed values via `getComputedStyle()`, never a class name — a class name can stay unchanged while the value it resolves to changes.

## SHOULD
- Keep this list identical to the monolith's `solution-ui-testing` list so a snapshot means the same thing in both catalogs — extend it only when a real regression class slips through.

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create.md|Testing/read-visual-style-properties.ts.create]]

# Check list

- [ ] The helper is in `projects/design-system/testing/`, not duplicated per component
- [ ] Every style-snapshot spec imports `VISUAL_STYLE_PROPERTIES` / `readVisualStyleProperties` from here
- [ ] No spec reads `className` in place of `getComputedStyle()`

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create.md|Testing/read-visual-style-properties.ts.create]]

# Unittest TestCases

- [ ] WHEN `readVisualStyleProperties` is called on an element THEN it returns exactly the properties in `VISUAL_STYLE_PROPERTIES`, resolved to computed values
- [ ] WHEN a `--ds-*` token changes an element's resolved colour without touching its class name THEN the returned `color`/`backgroundColor` reflects the new value

__Applied solutions:__
- [[skills/angular/architecture/v3.1/solutions/solution-design-system-ui-testing.skill/solution-design-system-ui-testing.skill.md|solution-design-system-ui-testing]] - [[skills/angular/architecture/v3.1/solutions/solution-ui-testing.skill/Implementation/Testing/read-visual-style-properties.ts.create.md|Testing/read-visual-style-properties.ts.create]]
