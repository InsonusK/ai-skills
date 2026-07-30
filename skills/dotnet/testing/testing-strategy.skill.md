---
name: testing-strategy
description: Defines which .NET classes and workflows require their own dedicated unit test — Validators, ValueObjects, Entities, and usecases — and the isolation boundary between them.
whenToUse: When deciding whether a .NET class needs its own dedicated test class, or reviewing whether a Validator/ValueObject/Entity is tested only indirectly through another component's test.
tags:
  - unit-testing
  - dotnet
---

# Goal
- Define which .NET classes and workflows must have their own dedicated unit test, and which may be covered indirectly through another component's test.

# Scope
This skill defines test *scope and isolation* — which classes/workflows need a dedicated test, and where mocking sub-components is allowed. It does not define assertion strength, coverage tooling, or which scenario types (happy/boundary/negative/error/...) to enumerate — see [no-test-theater](skills/common-workflow/test/no-test-theater.skill/no-test-theater.skill.md) and [no-test-theater-dotnet](skills/dotnet/testing/no-test-theater-dotnet.skill.md) for that.

# Core Principle
- Validators, ValueObjects, and Entities own their validation logic and must be proven correct in their own test, not only as a side effect of another component's test.
- A usecase (inbound sync call, inbound async message, cron job) is its own test unit: its test proves the whole orchestration, not just the individual components it calls.

# Rule

## MUST
- Give every Validator, ValueObject, and Entity its own dedicated test class — do not rely on another component's test to exercise its validation logic.
- In each Validator/ValueObject/Entity test, assert every distinct value or combination of values that changes behavior, not just one representative valid case and one representative invalid case. Use [no-test-theater](skills/common-workflow/test/no-test-theater.skill/no-test-theater.skill.md) to decide which scenario types (happy/boundary/negative/...) apply.
- Give every usecase (inbound sync call, inbound async message, cron job) its own complex test covering the main success case and the most important invalid cases. See [no-test-theater-dotnet](skills/dotnet/testing/no-test-theater-dotnet.skill.md) for how such a test must assert the response and orchestration call order.
- Cover other components (not Validators/ValueObjects/Entities/usecases) the same as any other unit: main cases and edge cases.

## MAY
- Mock sub-validators, sub-ValueObjects, or sub-Entities when unit-testing a Validator/ValueObject/Entity that composes them, to keep the test isolated to that unit's own logic.

# Anti-patterns
- **Validation logic tested only through another component**
  - Example: `OrderValidator`'s "negative quantity" rule is only exercised inside `OrderService_Test`, with no `OrderValidator_Test`.
  - Consequence: a change to `OrderValidator` that breaks one of its rules stays invisible until something downstream happens to hit that exact path; most of `OrderValidator`'s own logic stays unverified.
  - Instead: give `OrderValidator` its own test class covering its own scenarios directly.

- **One representative case instead of every distinguishing value**
  - Example: an enum-based validator tested with one valid and one invalid enum value, when the enum has 5 members with different rules.
  - Consequence: 3 of 5 enum branches are never executed by any test.
  - Instead: assert every value/combination that changes behavior.

# Check list
- [ ] Every Validator, ValueObject, and Entity has its own test class.
- [ ] Every distinguishing value/combination for a Validator/ValueObject/Entity has an assertion, not just one representative valid and one representative invalid case.
- [ ] Every usecase (inbound sync/async, cron job) has its own test.