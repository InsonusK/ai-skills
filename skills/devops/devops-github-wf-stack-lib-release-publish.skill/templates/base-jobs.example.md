# Shared base jobs: changes + check-version

Every release-publish workflow — [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] and every `devops-github-wf-stack-lib-release-publish-in-{stack}` — starts from these same two jobs, byte-for-byte. They only ever differ in the final job(s) built on top: `docker-publish` for the Docker workflow, `publish` for the stack-lib workflow. Neither job pre-combines `check-changes`' outputs into a single `relevant` boolean — each consumer reads exactly the raw outputs it needs (`code`/`workflow`/`docker` for Docker, `code`/`workflow` only for a package) in its own `if:`, so the same `changes` job serves every consumer without modification.

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

  # --- everything below this line is the one job that differs per consumer ---
```

`check-version` always exposes all four outputs (`current`, `bumped`, `publishable`, `timestamp`), even though a given consumer only reads some of them — `docker-publish` reads `current`/`timestamp` only, `publish` (stack-lib) reads `current`/`publishable`/`timestamp`, and [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] reads `current`/`bumped`/`publishable`. Exposing the full, uniform set here — rather than a different subset per workflow — is what keeps this job identical everywhere it's copied.
