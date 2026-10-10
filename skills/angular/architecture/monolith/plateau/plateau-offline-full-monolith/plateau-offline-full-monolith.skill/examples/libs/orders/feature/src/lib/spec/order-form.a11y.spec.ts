import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test('OrderFormComponent — no automatically detectable accessibility violations', async ({ page }) => {
  await page.goto('/order-form/idle');
  const results = await new AxeBuilder({ page }).analyze();
  expect(results.violations).toEqual([]);
});
