// Shared living documentation: classic JSON and Messages use one renderer.
// Usage: node tools/livingdoc/render.mjs <cucumber-dir> <out-dir> [<legend.json> [<scenarios.json>]]
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import { generate } from 'multiple-cucumber-html-reporter';

const [inDir, outDir, legendFile, scenariosFile] = process.argv.slice(2);
const legend = legendFile ? JSON.parse(fs.readFileSync(legendFile, 'utf8')) : null;
const escapeHtml = (text) => String(text).replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
// One section per group of the legend - { groups: [{title, where, items: [{name, meaning,
// shownAs?}]}] }: names in one column, meanings aligned in the next. shownAs is the word
// this report uses for a status it has no name for.
const legendCss = '.tag-legend{max-width:60rem;margin:2rem auto;padding:0 1rem;text-align:left;font-size:.875rem;line-height:1.5}'
  + '.tag-legend h2{font-size:1.125rem;font-weight:700;margin:0 0 .5rem}'
  + '.tag-legend h3{font-size:.9375rem;font-weight:700;margin:1.25rem 0 .125rem}'
  + '.tag-legend p{margin:0 0 .5rem;opacity:.75}'
  + '.tag-legend dl{display:grid;grid-template-columns:max-content 1fr;gap:.25rem 1.25rem;margin:0}'
  + '.tag-legend dt{font-family:ui-monospace,SFMono-Regular,Menlo,Consolas,monospace;white-space:nowrap}'
  + '.tag-legend dd{margin:0}';
const legendHtml = legend
  ? `<section class="tag-legend"><style>${legendCss}</style><h2>Tags and statuses</h2>`
    + legend.groups.map((g) => `<h3>${escapeHtml(g.title)}</h3><p>${escapeHtml(g.where)}</p><dl>`
      + g.items.map((i) => `<dt>${escapeHtml(i.name)}</dt><dd>${escapeHtml(i.meaning)}`
        + `${i.shownAs ? ` - shown here as "${escapeHtml(i.shownAs)}"` : ''}</dd>`).join('')
      + '</dl>').join('')
    + '</section>'
  : '';

// Complete excluded/unwired scenarios and actual runner results with no published steps.
// Each fallback states why details are absent; its status comes from the real inventory.
const inventory = scenariosFile && fs.existsSync(scenariosFile)
  ? JSON.parse(fs.readFileSync(scenariosFile, 'utf8')).scenarios : [];
const stepStatus = { passed: 'passed', failed: 'failed', todo: 'pending', broken: 'skipped', 'not-run': 'undefined' };
const pathEnd = (uri) => uri.replace(/^(\.{1,2}\/)+/, '');
function completeInventory(features) {
  for (const entry of inventory) {
    let feature = features.find((f) => f.name === entry.feature && entry.uri.endsWith(pathEnd(f.uri ?? '')));
    if (!feature) {
      feature = { uri: entry.uri, id: entry.uri, keyword: 'Feature', name: entry.feature, line: 1, description: '',
        tags: entry.tags.filter((t) => t.startsWith('@type/')).map((name) => ({ name, line: 1 })), elements: [] };
      features.push(feature);
    }
    if (['passed', 'failed'].includes(entry.status)) {
      const escaped = entry.scenario.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
      const namePattern = new RegExp(`^${escaped.replace(/<[^>]+>/g, '.*?')}(?:$| - )`);
      const represented = feature.elements.some((element) => namePattern.test(element.name)
        && entry.tags.every((tag) => element.tags?.some((t) => t.name === tag)));
      if (represented) continue;
    }
    const reason = entry.status === 'not-run' ? 'no runner executed this scenario'
      : ['passed', 'failed'].includes(entry.status)
        ? `runner result: ${entry.status}; this runner did not publish step details`
        : (entry.note || 'no reason given');
    feature.elements.push({
      keyword: 'Scenario', type: 'scenario', id: `${entry.uri}:${entry.line}`, line: entry.line, description: '',
      name: entry.examples ? `${entry.scenario} - ${entry.examples}` : entry.scenario,
      tags: entry.tags.map((name) => ({ name, line: entry.line })),
      steps: [{ keyword: `${entry.status}: `, name: reason, line: entry.line, match: { location: '' },
        result: { status: stepStatus[entry.status], duration: 0 } }],
    });
  }
}
const files = fs.readdirSync(inDir).sort();
const ndjson = files.filter((f) => f.endsWith('.ndjson'));
const json = files.filter((f) => f.endsWith('.json'));
fs.mkdirSync(outDir, { recursive: true });

