---
name: solution-go-repository-structure
description: The baseline repository layout for a Go web-service — go.mod, Makefile, and a cmd/{service}/main.go composition root with no adapters wired yet
whenToUse: when starting a new Go web-service repository from scratch, or reviewing whether an existing one's root layout (go.mod, Makefile, cmd/{service}/main.go) matches this family's baseline
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/go-repository-structure
  - stack/go
  - concern/architecture
creates:
  - "go.mod"
  - "Makefile"
  - "cmd/{service}/main.go"
  - "internal/config/config.go"
  - "internal/version/version.go"
extends:
depends_on:
built_on_plateau:
adr:
---

# Goal
- Give every Go web-service in this family the same starting point: a pinned `go.mod`, a `Makefile` with the three universal lifecycle targets (`build`, `run`, `lint`), and a `cmd/{service}/main.go` composition root that loads config and exits cleanly, with nothing else wired in yet.
- Establish the env-var configuration convention (a flat `Config` struct, a small `loader` helper, and the `<NAME>_FILE` secret-file override) every later solution extends with its own fields.

# Capabilities
- A repository any later solution in this catalog can extend without first inventing where config loading, the build, or the composition root live.
- One place (`main.go`'s `run()`) every adapter's wiring converges on, so reading `main.go` alone tells a reader everything the service does today.

# Core Principles
- `go.mod` is the only place a dependency's version is pinned — this family needs no separate central-package-management solution the way a NuGet-based stack does.
- `main.go` is the single composition root: every `New*` constructor call for a concrete adapter happens inside `run()`, nowhere else.
- Config is a flat struct sourced from environment variables, starting with zero fields — each solution that needs one extends `Config` and `Load()` with its own, never introduces a second config mechanism.
- `run()` returns `error`; `main()` is the only place that logs a fatal error and sets the process exit code.

# Requirements
GO MODULES / STANDARD LIBRARY:
- `log/slog` (standard library) — `main()`'s own fatal-error report; the process-wide logging convention itself belongs to [[skills/go/architecture/solutions/solution-go-app-logging.skill/solution-go-app-logging.skill.md|solution-go-app-logging]], not this solution.
- `os`, `os/signal`, `syscall` (standard library) — process exit code and `SIGINT`/`SIGTERM`-triggered shutdown context.

# Template Skill Mutations
FILES:
- [[./Implementation/Repository.create.md|Repository]] - create - `go.mod`, `Makefile` (`build`/`run`/`lint` targets), `cmd/{service}/main.go`, `.gitignore`
- [[./Implementation/internal/config/config.go.create.md|internal/config/config.go]] - create - env-var `Config` loader, starting with zero `Config` fields
- [[./Implementation/internal/version/version.go.create.md|internal/version/version.go]] - create - build-time version variable

# Workflow

## Cold start (happy path)
1. `main()` calls `run()`.
2. `run()` calls `config.Load()`.
3. `run()` returns `nil` — nothing else is wired yet at this solution's baseline; every later solution extends this method to construct and start its own adapter.
4. `main()` sees a `nil` error and returns, exit code `0`.

## Config load failure
1. `config.Load()` finds a required environment variable unset, or an `<NAME>_FILE` path that cannot be read.
2. `run()` returns that error unchanged.
3. `main()` logs it via `slog.Error("fatal", "error", err)` and calls `os.Exit(1)`.

# Rules

## MUST
- [[./Implementation/Repository.create.md#MUST|Repository]]
- [[./Implementation/internal/config/config.go.create.md#MUST|internal/config/config.go]]
- [[./Implementation/internal/version/version.go.create.md#MUST|internal/version/version.go]]

## SHOULD
- [[./Implementation/Repository.create.md#SHOULD|Repository]]

# Check list
- [ ] `go.mod` exists with a pinned Go version and no unpinned dependency.
- [ ] `make build`, `make run`, and `make lint` all succeed against the empty baseline.
- [ ] `cmd/{service}/main.go` contains no adapter construction — `run()` only loads config and returns.
- [ ] `internal/config/config.go`'s `Config` struct compiles with zero fields.
