---
name: own json log provider
description: Which logger provider writes the dotnet base plateau's log — an own ILoggerProvider, the built-in JSON console, or Serilog
problem: Every part logs through ILogger; the base plateau's provider must write one JSON line per record with the fields of the other stacks and drop the payload attribute, and the logging variation point must later mask it; which provider does that?
decision: An own JsonLogProvider of about one hundred lines in BuildingBlocks; the choice between own masking and Serilog with Destructurama.Attributed is made with the logging variation point in dotnet, where it replaces only the AddProvider line.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The Go base plateau writes `{"time","level","msg",...attributes}` and drops the `payload` attribute. A dotnet service must write the same fields, so one log pipeline reads every stack. Code writes through `ILogger` whatever the provider, so the provider is one line of `Program.cs`.

# Selected variant
[[#Own ILoggerProvider]]
- Owner decision, 2026-10-21: Serilog is reconsidered with the logging variation point.

# Searched variants

## Own ILoggerProvider

**Selected.**

### Description
`JsonLogProvider` writes `time`, `level` (`DEBUG`, `INFO`, `WARN`, `ERROR`), `msg`, the attributes in order, `error` for an exception (and `exception` with its stack when it has no kind), and skips `payload`.

### Benefits
- No package; the exact fields and units (a duration in nanoseconds) of the Go base plateau.

### Costs
- About one hundred lines kept in the catalog; masking is written with the logging variation point.

## Built-in JSON console

### Description
`AddJsonConsole()`.

### Benefits
- In the framework.

### Costs
- Writes `Timestamp`, `LogLevel`, `Category`, and a nested `State`: other fields than every other stack; it cannot drop one attribute.

## Serilog

### Description
`Serilog.Extensions.Logging` with a custom formatter and a filter for the payload property.

### Benefits
- Ready masking by attributes (`Destructurama.Attributed`: `[NotLogged]`, `[LogMasked]`) for the logging variation point.

### Costs
- Packages in the base for what one file does; its compact JSON (`@t`, `@mt`) still needs a formatter of its own to match the other stacks.
