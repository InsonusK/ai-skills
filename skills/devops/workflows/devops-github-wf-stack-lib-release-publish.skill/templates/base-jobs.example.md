# Shared base jobs: changes + check-version + unit-test

Every release-publish workflow — [[skills/devops/workflows/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] and every `devops-github-wf-stack-lib-release-publish-in-{stack}` — starts from these same three jobs, byte-for-byte. They only ever differ in the final job built on top: `docker-publish` for the Docker workflow, `publish` for the stack-lib workflow. Neither `changes` nor `check-version` pre-combines `check-changes`' outputs into a single `relevant` boolean — each consumer reads exactly the raw outputs it needs (`code`/`workflow`/`docker` for Docker, `code`/`workflow` only for a package) in its own `if:`, so the same `changes` job serves every consumer without modification.

`unit-test` runs in parallel with `check-version` — both `needs: changes` only, never each other — exactly the same shape as [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]]'s `unit-test`/`version-check` pair. This exists so a direct push to `master`/`develop` (bypassing the PR gate, or a PR merged before this code existed) still can't publish a Docker image or package built from code that fails its own tests: the final `docker-publish`/`publish` job additionally `needs: unit-test` and refuses to run when it failed.

```yaml
on:
  push:
    branches:
      - master
      - develop
  workflow_dispatch:

jobs:
  changes:
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

  check-version:
    runs-on: ubuntu-latest
    outputs:
      current: ${{ steps.version.outputs.current }}
      bumped: ${{ steps.version.outputs.bumped }}
      publishable: ${{ steps.version.outputs.publishable }}
      timestamp: ${{ steps.timestamp.outputs.value }}
    steps:
      - uses: actions/checkout@v4
        with:
          fetch-depth: 0
      - uses: ./.github/actions/check-version
        id: version
      - id: timestamp
        run: echo "value=$(date -u +%Y%m%d%H%M%S)" >> "$GITHUB_OUTPUT"

  unit-test:
    needs: changes
    if: needs.changes.outputs.code == 'true' || needs.changes.outputs.test == 'true'
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - run: make unit-test

  # --- everything below this line is the one job that differs per consumer ---
```

`check-version` always exposes all four outputs (`current`, `bumped`, `publishable`, `timestamp`), even though a given consumer only reads some of them — `docker-publish` reads `current`/`timestamp` only, `publish` (stack-lib) reads `current`/`publishable`/`timestamp`, and [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] reads `current`/`bumped`/`publishable`. Exposing the full, uniform set here — rather than a different subset per workflow — is what keeps this job identical everywhere it's copied.

## Wiring `unit-test` into the final publish job

`unit-test` is conditionally skipped (no `code`/`test` change), exactly like `changes`' other consumers. GitHub's default `needs` gating treats a skipped upstream job the same as a failed one — it would cascade-skip `docker-publish`/`publish` even on a push that only touched the `Dockerfile`, a real regression from today's behavior. So the final job must override the default with its own `always()`-based condition, never a plain boolean `if:`:

```yaml
  docker-publish: # or `publish` for stack-lib
    needs: [changes, check-version, unit-test]
    if: >-
      always() &&
      needs.changes.result == 'success' &&
      needs.check-version.result == 'success' &&
      needs.unit-test.result != 'failure' &&
      (needs.changes.outputs.code == 'true' || needs.changes.outputs.workflow == 'true' || needs.changes.outputs.docker == 'true')
```

`unit-test.result != 'failure'` accepts both `success` and `skipped` — a skipped run (nothing to test) still allows publishing, exactly as it does today; only an actual test failure blocks it. `changes.result`/`check-version.result` are pinned to `success` explicitly because `always()` otherwise waives that too.
