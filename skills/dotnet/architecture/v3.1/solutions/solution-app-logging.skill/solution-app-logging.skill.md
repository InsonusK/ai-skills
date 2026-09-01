---
name: solution-app-logging
description: Basic structured application logging through Microsoft.Extensions.Logging with a console provider, wired once in App.Host, shaped so a file (or other) sink can be added later without touching call sites
whenToUse: when wiring logging into a new service's composition root, adding a log statement in a handler/behavior/service, or reviewing whether a class logs through ILogger<T> rather than Console or a static logger
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - solution/app-logging
  - stack/dotnet
  - concern/architecture
  - concern/observability
  - logging
creates:
  - "App.Host.DependencyInjection.LoggingRegistration.cs"
  - "Shared.Logging.LogEvents.cs"
extends:
  - "App.Host.csproj"
  - "App.Host.Program.cs"
  - "Shared.csproj"
depends_on:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]"
adr:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/adr/console-first-extensible-sink.md|Console provider first, sink-extensible, no read-back]]"
---

# Goal
- Give every class one way to log — `ILogger<T>` from `Microsoft.Extensions.Logging`, injected — and one place it is configured (`App.Host`).
- Emit structured (message-template) log entries, not interpolated strings, so a later structured sink can index them.
- Keep the provider swap (console today, file or OTLP later) a single-file change in `App.Host`, invisible to every call site.

# Capabilities
- Structured, level-filtered application logging available in any project via constructor-injected `ILogger<T>`.
- One composition-root call (`AddAppLogging()`) that every service uses identically.
- A `LogEvents` catalogue of `EventId`s so the important entries (module registered, pipeline short-circuit, unhandled exception) are greppable and stable across a provider change.
- A shape that adds a file sink later by editing `LoggingRegistration.cs` only.

# Core Principles
- Log through `ILogger<T>` only — never `Console.WriteLine`, `Trace`, `Debug`, or a `static` logger.
- Use message templates with named placeholders (`_logger.LogInformation("Module {Module} registered", name)`), never string interpolation or concatenation into the message.
- Every deliberate, searched-for log line carries an `EventId` from `Shared.Logging.LogEvents`; ad-hoc diagnostic lines may omit it.
- Levels: `Critical`/`Error` for a failed operation a human must see, `Warning` for a handled-but-notable condition, `Information` for lifecycle milestones, `Debug`/`Trace` for development only and off by default.
- Never log a secret, a full request/response body, or PII — log identifiers and shapes.
- `App.Host` owns the provider and the level configuration (from `appsettings.json` `Logging` section); no other project configures logging.
- This solution writes logs; it never reads them back. Querying logs from a file is out of scope of this family.

# Boundaries
- Request/trace correlation (an `Activity`/`TraceId` on every log line) is a cross-cutting concern better added as a Plateau Component (tracing) — this solution does not implement it, though its message templates leave room for scope enrichment.
- `solution-mediator-exception-handler`'s "log the unhandled exception as Critical" step uses the `ILogger<T>` this solution configures; if this solution is absent, that handler falls back to the framework default logger. Neither requires the other structurally.

# Adr
- [[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/adr/console-first-extensible-sink.md|Console provider first, sink-extensible, no read-back]]
  - Selected variant: `Microsoft.Extensions.Logging` + console provider, configured in `App.Host`, `LogEvents` catalogue in `Shared`, file sink deferred.

# Requirements
SOLUTION:
- [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]]
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/App.Host.csproj.create.md|App.Host.csproj]] - hosts `LoggingRegistration` and the `AddAppLogging()` call in `Program.cs`
  - [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/Implementation/Shared.csproj.create.md|Shared.csproj]] - hosts the `LogEvents` catalogue

NUGET:
- `Microsoft.Extensions.Logging` {version} — `ILogger<T>`, `ILoggingBuilder`
- `Microsoft.Extensions.Logging.Console` {version} — the default provider
- (versions declared in `Directory.Packages.props` per [[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/solution-central-package-management.skill.md|solution-central-package-management]])

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/Implementation/App.Host.csproj.extend.md|App.Host.csproj]] - extend - configure the logging provider and levels in the composition root
  - [[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/Implementation/App.Host.csproj.extend/LoggingRegistration.cs.create.md|LoggingRegistration.cs]] - create - `AddAppLogging()` — provider + level configuration, the one place a sink is swapped
- [[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/Implementation/Shared.csproj.extend.md|Shared.csproj]] - extend - host the shared `EventId` catalogue
  - [[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/Implementation/Shared.csproj.extend/LogEvents.cs.create.md|LogEvents.cs]] - create - stable `EventId` constants for the searched-for log lines

# Workflow

## Configure logging for a new service
1. `Program.cs` calls `builder.Services.AddAppLogging(builder.Configuration)` — nothing else configures logging.
2. `appsettings.json` `Logging:LogLevel` sets the default and per-category minimum levels.
3. Every class takes `ILogger<ThisClass>` in its constructor and logs through it.

## Add a file sink later
1. Add the sink package's `<PackageVersion>` to `Directory.Packages.props` and `<PackageReference>` to `App.Host`.
2. Add one `.AddFile(...)`-style line inside `LoggingRegistration.AddAppLogging()`.
3. No call site changes; no `LogEvents` changes.

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/Implementation/App.Host.csproj.extend/LoggingRegistration.cs.create.md#MUST|LoggingRegistration.cs.create]]
- [[skills/dotnet/architecture/v3.1/solutions/solution-app-logging.skill/Implementation/Shared.csproj.extend/LogEvents.cs.create.md#MUST|LogEvents.cs.create]]
- Inject `ILogger<T>`; never call `Console.WriteLine`, `System.Diagnostics.Trace`, or a `static` logger from production code.
  - Risk: those writes bypass level filtering and the configured provider, so they cannot be turned off, redirected, or structured — and they vanish when the sink changes.
  - Fix: take `ILogger<ThisType>` in the constructor and log through it.
- Use message templates with named placeholders, never string interpolation in the message argument.
  - Risk: an interpolated message is an opaque string — a structured sink cannot index `{OrderId}`, and every distinct value becomes a distinct message, breaking aggregation.
  - Fix: `_logger.LogInformation("Order {OrderId} placed", id)` — the template is constant, the values are structured.
- Configure the provider and levels only in `App.Host` (`LoggingRegistration` + `appsettings.json`).
  - Risk: a module or BuildingBlocks adding its own provider double-writes every line and makes the effective configuration impossible to reason about.
  - Fix: one `AddAppLogging()` in the composition root; no `AddLogging`/`AddConsole` anywhere else.
- Never log a secret, credential, full request/response body, or PII.
  - Risk: logs are shipped, retained, and widely readable — a token or personal data in a log line is a disclosure incident.
  - Fix: log identifiers, counts, and shapes; redact or omit the rest.

## SHOULD
- Give every log line that code or an operator will search for a stable `EventId` from `Shared.Logging.LogEvents`.
- Keep `Debug`/`Trace` off by default in `appsettings.json`; enable per-category in `appsettings.Development.json`.

# Check list
- [ ] `AddAppLogging()` is called exactly once, in `Program.cs`.
- [ ] No `Console.WriteLine` / `Trace` / `static` logger in production code.
- [ ] Every logged message is a constant template with named placeholders.
- [ ] Provider and levels are configured only in `App.Host` + `appsettings.json`.
- [ ] `Shared.Logging.LogEvents` holds the `EventId`s for the milestone/critical lines.
- [ ] No secret, body, or PII appears in any log statement.
