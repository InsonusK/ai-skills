---
name: devops-github-wf-stack-lib-release-publish-in-typescript
description: TypeScript implementation of the stack-lib-release-publish workflow — `npm publish`, targeting registry.npmjs.org with the latest dist-tag (plain {version}) on master and GitHub's own npm feed npm.pkg.github.com ({version}-{timestamp}, tagged snapshot) on develop
whenToUse: when creating or updating `.github/workflows/stack-lib-release-publish.yml` in a TypeScript project that devops-github-action-check-version-in-typescript reports as publishable
updated: 20260915
tags:
  - stack/typescript
  - concern/ci
  - github-actions
  - release

---

# Scope
This skill adds TypeScript-specific mechanics on top of [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]] — apply both together; this skill only covers what publishing an npm package adds to that shared shape.

# Core Principle
- npm has a real `latest` dist-tag, unlike NuGet/PyPI — the `master` publish sets it explicitly; the `develop` publish must explicitly avoid it (tag `snapshot` instead).
- `package.json`'s `version` must equal the string being published before `npm publish` runs — npm refuses to publish a version that doesn't match the manifest.
- Both `master` and `develop` reuse `./.github/actions/check-changes`/`./.github/actions/check-version` exactly as [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] does, including its shared timestamp.

# Rule

## MUST

### Start from the shared base, then add this publish job
Open [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] and copy its `on:` trigger and `changes`/`check-version` jobs verbatim into `.github/workflows/stack-lib-release-publish.yml` — never reconstruct them from prose memory; add only the `publish` job below. Any deviation from either this job or the shared base gets confirmed with the user first and folded back into the example, not shipped silently.
```yaml
  publish:
    needs: [changes, check-version]
    if: >-
      (needs.changes.outputs.code == 'true' || needs.changes.outputs.workflow == 'true')
      && needs.check-version.outputs.publishable == 'true'
    runs-on: ubuntu-latest
    permissions:
      id-token: write
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-node@v4
        with:
          node-version: 'lts/*'

      - id: publish-version
        run: |
          if [ "${{ github.ref_name }}" = "master" ]; then
            echo "value=${{ needs.check-version.outputs.current }}" >> "$GITHUB_OUTPUT"
          else
            echo "value=${{ needs.check-version.outputs.current }}-${{ needs.check-version.outputs.timestamp }}" >> "$GITHUB_OUTPUT"
          fi

      - run: npm version --no-git-tag-version --allow-same-version ${{ steps.publish-version.outputs.value }}

      - run: npm run build --if-present

      - name: Publish to npmjs.org
        if: github.ref_name == 'master'
        run: npm publish --tag latest --provenance --access public
        env:
          NODE_AUTH_TOKEN: ${{ secrets.NPM_TOKEN }}

      - name: Publish to GitHub Packages
        if: github.ref_name == 'develop'
        run: |
          echo "@${{ github.repository_owner }}:registry=https://npm.pkg.github.com" >> .npmrc
          npm publish --tag snapshot
        env:
          NODE_AUTH_TOKEN: ${{ secrets.GITHUB_TOKEN }}
```
- Violation: publishing a `develop` snapshot with npm's default `latest` dist-tag, or to `registry.npmjs.org` instead of `npm.pkg.github.com`.
- Risk: a snapshot build tagged `latest` becomes what every consumer installing without a version pin actually gets, replacing the real released version with disposable, timestamp-tagged code.
- Fix: publish snapshots under the `snapshot` dist-tag (never `latest`) to GitHub's own npm registry, as shown.

### Set the version with npm version, never a manual edit
Set the version with `npm version --no-git-tag-version --allow-same-version` before publishing — never hand-edit `package.json`'s `version` field.
- Risk: `npm publish` refuses to publish a version that doesn't exactly match `package.json`; a hand-edit is more error-prone than npm's own command.
- Fix: use `npm version` as shown, with `--no-git-tag-version` since this workflow must not create a git tag itself (that already happens via [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]], for `master` only).

## SHOULD
- Publish with `--provenance` (npm's build-provenance attestation) on the `master` publish, given the job's `id-token: write` permission.

# Check list
- [ ] `.github/workflows/stack-lib-release-publish.yml` exists, following [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]]'s shared trigger/gating rules.
- [ ] `changes`/`check-version` are copied from [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] unmodified.
- [ ] `master` publishes to `registry.npmjs.org` tagged `latest`, plain `{version}`; `develop` publishes to `npm.pkg.github.com` tagged `snapshot`, `{version}-{timestamp}`.
- [ ] The version is set via `npm version --no-git-tag-version`, never a manual `package.json` edit.
- [ ] `NPM_TOKEN` comes from repository secrets, never hardcoded.
