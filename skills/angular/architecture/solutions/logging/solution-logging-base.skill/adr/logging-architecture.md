---
name: logging-architecture
description: How the application logs to the console, and how that design leaves room for a future backend-sending extension without a rewrite
problem: Whether to use raw console calls or a custom logging abstraction, and whether log entries are plain strings or structured objects
decision: A custom LoggerService, backed by a pluggable list of LogSink implementations (this solution registers only a ConsoleLogSink), taking structured log entries (message + context object)
---

# Problem

The application needs a consistent way to log during development and in production, with the explicit expectation (already identified in the architecture's solution backlog) that a second solution will later add sending logs to a backend, without duplicating or replacing everything already logged through this one. We need to decide whether that means writing directly to `console.*` with conventions, or introducing an abstraction — and, separately, whether a log entry is a plain string or a structured object, since that choice shapes how useful logs are once a backend sink needs to filter, correlate, or query them.

# Selected variant

**Selected variant:** [[#Custom LoggerService over pluggable LogSinks, structured entries]]

A `LoggerService` is the only way any code in the application logs. Internally, it forwards each structured log entry to every registered `LogSink`. This solution registers exactly one sink — `ConsoleLogSink` — so today's behavior is "log to the console," but the seam for a second sink (backend-sending, added by the future "Логирование (глобальное)" solution) already exists and requires no change to call sites.

# Searched variants

## Custom LoggerService over pluggable LogSinks, structured entries

### Description

Every log call goes through `LoggerService.debug/info/warn/error(message, context?)`, where `context` is a plain object (e.g. `{ feature: 'orders', orderId: '123' }`). The service iterates over an injected, multi-provider list of `LogSink` implementations (`LOG_SINKS`, `{ multi: true }`) and forwards each entry to all of them. This solution provides only `ConsoleLogSink`. Minimum log level is read from environment-specific configuration, filtering out `debug`/`info` in production builds while keeping `warn`/`error` always on.

### Benefits

- The future "Логирование (глобальное)" solution adds a second sink (e.g. `BackendLogSink`) by registering it alongside `ConsoleLogSink` — no call site anywhere in the codebase changes
- Structured context (feature name, correlation IDs, relevant IDs) makes logs filterable/queryable the moment a backend sink exists, instead of requiring a later rewrite of every log call to add structure retroactively
- A single place (`LoggerService`) to enforce the "never log tokens/passwords/PII" rule and environment-based level filtering, rather than relying on every call site to remember it
- Testable in isolation: a fake `LogSink` can capture entries in tests without touching the real console

### Costs

- Slightly more code upfront than calling `console.log` directly — a service, an interface, one concrete sink, and a DI token to write before the first log statement can be made
- Every log call site needs a small amount of discipline to pass structured context rather than concatenating a string, which is a habit change for anyone used to plain `console.log('user %s did X', id)` calls

## Raw console calls with naming conventions

### Description

Code calls `console.log`/`console.warn`/`console.error` directly, with a documented convention for message prefixes (e.g. `[Orders] ...`).

### Benefits

- Zero abstraction to write or learn — every engineer already knows `console.*`
- No indirection between a log call and what appears in devtools

### Costs

- Adding a backend sink later means either wrapping `console.*` globally (fragile, hard to keep consistent) or touching every existing call site — exactly the rewrite this solution's pluggable-sink design exists to avoid
- No structured context by default — retrofitting structure onto string-based logs already spread across the codebase is far more work than establishing it from the start
- Nothing centrally enforces the "never log tokens/PII" rule or environment-based level filtering; each call site would need its own discipline

## Custom LoggerService, but with plain string messages (no structured context)

### Description

Same `LoggerService`/pluggable-sink design, but log calls only take a message string, with any extra detail interpolated into the string.

### Benefits

- Faster to write a log call in the moment — no need to shape a context object
- Still gets the sink pluggability benefit of the selected variant

### Costs

- A future backend sink (or even local devtools filtering) cannot reliably query "all logs for feature X" or "all logs for order 123" without string-parsing, which is fragile compared to reading a structured field
- Loses most of the practical benefit of introducing the abstraction in the first place, since the main payoff of structured logging is exactly what a backend-sending sink needs later
