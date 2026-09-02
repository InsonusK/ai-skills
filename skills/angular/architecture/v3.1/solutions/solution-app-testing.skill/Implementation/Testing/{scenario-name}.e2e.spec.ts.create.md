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
- E2E scenarios must navigate and interact exactly as a real user would (URLs, roles, labels) — not by calling internal application APIs directly.
- E2E tests must never be used as a substitute for unit/component/integration coverage — they are reserved for a small number of critical user-facing scenarios, given their higher cost per test (real browser, real navigation, generally slower than the layers below).
- E2E specs must group related scenarios under a `test.describe('<feature-or-journey>', () => { ... })` block.

## SHOULD
- **Writing a large number of fine-grained e2e tests for logic already covered by unit/component tests** — Consequence: slow, expensive test suite that duplicates coverage already provided more cheaply at a lower layer — Instead: keep e2e tests focused on a small set of critical, cross-cutting user journeys; push detailed logic coverage down to unit/component/integration tests

# Check list

- [ ] E2E scenarios interact only through real user-facing navigation and controls
- [ ] The e2e suite stays focused on critical journeys, not exhaustive logic coverage already provided elsewhere
- [ ] E2E specs group related scenarios under a `test.describe('<feature-or-journey>', ...)` block

# Unittest TestCases

- [ ] WHEN a critical user journey (e.g. adding an order) is run in a real browser THEN
  - [ ] the scenario completes and the expected confirmation is visible to the user
