---
name: own masking not serilog
description: How the dotnet logging variation point masks the payload — code of its own mirroring Go, Serilog with Destructurama.Attributed, or Microsoft.Extensions.Compliance.Redaction
problem: The dotnet provider must write the payload masked by field marks, proto debug_redact, safety-net names, and JSON keys, write unreadable bodies unparsed, cut by size, and pass the shared conformance scenarios every stack passes; which masking does that, and where do the marks a command's fields carry live?
decision: Masking of its own in the MaskedLogging project (PayloadMask, MaskedLogProvider), the line written through the base JsonLogLine; the marks [LogMask] and [LogOmit] live in Shared, so a module's Commands project depends on no logging package.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The base plateau writes its own JSON line and drops the payload ([own-json-log-provider](library/architecture/dotnet/base-plateau-in-dotnet.skill/adr/own-json-log-provider.md)); the choice of masking was left to this variation point. What is masked is fixed by the shared scenarios of [vp-logging](library/architecture/spec/vp-logging.skill/vp-logging.skill.md): marks, `debug_redact`, names, JSON bodies and texts, `Any`, unparsed bodies with one warning, the cut, `LOG_PAYLOAD`, the request id.

# Selected variant
[[#Masking of its own]]
- Owner decision, 2026-10-22: own masking, proven by one set of scenarios shared with every stack.

# Searched variants

## Masking of its own

**Selected.**

### Description
`PayloadMask.Apply(payload)` walks the value to a `JsonNode` — objects by their properties, dictionaries by their keys, proto messages by their descriptors, bytes and JSON texts as parsed JSON — and reports the bodies it could not read; `MaskedLogProvider` cuts it, adds the request id, writes the line through `JsonLogLine`, and warns once per unparsed body. `[LogMask]` and `[LogOmit]` are in `Shared`.

### Benefits
- One design with Go: the same rules, the same scenarios, the same output.
- No package beyond `Google.Protobuf`; the attach changes only the provider line.
- `Shared` references nothing, so marking a command's field adds no logging dependency to the module's `Commands` project.

### Costs
- About four hundred lines of security-critical code kept in the catalog; held by the shared scenarios and a Stryker run.

## Serilog with Destructurama.Attributed

### Description
`Serilog.Extensions.Logging` under `ILogger`, `[LogMasked]` / `[NotLogged]` from `Destructurama.Attributed`, a template of `Serilog.Expressions` for the fields.

### Benefits
- Ready attribute masking, a known library, sinks beyond standard error.

### Costs
- Covers the marks only: `debug_redact`, names, JSON bodies and texts, `Any`, unparsed bodies, `LOG_PAYLOAD`, and the cut are written as policies of its own anyway.
- The attributes are in a package that depends on Serilog: every module's `Commands` project would depend on the logger.
- Its compact JSON (`@t`, `@mt`) needs a template to write the fields of every other stack.

## Microsoft.Extensions.Compliance.Redaction

### Description
Data-classification attributes on the fields and `EnableRedaction()` on the logging builder.

### Benefits
- The framework's own way; the attribute package is small and depends on no logger.

### Costs
- Redacts only values logged through source-generated `[LoggerMessage]` methods with `[LogProperties]`: every log call would be rewritten.
- Covers neither proto messages, JSON bodies, nor `LOG_PAYLOAD`.
