---
description: Construct the PostgreSQL-backed store and pass it to the domain service
project_name: "cmd/{service}"
name: main
element_kind: functions
change_kind: extend
tags:
  - solution/persistent-db
  - element/cmd-service-main-go
---

# Implementation changes
```go
import (
	// ...
	"{module-path}/internal/infrastructure/{store}"
)

func run() error {
	// ... config load, logging.Init unchanged ...

	store, err := {store}.New(ctx, cfg.DatabaseDSN)
	if err != nil {
		return err
	}
	defer store.Close()

	domainService := services.New{Service}(store) // appends to any port(s) another applied solution already added
}
```

# Rule changes

## MUST
- `{store}.New` must be called with the same `ctx` `run()` uses for shutdown, and its error checked before anything is constructed on top of it.
  - Risk: ignoring a connection failure at startup lets the process report itself healthy while every real request then fails.
  - Fix: `return err` immediately if `{store}.New` fails; nothing later in `run()` executes.
- `store.Close()` must be deferred immediately after a successful `New`.
  - Risk: an un-closed connection pool leaks every process restart.
  - Fix: `defer store.Close()` right after the error check.

# Check list
- [ ] `run()` returns immediately if `{store}.New` fails.
- [ ] `store.Close()` is deferred immediately after construction.
