---
name: plateau-gw009-001--file-config-config
description: internal/config/config.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when adding or changing an environment setting of this plateau, including the migration mode and the TaskBox worker settings
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
  - "[[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]]"
  - "[[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]]"
  - "[[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
  - "[[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]"
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: internal/config/config.go
---

# Goal
Load every runtime setting from environment variables, including the migration mode and the TaskBox worker pool.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/config/config.go.create.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/config/config.go.extend.md|config.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/config/config.go.extend.md|config.go]]

# Core Principles
- Apply ONE plateau template per file.
- Every setting supports a `<NAME>_FILE` container-secret override.
- `Load()` stops at the first error.
- `MIGRATE_ON_START` defaults to `false` (Job mode).
- `TASKBOX_WORKERS` (2), `TASKBOX_LEASE` (5m), `TASKBOX_POLL_INTERVAL` (1s), `RECHECK_AFTER` (1h) — durations parsed by `getDuration`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/config/config.go.create.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/config/config.go.extend.md|config.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/config/config.go.extend.md|config.go]]

# Implementation
```go
// Skill: file-config-config
// Plateau: plateau-gw009-001
// Version: 20260928120000

// Package config loads runtime settings from environment variables.
package config

import (
	"fmt"
	"os"
	"strconv"
	"strings"
	"time"
)

// Config holds all runtime settings, sourced from environment variables.
type Config struct {
	LogLevel         string
	HTTPListenPort   string
	GRPCListenPort   string
	ReputationAddr   string
	RedisHost        string
	RedisPort        string
	RedisPassword    string
	RedisDB          int
	DatabaseDSN      string
	MigrateOnStart   bool
	TaskWorkers      int
	TaskLease        time.Duration
	TaskPollInterval time.Duration
	RecheckAfter     time.Duration
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		LogLevel:         l.get("LOG_LEVEL", "info"),
		HTTPListenPort:   l.get("HTTP_LISTEN_PORT", "8080"),
		GRPCListenPort:   l.get("GRPC_LISTEN_PORT", "50051"),
		ReputationAddr:   l.require("REPUTATION_ADDR"),
		RedisHost:        l.get("REDIS_HOST", "localhost"),
		RedisPort:        l.get("REDIS_PORT", "6379"),
		RedisPassword:    l.get("REDIS_PASSWORD", ""),
		RedisDB:          l.getInt("REDIS_DB", 0),
		DatabaseDSN:      l.require("DATABASE_DSN"),
		MigrateOnStart:   l.getBool("MIGRATE_ON_START", false),
		TaskWorkers:      l.getInt("TASKBOX_WORKERS", 2),
		TaskLease:        l.getDuration("TASKBOX_LEASE", 5*time.Minute),
		TaskPollInterval: l.getDuration("TASKBOX_POLL_INTERVAL", time.Second),
		RecheckAfter:     l.getDuration("RECHECK_AFTER", time.Hour),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}

// loader reads env vars, stopping at the first error so Load's field list
// stays a flat struct literal instead of an if-err-chain per field.
type loader struct {
	err error
}

// get returns the env var's value, or def if unset. Every var also accepts
// a "<NAME>_FILE" form pointing at a file whose trimmed contents become the
// value (container-secret convention); if both are set, "<NAME>_FILE" wins.
func (l *loader) get(key, def string) string {
	if l.err != nil {
		return ""
	}
	if path := os.Getenv(key + "_FILE"); path != "" {
		data, err := os.ReadFile(path)
		if err != nil {
			l.err = fmt.Errorf("read %s_FILE (%s): %w", key, path, err)
			return ""
		}
		return strings.TrimSpace(string(data))
	}
	if v := os.Getenv(key); v != "" {
		return v
	}
	return def
}

// getInt is like get, but parses the value as an integer.
func (l *loader) getInt(key string, def int) int {
	v := l.get(key, "")
	if l.err != nil || v == "" {
		return def
	}
	n, err := strconv.Atoi(v)
	if err != nil {
		l.err = fmt.Errorf("%s: %w", key, err)
		return def
	}
	return n
}

// getBool is like get, but parses the value as a boolean.
func (l *loader) getBool(key string, def bool) bool {
	v := l.get(key, "")
	if l.err != nil || v == "" {
		return def
	}
	b, err := strconv.ParseBool(v)
	if err != nil {
		l.err = fmt.Errorf("%s: %w", key, err)
		return def
	}
	return b
}

// getDuration is like get, but parses the value as a time.Duration ("90s").
func (l *loader) getDuration(key string, def time.Duration) time.Duration {
	v := l.get(key, "")
	if l.err != nil || v == "" {
		return def
	}
	d, err := time.ParseDuration(v)
	if err != nil {
		l.err = fmt.Errorf("%s: %w", key, err)
		return def
	}
	return d
}

// require is like get, but records an error if the resulting value is empty.
func (l *loader) require(key string) string {
	v := l.get(key, "")
	if l.err == nil && v == "" {
		l.err = fmt.Errorf("%s is required", key)
	}
	return v
}
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/config/config.go.create.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/config/config.go.extend.md|config.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/config/config.go.extend.md|config.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Never apply several plateau templates per file.
- An extending solution adds its own field(s) to the same `Config{...}` literal — never a second `loader{}`/struct literal.
- Never read `os.Getenv` directly outside this file.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/config/config.go.create.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/config/config.go.extend.md|config.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/config/config.go.extend.md|config.go]]

# Check list
- [ ] `LOG_LEVEL` defaults to `"info"`; `HTTP_LISTEN_PORT` defaults to `"8080"`; `GRPC_LISTEN_PORT` defaults to `"50051"`; `REPUTATION_ADDR` and `DATABASE_DSN` are required; `REDIS_HOST`/`REDIS_PORT` default to `localhost`/`6379`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/config/config.go.create.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]] - [[skills/go/architecture/solutions/solution-go-app-logging.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-http-api.skill/solution-go-http-api.skill.md|solution-go-http-api]] - [[skills/go/architecture/solutions/solution-go-http-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] - [[skills/go/architecture/solutions/solution-grpc-api.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/config/config.go.extend.md|internal/config/config.go]]
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/internal/config/config.go.extend.md|config.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/config/config.go.extend.md|config.go]]
