---
name: plateau-monitored-app--class-scenario-name-e2e-spec
description: Generic pattern for a Playwright end-to-end test in apps/platform-shell-e2e, exercising a real user scenario through a real browser — monitored-app plateau
domain: skill
type: template
plateau: monitored-app
artifact_type: test
version: 20260711220000
tags:
  - skill/template/class
  - plateau/monitored-app
created_by:
  - "[[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]]"
---

> Generic pattern, not tied to one concrete scenario — every Playwright spec added to `apps/platform-shell-e2e` follows this.

# Goal

- Verify a complete user-facing scenario through a real browser, across real routing (including lazy-loaded features)

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create|Testing/{scenario-name}.e2e.spec.ts.create]]

# Core Principles

- Apply ONE plateau template per class/artifact
- E2E tests are reserved for a small number of critical, cross-cutting user journeys, given their higher cost per test than the layers below

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create|Testing/{scenario-name}.e2e.spec.ts.create]]

# Naming convention

| use case | file name pattern | file name |
| -------- | -------------------- | --------- |
| E2E scenario spec | `{scenario-name}.e2e.spec.ts` | `add-order.e2e.spec.ts` |

# Implementation

```typescript
// Skill: class-scenario-name-e2e-spec
// Plateau: monitored-app
// Version: 20260711220000

import { test, expect } from '@playwright/test';

test('user can add an order from the orders feature', async ({ page }) => {
  await page.goto('/orders');
  await page.getByRole('button', { name: /add order/i }).click();
  await page.getByLabel(/quantity/i).fill('2');
  await page.getByRole('button', { name: /submit/i }).click();

  await expect(page.getByText(/order added/i)).toBeVisible();
});
```

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create|Testing/{scenario-name}.e2e.spec.ts.create]]

# Rules

## MUST
- E2E scenarios MUST navigate and interact exactly as a real user would (URLs, roles, labels) — not by calling internal application APIs directly.
- E2E tests MUST NOT be used as a substitute for unit/component/integration coverage.

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create|Testing/{scenario-name}.e2e.spec.ts.create]]

# Anti-patterns

- Apply SEVERAL plateau templates per class/artifact
- **Writing a large number of fine-grained e2e tests for logic already covered by unit/component tests**
  - Consequence: slow, expensive test suite that duplicates coverage already provided more cheaply at a lower layer
  - Instead: keep e2e tests focused on a small set of critical, cross-cutting user journeys

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create|Testing/{scenario-name}.e2e.spec.ts.create]]

# Check list

- [ ] E2E scenarios interact only through real user-facing navigation and controls
- [ ] The e2e suite stays focused on critical journeys, not exhaustive logic coverage already provided elsewhere

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create|Testing/{scenario-name}.e2e.spec.ts.create]]

# Unittest TestCases

- [ ] WHEN a critical user journey (e.g. adding an order) is run in a real browser THEN
  - [ ] the scenario completes and the expected confirmation is visible to the user

__Applied solutions:__
- [[skills/angular/architecture/solutions/solution-testing.skill/solution-testing.skill|solution-testing]] - [[skills/angular/architecture/solutions/solution-testing.skill/Implementation/Testing/{scenario-name}.e2e.spec.ts.create|Testing/{scenario-name}.e2e.spec.ts.create]]
