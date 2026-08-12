---
name: no-test-theater-dotnet
description: xUnit/.NET-specific rules for assertion strength — Ardalis.Result status checks, branch vs. line coverage, Stryker.NET mutation testing, and integration-test requirements per API endpoint.
whenToUse: When writing or reviewing xUnit tests in a .NET project.
tags:
  - stack/dotnet
  - concern/testing/unit
  - xunit
  - concern/testing

---

# Goal
- Add .NET/xUnit-specific rules on top of the language-agnostic test-quality rules.

# Scope
This skill extends [no-test-theater](skills/common-workflow/test/no-test-theater.skill/no-test-theater.skill.md) — apply both together; this skill only adds .NET-specific rules. It does not define test class/file naming or folder layout — see [dotnet-unittest](skills/dotnet/testing/dotnet-unittest.skill/dotnet-unittest.skill.md) for that, and [testing-strategy](skills/dotnet/testing/testing-strategy.skill.md) for which classes/usecases must have their own dedicated test class.

# Core Principle
- `IsSuccess == false` and "no exception thrown" are not assertions — they hide which of several failure states actually happened.

# Rule

## MUST
- Follow the naming convention `MethodOrScenario_ExpectedBehavior_WhenCondition`, or BDD style via `[Fact(DisplayName = "...")]` with a full sentence.
- For methods returning `Ardalis.Result`, assert the concrete `Result.Status` (`Ok`, `NotFound`, `Invalid`, `Conflict`, etc.) — not only `result.IsSuccess == false`.
- For `Invalid` results, assert the concrete `ValidationErrors` (field + message) — not only their count.
- Measure coverage as branch coverage via `coverlet` + `reportgenerator`, not line coverage — line coverage does not show that only one side of an `if/else` was exercised.
- Add an integration test (`WebApplicationFactory` / Testcontainers) for every new API endpoint, covering at minimum the happy path plus one applicable error path (401/404/409/422).
- For a usecase/workflow test (inbound sync call, inbound async message, cron job) that orchestrates several components, assert both the full expected response and that the components were called in the expected order — not just that each method was called at some point.

## SHOULD
- Run mutation testing via **Stryker.NET** on core business logic (skip DTOs/mappers). Treat a mutation score below 60% on changed files as a review trigger, not an automatic blocker — tune the exact threshold per project.

# Anti-patterns
- **Checking only `IsSuccess == false`**
  - Example: `Assert.False(result.IsSuccess)` for a test named `Update_ReturnsNotFound_WhenMissing`.
  - Consequence: the test also passes if the result is `Invalid` or `Conflict` instead of `NotFound` — it does not prove what its name claims.
  - Instead: `Assert.Equal(ResultStatus.NotFound, result.Status)`.

- **Counting `ValidationErrors` instead of checking them**
  - Example: `Assert.Single(result.ValidationErrors)`.
  - Consequence: passes even if the single error is for the wrong field or has the wrong message.
  - Instead: assert the specific field name and message.

- **Trusting line coverage for conditional logic**
  - Example: 100% line coverage on a method with an `if/else`, but only the `if` branch has a test.
  - Consequence: the `else` branch can be broken with no test failing.
  - Instead: use branch coverage reports and add a test for each branch.

- **Call order asserted only as "was called", not "called in order"**
  - Example: `mock.Verify(x => x.StepA()); mock.Verify(x => x.StepB());` with no ordering check, for a usecase where `StepA` must run before `StepB`.
  - Consequence: passes even if `StepB` runs before `StepA`, which can be the actual orchestration bug.
  - Instead: use an ordered verification (e.g. Moq `MockSequence`, NSubstitute `Received.InOrder`) to assert the call sequence.

# Check list
- [ ] Every `Ardalis.Result`-returning test asserts a concrete `Result.Status`, not just `IsSuccess`.
- [ ] Every `Invalid`-result test asserts specific `ValidationErrors` (field + message).
- [ ] Coverage reviewed is branch coverage, not line coverage.
- [ ] Every new API endpoint has an integration test for the happy path and at least one error path.
- [ ] Usecase/workflow tests assert both the full response and the call order of orchestrated components.
- [ ] Mutation testing (where configured) reviewed for changed files.
