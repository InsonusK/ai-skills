# Example: stateless-non-interactive-service

A minimal runnable example of the `stateless-non-interactive-service` plateau.

The solution shows a non-interactive .NET worker that:

- uses the fixed four-project module shape (`Sample.Api`, `Sample.Application`, `Sample.Domain`, `Sample.Interfaces`);
- adds a dedicated test project for every production project except `Sample.Api`;
- keeps `Shared` for cross-cutting primitives and `BuildingBlocks` for reusable framework patterns;
- uses `App.Host` as the single composition root;
- registers modules via `AddModules()` and pipeline behaviors via `AddPipeline()`;
- registers `ExceptionHandlingBehavior` first in the MediatR pipeline;
- has no API surface and no persistence layer (`standalone: false`).
- implements the full conformance-testing contract from `solution-dotnet-conformance-testing` (`make unit-test` / `mutation-test` / `test-report` / `test-and-report`) using Reqnroll, coverlet, and Stryker.NET.

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

## What the example demonstrates

`App.Host/Program.cs` builds the host, wires the single `Sample` module, and dispatches a `GreetCommand` through MediatR. The pipeline catches unhandled exceptions inside `ExceptionHandlingBehavior` and returns a safe `Result.Error`.
