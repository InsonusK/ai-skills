# Pull-request workflow example

Project: any stack that has `.github/actions/check-changes` and `.github/actions/check-version` (see the matching `devops-github-action-check-changes-in-{stack}`/`devops-github-action-check-version-in-{stack}` skills). Only the `Set up {stack}` step below changes between stacks.

```yaml
name: Pull request

on:
  pull_request:
    branches:
      - master
      - develop

jobs:
  changes:
    name: Detect changed files
    runs-on: ubuntu-latest
    outputs:
      code: ${{ steps.filter.outputs.code }}
      test: ${{ steps.filter.outputs.test }}
      workflow: ${{ steps.filter.outputs.workflow }}
      docker: ${{ steps.filter.outputs.docker }}
      docs: ${{ steps.filter.outputs.docs }}
    steps:
      - uses: actions/checkout@v4
      - uses: ./.github/actions/check-changes
        id: filter

  version-check:
    name: Version bump check
    needs: changes
    if: github.base_ref == 'master' && (needs.changes.outputs.code == 'true' || needs.changes.outputs.workflow == 'true' || needs.changes.outputs.docker == 'true')
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - uses: ./.github/actions/check-version
        id: version
      # check-version only reports `bumped` - it never fails on its own, so
      # without this step the job always passes and the bump is never enforced.
      - name: Require a version bump
        if: steps.version.outputs.bumped != 'true'
        run: |
          echo "::error::Version ${{ steps.version.outputs.current }} must be strictly greater than master's"
          exit 1

  # The project's Makefile says which test kinds exist; this workflow never names one.
  test-kinds:
    name: List test kinds
    needs: changes
    if: needs.changes.outputs.code == 'true' || needs.changes.outputs.test == 'true'
    runs-on: ubuntu-latest
    outputs:
      kinds: ${{ steps.kinds.outputs.kinds }}
    steps:
      - uses: actions/checkout@v4
      - id: kinds
        run: echo "kinds=$(make -s test-kinds | cut -d' ' -f1 | jq -Rsc 'split("\n") | map(select(. != ""))')" >> "$GITHUB_OUTPUT"
      # Fails with "you forgot to add a badge" when the README and the declared badges disagree.
      - run: make test-readme-check

  test-kind:
    name: Test (${{ matrix.kind }})
    needs: test-kinds
    strategy:
      fail-fast: false
      matrix:
        kind: ${{ fromJSON(needs.test-kinds.outputs.kinds) }}
    runs-on: ubuntu-latest
    env:
      # The one fact this workflow states about the run; each kind decides what it means
      # for it and logs that. DELTA_BASE is deliberately not passed - see the skill's
      # "Never gate a PR on mutation testing".
      TEST_RUN_PURPOSE: check
    steps:
      - uses: actions/checkout@v4

      # The only step that changes between stacks: install the project's
      # toolchain version (and cache dependencies) instead of relying on
      # whatever ubuntu-latest preinstalls.
      # - name: Set up {stack}
      #   uses: actions/setup-{stack}@v...

      - run: make test-kind-${{ matrix.kind }}

  report:
    name: Pull request report
    needs: [changes, version-check, test-kinds, test-kind]
    if: always()
    runs-on: ubuntu-latest
    steps:
      - name: Aggregate result
        run: |
          fail=0

          echo "Changes:"
          echo "- code    : ${{ needs.changes.outputs.code }}"
          echo "- test    : ${{ needs.changes.outputs.test }}"
          echo "- workflow: ${{ needs.changes.outputs.workflow }}"
          echo "- docker  : ${{ needs.changes.outputs.docker }}"
          echo "- docs    : ${{ needs.changes.outputs.docs }}"

          for j in "version-check ${{ needs.version-check.result }}" \
                   "test-kinds ${{ needs.test-kinds.result }}" \
                   "test-kind ${{ needs.test-kind.result }}"; do
            set -- $j
            echo "$1: $2"
            [ "$2" = "failure" ] && fail=1
          done
          [ "$fail" = "0" ] && echo "PR validation passed (skipped jobs count as passing)" || exit 1
```
