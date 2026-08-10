# Report-publishing workflow example

Project: any stack that implements the [[skills/common-workflow/test/bdd-coverage-mutation-testing.skill/bdd-coverage-mutation-testing.skill.md|bdd-coverage-mutation-testing]] `make` contract (`make cucumber-test`, `make mutation-test`, `make result-page`). Only the `Set up {stack}` step below changes between stacks — everything else is identical because the workflow only ever calls `make` targets.

```yaml
name: Publish reports

on:
  push:
    branches:
      - master
  workflow_dispatch:

permissions:
  contents: read
  pages: write
  id-token: write

# A push that lands while a previous run of this workflow is still going means that
# run's eventual report would describe code no longer on master - cancel it instead of
# letting it finish and publish stale numbers. This does risk interrupting an in-flight
# `deploy` step (GitHub's own Pages template avoids that with cancel-in-progress: false),
# but deploy is short and the next successful run republishes cleanly, so the trade-off
# favors cancellation here.
concurrency:
  group: ${{ github.workflow }}-${{ github.ref }}
  cancel-in-progress: true

jobs:
  changes:
    runs-on: ubuntu-latest
    outputs:
      relevant: ${{ steps.filter.outputs.code == 'true' || steps.filter.outputs.tests == 'true' || steps.filter.outputs.ci == 'true' }}
    steps:
      - uses: actions/checkout@v4

      - uses: dorny/paths-filter@v3
        id: filter
        with:
          filters: |
            code:
              - 'src/**'
            tests:
              - 'features/**'
              - 'test/**'
            ci:
              - '.github/workflows/**'
              - 'Makefile'
              - 'scripts/**'

  cucumber-test:
    needs: changes
    if: needs.changes.outputs.relevant == 'true'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          submodules: recursive

      # - name: Set up {stack}
      #   uses: actions/setup-{stack}@v...

      - name: Run Cucumber tests with coverage
        run: make cucumber-test WITH_CODE_COVERAGE=true

      - uses: actions/upload-artifact@v4
        with:
          name: cucumber-test-report
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
      # this workflow only reports the score. The PR-gate workflow already enforced the
      # threshold before this code reached master.
      - name: Run mutation tests
        run: make mutation-test

      - uses: actions/upload-artifact@v4
        with:
          name: mutation-test-report
          path: |
            tmp/result
            tmp/report/mutation
          if-no-files-found: error

  # result-page and deploy have no `if:` of their own - when cucumber-test/mutation-test
  # are skipped by the path filter, `needs` makes these cascade-skip automatically.
  result-page:
    needs: [cucumber-test, mutation-test]
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      # upload-artifact uses the least common ancestor of each job's upload paths (tmp/...)
      # as the artifact root, so it strips the "tmp/" prefix - download back into "tmp" to
      # restore it, matching what `make result-page` expects (tmp/result, tmp/report).
      - uses: actions/download-artifact@v4
        with:
          pattern: "*-test-report"
          merge-multiple: true
          path: tmp

      - name: Assemble GitHub Pages site
        run: make result-page

      - name: Upload Pages artifact
        uses: actions/upload-pages-artifact@v3
        with:
          path: public

  deploy:
    needs: result-page
    runs-on: ubuntu-latest
    environment:
      name: github-pages
      url: ${{ steps.deployment.outputs.page_url }}
    steps:
      - name: Deploy to GitHub Pages
        id: deployment
        uses: actions/deploy-pages@v4
```

## README badges produced by this workflow

```markdown
[![PR master](https://github.com/{org}/{repo}/actions/workflows/pr_master.yml/badge.svg)](https://github.com/{org}/{repo}/actions/workflows/pr_master.yml)
[![Tests](https://img.shields.io/endpoint?url=https://{org}.github.io/{repo}/tests-badge.json)](https://{org}.github.io/{repo}/tests/)
[![Coverage](https://img.shields.io/endpoint?url=https://{org}.github.io/{repo}/coverage-badge.json)](https://{org}.github.io/{repo}/coverage/)
[![Mutation score](https://img.shields.io/endpoint?url=https://{org}.github.io/{repo}/mutation-badge.json)](https://{org}.github.io/{repo}/mutation/reports/mutation-report.html)
```

The four badges are independent: the first is GitHub's native workflow-status badge for the PR-gate workflow; the other three are shields.io [endpoint badges](https://shields.io/badges/endpoint-badge) reading `<label>-badge.json` files that `make result-page` writes into `public/` on every run of this workflow — never hand-edited.
