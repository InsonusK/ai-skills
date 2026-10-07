# Release-test-report workflow example

Project: any stack that implements the [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]] `make` contract and has `.github/actions/check-changes` implemented (see `devops-github-action-check-changes-in-{stack}`). Only the `Set up {stack}` step below changes between stacks — everything else is identical because the workflow only ever calls `make` targets.

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

env:
  # Facts about this run and the directories this workflow chose - the whole of what it
  # tells the project's tests. The report goes into a subfolder of the published site,
  # so a project that publishes its own pages keeps them.
  TEST_RUN_PURPOSE: report
  TEST_WORK_DIR: tmp/testing
  TEST_REPORT_DIR: site/testing

jobs:
  changes:
    runs-on: ubuntu-latest
    outputs:
      relevant: ${{ github.event_name == 'workflow_dispatch' || steps.filter.outputs.code == 'true' || steps.filter.outputs.test == 'true' || steps.filter.outputs.workflow == 'true' }}
    steps:
      - uses: actions/checkout@v4
      - uses: ./.github/actions/check-changes
        id: filter

  # The project's Makefile says which test kinds exist; this workflow never names one.
  test-kinds:
    needs: changes
    if: needs.changes.outputs.relevant == 'true'
    runs-on: ubuntu-latest
    outputs:
      kinds: ${{ steps.kinds.outputs.kinds }}
    steps:
      - uses: actions/checkout@v4
      - id: kinds
        run: echo "kinds=$(make -s test-kinds | cut -d' ' -f1 | jq -Rsc 'split("\n") | map(select(. != ""))')" >> "$GITHUB_OUTPUT"

  test-kind:
    name: Test (${{ matrix.kind }})
    needs: test-kinds
    strategy:
      fail-fast: false
      matrix:
        kind: ${{ fromJSON(needs.test-kinds.outputs.kinds) }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          submodules: recursive

      # - name: Set up {stack}
      #   uses: actions/setup-{stack}@v...

      # Report-only: a kind exits non-zero when its own checks failed (a red test, a
      # surviving mutant), and this workflow never blocks on that - the failure shows in
      # the published report and its badges. The kind wrote its results before exiting.
      - run: make test-kind-${{ matrix.kind }}
        continue-on-error: true

      # The kind's one directory - handed to the report job as it is.
      - uses: actions/upload-artifact@v4
        if: ${{ !cancelled() }}
        with:
          name: test-kind-${{ matrix.kind }}
          path: ${{ env.TEST_WORK_DIR }}/kinds/${{ matrix.kind }}
          include-hidden-files: true
          if-no-files-found: warn

  # Runs whenever the kinds were listed - also after a failed kind - and cascade-skips
  # with them when the path filter found nothing relevant.
  test-report:
    needs: [test-kinds, test-kind]
    if: ${{ !cancelled() && needs.test-kinds.result == 'success' }}
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/download-artifact@v4
        with:
          pattern: test-kind-*
          path: tmp/artifacts

      - name: Put every kind's directory back under the work directory
        run: |
          mkdir -p "$TEST_WORK_DIR/kinds"
          for d in tmp/artifacts/test-kind-*; do
            [ -d "$d" ] && mv "$d" "$TEST_WORK_DIR/kinds/${d##*/test-kind-}"
          done

      - name: Build the report
        run: make test-report

      - uses: actions/upload-pages-artifact@v3
        with:
          path: site

  deploy:
    needs: test-report
    if: ${{ !cancelled() && needs.test-report.result == 'success' }}
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
[![Test report](https://img.shields.io/badge/test-report-blue)](https://{org}.github.io/{repo}/testing/)
[![{name}](https://img.shields.io/endpoint?url=https://{org}.github.io/{repo}/testing/badges/{name}.json)](https://{org}.github.io/{repo}/testing/reports/{name}/)
```

The first badge is GitHub's native workflow-status badge for the pull-request workflow; the second links the report's entry page. The last line is the pattern for every badge the project's tests declare — one line per name `make test-kinds` prints after a kind — a shields.io [endpoint badge](https://shields.io/badges/endpoint-badge) reading `badges/{name}.json` and linking `reports/{name}/`, both written by `make test-report` on every run of this workflow. This workflow never lists the names: the project adds a line when it adds a test kind, and `make test-readme-check` in the pull-request workflow fails when one is missing.
