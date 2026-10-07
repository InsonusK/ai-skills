---
description: One stack-independent script rendering a standard Cucumber report into the living-doc HTML
element_kind: file
change_kind: create
tags:
  - solution/conformance-testing
  - element/tools-livingdoc-render-mjs
---

# Goals
- Turn whatever standard Cucumber report the runner wrote into `$TEST_KIND_DIR/report/tests/livingdoc/`, choosing the renderer by protocol, with no stack-specific code.

# Core Principles
- Input is `$TEST_KIND_DIR/report/tests/cucumber/`: `*.ndjson` files are Cucumber Messages, `*.json` files are classic Cucumber JSON. A runner writes one protocol only.
- Cucumber Messages → `@cucumber/html-formatter`, one page per `.ndjson` file (plus an index page when there are several). Classic JSON → `multiple-cucumber-html-reporter`, all files into one report.
- Classic JSON is normalized in a temporary copy before rendering: a feature with no `tags` gets `tags: []` (godog omits the key). The runner's own files are never modified.

# Implementation changes
`tools/livingdoc/render.mjs`:
```js
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
```

# Rule changes

## MUST
- Copy this script verbatim into every stack; never add a stack-specific branch.
  - Risk: a per-stack renderer reintroduces the per-stack report formats this step exists to remove.
  - Fix: make the runner write the standard protocol instead.
- Never modify the runner's files in `$TEST_KIND_DIR/report/tests/cucumber/`.
  - Risk: a downstream consumer reading the raw report sees altered data.
  - Fix: normalize in a temporary copy, as above.

# Check list
- [ ] `tools/livingdoc/render.mjs` matches this file.
- [ ] After `make test-kind-unit` with npm available, `$TEST_KIND_DIR/report/tests/livingdoc/index.html` exists.
