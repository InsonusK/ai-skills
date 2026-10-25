---
name: project-version-in-typescript
description: The version of a TypeScript or Angular repository — the version field of the root package.json, one version for the whole workspace, workspace packages carrying 0.0.0 and stamped with the root version at build time
whenToUse: when you add or review the version of a package.json in a TypeScript, Angular, or Nx repository, or when a package built from a workspace project must carry the repository's version
updated: 20261025
tags:
  - stack/typescript
  - concern/coding
  - versioning
adr:
  - adr/root-package-json-for-workspaces.md
---

# Goal
- **The version in TypeScript** - A TypeScript repository that keeps every rule of [project-version](library/develop/core/project-version.skill.md); this skill details it for TypeScript and Angular and restates none of its rules.

# Core Principle
- **One version per repository** - A repository has one version even when it holds several packages, an Angular or Nx workspace included. Decision recorded in [root-package-json-for-workspaces](./adr/root-package-json-for-workspaces.md).

# Rule

## MUST

### Record the version in the root package.json
Keep the version in the `version` field of the `package.json` at the repository root, also when that file is `"private": true`.
- Violation: a root `package.json` without `version`; the number kept in `projects/{library}/package.json` or a `VERSION` file.
- Risk: the tools that read the version find none, or read one the published package does not carry.
- Fix: add the field to the root `package.json`.

```json
{
  "name": "todo-workspace",
  "private": true,
  "version": "1.4.0"
}
```

### Keep 0.0.0 in workspace packages
Keep `"version": "0.0.0"` in the `package.json` of every workspace project, and give the built package the root version at build time.
- Violation: `projects/{library}/package.json` raised by hand next to the root one.
- Risk: the two numbers part on the first forgotten bump, and the published package carries a version that was never checked.
- Fix: the build stamps its output folder with the root version.

```bash
# projects/todo-lib/package.json keeps "version": "0.0.0"
ng build todo-lib
(cd dist/todo-lib && npm pkg set version="$(make -s version)")   # dist/todo-lib/package.json: "version": "1.4.0"
```

# Check list
- [ ] The root `package.json` has a `version` field.
- [ ] Every workspace project's `package.json` holds `0.0.0`.
