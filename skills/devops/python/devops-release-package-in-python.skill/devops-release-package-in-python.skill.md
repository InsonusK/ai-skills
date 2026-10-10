---
name: devops-release-package-in-python
description: Release action for a Python library — builds the package and uploads {version} to PyPI from master and {version}.dev{timestamp} to TestPyPI from develop; in a pull request it only builds
whenToUse: when a Python library needs `.github/actions/release/action.yml`, or when you review how its package is versioned and uploaded
updated: 20261010
tags:
  - stack/python
  - concern/ci
  - github-actions
  - release
---

# Goal
- `.github/actions/release/action.yml` in the project, an unchanged copy of this skill's file.
- The repository secrets `RELEASE_REGISTRY_TOKEN` (a PyPI API token) and `SNAPSHOT_REGISTRY_TOKEN` (a TestPyPI API token).
- From `master`: `{version}` on PyPI. From `develop`: `{version}.dev{timestamp}` on TestPyPI.

# Core Principle
- This skill is one of the release actions of [[skills/devops/core/devops-github-wf-release.skill/devops-github-wf-release.skill.md|devops-github-wf-release]]; a project has exactly one, and both workflows call it by its fixed path.
- **Snapshot form of PEP 440** - A snapshot is `{version}.dev{timestamp}`, which sorts before the release; `{version}-{timestamp}` is a post-release and would be installed instead of it.

# Rule

## MUST

### Copy action.yml verbatim
Copy [[./assets/.github/actions/release/action.yml|action.yml]] verbatim to `.github/actions/release/action.yml`; do not modify it.
- Risk: the workflows pass `channel`, `version`, `timestamp`, and two tokens and read `notes`; an edited action that drops one fails every push.
- Fix: restore the file from the asset; propose a needed change to the user and make it in the skill.

### Store the two registry tokens
Create the repository secrets `RELEASE_REGISTRY_TOKEN` with a PyPI API token and `SNAPSHOT_REGISTRY_TOKEN` with a TestPyPI API token before the first push.
- Risk: the upload fails at the end of the first release, after the tests.
- Fix: add both secrets in the repository settings.

### Keep the version literal on one line
Keep `version = "{version}"` at the start of a line in the `[project]` table of `pyproject.toml`.
- Violation: `version="1.4.0"` without spaces, or the key indented.
- Risk: the action rewrites that line for a snapshot; when it does not match, the snapshot is built as `{version}` and TestPyPI refuses the second one.
- Fix: write the line exactly as shown.

# Check list
- [ ] `.github/actions/release/action.yml` is an unchanged copy of this skill's file.
- [ ] The secrets `RELEASE_REGISTRY_TOKEN` and `SNAPSHOT_REGISTRY_TOKEN` exist.
- [ ] `pyproject.toml` has `version = "…"` at the start of a line.
