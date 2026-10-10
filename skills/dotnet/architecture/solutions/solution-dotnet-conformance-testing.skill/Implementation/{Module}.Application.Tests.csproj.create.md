---
version: 20261009220001
description: Create the catalog test project for {Module}.Application and define its reference boundary
name: "{Module}.Application.Tests"
element_kind: project
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/module-application-tests

---

# Goals
- Give `{Module}.Application` its catalog-selected test project with its production dependency boundary mirrored.

# Core Principles
- Step definitions are command-shaped, not validator-shaped — see [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]: a command goes in, a `Result` comes out, proven against the real handler, never `{Module}.Domain`'s types directly.

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for the project contents, packages and runner configuration.

# Allowed Dependencies
- Reference `{Module}.Application` and mirror the tested production project's [Allowed Dependencies](skills/dotnet/architecture/solutions/solution-sln-structure.skill/Implementation/{Module}.Application.csproj.create.md#allowed-dependencies), including only contributions of solutions actually applied to it.

# Rules

## MUST
- Mirror the tested production project's [Allowed Dependencies](#allowed-dependencies), plus that production project itself.
  - Risk: an independently maintained list either permits an illegal dependency or forbids a legitimate production dependency.
  - Fix: derive the boundary from the production skill after its applied solution contributions are assembled.
- Step definitions must call `{Module}.Application`'s real handlers/validators — never `{Module}.Domain`'s types directly.
  - Risk: calling `{Module}.Domain` directly bypasses the orchestration `{Module}.Application` is responsible for, proving the wrong layer.
  - Fix: call the handler/validator under test the same way a real caller would.

# Check list
- [ ] Project references stay within the tested production project and its applied Allowed Dependencies.
- [ ] Step definitions call `{Module}.Application`'s handlers/validators, not `{Module}.Domain` directly.
