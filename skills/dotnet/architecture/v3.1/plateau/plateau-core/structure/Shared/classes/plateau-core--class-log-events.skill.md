---
name: plateau-core--class-log-events
description: Class LogEvents in the plateau-core plateau — the static catalogue of stable EventId constants for searched-for log lines, in Shared/Logging
whenToUse: when adding a stable EventId for a new deliberate log line, or referencing an existing one from a log call site
domain: skill
type: template
plateau: core
version: 20260902000000
tags:
  - skill/template/class
  - plateau/core
created_by:
  - "[[../../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]]"
---

# Goal
- Give every deliberate, searched-for log line a stable `EventId` that survives a logging-provider change.

__Applied solutions:__
- [[../../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]] - [[../../../../../solutions/solution-app-logging.skill/Implementation/Shared.csproj.extend/LogEvents.cs.create.md|LogEvents.cs.create]]

# Core Principles
- Apply ONE plateau template per class.
- A `static class` of `public static readonly EventId` fields only — no logging logic, no methods.
- Numeric ranges carry meaning: `1xxx` host lifecycle, `2xxx` pipeline, `3xxx` outbound integrations, `5xxx` failures.
- An id's number never changes once used — new solutions append in the right range.
- The `ILogger<T>` call stays at the call site; `LogEvents` only names the ids.

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| --- | --- | --- | --- | --- |
| Event id catalogue | `LogEvents` | `LogEvents` | `LogEvents.cs` | `LogEvents.cs` |
| One event id | `{PascalCaseFact}` | `UnhandledException` | (field) | — |

# Implementation
```csharp
// Skill: plateau-core--class-log-events
// Plateau: core
// Version: 20260902000000
using Microsoft.Extensions.Logging;

namespace Shared.Logging;

public static class LogEvents
{
    // 1xxx — host lifecycle
    public static readonly EventId ModuleRegistered   = new(1001, nameof(ModuleRegistered));
    public static readonly EventId HostStarted        = new(1002, nameof(HostStarted));

    // 2xxx — pipeline
    public static readonly EventId RequestRejected    = new(2001, nameof(RequestRejected)); // validation short-circuit

    // 3xxx — outbound integrations
    public static readonly EventId OutboundCallFailed = new(3001, nameof(OutboundCallFailed)); // an outbound HTTP/gRPC call mapped to a failed Result

    // 5xxx — failures
    public static readonly EventId UnhandledException = new(5001, nameof(UnhandledException));
}
```

At plateau-core `UnhandledException` is used by `ExceptionHandlingBehavior`; `RequestRejected` is available to `ValidationBehavior`; the `1xxx`/`3xxx` ids are registered for the lifecycle and outbound-integration solutions that use them later.

__Applied solutions:__
- [[../../../../../solutions/solution-app-logging.skill/solution-app-logging.skill.md|solution-app-logging]] - [[../../../../../solutions/solution-app-logging.skill/Implementation/Shared.csproj.extend/LogEvents.cs.create.md|LogEvents.cs.create]]

# Rules
MUST:
- Keep `LogEvents` a `static class` of `static readonly EventId` fields with a fixed number and a `nameof` name; no logging logic.
- Assign every `EventId` a number that never changes once used — append, never renumber or reuse.
- Keep the `1xxx`/`2xxx`/`3xxx`/`5xxx` ranges meaningful, documented in a comment.
- Never apply several plateau templates per class.
- Never call `ILogger` from inside `LogEvents`.

# Check list
- [ ] `Shared/Logging/LogEvents.cs` contains only `static readonly EventId` fields.
- [ ] Every field has a fixed number and a `nameof` name; ranges documented.

# Unittest TestCases
- [ ] WHEN `LogEvents` is reflected THEN every field is a `static readonly EventId` with a non-zero id.
- [ ] WHEN two fields are compared THEN no two share an id.
