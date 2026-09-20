---
description: Call Migrate before constructing Store, but only when MigrateOnStart is set — the startup-run mode's only call site
project_name: "cmd/{service}"
name: main
element_kind: functions
change_kind: extend
tags:
  - solution/go-db-migrations
  - element/cmd-service-main-go
---

# Implementation changes

**AS IS** (from [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/cmd/{service}/main.go.extend.md|solution-persistent-db's own main.go]]):
```go
func run() error {
	// ... config load, logging.Init unchanged ...

	store, err := {store}.New(ctx, cfg.DatabaseDSN)
	if err != nil {
		return err
	}
	defer store.Close()

	domainService := services.New{Service}(store)
}
```

**TO BE** (this solution — startup-run mode only):
```go
func run() error {
	// ... config load, logging.Init unchanged ...

	if cfg.MigrateOnStart {
		if err := {store}.Migrate(ctx, cfg.DatabaseDSN); err != nil {
			return fmt.Errorf("migrate: %w", err)
		}
	}

	store, err := {store}.New(ctx, cfg.DatabaseDSN)
	if err != nil {
		return err
	}
	defer store.Close()

	domainService := services.New{Service}(store)
}
```

When `cfg.MigrateOnStart` is `false` (this solution's default — see
[[../../internal/config/config.go.extend.md|config.go]]), `run()` behaves exactly as
`solution-persistent-db` left it: `{store}.New` connects to a schema `cmd/migrate` already
migrated. See [[../../../../adr/migration-mode-per-platform.md|adr/migration-mode-per-platform.md]]
for which deployment platforms set `MIGRATE_ON_START=true` and why.

# Rule changes

## MUST
- The `if cfg.MigrateOnStart` call must run to completion (or fail `run()`) before `{store}.New` is
  ever called — never in parallel, never after.
  - Risk: constructing a `Store` before the schema exists turns a startup-time configuration problem
    into a request-time "relation does not exist" failure discovered later, in production traffic.
  - Fix: `return` immediately if `Migrate` fails; call `{store}.New` only after it succeeds (or was
    skipped because `MigrateOnStart` is `false`).
- Never call `{store}.Migrate` from `run()` unconditionally — always gate it behind
  `cfg.MigrateOnStart`.
  - Violation: calling `{store}.Migrate(ctx, cfg.DatabaseDSN)` without the `if cfg.MigrateOnStart`
    guard.
  - Risk: an unconditional call reintroduces this solution's original startup-run-only design,
    silently overriding whatever platform-level Job the deploying team may already rely on and
    reintroducing the concurrency/masking problems `adr/migration-mode-per-platform.md` documents —
    every replica of every deployment would attempt it on every restart, not just the ones a team
    deliberately opted into startup-run mode.
  - Fix: keep the `if cfg.MigrateOnStart` guard exactly as shown; the flag is this solution's only
    switch between its two modes.

# Check list
- [ ] `{store}.Migrate` is called from `run()` only inside `if cfg.MigrateOnStart`, never
      unconditionally.
- [ ] When the call does run, it completes (or fails `run()`) before `{store}.New` is called.
