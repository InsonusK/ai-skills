---
description: main — the standalone deploy-time-job binary; loads config, calls Migrate, exits
project_name: "cmd/migrate"
name: main
element_kind: functions
change_kind: create
tags:
  - solution/go-db-migrations
  - element/cmd-migrate-main-go
---

# Naming convention
| use case | identifier name pattern | identifier name | file name pattern | file name |
| -------- | ------------------------ | ---------------- | ------------------- | --------- |
| the job binary | `main` | `main` | `main.go` | `main.go` |

# Implementation changes
```go
package main

import (
	"context"
	"log/slog"
	"os"

	"{module-path}/internal/config"
	"{module-path}/internal/infrastructure/{store}"
)

func main() {
	if err := run(); err != nil {
		slog.Error("fatal", "error", err)
		os.Exit(1)
	}
}

func run() error {
	cfg, err := config.Load()
	if err != nil {
		return err
	}

	ctx := context.Background()
	return {store}.Migrate(ctx, cfg.DatabaseDSN)
}
```

This is `cmd/{service}/main.go`'s own composition-root pattern (`main` logs and sets the exit code,
`run` returns an `error`), reused verbatim for a one-shot job instead of a long-running service — see
`solution-go-repository-structure`'s own `main.go.create.md` for the pattern this mirrors.

# Rule changes

## MUST
- `run()` must do nothing beyond loading config and calling `{store}.Migrate` — no server, no domain
  service, no other adapter.
  - Risk: growing this binary into a second composition root invites drift from `cmd/{service}`'s own
    wiring and defeats its purpose as a minimal, single-job binary.
  - Fix: keep `run()` to exactly the two calls shown; add nothing else here.
- `main()` must exit non-zero on any `Migrate` failure, via the same `slog.Error` + `os.Exit(1)`
  pattern `cmd/{service}/main.go` uses.
  - Risk: a deploy pipeline that does not see a non-zero exit code proceeds to roll out a service
    version against a schema that failed to migrate.
  - Fix: reuse the baseline's own `main()` shape unchanged; never swallow `run()`'s error.

# Check list
- [ ] `cmd/migrate`'s `run()` calls only `config.Load()` and `{store}.Migrate`.
- [ ] A `Migrate` failure exits the process with a non-zero status.

# Unittest TestCases
- [ ] Not unit-tested directly — same reasoning as `cmd/{service}/main.go`'s own baseline: correctness
      is verified by `go build` and by running this binary against a real PostgreSQL in the
      ground-truth example.
