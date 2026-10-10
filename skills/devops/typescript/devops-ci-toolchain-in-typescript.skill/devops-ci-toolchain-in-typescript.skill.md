---
name: devops-ci-toolchain-in-typescript
description: TypeScript implementation of devops-ci-toolchain, also used by Angular workspaces — the ready setup-toolchain composite action that installs the Node.js version the project declares in `engines.node` of the root `package.json`, with a committed `package-lock.json`, with the dependency cache
whenToUse: when a TypeScript or Angular project needs `.github/actions/setup-toolchain/action.yml`, or when a CI job of such a project runs on a toolchain version other than the project's
updated: 20261010
tags:
  - stack/typescript
  - concern/ci
  - github-actions
---

# Goal
- `.github/actions/setup-toolchain/action.yml`, an unchanged copy of this skill's asset.
- The toolchain version declared in `engines.node` of the root `package.json`, with a committed `package-lock.json`.

# Core Principle
- This skill details [[skills/devops/core/devops-ci-toolchain.skill.md|devops-ci-toolchain]] for TypeScript and Angular; apply both.

# Rule

## MUST

### Copy action.yml verbatim
Copy [[./assets/.github/actions/setup-toolchain/action.yml|action.yml]] verbatim to `.github/actions/setup-toolchain/action.yml`; do not modify it.
- Risk: a version typed into the action parts from the one developers use.
- Fix: restore the file from the asset and change the version where the project declares it.

### Declare the toolchain version in the project
Declare the toolchain version in `engines.node` of the root `package.json`, with a committed `package-lock.json`.
- Violation: a `package.json` without `engines.node`, or no lock file for the cache to key on.
- Risk: the action finds no version and the job fails, or installs the newest one and the tests run on a toolchain no developer has.
- Fix: add the declaration; the action and the dev container read the same file.

# Check list
- [ ] `.github/actions/setup-toolchain/action.yml` is byte-identical to this skill's asset.
- [ ] `engines.node` of the root `package.json`, with a committed `package-lock.json` declares the toolchain version.
