---
description: Add the PR-gate/trunk-gate CI jobs and README badges for the Cucumber/coverage/mutation quality gate
element_kind: repository
change_kind: extend
---

# Structure

## Project Structure
```
/.github
  /workflows
    ci.yml
/{package-name}
  /src
    index.ts
    {rule}-validator.ts
  /features
    {rule}.feature
    /step-definitions
      {rule}.steps.ts
  package.json
README.md
```

## Directory and class skills
| Directory | file | Description |
| ----------------- | ----------- |
| /.github/workflows | ci.yml | PR-gate and trunk-gate jobs |
| / | README.md | Coverage badge and mutation-score badge |

# Rules

## MUST
- `ci.yml` must define two separate jobs: a PR-gate job triggered on `pull_request` targeting `master`, and a trunk-gate job triggered on `push` to `master`.
- The PR-gate job must run `vitest run --coverage`, `cucumber-js`, and `stryker run` in an incremental/since mode scoped to the PR's diff against `master` (consult Stryker's current docs for the exact incremental-mode flag/config key), and fail the check on a red test or a new surviving mutant.
- The trunk-gate job must run the combined suite with full coverage across the whole package, `stryker run` without incremental scoping across the whole package, publish both reports as workflow artifacts, and regenerate the two README badges from this run.
- `README.md` must display a coverage badge and a mutation-score badge, both sourced from the latest trunk-gate artifact.

## MUST NOT
- The PR-gate job must not run `stryker run` without incremental scoping — full-package mutation testing belongs to the trunk gate only.
- The trunk-gate job must not block anything retroactively; a red trunk gate is fixed forward, not reverted through history rewriting.

# Anti-patterns
- **Running the same unscoped `stryker run` in both jobs**
  - Consequence: PR feedback becomes as slow as the trunk gate, and the team eventually disables mutation testing on PRs under deadline pressure.
  - Instead: enable Stryker's incremental/since mode against `master` in the PR-gate job only; the trunk-gate job runs the full, unscoped command.
- **Badge sourced from a PR run**
  - Consequence: the README shows numbers that were never actually merged, misleading anyone who checks it against `master`.
  - Instead: regenerate badge data only inside the trunk-gate job, after the merge.

# Unittest TestCases
- [ ] WHEN a PR touches `src/{rule}-validator.ts` and a mutant in it survives THEN the PR-gate job fails.
- [ ] WHEN `master` is updated THEN the trunk-gate job runs full coverage and full mutation testing and updates both README badges.
