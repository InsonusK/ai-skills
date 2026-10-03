---
name: plateau-gw009-001--file-cmd-migrate-main
description: cmd/migrate/main.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when editing the migration job's entry point
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]]"
source: cmd/migrate/main.go
---

# Goal
Apply every pending migration and exit.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/migrate/main.go.create.md|main.go]]

# Core Principles
- Apply ONE plateau template per file.
- Exit code non-zero on any migration error, so a deploy pipeline stops before the service starts.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/migrate/main.go.create.md|main.go]]

# Implementation
```go
// Skill: file-cmd-migrate-main
// Plateau: plateau-gw009-001
// Version: 20260928120000

package main

import (
	"context"
	"log/slog"
	"os"

	"{module-path}/internal/config"
	"{module-path}/internal/infrastructure/linkstore"
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
	return linkstore.Migrate(ctx, cfg.DatabaseDSN)
}
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/migrate/main.go.create.md|main.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Never do anything but load config and call `linkstore.Migrate`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/migrate/main.go.create.md|main.go]]

# Check list
- [ ] `DATABASE_DSN=… go run ./cmd/migrate` exits 0 on an up-to-date database.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-db-migrations.skill/solution-go-db-migrations.skill.md|solution-go-db-migrations]] - [[skills/go/architecture/solutions/solution-go-db-migrations.skill/Implementation/cmd/migrate/main.go.create.md|main.go]]
