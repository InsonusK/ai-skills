---
name: devops-ci-toolchain-in-dotnet
description: .NET implementation of devops-ci-toolchain — the ready setup-toolchain composite action that installs the .NET version the project declares in the `sdk.version` of a root `global.json`, with the dependency cache
whenToUse: when a .NET project needs `.github/actions/setup-toolchain/action.yml`, or when a CI job of a .NET project runs on a toolchain version other than the project's
updated: 20261010
tags:
  - stack/dotnet
  - concern/ci
  - github-actions
---

# Goal
- `.github/actions/setup-toolchain/action.yml`, an unchanged copy of this skill's asset.
- The toolchain version declared in the `sdk.version` of a root `global.json`.

# Core Principle
- This skill details [[skills/devops/core/devops-ci-toolchain.skill.md|devops-ci-toolchain]] for .NET; apply both.

# Rule

## MUST

### Copy action.yml verbatim
Copy [[./assets/.github/actions/setup-toolchain/action.yml|action.yml]] verbatim to `.github/actions/setup-toolchain/action.yml`; do not modify it.
- Risk: a version typed into the action parts from the one developers use.
- Fix: restore the file from the asset and change the version where the project declares it.

### Declare the toolchain version in the project
Declare the toolchain version in the `sdk.version` of a root `global.json`.
- Violation: no `global.json`, the SDK version known only from the dev container image.
- Risk: the action finds no version and the job fails, or installs the newest one and the tests run on a toolchain no developer has.
- Fix: add the declaration; the action and the dev container read the same file.

# Check list
- [ ] `.github/actions/setup-toolchain/action.yml` is byte-identical to this skill's asset.
- [ ] the `sdk.version` of a root `global.json` declares the toolchain version.
