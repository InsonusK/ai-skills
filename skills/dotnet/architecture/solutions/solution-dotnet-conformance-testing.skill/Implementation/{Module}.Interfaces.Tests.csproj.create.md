---
version: 20261009220000
description: Create the catalog test project for {Module}.Interfaces and define its reference boundary
name: "{Module}.Interfaces.Tests"
element_kind: project
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/module-interfaces-tests

---

# Goals
- Give `{Module}.Interfaces` a dedicated test project, referencing `{Module}.Interfaces` only.

# Core Principles
- Step definitions are shape-shaped, not validator-shaped — see [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]: `{Module}.Interfaces` is declarations-only, so scenarios prove equality/serialization round-trip, never "is this input valid".

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for the project contents, packages and runner configuration.

# Allowed Dependencies
- `{Module}.Interfaces` — nothing else. `{Module}.Interfaces.Tests` may reference exactly what `{Module}.Interfaces.csproj` itself is allowed to reference (per `solution-sln-structure`: nothing), plus `{Module}.Interfaces` itself.

# Rules

## MUST
- Reference `{Module}.Interfaces` and nothing else.
  - Risk: referencing `{Module}.Application` or `{Module}.Domain` here would let a contract test depend on implementation details it is meant to be isolated from.
  - Fix: keep this project scoped to `{Module}.Interfaces`'s own declarations.

# Check list
- [ ] `{Module}.Interfaces.Tests.csproj` references only `{Module}.Interfaces`.
