---
version: 20261009220000
description: Step definitions binding a Gherkin feature file to a Shared primitive's behavior
project_name: "Shared.Tests"
name: "{Rule}Steps"
element_kind: class
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/shared-tests-rulesteps

---

# Goals
- Prove every scenario in the owning feature file against `Shared`'s real primitive/result-helper behavior.

# Core Principles
- `Shared` holds cross-cutting primitives, not business or orchestration logic, so its scenarios are value-shaped: given one or more primitive values, prove how they compare or combine — never "is this input valid" (that belongs to a module's own Domain/Application).

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one primitive's scenarios | {Rule}Steps | ConflictResultSteps | {Rule}Steps.cs | ConflictResultSteps.cs |

# Implementation changes
Apply [binding organization](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#one-binding-class-per-domain-concept), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [concrete Result assertions](skills/testing/dotnet/no-test-theater-in-dotnet.skill.md#assert-the-concrete-ardalisresult-state). The layer-specific action and observation are:

- Construct the cross-cutting primitive values and observe comparison or combination; module-specific validity belongs to that module.

# Rule changes

## MUST
- Never introduce a module-specific concept into a `Shared.Tests` scenario.
  - Risk: a module-specific scenario here would only be discoverable by someone browsing `Shared`, not the module it actually concerns.
  - Fix: keep `Shared.Tests` scenarios scoped to genuinely cross-cutting primitives.

# Check list
- [ ] The layer-specific action and observation match this project's responsibility; generic binding rules are applied.

# Unittest TestCases
- [ ] WHEN two equal values are compared THEN `ThenEqual` passes.
- [ ] WHEN two different values are compared THEN `ThenDifferent` passes.
