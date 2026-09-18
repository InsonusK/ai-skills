---
name: plateau-dual-api-service--file-logging-logger
description: internal/logging/logger.go of the plateau-dual-api-service plateau
whenToUse: when creating or editing internal/logging/logger.go
domain: skill
type: template
plateau: plateau-dual-api-service
version: 20260917010000
tags:
  - skill/template/file
  - plateau/plateau-dual-api-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]]"
---

# Goal
Set the process-wide `slog` default handler once, from a single log-level setting.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/logging/logger.go.create.md|internal/logging/logger.go]]

# Core Principles
- Apply ONE plateau template per file.

# Implementation
```go
// Skill: file-logging-logger
// Plateau: plateau-dual-api-service
// Version: 20260917010000

package logging

import (
	"log/slog"
	"os"
	"strings"
)

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
Verified against this plateau's own `example/internal/logging/logger.go`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/logging/logger.go.create.md|internal/logging/logger.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Init` is the only place `slog.SetDefault` is called.
- Never construct a per-package or per-request `*slog.Logger` instance.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/logging/logger.go.create.md#MUST|internal/logging/logger.go]]

# Unittest TestCases
- [ ] WHEN `parseLevel` is called with `"debug"`, `"warn"`, `"warning"`, `"error"`, or an unrecognized/empty string THEN it returns the matching `slog.Level`, defaulting to `LevelInfo`

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/logging/logger.go.create.md|internal/logging/logger.go]]
