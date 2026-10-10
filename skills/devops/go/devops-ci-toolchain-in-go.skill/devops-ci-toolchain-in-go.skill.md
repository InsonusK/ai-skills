---
name: devops-ci-toolchain-in-go
description: Go implementation of devops-ci-toolchain — the ready setup-toolchain composite action that installs the Go version the project declares in the `go` directive of `go.mod`, with the dependency cache
whenToUse: when a Go project needs `.github/actions/setup-toolchain/action.yml`, or when a CI job of a Go project runs on a toolchain version other than the project's
updated: 20261010
tags:
  - stack/go
  - concern/ci
  - github-actions
---

# Goal
- `.github/actions/setup-toolchain/action.yml`, an unchanged copy of this skill's asset.
- The toolchain version declared in the `go` directive of `go.mod`.

# Core Principle
- This skill details [[skills/devops/core/devops-ci-toolchain.skill.md|devops-ci-toolchain]] for Go; apply both.

# Rule

## MUST

### Copy action.yml verbatim
Copy [[./assets/.github/actions/setup-toolchain/action.yml|action.yml]] verbatim to `.github/actions/setup-toolchain/action.yml`; do not modify it.
- Risk: a version typed into the action parts from the one developers use.
- Fix: restore the file from the asset and change the version where the project declares it.

### Declare the toolchain version in the project
Declare the toolchain version in the `go` directive of `go.mod`.
- Violation: a `go-version: "1.26"` input written into the action.
- Risk: the action finds no version and the job fails, or installs the newest one and the tests run on a toolchain no developer has.
- Fix: add the declaration; the action and the dev container read the same file.

# Check list
- [ ] `.github/actions/setup-toolchain/action.yml` is byte-identical to this skill's asset.
- [ ] the `go` directive of `go.mod` declares the toolchain version.
