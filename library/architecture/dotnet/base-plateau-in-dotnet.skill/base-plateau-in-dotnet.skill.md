---
name: base-plateau-in-dotnet
description: The base plateau in dotnet — the projects of its layers, MediatR 12 handlers with pipeline behaviors as the decorators, the error kinds as exceptions, the startup entry, the runner of the entry points with the exit codes, the JSON log provider that drops the payload, the composition root on a plain ServiceCollection with its three marked sections, and the Reqnroll scenarios that cover each project
whenToUse: when you create a new dotnet service from the architecture catalog, or when you attach a variation point to, or review, a dotnet application built on the base plateau
updated: 20261022
tags:
  - stack/dotnet
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/mediatr-12-as-the-handler-contract.md
  - adr/service-collection-without-generic-host.md
  - adr/own-json-log-provider.md
---

# Goal
- **The base plateau in dotnet** - A dotnet solution that keeps every property of [base-plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md); this skill details that skill's implementation for dotnet and restates none of its rules.
- **Same shape in every dotnet application** - The [pipeline behaviors](./glossary/pipeline-behavior.md), the [startup entry](./glossary/startup-entry.md), the [entry runner](./glossary/entry-runner.md), and the [composition-root](./glossary/composition-root.md) sections are identical across dotnet applications, so a variation point's instructions name the same file and the same section everywhere.

# Core Principle
- **Layers per app-layers-in-dotnet** - Projects, references, entities, and fakes follow [app-layers-in-dotnet](library/develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md); this skill adds only what the base plateau itself contains.
- **MediatR is the handler contract** - `IRequestHandler<TCommand, TResult>` is the one contract, `IPipelineBehavior<TCommand, TResult>` the decorator, `ISender` the way every entry point reaches a handler. Decision recorded in [mediatr-12-as-the-handler-contract](./adr/mediatr-12-as-the-handler-contract.md).
- **Delivered, not retyped** - The fixed code (`Shared`, `BuildingBlocks`, `Api/Core/Entry`, `Api/Startup`) is copied from templates and has no placeholder; only the module, its commands, and the composition-root lines are written per application.

# Rule

## MUST

### Base projects
Create the projects and folders [app-layers-in-dotnet](library/develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md) assigns to the api, command, and domain layers, with the startup entry as the only entry point and the base settings in their own folder.

| Part | Project / folder |
|---|---|
| startup entry | `src/App/App.Host/Api/Startup` |
| [runner of the entry points](./glossary/entry-runner.md): `Entry`, `EntryRunner.RunAllAsync`, `ExitCode` | `src/App/App.Host/Api/Core/Entry` |
| composition root | `src/App/App.Host/Program.cs` |
| base settings (`LOG_LEVEL` only) | `src/App/App.Host/Config/AppConfig.cs` |
| version of the build | `src/App/App.Host/Version/AppVersion.cs`, the `<Version>` of `Directory.Build.props` per `project-version-in-dotnet` |
| commands and their results — one file each | `src/Modules/{Module}/{Module}.Commands` |
| handlers — one file each | `src/Modules/{Module}/{Module}.Application/Commands` |
| entities, domain services, `{Module}Errors` | `src/Modules/{Module}/{Module}.Domain/{Entities,Services,Errors}` |
| [pipeline behaviors](./glossary/pipeline-behavior.md) | `src/BuildingBlocks/Pipeline` |
| [logging](./glossary/logging.md): `LogAttr`, `Write`, `JsonLogLine`, `JsonLogProvider`, `RequestId` | `src/BuildingBlocks/Logging` |
| error kinds | `src/Shared/Errors`, per [error-kinds-in-dotnet](library/develop/dotnet/error-kinds-in-dotnet.skill/error-kinds-in-dotnet.skill.md) |
| one test project per project above, but `Shared` and `Commands` | `{Project}.Tests` beside the project it tests (`src/Modules/Todo/Todo.Domain.Tests`), per [cucumber-testing-in-dotnet](library/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md) |
| solution, versions | `{App}.slnx`, `Directory.Build.props`, `Directory.Packages.props` (central package versions) |

- Risk: a part in a project of its own choosing breaks every variation point's instruction that names the file to edit.
- Fix: move the code to the project and folder in the table.

### Copy the fixed code
Copy, unchanged, [BuildingBlocks.csproj](./templates/src/BuildingBlocks/BuildingBlocks.csproj), [LoggingBehavior.cs](./templates/src/BuildingBlocks/Pipeline/LoggingBehavior.cs), [LogAttr.cs](./templates/src/BuildingBlocks/Logging/LogAttr.cs), [JsonLogLine.cs](./templates/src/BuildingBlocks/Logging/JsonLogLine.cs), [JsonLogProvider.cs](./templates/src/BuildingBlocks/Logging/JsonLogProvider.cs), [RequestId.cs](./templates/src/BuildingBlocks/Logging/RequestId.cs), [Entry.cs](./templates/src/App/App.Host/Api/Core/Entry/Entry.cs), [EntryRunner.cs](./templates/src/App/App.Host/Api/Core/Entry/EntryRunner.cs), and [StartupEntry.cs](./templates/src/App/App.Host/Api/Startup/StartupEntry.cs) to the same paths under the solution root, and the `Shared` project from [error-kinds-in-dotnet](library/develop/dotnet/error-kinds-in-dotnet.skill/error-kinds-in-dotnet.skill.md).
- Risk: retyped code drifts from the one every variation point's behavior and entry point is written against.
- Fix: copy the templates; the namespaces (`BuildingBlocks`, `Shared`, `App.Host`) are the same in every application, so there is nothing to fill.

