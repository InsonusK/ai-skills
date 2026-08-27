# Example: service-with-validated-module-interaction

A runnable example of the `service-with-validated-module-interaction` plateau.

This example is built on top of the [`stateless-non-interactive-service`](../../plateau-stateless-non-interactive-service/plateau-stateless-non-interactive-service.skill/plateau-stateless-non-interactive-service.skill.md) example — it keeps the same project shape, composition root, and conformance-testing contract, and adds a validated command chain with Value Objects, entity behavior, and FluentValidation.

## What the example demonstrates

- `App.Host/Program.cs` builds the host, wires the `Sample` module, and dispatches both the parent plateau's `GreetCommand` (unchanged) and this plateau's own `CreateTaskCommand` through MediatR — the parent's command chain and project shape (including the `Sample.Api` placeholder project) are carried forward, not replaced; see `adr/restore-sample-api-and-greet-command.md` for why.
- `ValidationBehavior` is registered right after `ExceptionHandlingBehavior` in `AddPipeline()` and short-circuits invalid requests with `Result.Invalid` before any handler runs.
- `Sample.Interfaces` declares `SoftEmail` and `CreateTaskCommand`.
- `Sample.Domain` declares the strict `Email` Value Object and the `TaskItem` entity with guarded behavior.
- `Sample.Application` declares `CreateTaskHandler`, `CreateTaskValidator`, and `EmailPropertyValidator`, plus `SampleApplicationRegistration` for DI self-registration.
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
