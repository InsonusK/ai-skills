---
name: root-package-json-for-workspaces
description: Where the version of a TypeScript repository with several packages — an Angular or Nx workspace — is recorded
problem: An Angular library workspace publishes `projects/{library}/package.json`, while its root `package.json` is private. Which file records the version that `make version` prints, and does Angular need a version skill of its own?
decision: The root `package.json` records the one version of the repository; workspace packages are stamped from it at build time; Angular uses the TypeScript skill.
tags:
  - stack/typescript
  - framework/angular
  - concern/ci
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`read-version.sh` is copied verbatim, so it cannot hold a project-specific path. An Angular library workspace keeps the published package's manifest under `projects/{library}/`, an Nx workspace may hold many packages, and a plain TypeScript project has only the root manifest. Which file records the version?

# Selected variant
[[#Root package.json, packages stamped at build]]

# Searched variants

## Root package.json, packages stamped at build

**Selected.**

### Description
The version is the `version` field of the root `package.json` in every TypeScript repository. A package built from a workspace project receives it in its build output before publishing.

### Benefits
- One reader for TypeScript, Angular, and Nx; no Angular version skill.
- One number to raise and to check in a pull request.

### Costs
- A workspace package's own manifest shows `0.0.0` in the source tree.
- The publish step must stamp the built package.

## The published package's own manifest

### Description
`read-version.sh` reads `projects/{library}/package.json`; the path is a placeholder filled per project.

### Benefits
- The source manifest shows the published version.

### Costs
- The reader becomes a template, different in every project.
- A workspace with two libraries has two versions and no answer to `make version`.

## A version skill per framework

### Description
`devops-project-version-in-angular` with its own reader next to the TypeScript one.

### Benefits
- Each repository shape has its own instructions.

### Costs
- Two skills that differ only in a path, and a third for Nx.