### Pin MediatR below 13
Reference `MediatR` 12.5.0 in `{Module}.Application`, `BuildingBlocks`, and `App.Host`, and `MediatR.Contracts` 2.0.1 in `{Module}.Commands`, through `Directory.Packages.props`.
- Violation: `MediatR` 13 or later; a `PackageReference` with its own `Version`.
- Risk: MediatR 13+ needs a commercial license key; a version per project lets two projects of one solution load different contracts.
- Fix: one `PackageVersion` per package in `Directory.Packages.props`.

### One file per behavior
Put each pipeline behavior in its own file of `src/BuildingBlocks/Pipeline` — `LoggingBehavior.cs` now, `{Step}Behavior.cs` later — and add it with one `AddOpenBehavior` line in `// [handlers]`.
- Violation: a unit of work written into `LoggingBehavior`; a behavior registered with `services.AddTransient(typeof(IPipelineBehavior<,>), ...)` outside `// [handlers]`.
- Risk: a variation point that brings a behavior edits a base file, and two such variation points edit the same one.
- Fix: the variation point adds `src/BuildingBlocks/Pipeline/{Step}Behavior.cs` and its `AddOpenBehavior` line; the order of the lines is the order of wrapping, outermost first.

### Errors carry a kind of error-kinds
Raise every business-rule error as a factory method of `{Module}Errors` per [error-kinds-in-dotnet](library/develop/dotnet/error-kinds-in-dotnet.skill/error-kinds-in-dotnet.skill.md), the kind chosen by [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md)'s table.
- Risk: an entry point comparing individual errors must change with every new rule; a rule error with the wrong kind tells the caller to fix an input that is correct.
- Fix: the example's `TodoErrors.AlreadyDone()` is failed precondition (the input is valid, the item's state forbids it); the others are invalid.

### One file per command
Declare a command as a `sealed record {Command}(...) : IRequest<{Command}Result>` with its result records in `{Module}.Commands/{Command}.cs`, and its handler as `sealed class {Command}Handler : IRequestHandler<{Command}, {Command}Result>` in `{Module}.Application/Commands/{Command}Handler.cs`.
- Risk: a command without `IRequest<TResult>` cannot be sent through `ISender` and skips every behavior; a result type in `Application` cannot be seen by an entry point.
- Fix: take every service the handler uses as a constructor parameter typed by an interface; MediatR builds the handler from the container.

### Compose in the three marked sections
Build the container in `Program.RunAsync(stdout, stderr, stopping)` of `src/App/App.Host/Program.cs` on a plain `ServiceCollection` under the comments `// [services]`, `// [handlers]`, and `// [entries]`, in this order, with `Main` only registering `SIGINT`/`SIGTERM` (`PosixSignalRegistration`) to cancel `stopping` and returning `RunAsync(Console.Out, Console.Error, stopping.Token)`. Decision recorded in [service-collection-without-generic-host](./adr/service-collection-without-generic-host.md).
- Violation: `Host.CreateApplicationBuilder`, `appsettings.json`, an `IHostedService`; handlers registered outside `// [handlers]`.
- Risk: a variation point whose instruction says "add after `// [handlers]`" finds no such line and guesses where its code goes; a second lifecycle beside `EntryRunner` starts or stops an entry point on its own.
- Fix: keep the three comments even when a section is empty; `BuildServiceProvider` runs between `// [handlers]` and `// [entries]`.

### Log through LogAttr and the composition root's provider
Add the one provider in `RunAsync` as `services.AddLogging(logging => logging.SetMinimumLevel(config.LogLevel).AddProvider(new JsonLogProvider(stderr)))`, write every record with `logger.Write(level, "{fixed message}", LogAttr.Of(...), ...)`, and pass the data of a call only as `LogAttr.Payload(v)` with `LogAttr.Failed(error is not null)` on the record that ends it. Decision recorded in [own-json-log-provider](./adr/own-json-log-provider.md).
- Violation: `logger.LogInformation("called {Body}", body)`; `AddConsole()` or `AddJsonConsole()`; a second `LoggerFactory` built in a project.
- Risk: data written into the message or under another key escapes the base provider's drop and the logging variation point's mask; the built-in JSON console writes a nested `State` that no log pipeline of the other stacks reads.
- Fix: the logging variation point replaces only the `AddProvider` call.

