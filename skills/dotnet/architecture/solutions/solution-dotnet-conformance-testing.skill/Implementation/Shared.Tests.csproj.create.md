---
version: 20261009220000
description: Create the catalog test project for Shared and define its reference boundary
name: "Shared.Tests"
element_kind: project
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/shared-tests

---

# Goals
- Give `Shared` a dedicated test project, referencing `Shared` only.

# Core Principles
- Step definitions are value-shaped — see [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]: given one or more primitive values, prove how they compare/combine, never "is this input valid" (that belongs to a module's own Domain/Application). `Shared` is mostly interfaces and primitives today, so its own test suite is small — it grows as `Shared` gains real behavior.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for the project contents, packages and runner configuration.

# Allowed Dependencies
- `Shared` — nothing else, mirroring `Shared.csproj`'s own zero project references.

# Rules

## MUST
- Reference `Shared` and nothing else.
  - Risk: referencing any other project here contradicts `Shared`'s own MUST rule of zero project references, and a test could pass by relying on something `Shared` itself is never allowed to depend on.
  - Fix: keep this project scoped to `Shared` only.

# Check list
- [ ] `Shared.Tests.csproj` references only `Shared`.
