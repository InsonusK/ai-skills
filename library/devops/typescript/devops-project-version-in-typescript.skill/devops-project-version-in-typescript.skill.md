---
name: devops-project-version-in-typescript
description: TypeScript implementation of devops-project-version, also used by Angular workspaces — the read-version.sh that prints the version field of the root package.json, where project-version-in-typescript records the version, and the stamping of built workspace packages with it
whenToUse: when a TypeScript or Angular project needs `make version`/`make version-check`, or when you add or review the `version` of its root `package.json` or `tools/version/read-version.sh`
updated: 20261025
tags:
  - stack/typescript
  - concern/ci
  - versioning
---

# Goal
- The version recorded per [project-version-in-typescript](library/develop/typescript/project-version-in-typescript.skill/project-version-in-typescript.skill.md): the `version` field of the root `package.json`.
- `tools/version/read-version.sh`, an unchanged copy of this skill's asset.
- In a workspace of several packages, no other `package.json` whose version is raised by hand.

# Core Principle
- This skill details [[library/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]] for TypeScript; apply both.
- **Reads what project-version-in-typescript records** - Where the version lives and that workspace packages keep `0.0.0` are [project-version-in-typescript](library/develop/typescript/project-version-in-typescript.skill/project-version-in-typescript.skill.md)'s; this skill reads it and stamps built packages with it.

# Rule

## MUST

### Copy read-version.sh verbatim
Copy [[./assets/tools/version/read-version.sh|read-version.sh]] verbatim to `tools/version/read-version.sh`; do not modify it.
- Risk: a `grep` for `"version"` also matches a dependency entry.
- Fix: restore the file from the asset; it parses the file with Node.

```
"version": "1.4.0"          ← the root `package.json`, recorded per project-version-in-typescript
$ sh tools/version/read-version.sh
1.4.0
```

### Stamp workspace packages at build time
Set the version of a package built from a workspace project from `make -s version` in its build output, never in the project's own `package.json`.
- Violation: `projects/{library}/package.json` raised by hand next to the root one.
- Risk: the two numbers part on the first forgotten bump, and the published package carries a version that was never checked.
- Fix: keep `0.0.0` in the project's `package.json` and run `npm pkg set version="$(make -s version)"` in the built package folder before publishing.

# Check list
- [ ] `tools/version/read-version.sh` is byte-identical to this skill's asset.
- [ ] `make -s version` prints the `version` of the root `package.json`.
- [ ] No other `package.json` in the repository carries a hand-raised version.
