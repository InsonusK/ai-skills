---
name: vp-logging-in-dotnet
description: The logging variation point in dotnet — the MaskedLogging project copied from templates (MaskedLogProvider writing the payload masked by [LogMask] / [LogOmit] marks from Shared, [JsonIgnore], proto debug_redact, and secret names, cut to LOG_PAYLOAD_MAX_BYTES, kept by LOG_PAYLOAD, with the request_id of the call, unreadable bodies written unparsed and warned once) and the provider line of Program.cs replaced
whenToUse: when a dotnet application built on the base plateau must log the data of its calls, when you mark a property of a dotnet command, request type, or proto message as secret or not to log, or when you review such an application's log
updated: 20261102
tags:
  - stack/dotnet
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
adr:
  - adr/own-masking-not-serilog.md
---

# Goal
- **The logging variation point in dotnet** - A dotnet application that keeps every property of [vp-logging](library/architecture/spec/vp-logging.skill/vp-logging.skill.md); this skill details that skill's implementation for dotnet and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-dotnet](library/architecture/dotnet/base-plateau-in-dotnet.skill/base-plateau-in-dotnet.skill.md)'s example.

# Core Principle
- **One provider** - Every part already logs through `ILogger` with `LogAttr.Payload(v)` ([logging](library/architecture/dotnet/base-plateau-in-dotnet.skill/glossary/logging.md)); `MaskedLogProvider` is the provider that writes that payload instead of dropping it, through the same `JsonLogLine` as the base provider.
- **Marks without a logger** - `[LogMask]` and `[LogOmit]` live in `Shared`, which references nothing, so a module's `Commands` project marks its fields without depending on any logging package ([ADR](./adr/own-masking-not-serilog.md)).

```
src/
├── Shared/Logging/LogMarks.cs         [LogMask], [LogOmit]
├── MaskedLogging/                     copied from templates, never edited
│   ├── MaskedLogConfig.cs             LOG_PAYLOAD, LOG_PAYLOAD_MAX_BYTES, LOG_PAYLOAD_UNMASKED
│   ├── MaskedLogProvider.cs           payload kept, masked, cut; request_id; one warning per unparsed body
│   ├── PayloadMask.cs                 marks, [JsonIgnore], debug_redact, secret names, JSON, Any, unparsed
│   └── ProtoTypes.cs                  the proto messages of the process, for an Any
└── MaskedLogging.Tests/
    ├── features/                      the shared scenarios (conformance.copy) + Payload.feature
    └── Steps/                         the dotnet step definitions
```

# Rule

## MUST

### Copy the project from templates
Copy, unchanged, [MaskedLogging/](./templates/src/MaskedLogging/) to `src/MaskedLogging/` and [LogMarks.cs](./templates/src/Shared/Logging/LogMarks.cs) to `src/Shared/Logging/`, with the example's `src/MaskedLogging.Tests/`; add both projects to the solution, a reference to `MaskedLogging` to `App.Host.csproj`, and `Google.Protobuf` `3.35.1` and `Grpc.Tools` `2.84.0` (the versions of [vp-grpc-api-in-dotnet](library/architecture/dotnet/vp-grpc-api-in-dotnet.skill/vp-grpc-api-in-dotnet.skill.md)) to `Directory.Packages.props`.
- Risk: a retyped mask misses a form of payload — a JSON body, a proto message, a header map — and leaks it.
- Fix: copy the files; `make test` passes.

### Replace the provider line of Program.cs
Load `MaskedLogConfig` beside `AppConfig` and replace `.AddProvider(new JsonLogProvider(stderr))` of the `services.AddLogging` line with `MaskedLogProvider`, giving it the level the builder filters at.

```csharp
AppConfig config;
MaskedLogConfig loggingConfig;
try
{
    config = AppConfig.Load();
    loggingConfig = MaskedLogConfig.Load(config.LogLevel); // throws FormatException naming its variable;
                                                           // LOG_PAYLOAD_UNMASKED needs LOG_LEVEL=debug
}
// ...
services.AddLogging(logging => logging.SetMinimumLevel(config.LogLevel)
    .AddProvider(new MaskedLogProvider(stderr, config.LogLevel, loggingConfig)));
```

- Violation: `MaskedLogProvider` added beside `JsonLogProvider`; a second provider of any kind (console, debug).
- Risk: every record is written twice, once unmasked by the other provider.
- Fix: one provider; the boot logger of a failed start keeps `JsonLogProvider`, which writes no payload.

### Mark secrets with LogMask and debug_redact
Mark a property of a command, an HTTP request type, or a result `[property: LogMask]` when its value is secret and `[property: LogOmit]` when it must not be logged at all (a property with `[JsonIgnore]` is left out as well); mark a field of a proto message `[debug_redact = true]`; a `Commands` project that marks a field references `Shared`.

```csharp
public sealed record AddUserCommand(
    string Name,
    [property: LogMask] string Password, // masked by its name too; the mark says it on purpose
    [property: LogOmit] byte[] Avatar)   // large, and of no use in a log
    : IRequest<AddUserResult>;
```

- Violation: `[LogMask]` without `property:` on a positional record; a secret whose name matches no safety-net name (`Pin`, `Otp`, `Iban`) left unmarked.
- Risk: the marks are valid on properties only, so a mark on the constructor parameter does not compile — but an unmarked secret the safety net does not know is written as it is.
- Fix: mark every secret property when the type is written; the names are a second line, not the first.

### Log only through attributes
Write the data of a call only as `LogAttr.Payload(v)` through `logger.Write`; a message template (`logger.LogInformation("... {payload}", v)`) is written as its template, and a scope is not written at all.
- Risk: a value rendered into the text of a record is not masked.
- Fix: fixed text, every value an attribute ([base-plateau-in-dotnet](library/architecture/dotnet/base-plateau-in-dotnet.skill/base-plateau-in-dotnet.skill.md)).

### Run the shared scenarios as copied
Keep the shared files of `src/MaskedLogging.Tests/features/` byte for byte as `make conformance-sync` writes them from [vp-logging](library/architecture/spec/vp-logging.skill/vp-logging.skill.md)'s `conformance/`; the test project compiles their `masking.proto`; put a scenario only dotnet needs in `Payload.feature`.
- Violation: a shared scenario reworded to suit a dotnet step; a step that passes by testing something else than its text says.
- Risk: dotnet stops proving a guarantee every other stack proves, and the leak it guards ships in dotnet only.
- Fix: change the source in the spec skill, run `make conformance-sync` at the repository root, and fix the dotnet code until the copy passes.

### Choose LOG_PAYLOAD per deployment
Set `LOG_PAYLOAD=errors` in the deployment of a service whose request volume makes full payloads too costly, and leave the default `always` otherwise.
- Risk: full bodies on every request multiply the log volume and its cost.
- Fix: the setting is read at start; no rebuild.

# Check list
- [ ] `src/MaskedLogging/` and `src/Shared/Logging/LogMarks.cs` are copies of the templates; `App.Host` references `MaskedLogging`.
- [ ] `Program.cs` adds only `MaskedLogProvider`, built with `MaskedLogConfig.Load()` and the configured level.
- [ ] Every secret property is marked `[property: LogMask]` or `[debug_redact = true]`; properties not to log are `[property: LogOmit]`.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` and `bash test/check-conformance.sh` in this repository pass.
- [ ] After a change to `MaskedLogging/`, the Stryker run of the [leak catalog](library/architecture/spec/vp-logging.skill/leak-catalog.md) leaves only the survivors it lists for dotnet.

# Example
One call of the example, each part with its class and file:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the base plateau's to-do planner with the logging variation point attached: `make run`, `make test`.
