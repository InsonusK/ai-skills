---
version: 20261009220000
description: Step definitions binding a Gherkin feature file to a pipeline behavior's technical contract
project_name: "BuildingBlocks.Tests"
name: "{Rule}Steps"
element_kind: class
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/buildingblocks-tests-rulesteps

---

# Goals
- Prove a `BuildingBlocks` pipeline behavior's technical contract — e.g. that `ExceptionHandlingBehavior` catches an unhandled exception and returns a generic error without leaking details — as a scenario, per [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]]'s "technical and architectural functions are described through Cucumber/Gherkin scenarios too" principle.

# Core Principles
- `BuildingBlocks` scenarios are technical-contract-shaped, not validation-shaped: given a pipeline condition (e.g. "the inner handler throws"), prove the behavior's observable contract (e.g. "a generic error is returned, nothing leaks") — never a business rule, since `BuildingBlocks` has none.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one behavior's scenarios | {Rule}Steps | ExceptionHandlingBehaviorSteps | {Rule}Steps.cs | ExceptionHandlingBehaviorSteps.cs |

# Implementation changes
Apply [binding organization](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#one-binding-class-per-domain-concept), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [concrete Result assertions](skills/testing/dotnet/no-test-theater-in-dotnet.skill.md#assert-the-concrete-ardalisresult-state). The layer-specific action and observation are:

- Invoke the pipeline behavior with a controlled next delegate and observe the returned result and whether exception details escape.

# Rule changes

## MUST
- Assert the observable technical contract (e.g. no exception details leak), not implementation detail.
  - Risk: asserting internal detail (e.g. exact log message wording) makes the scenario brittle to harmless refactors, unrelated to the contract it exists to prove.
  - Fix: assert only what the contract promises — here, a generic error with no leaked exception detail.

# Check list
- [ ] The layer-specific action and observation match this project's responsibility; generic binding rules are applied.

# Unittest TestCases
- [ ] WHEN the inner handler throws THEN the behavior returns a generic error, never re-throwing.
- [ ] WHEN the inner handler throws THEN no exception message/stack trace appears in the returned `Result`.
