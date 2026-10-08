import { test, expect } from '@playwright/test';
test('the user increments the counter', async ({ page }) => {
 await page.goto('/');
 await page.getByRole('button', { name: 'Increment' }).click();
 await expect(page.getByRole('status', { name: 'Count' })).toHaveText('1');
});
test('the counter region matches the reviewed appearance', async ({ page }) => {
 await page.goto('/');
 await expect(page.locator('app-root')).toHaveScreenshot('counter.png');
});
