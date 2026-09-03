import { test, expect } from '@playwright/test';

test.describe('federation smoke test', () => {
  test('host mounts the remote at /reports and shares one session', async ({ page }) => {
    await page.goto('/reports');
    // Anonymous: the remote renders its own not-authenticated state (never its own login).
    await expect(page.getByRole('status')).toHaveText(/sign in on the platform/i);

    // Sign in on the HOST — the remote sees it through the shared SessionContract singleton.
    await page.getByRole('button', { name: /sign in/i }).click();
    await expect(page.getByRole('heading', { name: /reports for ada/i })).toBeVisible();
  });

  test('a missing remote degrades to a fallback, not a shell crash', async ({ page }) => {
    // (point the manifest at an unreachable URL for this run)
    await page.goto('/reports');
    await expect(page.getByRole('alert')).toBeVisible();
  });
});
