import { test, expect } from '@playwright/test';

// Playwright — runs via `npm run e2e:visual` (needs the component-preview app served).
test.describe('OrderFormComponent — visual', () => {
  for (const scheme of ['light', 'dark'] as const) {
    test(`matches baseline (${scheme})`, async ({ page }) => {
      await page.emulateMedia({ colorScheme: scheme });
      await page.goto('/order-form/idle');
      await expect(page).toHaveScreenshot(`order-form-idle-${scheme}.png`);
    });
  }
});
