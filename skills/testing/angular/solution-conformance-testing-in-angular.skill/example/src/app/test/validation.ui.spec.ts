import { test, expect } from '@playwright/test';

test('normalizes the URL through the user form', async ({ page }) => {
  await page.goto('/');
  await page.getByLabel('URL', { exact: true }).fill('  HTTPS://EXAMPLE.COM/path  ');
  await page.getByRole('button', { name: 'Check URL' }).click();
  await expect(page.getByRole('status', { name: 'Validation result' })).toContainText('https://example.com/path');
});
test('shows a visible error for an unsupported scheme', async ({ page }) => {
  await page.goto('/');
  await page.getByLabel('URL', { exact: true }).fill('ftp://example.com');
  await page.getByRole('button', { name: 'Check URL' }).click();
  await expect(page.getByRole('alert')).toContainText('UNSUPPORTED_SCHEME');
});
test('submits from the keyboard and clears the previous error', async ({ page }) => {
  await page.goto('/');
  const input = page.getByLabel('URL', { exact: true });
  await input.fill('ftp://example.com');
  await input.press('Enter');
  await expect(page.getByRole('alert')).toBeVisible();
  await input.fill('https://example.com');
  await input.press('Enter');
  await expect(page.getByRole('status', { name: 'Validation result' })).toContainText('https://example.com');
  await expect(page.getByRole('alert')).toHaveCount(0);
});
test('matches the reviewed initial form appearance', async ({ page }) => {
  await page.goto('/');
  await page.evaluate(() => document.fonts.ready);
  await expect(page.getByRole('main')).toHaveScreenshot('linkcheck-form.png');
});
