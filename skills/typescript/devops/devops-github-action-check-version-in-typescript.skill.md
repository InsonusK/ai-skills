---
name: devops-github-action-check-version-in-typescript
description: TypeScript-specific implementation of the check-version reusable composite action — reads package.json's version, compares it semantically against the base ref using semver, and detects whether the project is publishable to npm
whenToUse: when creating or updating `.github/actions/check-version/action.yml` in a TypeScript project
updated: 20260915
tags:
  - stack/typescript
  - concern/ci
  - github-actions

---

# Scope
This skill adds TypeScript-specific mechanics on top of the `check-version` composite action consumed by [[skills/devops/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]], [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]], [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]], and [[skills/typescript/devops/devops-github-wf-stack-lib-release-publish-in-typescript.skill.md|devops-github-wf-stack-lib-release-publish-in-typescript]]. It does not cover those workflows' job graphs — only the `action.yml` this skill creates.

# Core Principle
- `package.json`'s `version` field is the single source of truth — never a separate `VERSION` file or a git tag.
- Version comparison is semantic (npm's `semver` package), never plain string comparison.
- "Publishable" means `package.json` does not set `"private": true`.

# Rule

## MUST

### Implement action.yml reading package.json
Create `.github/actions/check-version/action.yml` as a composite action that reads `version` from the current commit's `package.json` and from the base ref, comparing them with `semver`.
```yaml
name: check-version
description: Read and compare the project's version
runs:
  using: composite
  steps:
    - shell: bash
      run: npm install -g semver
    - id: version
      shell: bash
      run: |
        current=$(node -p "require('./package.json').version")
        publishable=$(node -p "require('./package.json').private === true ? 'false' : 'true'")

        bumped=true
        if [ -n "$BASE_REF" ] && git show "${BASE_REF}:package.json" > /tmp/base-package.json 2>/dev/null; then
          base=$(node -p "require('/tmp/base-package.json').version")
          if semver -r "<=${base}" "$current" > /dev/null 2>&1; then
            bumped=false
          fi
        fi

        echo "current=$current" >> "$GITHUB_OUTPUT"
        echo "bumped=$bumped" >> "$GITHUB_OUTPUT"
        echo "publishable=$publishable" >> "$GITHUB_OUTPUT"
      env:
        BASE_REF: >-
          ${{ github.event_name == 'pull_request' && format('origin/{0}', github.base_ref)
              || (github.event_name == 'push' && github.event.before) || '' }}
outputs:
  current:
    value: ${{ steps.version.outputs.current }}
  bumped:
    value: ${{ steps.version.outputs.bumped }}
  publishable:
    value: ${{ steps.version.outputs.publishable }}
```
- Violation: comparing `package.json`'s `version` as a plain string, or reading it from anywhere but the root `package.json`.
- Risk: `"10.0.0" < "2.0.0"` under string comparison gives a false failure; reading from the wrong location silently compares a stale or unrelated number.
- Fix: parse with `semver`, read `package.json`'s `version` exactly.

### publishable reflects the private flag
Set `publishable` to `true` only when `package.json` does not set `"private": true`.
- Violation: hardcoding `publishable: true` for every TypeScript project, including internal-only apps that set `"private": true` specifically to prevent an accidental `npm publish`.
- Risk: [[skills/typescript/devops/devops-github-wf-stack-lib-release-publish-in-typescript.skill.md|devops-github-wf-stack-lib-release-publish-in-typescript]]'s `publish` job runs (and either fails, or worse, actually publishes) a package that was deliberately marked private.
- Fix: gate on the `private` flag as shown above — never override or ignore it.

# Check list
- [ ] `.github/actions/check-version/action.yml` exists, reads `package.json`'s `version` from the current and base ref.
- [ ] Comparison uses `semver`, never plain string comparison.
- [ ] `publishable` is `true` only when `package.json` does not set `"private": true`.
- [ ] Outputs are named `current`, `bumped`, `publishable`, matching every consumer workflow's `needs.check-version.outputs.*` references.
