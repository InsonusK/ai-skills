# CI gate structure (generic, stack-agnostic)

Two separate CI jobs. Do not merge them into one job with conditional steps — keep the trigger, scope, and failure mode distinct so a reader can tell at a glance which gate they are looking at.

## Job 1 — PR gate
- Trigger: pull request targeting `master`.
- Steps:
  1. Install dependencies.
  2. Run the full unit test suite.
  3. Run the full Cucumber/Gherkin suite (step definitions call production code).
  4. Run mutation testing scoped to the files/lines changed in the PR's diff (delta only).
  5. Run the coverage tool and fail if coverage on changed files drops below the project's floor.
- Failure mode: blocks merge.

## Job 2 — Trunk gate
- Trigger: push/merge to `master`.
- Steps:
  1. Install dependencies.
  2. Run the full unit test suite.
  3. Run the full Cucumber/Gherkin suite.
  4. Run the coverage tool across the whole repository; publish the report as a CI artifact.
  5. Run mutation testing across the whole repository (not delta-scoped); publish the report as a CI artifact.
  6. Regenerate the coverage badge and mutation-score badge (e.g. a shields.io endpoint badge fed by the reports above) and update the README.
- Failure mode: does not block anything after the fact (the code is already on trunk), but a red trunk gate is an incident — fix forward immediately, do not let it stay red.

## Badge sourcing
- Both badges in the README must be regenerated from the **trunk gate**'s latest run, never from a PR run — a PR's mutation/coverage numbers are provisional until merged.
