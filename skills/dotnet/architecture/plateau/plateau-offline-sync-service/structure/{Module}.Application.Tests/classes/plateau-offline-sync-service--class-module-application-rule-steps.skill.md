---
name: plateau-offline-sync-service--class-module-application-rule-steps
description: Class {Rule}Steps in {Module}.Application.Tests of the plateau-offline-sync-service plateau — Reqnroll bindings driving a real handler or validator and asserting its Result
whenToUse: when writing the step definitions for a {Module}.Application.Tests feature file, or adding a scenario for a handler or validator
domain: skill
type: template
plateau: offline-sync-service
version: 20261009220000
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
---

# Goal
- Prove every scenario in `features/{Rule}.feature` against `{Module}.Application`'s real handler / validator — that it shapes and dispatches correctly and returns the expected `Result`, or that the validator fails the right rule.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Apply ONE plateau template per class.
- Command-shaped: a command goes in, a `Result` comes out, asserted against the real handler.
- Application collaborator seams include `IPublisher`; the handler scenario can use a no-op publisher when notification effects are outside its claim.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Step definitions for one handler / validator | `{Rule}Steps` | `GreetSteps` | `{Rule}Steps.cs` | `GreetSteps.cs` |

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Implementation
The layer-specific action and observation follow the contributing solution linked below; generic binding code comes from the testing skills.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Application.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Rules
MUST:
- Apply [production-code bindings](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#exercise-production-code-from-bindings) and [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must).
- Never apply several plateau templates per class.

# Check list
- [ ] The real handler/validator is invoked; assertions target its actual output.

# Unittest TestCases
- [ ] WHEN a scenario's command is valid THEN the real handler returns the expected `Result`.
- [ ] WHEN a scenario's command is invalid THEN the assertion names the exact error the real handler/validator returns.
