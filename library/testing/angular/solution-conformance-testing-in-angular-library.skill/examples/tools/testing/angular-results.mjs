import { readFileSync, writeFileSync, existsSync, mkdirSync, readdirSync, copyFileSync } from 'node:fs';
import { resolve, relative, basename, dirname } from 'node:path';

const [kind, runnerCode] = process.argv.slice(2);
if (!['components', 'ui'].includes(kind) || !process.env.TEST_KIND_DIR) {
  throw new Error('Expected components|ui and TEST_KIND_DIR');
}
const dir = process.env.TEST_KIND_DIR;
const report = resolve(dir, 'report', kind);
mkdirSync(report, { recursive: true });
const tests = [];
const errors = [];
const ranFiles = new Set();   // spec files the browser run executed
const attachments = [];       // actual and diff images of a failed screenshot comparison
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
          ranFiles.add(basename(spec.file ?? ''));
          for (const attachment of result?.attachments ?? []) {
            if (attachment.path && /-(actual|diff)\.png$/.test(attachment.name ?? '')) attachments.push(attachment);
          }
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
const links = [['runner.log', 'Runner log'], ['playwright/index.html', 'Playwright report: steps, traces, failure screenshots'],
  ['coverage/index.html', 'Component coverage, line by line']].filter(([file]) => existsSync(resolve(report, file)))
  .map(([file, title]) => `<li><a href="${file}">${title}</a></li>`).join('');

// Component coverage in one line, when the runner wrote its summary beside the HTML report.
let coverage = '';
const coverageSummary = resolve(report, 'coverage/coverage-summary.json');
if (existsSync(coverageSummary)) {
  const total = JSON.parse(readFileSync(coverageSummary, 'utf8')).total;
  coverage = `<p class="note">Component coverage: lines ${total.lines.pct}%, branches ${total.branches.pct}%.</p>`;
}

// Reviewed screenshots: every committed baseline of the repository is shown, compared in
// this run or not, with the actual image and the difference beside it when the comparison failed.
let visual = '';
if (kind === 'ui') {
  const baselines = [];
  const walk = (folder) => {
    for (const entry of readdirSync(folder, { withFileTypes: true })) {
      const path = resolve(folder, entry.name);
      if (entry.isDirectory()) {
        if (!['node_modules', 'dist', 'tmp', 'out', '.nx', '.angular', '.git'].includes(entry.name)) walk(path);
      } else if (entry.name.endsWith('.png') && path.includes('/__screenshots__/')) baselines.push(path);
    }
  };
  walk(process.cwd());
  const target = resolve(report, 'visual');
  const cards = baselines.sort().map((path, index) => {
    mkdirSync(target, { recursive: true });
    const name = basename(path, '.png');
    const spec = basename(dirname(path));
    const copy = (from, suffix) => { const file = `${index}-${name}${suffix}.png`; copyFileSync(from, resolve(target, file)); return `visual/${file}`; };
    const found = (suffix) => attachments.find((attachment) => attachment.name === `${name}-${suffix}.png` && existsSync(attachment.path));
    const actual = found('actual'), diff = found('diff');
    const state = actual || diff ? ['differs', 'failed'] : ranFiles.has(spec) ? ['matches', 'passed'] : ['not compared in this run', 'idle'];
    const figure = (src, caption) => `<figure><a href="${src}"><img src="${src}" alt="${escape(caption)}"></a><figcaption>${escape(caption)}</figcaption></figure>`;
    return `<article><h3>${escape(name)} <span class="chip ${state[1]}">${state[0]}</span></h3>
<p class="note">${escape(spec)} - reference: ${escape(relative(process.cwd(), path))}</p><div class="shots">
${figure(copy(path, ''), 'reference (reviewed, committed)')}${actual ? figure(copy(actual.path, '-actual'), 'this run') : ''}${diff ? figure(copy(diff.path, '-diff'), 'difference') : ''}</div></article>`;
  }).join('');
  visual = `<h2>Screenshot comparisons</h2>${cards || '<p class="note">No reviewed screenshot in this repository.</p>'}`;
}

const style = `body{font:14px/1.5 system-ui,sans-serif;max-width:70rem;margin:2rem auto;padding:0 1rem;color:#1c2733}
h1{font-size:1.5rem;margin:0 0 .25rem}h2{font-size:1.125rem;margin:2rem 0 .5rem}h3{font-size:1rem;margin:1rem 0 .125rem}
.note{color:#5b6875;margin:.125rem 0}.chip{font-size:.75rem;padding:.125rem .5rem;border-radius:1rem;color:#fff;vertical-align:middle}
.passed{background:#2e7d32}.failed{background:#c62828}.idle,.skipped{background:#78838e}
table{border-collapse:collapse;width:100%}th,td{text-align:left;padding:.375rem .5rem;border-bottom:1px solid #dde3e9;vertical-align:top}
pre{white-space:pre-wrap;margin:0;font-size:.8125rem}.shots{display:flex;gap:1rem;flex-wrap:wrap}
figure{margin:0}figure img{max-width:22rem;border:1px solid #c5ced6;display:block}figcaption{color:#5b6875;font-size:.8125rem}`;
const chip = (status) => `<span class="chip ${status === 'passed' ? 'passed' : status === 'failed' ? 'failed' : 'skipped'}">${escape(status)}</span>`;
writeFileSync(resolve(report, 'index.html'), `<!doctype html><html lang="en"><head><meta charset="utf-8">
<title>${kind}</title><style>${style}</style></head><body>
<h1>${kind === 'ui' ? 'Browser UI tests' : 'Component tests'} <span class="chip ${ok ? 'passed' : 'failed'}">${passed}/${tests.length} passed</span></h1>
<p class="note">Failed: ${failed}; skipped: ${skipped}; runner errors: ${errors.length}</p>${coverage}
${errors.length ? `<h2>Runner errors</h2><pre>${escape(errors.join('\n'))}</pre>` : ''}
<h2>Evidence</h2><ul>${links}</ul>
<!-- projects -->
${visual}
<h2>Tests</h2><table><thead><tr><th>Test</th><th>Status</th><th>Failure</th></tr></thead><tbody>
${tests.map(test => `<tr><td>${escape(test.name)}</td><td>${chip(test.status)}</td><td><pre>${escape(test.errors.join('\n'))}</pre></td></tr>`).join('')}
</tbody></table></body></html>`);
process.exitCode = ok ? 0 : 1;
