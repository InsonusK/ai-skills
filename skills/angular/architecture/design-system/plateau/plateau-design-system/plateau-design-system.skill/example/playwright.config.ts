import { defineConfig, devices } from '@playwright/test';

/**
 * Visual / style-snapshot / a11y specs for ds-* components, run against the
 * built `projects/demo` app. Baselines live next to each spec under spec/snapshot/.
 *
 * NOTE: written and configured; the Playwright runner cannot fork workers in the
 * sandbox this example was built in, so baselines are generated where CI runs.
 */
export default defineConfig({
  testDir: './projects/design-system/src/lib',
  testMatch: /\.(visual|style-snapshot|a11y)\.spec\.ts$/,
  snapshotPathTemplate: '{testDir}/{testFileDir}/snapshot/{arg}{ext}',
  fullyParallel: true,
  forbidOnly: !!process.env['CI'],
  retries: process.env['CI'] ? 2 : 0,
  reporter: 'list',
  use: {
    baseURL: 'http://localhost:4300',
    trace: 'on-first-retry',
  },
  projects: [{ name: 'chromium', use: { ...devices['Desktop Chrome'] } }],
  webServer: {
    command: 'npx ng serve demo --port 4300',
    url: 'http://localhost:4300',
    reuseExistingServer: !process.env['CI'],
    timeout: 120_000,
  },
});
