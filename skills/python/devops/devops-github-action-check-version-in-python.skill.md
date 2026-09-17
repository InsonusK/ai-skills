---
name: devops-github-action-check-version-in-python
description: Python-specific implementation of the check-version reusable composite action — reads pyproject.toml's project.version, compares it semantically against the base ref using packaging.Version, and detects whether the project is publishable to PyPI
whenToUse: when creating or updating `.github/actions/check-version/action.yml` in a Python project
updated: 20260915
tags:
  - stack/python
  - concern/ci
  - github-actions

---

# Scope
This skill adds Python-specific mechanics on top of the `check-version` composite action consumed by [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]], [[skills/devops/workflows/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]], [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]], and [[skills/python/devops/devops-github-wf-stack-lib-release-publish-in-python.skill.md|devops-github-wf-stack-lib-release-publish-in-python]]. It does not cover those workflows' job graphs — only the `action.yml` this skill creates.

# Core Principle
- `pyproject.toml`'s `project.version` is the single source of truth — never a git tag, a `__version__` constant, or any other duplicate location.
- Version comparison is semantic (`packaging.Version`), never plain string comparison — `"10.0.0"` must compare greater than `"2.0.0"`.
- "Publishable" means the project is meant to be built and uploaded as a package: `pyproject.toml` declares a `[build-system]` table.

# Rule

## MUST

### Implement action.yml reading pyproject.toml
Create `.github/actions/check-version/action.yml` as a composite action that reads the current commit's `pyproject.toml`, reads the same file from `origin/${{ github.base_ref }}` (PR context) or the previous commit (push context) when it exists, and compares them semantically.
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
    - shell: bash
      run: pip install packaging
    - id: version
      shell: python
      run: |
        import os
        import subprocess
        import tomllib
        from packaging.version import Version

        def read_version(ref):
            if ref:
                out = subprocess.run(
                    ["git", "show", f"{ref}:pyproject.toml"],
                    capture_output=True, text=True, check=True,
                )
                data = tomllib.loads(out.stdout)
            else:
                with open("pyproject.toml", "rb") as f:
                    data = tomllib.load(f)
            return data

        current = read_version(None)
        current_version = Version(current["project"]["version"])
        publishable = "build-system" in current

        base_ref = os.environ.get("BASE_REF") or ""
        bumped = "true"
        if base_ref:
            try:
                base = read_version(base_ref)
                base_version = Version(base["project"]["version"])
                bumped = "true" if current_version > base_version else "false"
            except subprocess.CalledProcessError:
                bumped = "true"  # no prior pyproject.toml to compare against

        with open(os.environ["GITHUB_OUTPUT"], "a") as f:
            f.write(f"current={current_version}\n")
            f.write(f"bumped={bumped}\n")
            f.write(f"publishable={'true' if publishable else 'false'}\n")
      env:
        # PR context (devops-github-wf-pull-request): compare against the PR's base
        # branch (needs the "origin/" remote prefix). Push context
        # (devops-github-wf-release-info-publish, the only push-context consumer of
        # `bumped` — docker-release-publish/stack-lib-release-publish-in-python only
        # read `current`/`publishable`, never `bumped`): compare against the commit
        # SHA immediately before this push (used as-is, it is already a full SHA),
        # so `bumped` reflects whether this specific push raised the version.
        # workflow_dispatch has no "before" commit to diff, so BASE_REF is empty and
        # `bumped` defaults to true.
        BASE_REF: >-
          ${{ github.event_name == 'pull_request' && format('origin/{0}', github.base_ref)
              || (github.event_name == 'push' && github.event.before) || '' }}
```
- Violation: comparing `pyproject.toml`'s version as a plain string, or reading it from anywhere but `project.version`.
- Risk: `"10.0.0" < "2.0.0"` under string comparison gives a false failure; reading from the wrong location silently compares a stale or unrelated number.
- Fix: parse with `packaging.Version` and read `[project].version` exactly.

### publishable reflects [build-system] presence
Set `publishable` to `true` only when `pyproject.toml` has a `[build-system]` table.
- Violation: hardcoding `publishable: true` for every Python project, including internal-only services never meant to ship to PyPI.
- Risk: [[skills/python/devops/devops-github-wf-stack-lib-release-publish-in-python.skill.md|devops-github-wf-stack-lib-release-publish-in-python]]'s `publish` job runs (and fails, missing PyPI credentials) for a project that was never meant to publish a package.
- Fix: gate on `[build-system]`'s presence, exactly as read above.

# Check list
- [ ] `.github/actions/check-version/action.yml` exists, reads `pyproject.toml`'s `project.version` from the current and base ref.
- [ ] Comparison uses `packaging.Version`, never plain string comparison.
- [ ] `publishable` is `true` only when `pyproject.toml` has a `[build-system]` table.
- [ ] Outputs are named `current`, `bumped`, `publishable`, matching every consumer workflow's `needs.check-version.outputs.*` references.
