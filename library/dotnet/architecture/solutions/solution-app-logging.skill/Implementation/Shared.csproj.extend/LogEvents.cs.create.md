---
description: Stable EventId constants for the log lines code or operators search for
name: LogEvents.cs
element_kind: class
change_kind: create
tags:
  - solution/app-logging
  - element/log-events
---

# Goals
- One `EventId` per deliberate, searched-for log line, stable across provider swaps.

# Implementation

```csharp
namespace Shared.Logging;

public static class LogEvents
{
    // 1xxx — host lifecycle
    public static readonly EventId ModuleRegistered = new(1001, nameof(ModuleRegistered));
    public static readonly EventId HostStarted      = new(1002, nameof(HostStarted));

    // 2xxx — pipeline
    public static readonly EventId RequestRejected     = new(2001, nameof(RequestRejected)); // validation short-circuit

    // 3xxx — outbound integrations
    public static readonly EventId OutboundCallFailed  = new(3001, nameof(OutboundCallFailed)); // an outbound HTTP/gRPC dependency call mapped to a failed Result

    // 5xxx — failures
    public static readonly EventId UnhandledException  = new(5001, nameof(UnhandledException));
}
```

Ranges: `1xxx` lifecycle, `2xxx` pipeline, `3xxx` outbound integrations, `5xxx` failures. A new solution that needs a stable event adds a constant in the right range.

# Allowed Dependencies
- Microsoft.Extensions.Logging.Abstractions

# Rules

## MUST
- Assign each `EventId` a number that never changes once used.
  - Risk: renumbering breaks every saved query, alert, and dashboard keyed on the id.
  - Fix: append new ids; never reuse or renumber.

## SHOULD
- Keep the numeric ranges (`1xxx`/`2xxx`/`3xxx`/`5xxx`) meaningful so an id hints at its category.

# Check list
- [ ] Every field is a `static readonly EventId` with a fixed number and a `nameof` name.
- [ ] Ranges are documented in a comment.
