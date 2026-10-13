---
description: Process-wide structured logging setup
project_name: internal/logging
name: logging
element_kind: functions
change_kind: create
tags:
  - solution/go-app-logging
  - element/internal-logging-logger-go
---

# Goals
- Set the process-wide `slog` default handler once, from a single log-level setting.

# Implementation changes
```go
// Package logging configures the process-wide structured logger.
package logging

import (
	"log/slog"
	"os"
	"strings"
)

// Init sets the process-wide default slog handler. Call it once, in main,
// before any other package logs.
func Init(level string) {
	slog.SetDefault(slog.New(slog.NewTextHandler(os.Stderr, &slog.HandlerOptions{
		Level: parseLevel(level),
	})))
}

func parseLevel(level string) slog.Level {
	switch strings.ToLower(level) {
	case "debug":
		return slog.LevelDebug
	case "warn", "warning":
		return slog.LevelWarn
	case "error":
		return slog.LevelError
	default:
		return slog.LevelInfo
	}
}
```

# Rule changes

## MUST
- `Init` must be the only place in the module that calls `slog.SetDefault`.
  - Risk: a second call elsewhere silently overrides this one, and which handler ends up active depends on call order instead of being obvious from reading `main.go`.
  - Fix: keep `slog.SetDefault` to this one function; every other package logs via the package-level `slog.*` functions only.
- Never construct a per-package or per-request `*slog.Logger` instance.
  - Risk: a second logger instance can be configured differently (level, format) from the process-wide default, so two packages' logs stop being comparable.
  - Fix: call the package-level `slog.Info`/`slog.Warn`/`slog.Error`/`slog.Debug` functions everywhere; they route through the one default handler `Init` installed.

# Check list
- [ ] `parseLevel` defaults to `slog.LevelInfo` for an empty or unrecognized value.

# Unittest TestCases
- [ ] WHEN `parseLevel` is called with `"debug"`, `"warn"`, `"warning"`, `"error"`, or an unrecognized/empty string THEN it returns the matching `slog.Level`, defaulting to `LevelInfo`
