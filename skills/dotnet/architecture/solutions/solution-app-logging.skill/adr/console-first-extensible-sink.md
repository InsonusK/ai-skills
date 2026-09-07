---
name: console-first-extensible-sink
description: Which logging stack the family starts with and how a richer sink is added later
problem: A new service needs application logging now, but the family does not yet want to commit to a log-aggregation product or a file format, and must never make swapping the sink a call-site change.
decision: Microsoft.Extensions.Logging with a console provider, configured in App.Host, EventId catalogue in Shared; file/OTLP sinks deferred to a one-line change in LoggingRegistration.
tags:
  - solution/app-logging
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Every service in the family needs to log. The `AppLogging` feature is deliberately minimal — "basic structured logging to the console, extensible to file later, no read-back". The decision is which abstraction and provider to start with, and how to keep a later sink change from rippling across call sites.

# Selected variant

**Selected variant:** [[#Microsoft.Extensions.Logging + console provider, sink localised in App.Host]]

# Searched variants

## Serilog from the start

### Description
Adopt Serilog (or NLog) as the logging framework, with sinks configured from `appsettings.json`.

### Benefits
- Rich sink ecosystem, structured output, enrichers out of the box
- `appsettings`-driven sink configuration with no code change

### Costs
- A third-party framework and its configuration model in every service before the family has decided it needs one
- Call sites either use `ILogger<T>` (then Serilog is just a provider — no reason to pick it early) or `Serilog.ILogger` (a lock-in the feature explicitly wants to avoid)
- More than "basic" — the feature asks for the minimum, extensible later

## Microsoft.Extensions.Logging + console provider, sink localised in App.Host (selected)

### Description
Call sites use `ILogger<T>` from `Microsoft.Extensions.Logging`. `App.Host/DependencyInjection/LoggingRegistration.cs` clears providers, binds levels from configuration, and adds the console provider. A `LogEvents` catalogue of `EventId`s lives in `Shared`. Adding a file or OTLP sink later is a single line inside `LoggingRegistration` plus a package entry — no call site changes, because call sites never named a provider.

### Benefits
- Zero third-party framework; the abstraction is already in the SDK
- `ILogger<T>` is the idiomatic call-site type any future provider plugs under
- The sink is swappable from exactly one file, matching the feature's "extensible to file later" intent
- `EventId` catalogue keeps the important lines stable across a provider change

### Costs
- The console provider alone is not a production log solution — a sink must be added before real deployment (accepted: the feature is explicitly a starting point)
- Structured-logging discipline (message templates, no interpolation) has to be enforced by rule rather than by a framework that makes interpolation awkward

## No logging solution, rely on framework defaults

### Description
Do not add a logging solution; let `WebApplication.CreateBuilder` register its default console logger.

### Benefits
- Nothing to build

### Costs
- No `EventId` catalogue, no agreed levels, no single swap point — every later change is ad hoc
- The feature owner explicitly wants logging tracked as a first-class common feature, not assumed infrastructure
