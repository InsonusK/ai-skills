import { defineConfig, devices } from '@playwright/test';

// Visual-regression + a11y suite for the plateau's UI components (not an @nx/playwright project —
// run directly). Serve the preview app first:  npx nx run component-preview:serve --port=4300
export default defineConfig({
  testDir: 'libs',
  testMatch: ['**/spec/*.{visual,style-snapshot,a11y}.spec.ts'],
  timeout: 30_000,
  workers: 1,
  reporter: 'line',
  snapshotPathTemplate: '{testDir}/{testFileDir}/snapshot/{arg}{ext}',
  use: {
    baseURL: process.env['BASE_URL'] || 'http://localhost:4300',
    launchOptions: { args: ['--no-sandbox', '--disable-dev-shm-usage', '--disable-gpu'] },
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
});
