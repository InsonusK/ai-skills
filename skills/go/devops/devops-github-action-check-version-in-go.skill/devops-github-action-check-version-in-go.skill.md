---
name: devops-github-action-check-version-in-go
description: Go-specific implementation of the check-version reusable composite action — reads a root VERSION file (since go.mod carries no version field), compares it semantically against the base ref, and always reports publishable=false since Go has no publish-package job
whenToUse: when creating or updating `.github/actions/check-version/action.yml` in a Go project
updated: 20260915
tags:
  - stack/go
  - concern/ci
  - github-actions

adr:
  - "[[./adr/version-source-file.md|Version source file]]"
---

# Scope
This skill adds Go-specific mechanics on top of the `check-version` composite action consumed by [[skills/devops/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]], [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]], and [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] — Go has no `stack-lib-release-publish` implementation (see [# Core Principle](#core-principle)). It does not cover those workflows' job graphs — only the `action.yml` this skill creates.

# Core Principle
- A root `VERSION` plain-text file is the single source of truth for a Go project's version — `go.mod` has no version field of its own. See [[./adr/version-source-file.md|Version source file]] for why this convention was chosen over a bare git tag or a `version.go` constant.
- Version comparison is semantic (dotted-tuple comparison of `MAJOR.MINOR.PATCH`), never plain string comparison.
- `publishable` is always `false` — Go has no `publish-package` reusable action; the Go module proxy indexes a pushed `v{VERSION}` git tag automatically once `github-release` creates the release, so no separate publish step exists for Go.

# Adr
- [[./adr/version-source-file.md|Version source file]]
  - Selected variant: a root `VERSION` plain-text file, over a bare git tag or a `version.go` constant.

# Rule

## MUST

### Implement action.yml reading the VERSION file
Create `.github/actions/check-version/action.yml` as a composite action that reads `VERSION` from the current commit and from the base ref, comparing them as dotted `MAJOR.MINOR.PATCH` tuples.
```yaml
name: check-version
description: Read and compare the project's version
outputs:
  current:
    value: ${{ steps.version.outputs.current }}
  bumped:
    value: ${{ steps.version.outputs.bumped }}
  publishable:
    value: ${{ steps.version.outputs.publishable }}
runs:
  using: composite
  steps:
    - id: version
      shell: bash
      run: |
        current=$(cat VERSION | tr -d '[:space:]')

        bumped=true
        if [ -n "$BASE_REF" ]; then
          if base=$(git show "${BASE_REF}:VERSION" 2>/dev/null | tr -d '[:space:]'); then
            # Version-aware sort so "10.0.0" correctly ranks above "2.0.0".
            higher=$(printf '%s\n%s\n' "$base" "$current" | sort -V | tail -n1)
            if [ "$higher" = "$current" ] && [ "$current" != "$base" ]; then
              bumped=true
            else
              bumped=false
            fi
          fi  # else: no prior VERSION file to compare against - bumped stays true
        fi

        echo "current=$current" >> "$GITHUB_OUTPUT"
        echo "bumped=$bumped" >> "$GITHUB_OUTPUT"
        echo "publishable=false" >> "$GITHUB_OUTPUT"
      env:
        BASE_REF: >-
          ${{ github.event_name == 'pull_request' && format('origin/{0}', github.base_ref)
              || (github.event_name == 'push' && github.event.before) || '' }}
```
- Violation: reading the version from a git tag or a Go source constant instead of `VERSION` (see [[./adr/version-source-file.md|Version source file]] for why), or comparing versions lexically instead of as a dotted tuple (`sort -V`).
- Risk: lexical string comparison ranks `"10.0.0"` below `"2.0.0"`, producing a false version-bump failure.
- Fix: read `VERSION` exactly, compare with `sort -V` (version-aware sort) as shown above.

### Always report publishable=false
Hardcode `publishable=false` in this action's output — never wire it to a real check.
- Violation: adding logic that tries to detect "is this Go module meant to be published," and wiring a `stack-lib-release-publish-in-go` workflow to it.
- Risk: there is no `devops-github-wf-stack-lib-release-publish-in-go` skill and no such workflow exists (see [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]]'s `# Scope`) — a `true` here would make [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] link to a package that was never published.
- Fix: keep `publishable` hardcoded `false`; a Go module's only release artifact is the git tag [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] already creates, which `pkg.go.dev`/the Go module proxy picks up on its own.

# Check list
- [ ] `.github/actions/check-version/action.yml` exists, reads the root `VERSION` file from the current and base ref.
- [ ] Comparison uses version-aware (`sort -V`) or numeric-tuple comparison, never plain lexical string comparison.
- [ ] `publishable` is hardcoded `false`.
- [ ] Outputs are named `current`, `bumped`, `publishable`, matching every consumer workflow's `needs.check-version.outputs.*` references.
- [ ] [[./adr/version-source-file.md|Version source file]] is registered in this skill's `adr:` property and linked from the body.
