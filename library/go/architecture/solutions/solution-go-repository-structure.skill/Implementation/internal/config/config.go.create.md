---
description: Env-var configuration loader — a flat Config struct (starting empty) plus a loader helper every later solution extends
project_name: internal/config
name: config
element_kind: functions
change_kind: create
tags:
  - solution/go-repository-structure
  - element/internal-config-config-go
---

# Goals
- Give every solution that needs a runtime setting one place to add it — a field on `Config` and one line in `Load()` — instead of inventing a second configuration mechanism.
- Support a container-secret override (`<NAME>_FILE`) on every setting without any extending solution having to reimplement it.

# Core Principles
- `Config` is a flat struct; no nested structs, no per-feature sub-config type.
- Every setting is read from an environment variable, with an optional `<NAME>_FILE` path that wins when set (its trimmed file contents become the value) — the container-secrets convention.
- `Load()` stops at the first error, so an extending solution's own `Config` field reads as one line in a flat literal instead of an if-err-chain.

# Naming convention
| use case | identifier name pattern | identifier name | file name pattern | file name |
| -------- | ------------------------ | ---------------- | ------------------- | --------- |
| a new setting | `{Concern}{Aspect}` | `RedisHost` | (extends this file) | config.go |

# Implementation changes
```go
// Package config loads runtime settings from environment variables.
package config

import (
	"fmt"
	"os"
	"strconv"
	"strings"
)

// Config holds all runtime settings, sourced from environment variables.
// Every extending solution adds its own fields here.
type Config struct{}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{}
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

// require is like get, but records an error if the resulting value is empty.
func (l *loader) require(key string) string {
	v := l.get(key, "")
	if l.err == nil && v == "" {
		l.err = fmt.Errorf("%s is required", key)
	}
	return v
}
```

# Rule changes

## MUST
- An extending solution adds its own field to `Config` and its own `l.get`/`l.getInt`/`l.getBool`/`l.require` call inside the same `cfg := &Config{...}` literal — never a second call to `loader{}` or a second struct literal.
  - Risk: a second `loader` instance loses the "stop at first error" guarantee across the two, so a later field's error can silently overwrite or hide an earlier one.
  - Fix: extend the one existing struct literal and the one existing `l` instance.
- Never read `os.Getenv` directly outside this file.
  - Risk: a setting read directly bypasses the `<NAME>_FILE` secret convention, so it silently behaves differently from every other setting in a container deployment.
  - Fix: add the setting to `Config`/`Load()` here, even when only one extending solution needs it.

# Check list
- [ ] `Config` compiles with zero fields at this solution's baseline.
- [ ] Every later field added by an extending solution goes through `l.get`/`l.getInt`/`l.getBool`/`l.require`.

# Unittest TestCases
- [ ] WHEN a required variable is unset THEN `Load` returns a non-nil error naming that variable
- [ ] WHEN `{NAME}_FILE` and `{NAME}` are both set THEN the file's trimmed contents win
