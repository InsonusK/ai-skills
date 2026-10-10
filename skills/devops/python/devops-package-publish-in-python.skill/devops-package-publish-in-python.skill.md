---
name: devops-package-publish-in-python
description: Python implementation of devops-package-publish — the package job of release.yml that publishes to PyPI from master and to TestPyPI from develop
whenToUse: when a Python library must publish its package from `release.yml`, or when you review the `package` job of a Python project
updated: 20261010
tags:
  - stack/python
  - concern/ci
  - github-actions
  - release
---

# Goal
- The `package` job of `.github/workflows/release.yml` assembled from this skill's job file.
- `master` publishing `{version}` to PyPI; `develop` publishing `{version}.dev{timestamp}` to TestPyPI.
- The repository secrets `PYPI_API_TOKEN` and `TEST_PYPI_API_TOKEN`.

# Core Principle
- This skill details [[skills/devops/core/devops-package-publish.skill.md|devops-package-publish]] for Python; apply both.

# Rule

## MUST

### Assemble with this job file
Pass [[./templates/package-job.yml|package-job.yml]] to `assemble-workflow.sh` as `package=`, then replace `{package-name}` with `project.name` of `pyproject.toml`.
- Risk: a leftover placeholder publishes nothing or links the Release to a package that does not exist.
- Fix: search `release.yml` for `{` followed by a lowercase name and replace each.

### Keep the snapshot form of PEP 440
Leave the snapshot version as `{version}.dev{timestamp}`.
- Violation: `{version}-{timestamp}`, the form the image tag uses.
- Risk: PEP 440 reads `1.4.0-20261010120000` as a post-release, which sorts after `1.4.0` and is installed instead of it.
- Fix: keep the `publish` step of the job file.

# Check list
- [ ] `release.yml` holds the `package` job of this skill's file; no `{package-…}` placeholder is left.
- [ ] The secrets `PYPI_API_TOKEN` and `TEST_PYPI_API_TOKEN` exist.
- [ ] A `develop` push publishes `{version}.dev{timestamp}` to TestPyPI.
