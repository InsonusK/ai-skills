---
name: plateau-offline-sync-service--class-module-interfaces-rule-steps
description: Class {Rule}Steps in {Module}.Interfaces.Tests of the plateau-offline-sync-service plateau — Reqnroll bindings pinning a public contract's shape against the real declared type
whenToUse: when writing the step definitions for a {Module}.Interfaces.Tests feature file, or adding a scenario that pins a contract's marker or fields
domain: skill
type: template
plateau: offline-sync-service
version: 20261009220001
tags:
  - skill/template/class
  - plateau/offline-sync-service
created_by:
  - "[[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]]"
---

# Goal
- Prove every scenario in `features/{Rule}.feature` against the module's real public contract types — a command implements the right marker, a DTO carries the expected fields.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Core Principles
- Apply the linked [testing conventions](#testing-conventions).
- Apply ONE plateau template per class.
- Contract-shaped: construct the declared type, assert it is assignable to the right marker / exposes the right members.
- Contract scenarios enter through the module's declared Interfaces types; supporting references follow the owning test project's mirrored boundary.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Step definitions for one contract group | `{Rule}Steps` | `ContractsSteps` | `{Rule}Steps.cs` | `ContractsSteps.cs` |

# Testing conventions
Apply [test-project layout](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#keep-tests-in-separate-test-projects), [binding mechanics](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [solution-conformance-testing-in-dotnet](skills/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md) for layout, bindings, assertions, packages and runner configuration.

# Implementation
The layer-specific action and observation follow the contributing solution linked below; generic binding code comes from the testing skills.

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/solution-dotnet-conformance-testing.skill|solution-dotnet-conformance-testing]] - [[skills/dotnet/architecture/solutions/solution-dotnet-conformance-testing.skill/Implementation/{Module}.Interfaces.Tests.csproj.create/{Rule}Steps.cs.create|{Rule}Steps.cs]]

# Rules
MUST:
- Apply [production-code bindings](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#exercise-production-code-from-bindings) and [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must).
- Never reference `{Module}.Application` or `{Module}.Domain`.
- Never apply several plateau templates per class.

# Check list
- [ ] Only `{Module}.Interfaces` types are referenced.

# Unittest TestCases
- [ ] WHEN the feature runs THEN each scenario constructs a real contract type and its assertion passes.
