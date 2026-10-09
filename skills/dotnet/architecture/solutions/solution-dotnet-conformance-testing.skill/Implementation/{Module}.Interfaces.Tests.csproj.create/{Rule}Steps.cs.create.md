---
version: 20261009220000
description: Step definitions binding a Gherkin feature file to a DTO/command/query contract's shape
project_name: "{Module}.Interfaces.Tests"
name: "{Rule}Steps"
element_kind: class
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/module-interfaces-tests-rulesteps

---

# Goals
- Prove every scenario in the owning feature file against `{Module}.Interfaces`'s real declaration — that the contract's shape (equality, serialization round-trip) holds, since `{Module}.Interfaces` has no behavior to validate.

# Core Principles
- `{Module}.Interfaces` is declarations-only, so its scenarios are shape-shaped, not validation-shaped: given a value, prove it survives round-tripping (or compares correctly), not that it is "valid".

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one contract's scenarios | {Rule}Steps | ChangeCustomerEmailCommandSteps | {Rule}Steps.cs | ChangeCustomerEmailCommandSteps.cs |

# Implementation changes
Apply [binding organization](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#one-binding-class-per-domain-concept), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [concrete Result assertions](skills/testing/dotnet/no-test-theater-in-dotnet.skill.md#assert-the-concrete-ardalisresult-state). The layer-specific action and observation are:

- Construct the declared DTO/command/query and observe equality or the typed value after a serialization round-trip; validity belongs to Domain/Application.

# Rule changes

## MUST
- Cover equality and serialization round-trip, not validity — validity belongs to `{Module}.Application.Tests`/`{Module}.Domain.Tests`.
  - Risk: duplicating a validation check here creates a second copy of a condition that can drift from the real validator's.
  - Fix: keep scenarios here to shape/equality/serialization only.

# Check list
- [ ] The layer-specific action and observation match this project's responsibility; generic binding rules are applied.

# Unittest TestCases
- [ ] WHEN a value is serialized and deserialized THEN the round-tripped value equals the original.
