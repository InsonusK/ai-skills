import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('DsButtonComponent — accessibility', () => {
  test('has no automatically detectable violations (default)', async ({ page }) => {
    await page.goto('/button');
    const results = await new AxeBuilder({ page }).include('[data-preview="default"]').analyze();
    expect(results.violations).toEqual([]);
  });

  test('has no automatically detectable violations (disabled)', async ({ page }) => {
    await page.goto('/button');
    const results = await new AxeBuilder({ page }).include('[data-preview="disabled"]').analyze();
    expect(results.violations).toEqual([]);
  });
});
