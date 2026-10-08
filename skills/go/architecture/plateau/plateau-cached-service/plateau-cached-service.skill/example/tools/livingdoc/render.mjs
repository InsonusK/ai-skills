// Renders a standard Cucumber report into a living-doc HTML report.
// Usage: node tools/livingdoc/render.mjs <cucumber-dir> <out-dir> [<status-legend.json> [<scenarios.json>]]
// The legend - { groups: [{title, where, items: [{name, meaning, shownAs?}]}] } - is shown where the
// renderer has a place for it: the footer of the classic-JSON report and the index of several
// Messages reports. A single Messages report is the formatter's own page and takes none.
// scenarios.json - the unit kind's inventory - adds what a classic-JSON runner never reports:
// the scenarios excluded by @status/todo or @status/broken and those no runner executed, so
// the living doc lists every scenario with all its tags. A Messages stream holds them already.
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import readline from 'node:readline';
import { Readable } from 'node:stream';
import { pipeline } from 'node:stream/promises';
import { CucumberHtmlStream } from '@cucumber/html-formatter';
import { generate } from 'multiple-cucumber-html-reporter';

const [inDir, outDir, legendFile, scenariosFile] = process.argv.slice(2);
const legend = legendFile ? JSON.parse(fs.readFileSync(legendFile, 'utf8')) : null;
const escapeHtml = (text) => text.replace(/[&<>"]/g, (c) => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));
// One section per group of the legend; names in one column, meanings aligned in the next.
// withShownAs adds the word the classic-JSON report uses for a status it has no name for.
const legendCss = '.tag-legend{max-width:60rem;margin:2rem auto;padding:0 1rem;text-align:left;font-size:.875rem;line-height:1.5}'
  + '.tag-legend h2{font-size:1.125rem;font-weight:700;margin:0 0 .5rem}'
  + '.tag-legend h3{font-size:.9375rem;font-weight:700;margin:1.25rem 0 .125rem}'
  + '.tag-legend p{margin:0 0 .5rem;opacity:.75}'
  + '.tag-legend dl{display:grid;grid-template-columns:max-content 1fr;gap:.25rem 1.25rem;margin:0}'
  + '.tag-legend dt{font-family:ui-monospace,SFMono-Regular,Menlo,Consolas,monospace;white-space:nowrap}'
  + '.tag-legend dd{margin:0}';
const legendHtml = (withShownAs) => (legend
  ? `<section class="tag-legend"><style>${legendCss}</style><h2>Tags and statuses</h2>`
    + legend.groups.map((g) => `<h3>${escapeHtml(g.title)}</h3><p>${escapeHtml(g.where)}</p><dl>`
      + g.items.map((i) => `<dt>${escapeHtml(i.name)}</dt><dd>${escapeHtml(i.meaning)}`
        + `${withShownAs && i.shownAs ? ` - shown here as "${escapeHtml(i.shownAs)}"` : ''}</dd>`).join('')
      + '</dl>').join('')
    + '</section>'
  : '');

// Entries of the inventory the runner did not execute, as classic-JSON scenarios with one
// step that states why: todo -> pending, broken -> skipped, not-run -> undefined.
const notExecuted = scenariosFile && fs.existsSync(scenariosFile)
  ? JSON.parse(fs.readFileSync(scenariosFile, 'utf8')).scenarios.filter((e) => ['todo', 'broken', 'not-run'].includes(e.status))
  : [];
const stepStatus = { todo: 'pending', broken: 'skipped', 'not-run': 'undefined' };
const pathEnd = (uri) => uri.replace(/^(\.{1,2}\/)+/, '');
function addNotExecuted(features) {
  for (const entry of notExecuted) {
    let feature = features.find((f) => f.name === entry.feature && entry.uri.endsWith(pathEnd(f.uri ?? '')));
    if (!feature) {
      feature = { uri: entry.uri, id: entry.uri, keyword: 'Feature', name: entry.feature, line: 1, description: '',
        tags: entry.tags.filter((t) => t.startsWith('@type/')).map((name) => ({ name, line: 1 })), elements: [] };
      features.push(feature);
    }
    const reason = entry.status === 'not-run' ? 'no runner executed this scenario' : (entry.note || 'no reason given');
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

async function renderMessages(file, outFile) {
  const lines = readline.createInterface({ input: fs.createReadStream(file), crlfDelay: Infinity });
  const envelopes = Readable.from((async function* () {
    for await (const line of lines) if (line.trim()) yield JSON.parse(line);
  })());
  await pipeline(envelopes, new CucumberHtmlStream(), fs.createWriteStream(outFile));
}

if (ndjson.length === 1) {
  await renderMessages(path.join(inDir, ndjson[0]), path.join(outDir, 'index.html'));
} else if (ndjson.length > 1) {
  for (const f of ndjson) {
    await renderMessages(path.join(inDir, f), path.join(outDir, `${path.parse(f).name}.html`));
  }
  const links = ndjson.map((f) => `<li><a href="${path.parse(f).name}.html">${path.parse(f).name}</a></li>`);
  fs.writeFileSync(path.join(outDir, 'index.html'), `<!doctype html><meta charset="utf-8"><title>Living documentation</title><h1>Living documentation</h1><ul>${links.join('')}</ul>${legendHtml(false)}\n`);
} else if (json.length > 0) {
  const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'livingdoc-'));
  const features = [];
  for (const f of json) {
    for (const feature of JSON.parse(fs.readFileSync(path.join(inDir, f), 'utf8'))) {
      feature.tags ??= [];
      features.push(feature);
    }
  }
  addNotExecuted(features);
  fs.writeFileSync(path.join(tmp, 'report.json'), JSON.stringify(features));
  await generate({ jsonDir: tmp, reportPath: outDir, pageTitle: 'Living documentation', displayDuration: true,
    pageFooter: legendHtml(true) || undefined });
  fs.rmSync(tmp, { recursive: true, force: true });
} else {
  console.error(`livingdoc: no *.ndjson or *.json report in ${inDir}`);
  process.exit(1);
}
