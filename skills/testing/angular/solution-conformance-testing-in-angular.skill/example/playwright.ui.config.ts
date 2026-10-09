import { defineConfig } from '@playwright/test';
import { resolve } from 'node:path';

const kindDir = process.env['TEST_KIND_DIR'];
if (!kindDir) throw new Error('Run through make test-kind-ui');
// The kind script picks a free loopback port for every run, so a server that is still
// shutting down from a previous run can never collide with this one.
const port = process.env['UI_TEST_PORT'];
if (!port) throw new Error('Run through make test-kind-ui');
const baseURL = `http://127.0.0.1:${port}`;

export default defineConfig({
  testDir: './src',
  testMatch: ['**/spec/*.ui.spec.ts', '**/spec/*.visual.spec.ts',
    '**/spec/*.style-snapshot.spec.ts', '**/spec/*.a11y.spec.ts'],
  outputDir: resolve(kindDir, 'report/ui/artifacts'),
  snapshotPathTemplate: '{testDir}/{testFileDir}/__screenshots__/{testFileName}/{arg}{ext}',
  updateSnapshots: 'none',
  forbidOnly: true,
  retries: 0,
  workers: 1,
  reporter: [
    ['json', { outputFile: resolve(kindDir, 'result/ui.native.json') }],
    ['html', { outputFolder: resolve(kindDir, 'report/ui/playwright'), open: 'never' }],
  ],
  use: {
    browserName: 'chromium',
    baseURL,
    viewport: { width: 1280, height: 720 },
    locale: 'en-US',
    timezoneId: 'UTC',
    colorScheme: 'light',
    reducedMotion: 'reduce',
    trace: 'retain-on-failure',
    screenshot: 'only-on-failure',
  },
  expect: { toHaveScreenshot: { animations: 'disabled' } },
  webServer: {
    command: `npm run start -- --host 127.0.0.1 --port ${port}`,
    url: baseURL,
    reuseExistingServer: false,
    timeout: 120_000,
  },
});
