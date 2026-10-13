---
name: solution-go-app-logging
description: Process-wide structured logging via log/slog, initialized once at startup from a single log-level setting
whenToUse: when a Go web-service needs its first logging call, or reviewing whether a package logs through a package-level logger instead of constructing its own
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/go-app-logging
  - stack/go
  - concern/architecture
creates:
  - "internal/logging/logger.go"
extends:
  - "cmd/{service}/main.go"
  - "internal/config/config.go"
depends_on:
built_on_plateau:
adr:
---

# Goal
- Give every package in the module one logging convention: call `slog.Info`/`slog.Warn`/`slog.Error`/`slog.Debug` directly, against a single process-wide default handler configured once at startup.

# Capabilities
- Structured, leveled logs from every package with zero per-package setup — `slog`'s package-level default handler is set once, in `main.go`, before anything else runs.
- One env var (`LOG_LEVEL`) controls verbosity for the whole process.

# Core Principles
- `log/slog`'s global default logger is the only logging mechanism in this catalog — no per-component logger instance, no context-threaded logger, no third-party logging library.
- Every log call names the acting package and the action in its message (e.g. `"http: NotifyQuestion received"`), and attaches identifying values as structured key-value pairs, never string-interpolated into the message.

# Boundaries
- Reading logs back out (querying, shipping to a log aggregator) is out of scope — this solution only covers writing structured logs to stderr.

# Requirements
GO MODULES / STANDARD LIBRARY:
- `log/slog` (standard library) — `slog.NewTextHandler`, `slog.SetDefault`

# Template Skill Mutations
FILES:
- [[./Implementation/internal/logging/logger.go.create.md|internal/logging/logger.go]] - create - `Init(level)` sets the process-wide default handler
- [[./Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]] - extend - call `logging.Init(cfg.LogLevel)` first in `run()`
- [[./Implementation/internal/config/config.go.extend.md|internal/config/config.go]] - extend - add `LogLevel`

# Workflow

## Startup
1. `run()` loads config.
2. `run()` calls `logging.Init(cfg.LogLevel)` before constructing anything else.
3. Every subsequent `slog.*` call anywhere in the process goes through the handler `Init` installed.

# Rules

## MUST
- [[./Implementation/internal/logging/logger.go.create.md#MUST|internal/logging/logger.go]]
- [[./Implementation/cmd/{service}/main.go.extend.md#MUST|cmd/{service}/main.go]]

# Check list
- [ ] `logging.Init` is called before any other constructor in `run()`.
- [ ] No package other than `internal/logging` constructs its own `slog.Handler`.