### One Entry per entry point in [entries]
End `RunAsync` with one `EntryRunner.RunAllAsync` call listing a `new Entry(name, stopping => ...)` per entry point, and return `EntryRunner.ExitCode(error)`.

```csharp
// [entries] - every entry point, one Entry each, run together by EntryRunner.
var error = await EntryRunner.RunAllAsync(logger, stopping,
    new Entry("startup", stopping =>
        StartupEntry.RunAsync(sender, new PlanItemsCommand(StartupEntry.EnvList("STARTUP_TITLES")), stdout, stopping)));
return EntryRunner.ExitCode(error);
```

- Violation: an entry point's server started outside `RunAllAsync`; an exit code computed in `Program.cs`.
- Risk: the next variation point's entry point is never started, or the process keeps running with one entry point dead.
- Fix: a variation point adds its `Entry` to the list (health) or replaces the startup `Entry` with its own (an API); `RunAllAsync` and `ExitCode` stay as they are. An entry that shows its error itself sets `ReportsErrors: true`.

### Cover each project with scenarios
Cover `{Module}.Domain`, `{Module}.Application`, `BuildingBlocks`, and `App.Host` (startup entry, entry runner, settings) with Reqnroll scenarios in their test projects per [cucumber-testing-in-dotnet](library/testing/dotnet/cucumber-testing-in-dotnet.skill/cucumber-testing-in-dotnet.skill.md), tagged `@type/domain`, `@type/service` (handlers), `@type/tech-check` (behaviors, logging, settings, the runner), and `@type/api/cli` (startup entry).
- Risk: a project without scenarios lets a variation point break it unnoticed, since the variation point's example reruns the base scenarios as its regression check.
- Fix: one `{Project}.Tests` beside each project, with `features/` and `Steps/`; a test project whose scenarios set environment variables disables test parallelization.

### Run the scenarios with make test
Give the example's `Makefile` a `test` target running `dotnet test {App}.slnx --filter "Category!=status/todo&Category!=status/broken"`, and leave the testing contract (kinds, reports, mutation) to [solution-conformance-testing-in-dotnet](library/testing/dotnet/solution-conformance-testing-in-dotnet.skill/solution-conformance-testing-in-dotnet.skill.md), as base-plateau's "Examples show the build, not the testing contract" requires.
- Risk: a copied `tools/testing` must be updated in every example whenever the testing skills change.
- Fix: no `tools/`, `report-template/`, Stryker configuration, or `reqnroll.json` formatter in the example.

## SHOULD

### Name type parameters by their role
Name a type parameter after what it stands for (`TCommand`, `TResult`), never `T`, `TRequest`/`TResponse` alone, or a single letter, and document every public generic type and method with what each type parameter is and an example of it.
- Risk: `LoggingBehavior<T1, T2>` forces a reader to open MediatR to learn what each is, in exactly the shared code every variation point builds on.
- Fix: rename the parameter; add a `<typeparamref>` comment naming its role with a concrete type of the application.

### Base settings read in one Load
Read the base settings from environment variables in `AppConfig.Load()`, throwing a `FormatException` that names the variable it could not parse, with the same variable names and values as the other stacks (`LOG_LEVEL`: `debug`, `info`, `warn`, `error`).

# Check list
- [ ] Projects and folders follow [Base projects](#base-projects) and [app-layers-in-dotnet](library/develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md).
- [ ] `src/Shared`, `src/BuildingBlocks`, `src/App/App.Host/Api/Core/Entry`, and `src/App/App.Host/Api/Startup` are copies of the templates.
- [ ] `MediatR` is 12.5.0 and `MediatR.Contracts` 2.0.1, both in `Directory.Packages.props` only.
- [ ] Every business-rule error is a factory method of `{Module}Errors` carrying one kind.
- [ ] Every command is a record implementing `IRequest<TResult>` in `{Module}.Commands`, its handler in its own file of `{Module}.Application`.
- [ ] Each behavior is in its own file of `src/BuildingBlocks/Pipeline`, added by `AddOpenBehavior` in `// [handlers]`.
- [ ] `RunAsync` in `Program.cs` builds a plain `ServiceCollection` with `// [services]`, `// [handlers]`, `// [entries]` in this order; `[entries]` is one `EntryRunner.RunAllAsync` call returning `EntryRunner.ExitCode(error)`.
- [ ] Every record is written by `logger.Write` with fixed text; the data of a call is only `LogAttr.Payload`; the one provider is `JsonLogProvider`.
- [ ] The example holds no testing tooling; its `Makefile` runs `dotnet test`.
- [ ] `make test` passes, with scenarios in every test project listed in [Cover each project with scenarios](#cover-each-project-with-scenarios).

# Example
- [examples/](./examples/README.md) — the base plateau as a runnable to-do planner: `make run`, `make test`.
- [glossary/](./glossary/README.md) — how the pipeline behavior, the error kind, the composition root, the entry runner, the logging, and the startup entry work.
