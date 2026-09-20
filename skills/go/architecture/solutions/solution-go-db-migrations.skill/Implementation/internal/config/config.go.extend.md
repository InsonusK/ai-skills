---
description: Add MigrateOnStart — the one flag choosing between this solution's two supported modes
project_name: internal/config
name: config
element_kind: functions
change_kind: extend
tags:
  - solution/go-db-migrations
  - element/internal-config-config-go
---

# Implementation changes

**AS IS** (from [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/config/config.go.extend.md|solution-persistent-db's own config.go]]):
```go
type Config struct {
	DatabaseDSN string
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		DatabaseDSN: l.require("DATABASE_DSN"),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}
```

**TO BE** (this solution):
```go
type Config struct {
	DatabaseDSN    string
	MigrateOnStart bool
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		DatabaseDSN:    l.require("DATABASE_DSN"),
		MigrateOnStart: l.getBool("MIGRATE_ON_START", false),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}
```

`MigrateOnStart` selects between this solution's two mutually exclusive modes — see
[[../../../../solution-go-db-migrations.skill.md#Core%20Principles|this solution's own Core
Principles]] and [[../../../../adr/migration-mode-per-platform.md|adr/migration-mode-per-platform.md]]
for which platform sets it to which value and why. It defaults to `false` (Job mode) — a
deployment must opt into startup-run explicitly, never fall into it by leaving the variable unset.

# Rule changes

## MUST
- `MIGRATE_ON_START` must default to `false` when unset — never default to `true`.
  - Risk: a deployment platform's example config that forgets to set the variable at all would
    silently run in startup-run mode, which is only safe for a genuinely single-instance deployment
    — defaulting to `true` risks a multi-replica deployment inheriting the concurrency problems this
    mode exists to avoid, purely from an omission.
  - Fix: `l.getBool("MIGRATE_ON_START", false)`, exactly as shown above.

# Check list
- [ ] `MIGRATE_ON_START` defaults to `false` when the environment variable is unset.
- [ ] `DATABASE_DSN` is still required — unchanged from `solution-persistent-db`'s own rule.
