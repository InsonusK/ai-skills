// skills/devops/agent/run-action-step.mjs - prints the script of one `run` step of a composite
// action, with ${{ inputs.x }} replaced from INPUT_X, so that the step can be run locally.
//   node run-action-step.mjs ACTION-FILE STEP-ID | bash
import { readFileSync } from 'node:fs';
import { parse } from 'yaml';
const [file, id] = process.argv.slice(2);
const step = parse(readFileSync(file, 'utf8')).runs.steps.find((s) => s.id === id);
const expr = (v) => String(v).replace(/\$\{\{\s*inputs\.([a-z-]+)\s*\}\}/g, (_, n) => process.env[`INPUT_${n.toUpperCase().replace(/-/g, '_')}`] ?? '');
for (const [k, v] of Object.entries(step.env ?? {})) console.log(`export ${k}='${expr(v)}'`);
console.log(expr(step.run));
