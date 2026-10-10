---
name: devops-project-version-in-typescript
description: TypeScript implementation of devops-project-version, also used by Angular workspaces — the version recorded in the version field of the root package.json and the read-version.sh that prints it
whenToUse: when a TypeScript or Angular project needs `make version`/`make version-check`, or when you add or review the `version` of its root `package.json` or `tools/version/read-version.sh`
updated: 20261010
tags:
  - stack/typescript
  - concern/ci
  - versioning
adr:
  - adr/root-package-json-for-workspaces.md
---

# Goal
- The `version` field of the root `package.json` holding the project's version.
- `tools/version/read-version.sh`, an unchanged copy of this skill's asset.
- In a workspace of several packages, no other `package.json` whose version is raised by hand.

# Core Principle
- This skill details [[skills/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]] for TypeScript; apply both.
- **One version per repository** - A repository has one version even when it holds several packages — an Angular or Nx workspace included. Decision recorded in [[./adr/root-package-json-for-workspaces.md|root-package-json-for-workspaces]].

# Rule

## MUST

### Record the version in the root package.json
Keep the project's version in the `version` field of the `package.json` at the repository root, also when that file is `"private": true`.
- Violation: a root `package.json` without `version`, the number kept in `projects/{library}/package.json` or a `VERSION` file.
- Risk: `make version` prints `undefined` and refuses it, or reads a version the published package does not carry.
- Fix: add `"version": "1.4.0"` to the root `package.json`.

### Copy read-version.sh verbatim
Copy [[./assets/tools/version/read-version.sh|read-version.sh]] verbatim to `tools/version/read-version.sh`; do not modify it.
- Risk: a `grep` for `"version"` also matches a dependency entry.
- Fix: restore the file from the asset; it parses the file with Node.

### Stamp workspace packages at build time
Set the version of a package built from a workspace project from `make -s version` in its build output, never in the project's own `package.json`.
- Violation: `projects/{library}/package.json` raised by hand next to the root one.
- Risk: the two numbers part on the first forgotten bump, and the published package carries a version that was never checked.
- Fix: keep `0.0.0` in the project's `package.json` and run `npm pkg set version="$(make -s version)"` in the built package folder before publishing.

# Check list
- [ ] The root `package.json` has a `version` field.
- [ ] `tools/version/read-version.sh` is byte-identical to this skill's asset.
- [ ] `make -s version` prints that field.
- [ ] No other `package.json` in the repository carries a hand-raised version.
