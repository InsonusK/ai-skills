---
name: devops-github-wf-stack-lib-release-publish-in-dotnet
description: .NET implementation of the stack-lib-release-publish workflow — `dotnet pack` + `dotnet nuget push`, targeting nuget.org (plain {version}) on master and GitHub's own NuGet feed nuget.pkg.github.com ({version}-{timestamp}) on develop
whenToUse: when creating or updating `.github/workflows/stack-lib-release-publish.yml` in a .NET project that devops-github-action-check-version-in-dotnet reports as publishable
updated: 20260915
tags:
  - stack/dotnet
  - concern/ci
  - github-actions
  - release

---

# Scope
This skill adds .NET-specific mechanics on top of [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]] — apply both together; this skill only covers what publishing a NuGet package adds to that shared shape.

# Core Principle
- NuGet has no per-package "latest" tag the way Docker or npm do — publishing the version is the entire job; there is no second "also tag latest" step.
- Only production projects (`<IsPackable>true</IsPackable>`, not `*.Tests` projects) are packed and pushed.
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
    steps:
      - uses: actions/checkout@v4

      - id: publish-version
        run: |
          if [ "${{ github.ref_name }}" = "master" ]; then
            echo "value=${{ needs.check-version.outputs.current }}" >> "$GITHUB_OUTPUT"
          else
            echo "value=${{ needs.check-version.outputs.current }}-${{ needs.check-version.outputs.timestamp }}" >> "$GITHUB_OUTPUT"
          fi

      - run: dotnet pack --configuration Release -p:Version=${{ steps.publish-version.outputs.value }} --output ./nupkg

      - name: Push to nuget.org
        if: github.ref_name == 'master'
        run: dotnet nuget push "./nupkg/*.nupkg" --source https://api.nuget.org/v3/index.json --api-key "${{ secrets.NUGET_API_KEY }}" --skip-duplicate

      - name: Push to GitHub Packages (NuGet)
        if: github.ref_name == 'develop'
        run: |
          dotnet nuget add source --username ${{ github.actor }} --password "${{ secrets.GITHUB_TOKEN }}" \
            --store-password-in-clear-text --name github "https://nuget.pkg.github.com/${{ github.repository_owner }}/index.json"
          dotnet nuget push "./nupkg/*.nupkg" --source github --api-key "${{ secrets.GITHUB_TOKEN }}" --skip-duplicate
```
- Violation: pushing every project in the solution, including `*.Tests` projects, or publishing a `develop` snapshot to `nuget.org` itself.
- Risk: pushing test projects clutters the registry with packages nobody is meant to consume; publishing every snapshot to `nuget.org` pollutes the public feed with disposable, timestamp-tagged versions.
- Fix: `dotnet pack` only packs `<IsPackable>true</IsPackable>` projects by default — leave that MSBuild default in place; route `master` to `nuget.org` and `develop` to `nuget.pkg.github.com` as shown.

### Pass -p:Version explicitly, matching the computed publish version
Pass `-p:Version=${{ steps.publish-version.outputs.value }}` to `dotnet pack` — never rely on `Directory.Build.props`' `<Version>` alone, since that file only ever holds the bare release version, not a timestamped snapshot tag.
- Risk: without the override, a `develop` snapshot pack would carry the bare `current` version, colliding with the eventual real release of that same version.
- Fix: always pass `-p:Version` explicitly with the branch-appropriate computed value, as shown above.

## SHOULD
- Pass `--skip-duplicate` so a re-run of a job that already pushed successfully does not fail the whole workflow.

# Check list
- [ ] `.github/workflows/stack-lib-release-publish.yml` exists, following [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]]'s shared trigger/gating rules.
- [ ] `changes`/`check-version` are copied from [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] unmodified.
- [ ] `master` pushes to `nuget.org` under the plain `{version}`; `develop` pushes to `nuget.pkg.github.com` under `{version}-{timestamp}`.
- [ ] Only `<IsPackable>true</IsPackable>` projects are packed — `*.Tests` projects are never pushed.
- [ ] `-p:Version` is passed explicitly on every pack.
- [ ] `NUGET_API_KEY` comes from repository secrets, never hardcoded.
