import { readFileSync, writeFileSync, existsSync, mkdirSync } from 'node:fs';
import { resolve } from 'node:path';

const [kind, runnerCode] = process.argv.slice(2);
if (!['components', 'ui'].includes(kind) || !process.env.TEST_KIND_DIR) {
  throw new Error('Expected components|ui and TEST_KIND_DIR');
}
const dir = process.env.TEST_KIND_DIR;
const report = resolve(dir, 'report', kind);
mkdirSync(report, { recursive: true });
const tests = [];
const errors = [];
let native;
try {
  native = JSON.parse(readFileSync(resolve(dir, 'result', `${kind}.native.json`), 'utf8'));
  if (kind === 'components') {
    for (const suite of native.testResults ?? []) {
      for (const test of suite.assertionResults ?? []) {
        tests.push({ name: test.fullName ?? test.title, status: test.status,
          errors: test.failureMessages ?? [] });
      }
      if (suite.status === 'failed' && !suite.assertionResults?.length) {
        errors.push(suite.message || `Suite failed: ${suite.name}`);
      }
    }
    if (native.success !== true) errors.push('Vitest reported an unsuccessful run');
    if (native.numRuntimeErrorTestSuites > 0) errors.push('Vitest runtime errors');
  } else {
    const visit = (suite, parents = []) => {
      const path = [...parents, suite.title].filter(Boolean);
      for (const spec of suite.specs ?? []) {
        for (const test of spec.tests ?? []) {
          const result = test.results?.at(-1);
          tests.push({ name: [...path, spec.title, test.projectName].filter(Boolean).join(' / '),
            status: test.status === 'expected' && result?.status === 'passed' ? 'passed' :
              result?.status === 'skipped' ? 'skipped' : 'failed',
            errors: (result?.errors ?? []).map(error => error.message ?? String(error)) });
        }
      }
      for (const child of suite.suites ?? []) visit(child, path);
    };
    for (const suite of native.suites ?? []) visit(suite);
    errors.push(...(native.errors ?? []).map(error => error.message ?? String(error)));
  }
} catch (error) {
  errors.push(`Native result unavailable: ${error.message}`);
}
if (Number(runnerCode) !== 0) errors.push(`Runner exit code: ${runnerCode}`);
if (!tests.length) errors.push('No tests executed');
const passed = tests.filter(test => test.status === 'passed').length;
const skipped = tests.filter(test => ['pending', 'todo', 'skipped', 'disabled'].includes(test.status)).length;
const failed = tests.length - passed - skipped;
const ok = errors.length === 0 && tests.length > 0 && passed === tests.length;
const summary = { total: tests.length, passed, failed, skipped, errors, tests };
writeFileSync(resolve(dir, 'result', `${kind}-test.json`), JSON.stringify(summary, null, 2));
writeFileSync(resolve(dir, 'badges', `${kind}.json`), JSON.stringify({ schemaVersion: 1,
  label: kind, message: tests.length ? `${passed}/${tests.length} passed${errors.length ? '; runner failed' : ''}` : 'no tests / runner failed',
  color: ok ? 'brightgreen' : 'red' }));
const escape = value => String(value ?? '').replace(/[&<>"']/g, character =>
  ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' })[character]);
const links = [['runner.log', 'Runner log'], ['playwright/index.html', 'Playwright report'],
  ['coverage/index.html', 'Component coverage']].filter(([file]) => existsSync(resolve(report, file)))
  .map(([file, title]) => `<li><a href="${file}">${title}</a></li>`).join('');
writeFileSync(resolve(report, 'index.html'), `<!doctype html><html lang="en"><meta charset="utf-8">
<title>${kind}</title><h1>${kind}: ${passed}/${tests.length} passed</h1>
<p>Failed: ${failed}; skipped: ${skipped}; runner errors: ${errors.length}</p><ul>${links}</ul>
<pre>${escape(errors.join('\n'))}</pre><table><thead><tr><th>Test</th><th>Status</th><th>Failure</th></tr></thead><tbody>
${tests.map(test => `<tr><td>${escape(test.name)}</td><td>${escape(test.status)}</td><td><pre>${escape(test.errors.join('\n'))}</pre></td></tr>`).join('')}
</tbody></table></html>`);
process.exitCode = ok ? 0 : 1;
