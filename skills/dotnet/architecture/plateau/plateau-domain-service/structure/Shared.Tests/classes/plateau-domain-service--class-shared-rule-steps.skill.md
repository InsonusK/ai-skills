---
name: plateau-domain-service--class-shared-rule-steps
description: Class {Rule}Steps in Shared.Tests of the plateau-domain-service plateau — Reqnroll bindings proving one Shared primitive's scenarios against the real type
whenToUse: when writing the step definitions for a Shared.Tests feature file, or adding a scenario for a Shared primitive
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
- Prove every scenario in `features/{Rule}.feature` against `Shared`'s real primitive / marker behavior.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Apply ONE plateau template per class.
- Value-shaped: given one or more primitive values, prove how they compare/combine — never "is this input valid" (a module concern).

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Step definitions for one primitive | `{Rule}Steps` | `CommandMarkersSteps` | `{Rule}Steps.cs` | `CommandMarkersSteps.cs` |

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Implementation
The layer-specific action and observation follow the contributing solution linked below; generic binding code comes from the testing skills.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/Shared.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Rules
MUST:
- Apply [production-code bindings](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#exercise-production-code-from-bindings) and [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must).
- Keep scenarios cross-cutting — never a module-specific concept.
- Never apply several plateau templates per class.

# Check list
- [ ] Assertions target the real `Shared` type.

# Unittest TestCases
- [ ] WHEN the feature runs THEN every step binds and the scenario passes.
