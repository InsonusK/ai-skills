---
name: version-behind-make
description: How CI and developers read a project's version and check that it was raised
problem: Each stack had its own check-version composite action — bash, Python, PowerShell — that read the manifest and compared versions on the runner only. How should the version be read and compared so the same check runs locally and is written once?
decision: A shared `tools/version/` folder behind `make version` and `make version-check`; the comparison is one script for every stack, and only `read-version.sh` differs.
tags:
  - stack
  - concern/ci
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`check-version` existed as four composite actions with four comparison implementations (`sort -V`, `packaging.Version`, `System.Version`, `semver`), each reading its manifest at the current commit and, by a second code path, at the base ref. None ran outside GitHub Actions. How should the version be read and compared?

# Selected variant
[[#Shared scripts behind make, one reader per stack]]

# Searched variants

## Shared scripts behind make, one reader per stack

**Selected.**

### Description
`version.mk` and `version.sh` are identical in every project. `version.sh` validates `MAJOR.MINOR.PATCH`, compares numerically, and reads the base version by running the same `read-version.sh` in a detached worktree of `DELTA_BASE`. A stack ships only `read-version.sh`.

### Benefits
- The check runs locally and in any CI system.
- One comparison for every stack; a stack adds a few lines.
- The place of the version is written once — the base is read by the same reader.

### Costs
- Three files in every project.
- A pre-release version in the source is not supported.
- The base is checked out into a temporary worktree on every check.

## Composite action per stack

### Description
`.github/actions/check-version/action.yml`, written per stack, with outputs `current`, `bumped`, `publishable`.

### Benefits
- No files outside `.github/`.
- Each stack uses its ecosystem's version parser, pre-releases included.

### Costs
- Runs only on GitHub; a mistake is found after a push.
- Four comparison implementations and two read paths each.

## Git tag as the version

### Description
The latest `v*` tag is the version; nothing is recorded in the tree.

### Benefits
- No version file and no script.

### Costs
- A pull request has nothing to raise, so the version is chosen outside review.
- A build from a checkout without tags has no version.
