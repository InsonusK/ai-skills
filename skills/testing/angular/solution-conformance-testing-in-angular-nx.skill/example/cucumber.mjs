// One profile for the whole workspace. The unit kind runs it once per project and names the
// project's root in NX_TEST_PROJECT_ROOT; run by hand (`npx cucumber-js`) it takes every project.
import { resolve } from 'node:path';

// Step files are loaded through tsx; it resolves the workspace path aliases (@org/...) from here.
process.env['TSX_TSCONFIG_PATH'] ??= resolve('tsconfig.base.json');
const root = process.env['NX_TEST_PROJECT_ROOT'];
export default {
  paths: root
    ? [`${root}/src/**/features/**/*.feature`]
    : ['apps/**/features/**/*.feature', 'libs/**/features/**/*.feature'],
  requireModule: ['tsx/cjs'],
  require: ['apps/**/test/**/*.steps.ts', 'libs/**/test/**/*.steps.ts'],
  tags: 'not @status/todo and not @status/broken',
};
