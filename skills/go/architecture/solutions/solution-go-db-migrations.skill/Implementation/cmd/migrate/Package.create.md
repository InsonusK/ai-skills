---
description: A standalone binary that runs Migrate once and exits — this solution's Job-mode call site, run as a one-shot deploy-time job ahead of the app on platforms that use Job mode (see adr/migration-mode-per-platform.md)
name: "cmd/migrate"
element_kind: package
change_kind: create
tags:
  - solution/go-db-migrations
  - element/cmd-migrate
---

# Goals
- Give the deploy pipeline a binary that does exactly one thing — run `{store}.Migrate` and exit —
  so it can be wrapped as a Kubernetes `Job` (typically via a Helm `pre-install,pre-upgrade` hook) or
  a plain-manifest `kubectl apply` + `kubectl wait` step, per `devops-service-deploy.skill.md`'s own
  "migration step" rule: no HTTP server, no domain service, no long-lived process.

# Core Principles
- This solution's **Job-mode** call site — used on whichever platform
  [[../../../../adr/migration-mode-per-platform.md|adr/migration-mode-per-platform.md]] designates
  Job mode for (Kubernetes via Helm, or any platform where more than one instance of the service may
  run concurrently). On a platform designated **MigrateOnStart mode** instead, this binary exists but
  is never wired into the deploy pipeline — `cmd/{service}/main.go`'s own guarded call
  ([[../../internal/infrastructure/{store}/store.go.extend.md#MUST|store.go's own Rule]]) is the call
  site there. The two are never both wired for the same deployment. The schema's single source of
  truth is always `{store}`'s own `migrations/` directory, regardless of which call site applies it.

# Structure

## Repository place
```
cmd/
  migrate/
```

## Package Structure
```
cmd/migrate/
  main.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| main.go | Reads the DSN from config, calls `{store}.Migrate`, exits | [[./main.go.create.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/pressly/goose/v3 | v3.28 | transitively, via `{store}.Migrate` — this package never imports it directly |

# What Does NOT Belong Here
- Any domain-service construction, HTTP/gRPC server, or Kafka wiring — this binary's only job is to
  run `{store}.Migrate` and exit with a non-zero status on failure.
- A second goose call site — this package calls `{store}.Migrate`, never
  `github.com/pressly/goose/v3` directly (see [[./migrations.go.create.md#MUST|migrations.go's
  own Rule]] on this catalog's single call point).

# Allowed Dependencies
- `internal/config` (to load the DSN the same way `cmd/{service}/main.go` does)
- `internal/infrastructure/{store}` (for `{store}.Migrate`)

# Rules

## MUST
- [[./main.go.create.md#MUST|main.go]]

# Check list
- [ ] [[./main.go.create.md#Check list|main.go]]
- [ ] `cmd/migrate` imports no package outside `internal/config` and `internal/infrastructure/{store}`
