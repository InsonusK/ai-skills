---
name: plateau-domain-service--class-building-blocks-rule-steps
description: Class {Rule}Steps in BuildingBlocks.Tests of the plateau-domain-service plateau — Reqnroll bindings driving a real pipeline behavior and asserting the returned Result
whenToUse: when writing the step definitions for a BuildingBlocks.Tests feature file, or adding a scenario for a pipeline behavior
domain: skill
type: template
plateau: domain-service
version: 20261009220000
tags:
  - skill/template/class
  - plateau/domain-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
---

# Goal
- Prove every scenario in `features/{Rule}.feature` against a real `BuildingBlocks` pipeline behavior — drive it through a hand-built next-delegate and assert on the returned `Result`.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Apply ONE plateau template per class.
- Supply a `RequestHandlerDelegate<TResponse>` that records whether it ran and/or throws; observe the pipeline result.
- A private sample request `record` (implementing `ICommand<Result<string>>`) stands in for a real command — the behavior is generic.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Step definitions for one behavior | `{Rule}Steps` | `PipelineSteps` | `{Rule}Steps.cs` | `PipelineSteps.cs` |

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Implementation
The layer-specific action and observation follow the contributing solution linked below; generic binding code comes from the testing skills.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/BuildingBlocks.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Rules
MUST:
- Apply [production-code bindings](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#exercise-production-code-from-bindings) and [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must).
- Never apply several plateau templates per class.

# Check list
- [ ] The real behavior class is instantiated and invoked.

# Unittest TestCases
- [ ] WHEN the feature runs THEN each scenario asserts the real behavior's actual `Result`.
