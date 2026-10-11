// skills/devops/agent/workflows-check.mjs - what actionlint does not look at: a job that calls
// check-changes in a workflow started by a pull request holds 'pull-requests: read'. There
// dorny/paths-filter reads the changed files from the GitHub API, and in a private repository
// the call fails without that scope.
//   node skills/devops/agent/workflows-check.mjs FILE...
import { readFileSync } from 'node:fs';
import { parse } from 'yaml';

let failed = 0;
const fail = (file, msg) => { console.log(`FAIL  ${file}: ${msg}`); failed++; };
const triggers = (on) => (typeof on === 'string' ? [on] : Array.isArray(on) ? on : Object.keys(on ?? {}));
const readsPullRequests = (p) => p === 'read-all' || p === 'write-all' || ['read', 'write'].includes(p?.['pull-requests']);
for (const file of process.argv.slice(2)) {
  let workflow;
  try { workflow = parse(readFileSync(file, 'utf8')); } catch (e) { fail(file, `does not parse: ${e.message}`); continue; }
  if (!triggers(workflow?.on).some((t) => t === 'pull_request' || t === 'pull_request_target')) continue;
  for (const [name, job] of Object.entries(workflow.jobs ?? {})) {
    if (!(job.steps ?? []).some((s) => s.uses === './.github/actions/check-changes')) continue;
    // A job's permissions replace the workflow's; with neither, the token has the repository's default.
    if (!readsPullRequests(job.permissions ?? workflow.permissions)) fail(file, `job ${name} calls check-changes on a pull request without pull-requests: read`);
  }
}
if (failed) process.exit(1);
console.log(`workflows-check: ${process.argv.length - 2} workflows ok`);
