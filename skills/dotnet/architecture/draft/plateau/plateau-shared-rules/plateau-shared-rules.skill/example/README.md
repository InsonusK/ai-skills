# Example: shared-rules

A runnable example of the `shared-rules` plateau.

This example is built on top of the [`service-with-validated-module-interaction`](../../plateau-service-with-validated-module-interaction/plateau-service-with-validated-module-interaction.skill/plateau-service-with-validated-module-interaction.skill.md) example — it keeps the same fixed project shape, composition root, and conformance-testing contract, and adds a centralized, portable rule mechanism (`Sample.Domain.Rules`) with build-time structural guarantees.

## What the example demonstrates

- `App.Host/Program.cs` builds the host, wires the `Sample` module, and dispatches a `CreateTaskCommand` through MediatR.
- `ValidationBehavior` is registered right after `ExceptionHandlingBehavior` in `AddPipeline()` and short-circuits invalid requests with `Result.Invalid` before any handler runs.
- `Sample.Interfaces` declares `SoftEmail`, `SoftTitle`, `SoftSchedule`, and `CreateTaskCommand`.
- `Sample.Domain.Rules` declares:
  - `TitleRules` — Format rule (single named wrapper `SoftTitle`), checking required and max length.
  - `ScheduleRules` — Semantic rule (named wrapper `SoftSchedule`), checking that `DueDateTime` is not earlier than `StartDateTime`.
- `Sample.Domain` still declares the strict `Email` Value Object and the `TaskItem` entity, but `TaskItem.UpdateTitle` and `TaskItem.UpdateSchedule` now forward to `TitleRules.Check()` and `ScheduleRules.Check()` instead of owning the conditions locally.
- `Sample.Application` declares `CreateTaskHandler`, `CreateTaskValidator`, `EmailPropertyValidator`, `TitlePropertyValidator`, and `SchedulePropertyValidator`, plus `SampleApplicationRegistration` for DI self-registration.
- `Sample.Domain.Rules.Spec` is a non-project directory holding shared Gherkin sources (`TitleRules.feature`, `ScheduleRules.feature`) that are linked into three test projects and proven at each adapter layer.
- `Sample.Domain.Tests` includes Mono.Cecil architecture tests for dead-rule detection, `DomainException` scoping, rejection-code uniqueness/format, and guarded-property rule coverage.
- All production projects still have matching test projects and the full `make unit-test` / `mutation-test` / `test-report` / `test-and-report` conformance gate.

## Run the example

```bash
cd example
make run
```

## Run tests

```bash
make unit-test
make mutation-test
make test-report
make test-and-report
```
