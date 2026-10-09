---
version: 20261009220000
description: Step definitions binding a Gherkin feature file to the real production code being proven — validator-shaped scenarios for the Domain layer
project_name: "{Module}.Domain.Tests"
name: "{Rule}Steps"
element_kind: class
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/module-domain-tests-rulesteps

---

# Goals
- Prove every scenario in the owning feature file against `{Module}.Domain`'s real implementation of the rule.

# Core Principles
- Domain scenarios are validator-shaped: input → validation result; layer entry points differ across the catalog.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one rule | {Rule}Steps | EmailFormatSteps | {Rule}Steps.cs | EmailFormatSteps.cs |

# Implementation changes
Apply [binding organization](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#one-binding-class-per-domain-concept), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [concrete Result assertions](skills/testing/dotnet/no-test-theater-in-dotnet.skill.md#assert-the-concrete-ardalisresult-state). The layer-specific action and observation are:

- Invoke the Domain validator/entity invariant with scenario input and observe its validation result. Application and Interfaces entry points are outside this test project.

# Rule changes

## MUST
- Apply [production-code bindings](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#exercise-production-code-from-bindings).

# Check list
- [ ] The layer-specific action and observation match this project's responsibility; generic binding rules are applied.

# Unittest TestCases
- [ ] WHEN a scenario's input is valid THEN the Domain validation result is valid.
- [ ] WHEN a scenario's input is invalid THEN the Domain validation result identifies the violated invariant.
