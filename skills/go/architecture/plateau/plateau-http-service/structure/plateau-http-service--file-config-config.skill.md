---
name: plateau-http-service--file-config-config
description: internal/config/config.go of the plateau-http-service plateau
whenToUse: when adding a new runtime setting, or creating or editing internal/config/config.go
domain: skill
type: template
plateau: plateau-http-service
version: 20260917000000
tags:
  - skill/template/file
  - plateau/plateau-http-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
  - "[[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]]"
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
registry:
  - "[[skills/go/architecture/registry/internal-config-config-go.md|internal-config-config-go]]"
---

# Goal
Load runtime settings from environment variables into one flat `Config` struct.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/config/config.go.create.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]

# Core Principles
- Apply ONE plateau template per file.
- Every setting supports a `<NAME>_FILE` container-secret override.
- `Load()` stops at the first error.

# Implementation
```go
// Skill: file-config-config
// Plateau: plateau-http-service
// Version: 20260917000000

package config

import (
	"fmt"
	"os"
	"strconv"
	"strings"
)

type Config struct {
	LogLevel       string
	HTTPListenPort string
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		LogLevel:       l.get("LOG_LEVEL", "info"),
		HTTPListenPort: l.get("HTTP_LISTEN_PORT", "8080"),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}

type loader struct{ err error }

func (l *loader) get(key, def string) string { /* ... see solution-go-repository-structure's Implementation ... */ return def }
func (l *loader) getInt(key string, def int) int { return def }
func (l *loader) getBool(key string, def bool) bool { return def }
func (l *loader) require(key string) string { return "" }
```
Verified against this plateau's own `example/internal/config/config.go` (`go build`/`go vet` clean).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/config/config.go.create.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- An extending solution adds its own field(s) to the same `Config{...}` literal — never a second `loader{}`/struct literal.
- Never read `os.Getenv` directly outside this file.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/config/config.go.create.md#MUST|internal/config/config.go]]

# Check list
- [ ] `LOG_LEVEL` defaults to `"info"`; `HTTP_LISTEN_PORT` defaults to `"8080"`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]

# Unittest TestCases
- [ ] WHEN a required variable is unset THEN `Load` returns a non-nil error naming that variable
- [ ] WHEN `{NAME}_FILE` and `{NAME}` are both set THEN the file's trimmed contents win

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/config/config.go.create.md|internal/config/config.go]]
