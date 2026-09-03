---
description: Generic pattern for a Playwright end-to-end test in apps/platform-shell-e2e
project_name: platform-shell-e2e
name: "{scenario-name}"
element_kind: component
change_kind: create
tags:
  - solution/app-testing
  - element/scenario-name-e2e-spec-ts
---

# How this generic file is used
Applies to any Playwright spec added to `apps/platform-shell-e2e/src/spec`, exercising a real user scenario against the running application.

# Goals

- Verify a complete user-facing scenario through a real browser, across real routing (including lazy-loaded features and, where relevant, federated embeddable modules)

# Implementation changes

File: `apps/platform-shell-e2e/src/spec/{scenario-name}.e2e.spec.ts`

```typescript
import { test, expect } from '@playwright/test';

test.describe('Orders feature', () => {
  test('user can add an order from the orders feature', async ({ page }) => {
    await page.goto('/feature1');
    await page.getByRole('button', { name: /add order/i }).click();
    await page.getByLabel(/quantity/i).fill('2');
    await page.getByRole('button', { name: /submit/i }).click();

    await expect(page.getByText(/order added/i)).toBeVisible();
  });
});
```

# Rule changes

## MUST
- E2E scenarios navigate and interact exactly as a real user would — URLs, roles, labels — never by calling internal application APIs.
  - Risk: reaching into component or store internals makes the test pass while the actual user path is broken.
  - Fix: `page.goto(url)`, `getByRole`, `getByLabel`; assert on what the user sees.
- E2E tests are never a substitute for unit/component/integration coverage — only a small set of critical user-facing journeys.
  - Risk: fine-grained logic pushed into e2e turns a 30-second suite into a 20-minute flaky one that duplicates cheaper layers.
  - Fix: cover edge cases at the unit/component layer; keep e2e to the few journeys that must work end to end.
- E2E specs group related scenarios under a `test.describe('<feature-or-journey>', ...)` block.
  - Risk: a flat spec file gives no grouping in the report and no shared setup boundary.
  - Fix: one `test.describe` per feature or journey.

## SHOULD
- **Writing a large number of fine-grained e2e tests for logic already covered by unit/component tests** — Consequence: slow, expensive test suite that duplicates coverage already provided more cheaply at a lower layer — Instead: keep e2e tests focused on a small set of critical, cross-cutting user journeys; push detailed logic coverage down to unit/component/integration tests

# Check list

- [ ] E2E scenarios interact only through real user-facing navigation and controls
- [ ] The e2e suite stays focused on critical journeys, not exhaustive logic coverage already provided elsewhere
- [ ] E2E specs group related scenarios under a `test.describe('<feature-or-journey>', ...)` block

# Unittest TestCases

- [ ] WHEN a critical user journey (e.g. adding an order) is run in a real browser THEN
  - [ ] the scenario completes and the expected confirmation is visible to the user
