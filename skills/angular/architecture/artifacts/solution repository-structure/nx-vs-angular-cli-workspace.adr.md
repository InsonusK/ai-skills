---
name: nx-vs-angular-cli-workspace
description: Choice of tooling for the base Angular workspace layout
problem: Whether to use a plain Angular CLI multi-project workspace or Nx as the workspace tool for the base repository structure
decision: Use Nx as the workspace tool
---

# Problem

The repository will host, from the start, more than one deployable unit (platform shell + embeddable applications, see the "Встраиваемость платформы" solution) and a growing number of shared/feature libraries. We need to pick the workspace tooling that will host `apps/` and `libs/` before any project-level or file-level conventions can be defined, since it determines what a "project" even is (`angular.json` entry vs `project.json` + Nx graph node) and what tooling is available for enforcing boundaries and scaling CI.

# Selected variant

**Selected variant:** [[#Nx monorepo]]

Nx is selected as the base workspace tool. It provides everything a plain Angular CLI multi-project workspace does, plus affected-based builds/tests, task caching, enforced module boundaries via project tags, a dependency graph, and ready-made generators for Module Federation — all of which are needed by solutions planned right after this one (embeddability, offline-first).

# Searched variants

## Nx monorepo

### Description

Use Nx as the workspace tool. Each app/lib is its own Nx project with a `project.json`, tagged with a `type:*`/`scope:*` taxonomy that ESLint's `@nx/enforce-module-boundaries` rule checks on every lint run.

### Benefits

- `nx affected` runs build/lint/test only for projects impacted by a change, instead of the whole workspace
- Task results are cached locally and can be shared across CI runs
- `@nx/enforce-module-boundaries` turns architectural boundaries into a lint failure instead of a code-review convention
- `nx graph` visualizes dependencies between projects, which maps directly onto the platform/embedded-app split planned for the next solution
- Ready-made generators for Module Federation host/remote setups, needed for the embeddability solution

### Costs

- Extra concept to learn: Nx's own generators and `project.json` are not identical to the stock Angular CLI schematics
- Slight overhead for a workspace that stays small and never grows a second deployable unit

## Angular CLI workspace (multi-project, no Nx)

### Description

A single `angular.json` lists every app/lib project. No additional tool sits on top of the Angular CLI.

### Benefits

- No extra dependency or concept beyond the standard Angular toolchain
- Lowest possible learning curve for engineers who only know stock Angular CLI

### Costs

- No affected-detection: every CI run builds/tests/lints the entire workspace regardless of what changed
- No built-in enforcement of dependency boundaries between projects — a feature can import another feature's internals with nothing stopping it except review discipline
- No dependency graph tooling
- No ready-made Module Federation generators — the embeddability solution would need to configure everything by hand
