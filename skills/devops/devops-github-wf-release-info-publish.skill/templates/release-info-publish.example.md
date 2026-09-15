# Release-info-publish workflow example

Project: any stack with `.github/actions/check-version` implemented. `PACKAGE_URL_PATTERN`/`PACKAGE_NAME` below are the only stack-specific pieces — see [# Package link patterns](../devops-github-wf-release-info-publish.skill.md#package-link-patterns) for the four patterns, and read the package name from wherever the stack's manifest keeps it (`pyproject.toml`'s `project.name`, `package.json`'s `name`, the `.csproj`'s `<PackageId>`).

The `check-version` job below is copied verbatim from [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] — including its `timestamp` output, unused here — so it stays byte-for-byte identical to the same job in every other release-publish workflow. This workflow has no `changes` job; it gates solely on `bumped`.

```yaml
name: Release info publish

on:
  push:
    branches:
      - master
  workflow_dispatch:

jobs:
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

  github-release:
    needs: check-version
    if: needs.check-version.outputs.bumped == 'true'
    runs-on: ubuntu-latest
    permissions:
      contents: write
    steps:
      - uses: actions/checkout@v4

      - id: body
        run: |
          version="${{ needs.check-version.outputs.current }}"
          body="Version ${version}"

          if [ -n "$(git ls-files Dockerfile)" ]; then
            # Must match the lowercased image ref devops-github-wf-docker-release-publish
            # actually pushed - github.repository preserves case, Docker refs don't.
            image="ghcr.io/$(echo '${{ github.repository }}' | tr '[:upper:]' '[:lower:]')"
            body="${body}

          - Docker: \`${image}:${version}\`"
          fi

          if [ "${{ needs.check-version.outputs.publishable }}" = "true" ]; then
            # PACKAGE_NAME/PACKAGE_URL_PATTERN: read from the stack's own manifest and
            # the pattern for the stack in use (see the skill's "Package link patterns").
            package_url="https://pypi.org/project/${PACKAGE_NAME}/${version}/"
            body="${body}
          - Package: ${package_url}"
          fi

          {
            echo "value<<EOF"
            echo "$body"
            echo "EOF"
          } >> "$GITHUB_OUTPUT"
        env:
          PACKAGE_NAME: my-package

      - uses: softprops/action-gh-release@v2
        with:
          tag_name: v${{ needs.check-version.outputs.current }}
          generate_release_notes: true
          body: ${{ steps.body.outputs.value }}
```
