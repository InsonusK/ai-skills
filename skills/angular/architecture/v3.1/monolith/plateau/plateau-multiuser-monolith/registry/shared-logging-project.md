---
name: registry-shared-logging-project
description: Conflict Detection result for the `shared-logging-project` element at plateau-multiuser-monolith — the LOG_SINKS seam, created by solution-logging-base and extended by solution-logging-global with a second sink
tags:
  - concern/architecture
  - stack/typescript
  - element/shared-logging-project
---

# Element
`element/shared-logging-project` — `libs/shared/logging`: `LoggerService`, the `LogSink` interface and the `LOG_SINKS` multi-provider token, `MIN_LOG_LEVEL`, and the concrete sinks.

# Involved solutions
- [[skills/angular/architecture/v3.1/solutions/solution-logging-base.skill/solution-logging-base.skill.md|solution-logging-base]] (baseline, `.create` — `Logging/shared-logging.project.create` — the lib, `LoggerService`, the `LOG_SINKS` token, and `ConsoleLogSink`)
- [[skills/angular/architecture/v3.1/solutions/solution-logging-global.skill/solution-logging-global.skill.md|solution-logging-global]] (VP6, `.extend` — `Logging/backend-log-sink.ts.create` + `Logging/log-retry-queue.ts.create` + `Logging/logger.service.ts.extend` — a second `LogSink` on the same token, the bounded IndexedDB retry queue, and `LogLevel` gaining `'report'` + `LoggerService.report()`)

This is the shallowest plateau where both coexist — `solution-logging-base` is a common-baseline convention present from `plateau-online-monolith`; VP6 is first Yes here.

# Classification
`TMN` — Constraint `T` (VP6 `BackendLogDelivery` `requires` the base `ConsoleLogging` seam **and** VP3 `BackendDataAccess`; `solution-logging-global` declares `depends_on solution-logging-base` and `depends_on solution-api-http-layer`). Category `M` (code change: `logger.service.ts` gains a level and a method; the `LOG_SINKS` factory returns a second sink). Kind `N` (independent): `BackendLogSink` is a *new* provider on the multi-provider token the base built for exactly this; the `logger.service.ts.extend` only adds `report` to the `ORDER` map and a `report()` method — it removes nothing and changes no existing call path (`if (level !== 'report' && ...)` is purely additive).

# Ordering
`source: constraint` — VP6 requires the base seam, so `solution-logging-base` (create) always precedes `solution-logging-global` (extend). Recorded by `solution-logging-global`'s `depends_on solution-logging-base`.

# Resolution
**Canonical — no resolver.** The `LOG_SINKS` multi-provider token exists specifically to be extended with more sinks; adding `BackendLogSink` needs zero changes at any `LoggerService` call site. The example's `logger.service.spec.ts` asserts `report()` reaches the backend sink regardless of `MIN_LOG_LEVEL`, and `backend-log-sink.spec.ts` / `log-retry-queue.spec.ts` cover batching, `sendBeacon`, and bounded eviction.

# Architectural signal
N = 2. **Benign.** A pluggable-sinks logger extended with a second sink is the design intent — the exact analogue of the `store.config.ts` slice seam. Not a mis-drawn VP.
