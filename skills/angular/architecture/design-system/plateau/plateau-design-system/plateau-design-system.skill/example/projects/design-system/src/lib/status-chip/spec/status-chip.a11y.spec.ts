import { test, expect } from '@playwright/test';
import AxeBuilder from '@axe-core/playwright';

test.describe('DsStatusChipComponent — accessibility', () => {
  test('has no automatically detectable violations', async ({ page }) => {
    await page.goto('/status-chip');
    const results = await new AxeBuilder({ page }).include('[data-preview="default"]').analyze();
    expect(results.violations).toEqual([]);
  });
});
