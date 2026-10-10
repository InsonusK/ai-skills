// skills/devops/agent/changes-fixtures.mjs - ground truth for the check-changes actions.
// Reads the filters out of every stack's shipped action.yml and matches sample paths the way
// dorny/paths-filter does (picomatch, dot: true, predicate-quantifier: every): each path of
// changes-cases/common.tsv and changes-cases/{stack}.tsv must land in exactly the category named.
//   node skills/devops/agent/changes-fixtures.mjs
import { readFileSync, readdirSync, existsSync } from 'node:fs';
import { dirname, join } from 'node:path';
import { fileURLToPath } from 'node:url';
import picomatch from 'picomatch';
import { parse } from 'yaml';

const here = dirname(fileURLToPath(import.meta.url));
const root = join(here, '..');
const cases = (name) => readFileSync(join(here, 'changes-cases', `${name}.tsv`), 'utf8')
  .split('\n').filter((l) => l && !l.startsWith('#')).map((l) => l.split('\t'));

let failed = 0;
for (const stack of readdirSync(root)) {
  const action = join(root, stack, `devops-ci-changes-in-${stack}.skill`, 'assets/.github/actions/check-changes/action.yml');
  if (!existsSync(action)) continue;
  const step = parse(readFileSync(action, 'utf8')).runs.steps[0];
  if (step.with['predicate-quantifier'] !== 'every') throw new Error(`${action}: not predicate-quantifier: every`);
  const filters = Object.entries(parse(step.with.filters))
    .map(([name, patterns]) => [name, patterns.map((p) => picomatch(p, { dot: true }))]);
  let bad = 0;
  for (const [path, want] of [...cases('common'), ...cases(stack)]) {
    const got = filters.filter(([, ms]) => ms.every((m) => m(path))).map(([n]) => n).join(',') || '-';
    if (got !== want) { console.log(`  FAIL  ${stack}: ${path} is ${got}, wanted ${want}`); bad++; }
  }
  console.log(`${bad ? 'FAIL' : 'ok  '}  ${stack}`);
  failed += bad;
}
if (failed) { console.log('changes-fixtures: FAILED'); process.exit(1); }
console.log('changes-fixtures: all passed');
