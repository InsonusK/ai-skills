---
name: vp-cli-api-in-dotnet
description: The CLI API variation point in dotnet — Api/Cli/Pipeline copied from templates (Run building the command line with --version and help at every level, usage errors as the invalid kind, a crash of the action's glue as a failure with no kind, the generic command adapter with --output json|text, the run log), one {Resource}Handler per resource in Api/Cli/Endpoint, Commands.cs with the endpoints and the entry factory, and the startup entry replaced in Program.cs
whenToUse: when a dotnet application built on the base plateau must be run from a command line, when you offer a new command as an action of such an application, or when you review its Api/Cli package
updated: 20261113
tags:
  - stack/dotnet
  - concern/architecture
  - app-type/cli
---

# Goal
- **The CLI variation point in dotnet** - A dotnet application that keeps every property of [vp-cli-api](library/architecture/spec/vp-cli-api.skill/vp-cli-api.skill.md); this skill details its implementation for dotnet and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only what [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-dotnet](library/architecture/dotnet/base-plateau-in-dotnet.skill/base-plateau-in-dotnet.skill.md)'s example.

# Core Principle
- **Copied once, written per command** - `Pipeline/` is copied from templates and never edited; per command the application writes, in its resource's file of `Endpoint/`, a flag list, a `read`, an output type with `encode`, and a `text`.
- **MediatR is the way to the handlers** - Each endpoint's handler takes `ISender` and sends the command, so every command middleware runs; the entry itself takes no list of middleware.

```
src/App/App.Host/Api/Cli/
├── Pipeline/             copied, never edited
│   ├── Cli.cs            Resource, RunAsync(): the root, --version, help at every level, usage errors as invalid, a crash as a failure with no kind
│   ├── CliCommand.cs     CliFlag, CliFlags, Command(): flags → command → handler → JSON line or text
│   └── RunLog.cs         the run log: cli call received / cli call answered / cli action crashed
├── Endpoint/             one file per resource: {Resource}Handler with Resource()
│   └── ItemsHandler.cs
└── Commands.cs           CliEndpoints, AddCliEntry(), Run() for the scenarios
```

# Rule

## MUST

### Copy the pipeline from templates
Copy [Cli.cs](./templates/src/App/App.Host/Api/Cli/Pipeline/Cli.cs), [CliCommand.cs](./templates/src/App/App.Host/Api/Cli/Pipeline/CliCommand.cs), and [RunLog.cs](./templates/src/App/App.Host/Api/Cli/Pipeline/RunLog.cs) to `src/App/App.Host/Api/Cli/Pipeline/`, unchanged.
- Risk: a retyped command line drifts in its usage errors, exit codes, or output formats from every other CLI of the catalog.
- Fix: copy the files; `bash test/check-templates.sh` compares them with the example's.

### One file per resource in Endpoint
Write each resource in its own file `Api/Cli/Endpoint/{Resource}Handler.cs`: a `{Resource}Handler(ISender)` with its method `Resource()` returning one `CliCommand.Command<TCommand, TResult>(use, short, flags, read, handle, encode, text)` per command, and per action its output type. Keep in `Api/Cli/Commands.cs` only `CliEndpoints`, one property per resource, `Resources()`, `AddCliEntry`, and `Run`, which the scenarios call.

```csharp
CliCommand.Command<PlanItemsCommand, PlanItemsResult>(
    "plan", "Plan one open item per --title",
    flags: [new CliFlag("title", "the title of an item; repeat for several", Many: true)],
    read: flags => new PlanItemsCommand(flags.Values("title")),   // a FormatException is a usage error (exit 2)
    handle: (command, stopping) => sender.Send(command, stopping),
    encode: result => new PlanItemsOutput(...),
    text: result => ...),                                          // null: --output text writes indented JSON
```

- Violation: an action that reads `Environment.GetCommandLineArgs` or prints by itself; the command's result passed to `encode` unchanged; an action registered outside `Resources`.
- Risk: an action outside the adapter loses the usage-error kind and the output formats; the published output follows the command layer.
- Fix: follow the example's [ItemsHandler.cs](./examples/src/App/App.Host/Api/Cli/Endpoint/ItemsHandler.cs); a repeated value is a repeated flag (`Many: true`), not a comma-separated string.

### Replace the startup entry in Program.cs
Replace `AddStartupEntry` in the `// [entries]` list with `AddCliEntry("todo", AppVersion.Value, args, stdout, stderr)`, give `Main` and `RunAsync` the `string[] args`, and delete `src/App/App.Host/Api/Startup/`.

```csharp
// [entries] - every entry point, registered as an Entry and run
// together by EntryRunner until the first one ends. The CLI runs the
// action the arguments name and ends, and the process ends with it.
services.AddCliEntry("todo", AppVersion.Value, args, stdout, stderr);
```

- Violation: `Environment.Exit` inside the CLI package, or the startup entry kept beside the CLI.
- Risk: an exit code computed outside `EntryRunner.ExitCode` ignores the error kind; an `Environment.Exit` skips the graceful stop of every other entry.
- Fix: the entry prints the error as `Error: ...` — for an error with a kind, its own message, as the network entry points answer it — and returns it; its `ReportsErrors: true` keeps `EntryRunner` from logging it a second time. The logs of the run stay on stderr.

### Keep the run log and the answer of every failure inside RunAsync
Let `Cli.RunAsync` write the run log and answer every failure itself — a usage error as invalid, a failure of the handler with its kind kept and its message, a crash of the action's glue as a failure with no kind (`Error: internal error`, exit code `1`, the fault logged as "cli action crashed") — as [fixed-middleware-inside-the-entry](library/architecture/spec/base-plateau-extension.skill/adr/fixed-middleware-inside-the-entry.md) requires.
- Violation: a fault of the action left to the runtime, which ends the process with its own exit code and trace; a crash handler written in a resource's file.
- Risk: the run leaves no record, and a script reads a bug as a rejected command.
- Fix: the copied `Cli.cs`; the scenario "An action that crashes is answered as a failure with no kind and logged".

### Cover the entry point with scenarios
Name both `conformance/` folders in [conformance.copy](./conformance.copy): the fixed code is covered through a test resource of its own (`Steps/Cli/CliSteps.cs`), and each resource has one feature over the command handlers replaced by fakes (a fake `IRequestHandler` registered in a test container, [CliItemsSteps.cs](./examples/src/App/App.Host.Tests/Steps/Cli/CliItemsSteps.cs)); tag the resource features `@type/api/cli`.
- Risk: a test through the real command tests the domain again; an untested usage error exits `1`, and a script retries it forever.
- Fix: a resource feature checks only its own flags and output; `make test` passes.

## SHOULD

### Offer a new command in its resource's file
Offer a further command of an existing resource by adding its action line and its `flags`, `read`, `encode`, and `text` to the resource's file, and its scenario to the resource's feature; `Program.cs` does not change. A new resource adds its file and feature, one property of `CliEndpoints`, and one line of `Resources`.

# Check list
- [ ] `Api/Cli/Pipeline/{Cli,CliCommand,RunLog}.cs` are unchanged copies of the templates.
- [ ] Each resource has its own file in `Endpoint/` with its actions built by `CliCommand.Command`; `Commands.cs` holds only `CliEndpoints`, `Resources`, `AddCliEntry`, and `Run`.
- [ ] The `// [entries]` list of `Program.cs` has `AddCliEntry("todo", AppVersion.Value, args, ...)`; `Api/Startup/` is deleted.
- [ ] `make test` passes with the fixed code's feature and one feature per resource.
- [ ] `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow); `bash test/check-conformance.sh` reports no copy changed.

# Example
One call of the example, each part with its class and file:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the base plateau's to-do planner as a command line: `make run ARGS="items plan --title 'buy milk'"`, `make test`, `make run ARGS="--help"`.
