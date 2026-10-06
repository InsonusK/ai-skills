// Renders a standard Cucumber report into a living-doc HTML report.
// Usage: node tools/livingdoc/render.mjs <cucumber-dir> <out-dir>
import fs from 'node:fs';
import os from 'node:os';
import path from 'node:path';
import readline from 'node:readline';
import { Readable } from 'node:stream';
import { pipeline } from 'node:stream/promises';
import { CucumberHtmlStream } from '@cucumber/html-formatter';
import { generate } from 'multiple-cucumber-html-reporter';

const [inDir, outDir] = process.argv.slice(2);
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
  fs.writeFileSync(path.join(outDir, 'index.html'), `<!doctype html><title>Living documentation</title><ul>${links.join('')}</ul>\n`);
} else if (json.length > 0) {
  const tmp = fs.mkdtempSync(path.join(os.tmpdir(), 'livingdoc-'));
  for (const f of json) {
    const features = JSON.parse(fs.readFileSync(path.join(inDir, f), 'utf8'));
    for (const feature of features) feature.tags ??= [];
    fs.writeFileSync(path.join(tmp, f), JSON.stringify(features));
  }
  await generate({ jsonDir: tmp, reportPath: outDir, pageTitle: 'Living documentation', displayDuration: true });
  fs.rmSync(tmp, { recursive: true, force: true });
} else {
  console.error(`livingdoc: no *.ndjson or *.json report in ${inDir}`);
  process.exit(1);
}
