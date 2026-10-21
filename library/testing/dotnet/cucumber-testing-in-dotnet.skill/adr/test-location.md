---
name: test-location
description: Where a .NET solution keeps its test projects - beside the project each tests, or in a root tests/ tree mirroring src/
problem: The rule said "a {Project}.Tests project beside each production project", while the conformance example and the older dotnet catalog kept every test project in a root tests/ tree; the base plateau in dotnet followed the tree, and the Go and Python skills keep tests beside the code
decision: Each {Project}.Tests sits in the same parent folder as {Project}; no root tests/ tree
tags:
  - stack/dotnet
  - concern/testing
  - concern/documentation
  - concern/documentation/adr
---

# Problem
"Beside" read two ways: one test project per production project (wherever it lies), or the test project physically next to its project. The conformance example (`tests/Linkcheck.Tests`) and the older dotnet catalog took the first; the Go and Python skills take the second for packages (the `test-location` ADR of `cucumber-testing-in-python` measured it).

# Selected variant
[[#Test project beside its project]]
- Owner decision, 2026-10-21: one convention for every stack.

# Searched variants

## Test project beside its project

**Selected.**

### Description
`src/Modules/Todo/Todo.Domain/` and `src/Modules/Todo/Todo.Domain.Tests/`; `src/BuildingBlocks/` and `src/BuildingBlocks.Tests/`. The test project references `..\{Project}\{Project}.csproj`.

### Benefits
- A module and its specification are one folder: moving or deleting the module takes its tests along.
- One convention with Go and Python.
- Nothing in the tooling depends on the folder: `unit.sh` runs the solution, `mutation.sh` skips `*Tests.csproj` wherever it is, Reqnroll formatters write into each project's `bin/`.

### Costs
- Departs from the common dotnet `src/` + `tests/` split; `src/` holds test projects too.
- Settings shared by test projects cannot sit in a `tests/Directory.Build.props`; each test project sets them, or the root props applies them by a condition on `$(MSBuildProjectName.EndsWith('.Tests'))`.
- A CI path filter that separates code from tests matches `*.Tests/` instead of a top-level folder.

## Root tests/ tree mirroring src/

### Description
`tests/{Project}.Tests` for every production project.

### Benefits
- What most dotnet repositories and templates do; test settings in one `tests/Directory.Build.props`.

### Costs
- A project's specification lives in another tree, and the reader mirrors the path by hand.
- Differs from every other stack of the library.
