---
version: 20261009220000
description: Create the catalog test project for {Module}.Application and define its reference boundary
name: "{Module}.Application.Tests"
element_kind: project
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/module-application-tests

---

# Goals
- Give `{Module}.Application` a dedicated test project, referencing exactly what `{Module}.Application.csproj` itself is allowed to reference.

# Core Principles
- Step definitions are command-shaped, not validator-shaped — see [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]: a command goes in, a `Result` comes out, proven against the real handler, never `{Module}.Domain`'s types directly.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for the project contents, packages and runner configuration.

# Allowed Dependencies
- `{Module}.Application`, `{Module}.Domain` — the same two projects `{Module}.Application.csproj` itself is allowed to reference (per `solution-sln-structure`). No other module's project.

# Rules

## MUST
- Reference `{Module}.Application` and `{Module}.Domain` only — no other module's project, no `{Module}.Interfaces`-only shortcut around `{Module}.Application`'s own boundary.
  - Risk: referencing more than `{Module}.Application` itself is allowed to reach lets a test pass by exercising code that would be an architectural violation in production.
  - Fix: keep this project's references to exactly `{Module}.Application` and `{Module}.Domain`.
- Step definitions must call `{Module}.Application`'s real handlers/validators — never `{Module}.Domain`'s types directly.
  - Risk: calling `{Module}.Domain` directly bypasses the orchestration `{Module}.Application` is responsible for, proving the wrong layer.
  - Fix: call the handler/validator under test the same way a real caller would.

# Check list
- [ ] `{Module}.Application.Tests.csproj` references only `{Module}.Application` and `{Module}.Domain`.
- [ ] Step definitions call `{Module}.Application`'s handlers/validators, not `{Module}.Domain` directly.
