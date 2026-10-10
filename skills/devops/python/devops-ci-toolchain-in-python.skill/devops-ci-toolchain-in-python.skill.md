---
name: devops-ci-toolchain-in-python
description: Python implementation of devops-ci-toolchain — the ready setup-toolchain composite action that installs the Python version the project declares in `requires-python` of `pyproject.toml`, with the dependency cache
whenToUse: when a Python project needs `.github/actions/setup-toolchain/action.yml`, or when a CI job of a Python project runs on a toolchain version other than the project's
updated: 20261010
tags:
  - stack/python
  - concern/ci
  - github-actions
---

# Goal
- `.github/actions/setup-toolchain/action.yml`, an unchanged copy of this skill's asset.
- The toolchain version declared in `requires-python` of `pyproject.toml`.

# Core Principle
- This skill details [[skills/devops/core/devops-ci-toolchain.skill.md|devops-ci-toolchain]] for Python; apply both.

# Rule

## MUST

### Copy action.yml verbatim
Copy [[./assets/.github/actions/setup-toolchain/action.yml|action.yml]] verbatim to `.github/actions/setup-toolchain/action.yml`; do not modify it.
- Risk: a version typed into the action parts from the one developers use.
- Fix: restore the file from the asset and change the version where the project declares it.

### Declare the toolchain version in the project
Declare the toolchain version in `requires-python` of `pyproject.toml`.
- Violation: a `pyproject.toml` without `requires-python`.
- Risk: the action finds no version and the job fails, or installs the newest one and the tests run on a toolchain no developer has.
- Fix: add the declaration; the action and the dev container read the same file.

# Check list
- [ ] `.github/actions/setup-toolchain/action.yml` is byte-identical to this skill's asset.
- [ ] `requires-python` of `pyproject.toml` declares the toolchain version.
