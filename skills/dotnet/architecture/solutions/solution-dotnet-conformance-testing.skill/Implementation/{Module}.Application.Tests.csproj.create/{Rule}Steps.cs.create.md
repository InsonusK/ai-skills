---
version: 20261009220000
description: Step definitions binding a Gherkin feature file to a command handler's orchestration
project_name: "{Module}.Application.Tests"
name: "{Rule}Steps"
element_kind: class
change_kind: create
tags:
  - solution/dotnet-conformance-testing
  - element/module-application-tests-rulesteps

---

# Goals
- Prove every scenario in the owning feature file against `{Module}.Application`'s real handler — that it loads the right state, calls the right guarded domain method, and returns the expected `Result`.

# Core Principles
- Unlike `{Module}.Domain.Tests`' validator-shaped scenarios (input → valid/invalid), Application scenarios are command-shaped: a command goes in, a `Result` comes out.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Step definitions for one handler's scenarios | {Rule}Steps | ChangeCustomerEmailSteps | {Rule}Steps.cs | ChangeCustomerEmailSteps.cs |

# Implementation changes
Apply [binding organization](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#one-binding-class-per-domain-concept), [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md#must) and [concrete Result assertions](skills/testing/dotnet/no-test-theater-in-dotnet.skill.md#assert-the-concrete-ardalisresult-state). The layer-specific action and observation are:

- Construct the command, invoke `Handler.Handle(command, cancellationToken)` and observe the returned `Result` and any contracted collaborator calls. Do not substitute a direct Domain call for handler orchestration.

# Rule changes

## MUST
- Apply [production-code bindings](skills/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md#exercise-production-code-from-bindings).

# Check list
- [ ] The layer-specific action and observation match this project's responsibility; generic binding rules are applied.

# Unittest TestCases
- [ ] WHEN a scenario's command is valid THEN the handler returns the contracted success payload.
- [ ] WHEN a scenario's command is invalid THEN the handler returns the contracted failure result.
