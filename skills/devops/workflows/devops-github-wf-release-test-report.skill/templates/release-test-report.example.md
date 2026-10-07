# Release-test-report workflow example

Project: any stack that implements the [[skills/testing/solution-conformance-testing/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] `make` contract and has `.github/actions/check-changes` implemented (see `devops-github-action-check-changes-in-{stack}`). Only the `Set up {stack}` step below changes between stacks — everything else is identical because the workflow only ever calls `make` targets.

```yaml
name: Release test report

on:
  push:
    branches:
      - master
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

jobs:
  changes:
    runs-on: ubuntu-latest
    outputs:
      relevant: ${{ github.event_name == 'workflow_dispatch' || steps.filter.outputs.code == 'true' || steps.filter.outputs.test == 'true' || steps.filter.outputs.workflow == 'true' }}
    steps:
      - uses: actions/checkout@v4
      - uses: ./.github/actions/check-changes
        id: filter

  unit-test:
    needs: changes
    if: needs.changes.outputs.relevant == 'true'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          submodules: recursive

      # - name: Set up {stack}
      #   uses: actions/setup-{stack}@v...

      - name: Run tests with coverage
        run: make unit-test WITH_CODE_COVERAGE=true

      - uses: actions/upload-artifact@v4
        with:
          name: unit-test-report
          path: |
            tmp/result
            tmp/report/tests
            tmp/report/coverage
          if-no-files-found: error

  mutation-test:
    needs: changes
    if: needs.changes.outputs.relevant == 'true'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          submodules: recursive

      # - name: Set up {stack}
      #   uses: actions/setup-{stack}@v...

      # Full run (no ONLY_DELTA): this workflow only runs on master, where there's no PR
      # base branch to diff against, so the whole project is mutated. It never gates -
      # mutation testing runs only in this workflow, and only as a report.
      # continue-on-error is what actually makes that true: make mutation-test exits
      # with the underlying tool's own exit code (non-zero on a surviving mutant, per
      # solution-conformance-testing's contract) - without this, that failure would
      # fail the job and, since test-report's `needs` has no `if: always()`, cascade
      # into skipping test-report/deploy entirely instead of just reporting the score.
      - name: Run mutation tests
        run: make mutation-test
        continue-on-error: true

      - uses: actions/upload-artifact@v4
        with:
          name: mutation-test-report
          path: |
            tmp/result
            tmp/report/mutation
          if-no-files-found: error

  # test-report and deploy have no `if:` of their own - when unit-test/mutation-test
  # are skipped by the path filter, `needs` makes these cascade-skip automatically.
  test-report:
    needs: [unit-test, mutation-test]
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/download-artifact@v4
        with:
          pattern: "*-test-report"
          merge-multiple: true
          path: tmp

      - name: Assemble GitHub Pages site
        run: make test-report

      - uses: actions/upload-pages-artifact@v3
        with:
          path: public

  deploy:
    needs: test-report
    runs-on: ubuntu-latest
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - id: deployment
        uses: actions/deploy-pages@v4
```

## README badges produced by this workflow

```markdown
[![Pull request](https://github.com/{org}/{repo}/actions/workflows/pull-request.yml/badge.svg)](https://github.com/{org}/{repo}/actions/workflows/pull-request.yml)
[![Tests](https://img.shields.io/endpoint?url=https://{org}.github.io/{repo}/tests-badge.json)](https://{org}.github.io/{repo}/tests/)
[![Coverage](https://img.shields.io/endpoint?url=https://{org}.github.io/{repo}/coverage-badge.json)](https://{org}.github.io/{repo}/coverage/)
[![Mutation score](https://img.shields.io/endpoint?url=https://{org}.github.io/{repo}/mutation-badge.json)](https://{org}.github.io/{repo}/)
```

The four badges are independent: the first is GitHub's native workflow-status badge for the pull-request workflow; the other three are shields.io [endpoint badges](https://shields.io/badges/endpoint-badge) reading the `tests-badge.json`, `coverage-badge.json`, and `mutation-badge.json` files that `make test-report` writes into `public/` on every run of this workflow — never hand-edited. The mutation badge links to the landing page, not into `mutation/`: the native mutation report's entry file differs per stack (Stryker's `reports/mutation-report.html`, Go's `index.html`).
