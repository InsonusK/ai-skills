// skills/devops/agent/actions-check.mjs - what actionlint does not look at: every composite
// action the skills ship parses, every `run` step names its shell, and every release action
// has the inputs and the output the workflows pass and read.
//   node skills/devops/agent/actions-check.mjs FILE...
import { readFileSync } from 'node:fs';
import { parse } from 'yaml';

let failed = 0;
const fail = (file, msg) => { console.log(`FAIL  ${file}: ${msg}`); failed++; };
for (const file of process.argv.slice(2)) {
  let action;
  try { action = parse(readFileSync(file, 'utf8')); } catch (e) { fail(file, `does not parse: ${e.message}`); continue; }
  if (action?.runs?.using !== 'composite') { fail(file, 'not a composite action'); continue; }
  action.runs.steps.forEach((step, i) => {
    if (step.run && !step.shell) fail(file, `step ${i + 1} has run without shell`);
    if (!step.run && !step.uses) fail(file, `step ${i + 1} has neither run nor uses`);
  });
  if (file.includes('/actions/release/')) {
    const inputs = Object.keys(action.inputs ?? {}).join(',');
    if (inputs !== 'channel,version,timestamp,registry-token,snapshot-registry-token') fail(file, `inputs are ${inputs}`);
    if (!action.outputs?.notes) fail(file, 'no output notes');
    if (action.name !== 'release') fail(file, `name is ${action.name}`);
    if (!action.runs.steps.some((s) => s.id === 'names')) fail(file, 'no step with id names');
  }
}
if (failed) process.exit(1);
console.log(`actions-check: ${process.argv.length - 2} actions ok`);