// Convert the executed test cases, including their actual step results. The inventory
// supplies excluded and unwired scenarios for both protocols, through completeInventory.
function messagesToClassic(file) {
  const envelopes = fs.readFileSync(file, 'utf8').split(/\r?\n/).filter((l) => l.trim()).map(JSON.parse);
  const index = (key) => new Map(envelopes.filter((e) => e[key]).map((e) => [e[key].id, e[key]]));
  const pickles = index('pickle');
  const cases = index('testCase');
  const started = index('testCaseStarted');
  const nodes = new Map();
  function walk(node) {
    if (!node || typeof node !== 'object') return;
    if (node.id && node.location) nodes.set(node.id, node);
    for (const value of Object.values(node)) {
      if (Array.isArray(value)) value.forEach(walk);
      else if (value && typeof value === 'object') walk(value);
    }
  }
  const features = envelopes.filter((e) => e.gherkinDocument?.feature).map((e) => {
    const doc = e.gherkinDocument;
    walk(doc.feature);
    return { uri: doc.uri, id: doc.uri, keyword: doc.feature.keyword, name: doc.feature.name,
      description: doc.feature.description, line: doc.feature.location.line,
      tags: doc.feature.tags.map((t) => ({ name: t.name, line: t.location.line })), elements: [] };
  });
  // Retried cases replace their previous attempt; never double-count a pickle.
  const attempts = new Map();
  for (const envelope of envelopes) {
    if (!envelope.testCaseStarted) continue;
    const attempt = envelope.testCaseStarted;
    const testCase = cases.get(attempt.testCaseId);
    if (testCase) attempts.set(testCase.pickleId, attempt.id);
  }
  for (const [pickleId, attemptId] of attempts) {
    const pickle = pickles.get(pickleId);
    if (!pickle || pickle.tags.some((t) => ['@status/todo', '@status/broken'].includes(t.name))) continue;
    const testCase = cases.get(started.get(attemptId).testCaseId);
    const results = new Map(envelopes.filter((e) => e.testStepFinished?.testCaseStartedId === attemptId)
      .map((e) => [e.testStepFinished.testStepId, e.testStepFinished.testStepResult]));
    const feature = features.find((f) => f.uri === pickle.uri);
    if (!feature) throw new Error(`livingdoc: no feature for ${pickle.uri}`);
    const ast = nodes.get(pickle.astNodeIds[0]);
    const row = nodes.get(pickle.astNodeIds.at(-1));
    const steps = testCase.testSteps.map((t) => {
      const step = pickle.steps.find((p) => p.id === t.pickleStepId);
      const source = step ? nodes.get(step.astNodeIds[0]) : null;
      const result = results.get(t.id);
      const status = (result?.status ?? 'UNDEFINED').toLowerCase();
      const duration = result?.duration;
      const rendered = { keyword: source?.keyword ?? 'Hook: ', name: step?.text ?? 'test lifecycle hook', line: source?.location.line ?? 0,
        match: { location: '' }, result: { status, duration: duration ? Number(duration.seconds ?? 0) * 1e9 + (duration.nanos ?? 0) : 0 } };
      if (result?.message) rendered.result.error_message = result.message;
      if (step?.argument?.dataTable) rendered.rows = step.argument.dataTable.rows.map((r) => ({ cells: r.cells.map((c) => c.value) }));
      if (step?.argument?.docString) rendered.doc_string = { value: step.argument.docString.content };
      return rendered;
    });
    feature.elements.push({ keyword: ast?.keyword ?? 'Scenario', type: 'scenario',
      id: pickle.id, name: pickle.name, description: ast?.description ?? '',
      line: row?.location.line ?? ast?.location.line ?? 0,
      tags: pickle.tags.map((t) => ({ name: t.name, line: row?.location.line ?? 0 })), steps });
  }
  return features;
}

if (ndjson.length === 0 && json.length === 0) {
  throw new Error(`livingdoc: no *.ndjson or *.json report in ${inDir}`);
}
const features = [];
for (const file of ndjson) features.push(...messagesToClassic(path.join(inDir, file)));
for (const file of json) features.push(...JSON.parse(fs.readFileSync(path.join(inDir, file), 'utf8')));
for (const feature of features) { feature.tags ??= []; feature.elements ??= []; }
completeInventory(features);
const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'livingdoc-'));
try {
  fs.writeFileSync(path.join(tmp, 'report.json'), JSON.stringify(features));
  await generate({ jsonDir: tmp, reportPath: outDir, pageTitle: 'Living documentation', displayDuration: true,
    pageFooter: legendHtml || undefined });
} finally {
  fs.rmSync(tmp, { recursive: true, force: true });
}
