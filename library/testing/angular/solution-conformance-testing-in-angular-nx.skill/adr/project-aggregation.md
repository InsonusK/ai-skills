---
name: project-aggregation
description: How a kind knows which Nx projects it applies to and how their results become one report
problem: One repository holds several projects and the contract gives it one badge per kind - which projects does a kind run, and how are their results joined
decision: Applicability is read from the workspace as Nx sees it - a feature file, the standard test target, the standard e2e target; one run per project, one aggregated result
updated: 20261009
tags:
  - solution/conformance-testing-in-angular-nx
  - stack/typescript
  - framework/angular
  - concern/documentation
  - concern/documentation/adr
---

# Problem
One repository holds several projects, and the caller contract gives it one report with one badge per kind. Decide how a kind knows which projects it applies to, and how their results become one.

# Selected variant
**Selected variant:** [[#Read applicability from the workspace]]

# Searched variants

## Read applicability from the workspace

**Selected.**

### Description
A kind asks Nx for the projects and reads what each already has: `unit` applies where a project holds a `.feature` file, `components` where it has the standard `test` target, `ui` where it has the standard `e2e` target. Each selected project runs by itself into a directory of its own; the kind then prefixes every test name with its project and hands one result to the inherited adapter. The five badge names of the base stay.

### Benefits
- A workspace made by the official generators needs no testing declaration in any `project.json`; a new project is picked up when its target appears.
- `nx test {project}` and `nx e2e {project}` keep working for a developer, with the same configuration the kind uses.
- A project that has the target and no test fails its kind: the runner is told an empty suite is an error, and a missing result file is one too.

### Costs
- A project with logic and neither feature file nor target is not noticed by any kind; only the coverage and mutation reports show it.
- One Nx process per selected project instead of one `run-many`.

## Declare applicability in every project

### Description
Every `project.json` carries a block saying which kinds apply and one extra target per kind that the orchestration calls.

### Benefits
- An explicit "this kind does not apply here" for every project; a forgotten project is an error.

### Costs
- About forty lines of configuration per project that no Nx generator writes and no Nx documentation describes; it has to be repeated by hand for every new project.
- Dependency edges and targets drift from what the plugins infer. This was the first version of this skill and was replaced on 2026-10-09 for that reason.

## One badge per project

### Description
Expose each project as its own kind and badge.

### Benefits
- A project's state is visible in the README.

### Costs
- The number of public kinds depends on the workspace, and the README changes with every new library.
