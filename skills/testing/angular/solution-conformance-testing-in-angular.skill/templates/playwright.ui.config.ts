import { defineConfig } from '@playwright/test';
import { resolve } from 'node:path';

const kindDir = process.env['TEST_KIND_DIR'];
if (!kindDir) throw new Error('Run through make test-kind-ui');

export default defineConfig({
  testDir: './{SourceRoot}',
  testMatch: '**/test/*.ui.spec.ts',
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
    baseURL: '{BaseUrl}',
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
    command: '{ServeCommand}',
    url: '{BaseUrl}',
    reuseExistingServer: false,
    timeout: 120_000,
  },
});
