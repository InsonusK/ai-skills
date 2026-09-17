---
description: Repository root — go.mod, Makefile (build/run/lint), cmd/{service}/main.go composition root, .gitignore
element_kind: repository
change_kind: create
tags:
  - solution/go-repository-structure
  - element/repo-root
---

# Structure

## Repository Structure
```
go.mod
Makefile
.gitignore
cmd/
  {service}/
    main.go
```

## Directory and package skills
| Directory | file | Description |
| --------- | ---- | ----------- |
| cmd/{service} | main.go | Composition root: loads config, wires adapters in `run()`, no business logic |

# Implementation changes

`go.mod`:
```
module {module-path}

go 1.26
```

`Makefile`:
```makefile
SHELL := /bin/bash

.PHONY: build run lint

VERSION := $(shell cat VERSION 2>/dev/null || echo dev)
LDFLAGS := -X {module-path}/internal/version.Version=$(VERSION)

build:
	go build -ldflags "$(LDFLAGS)" -o bin/{service} ./cmd/{service}

run: build
	./bin/{service}

lint:
	go vet ./...
```

`cmd/{service}/main.go`:
```go
package main

import (
	"log/slog"
	"os"

	"{module-path}/internal/config"
)

func main() {
	if err := run(); err != nil {
		slog.Error("fatal", "error", err)
		os.Exit(1)
	}
}

func run() error {
	_, err := config.Load()
	if err != nil {
		return err
	}
	return nil
}
```

`.gitignore`:
```
/bin/
```

# Rule

## MUST
- The `unit-test`/`mutation-test`/`test-report`/`test-and-report` Makefile targets are never defined here — [[skills/go/architecture/solutions/solution-go-conformance-testing.skill/solution-go-conformance-testing.skill.md|solution-go-conformance-testing]] extends this `Makefile` with the full four-target testing contract; this solution only establishes `build`/`run`/`lint`.
  - Risk: duplicating or pre-guessing the testing targets here would drift from the shared `solution-conformance-testing` contract the moment that solution defines them for real.
  - Fix: leave testing entirely to the extending solution; this Makefile stays lifecycle-only.
- `run()` must return an `error` and do nothing else at this solution's baseline — no adapter, no server, no goroutine.
  - Risk: adding a placeholder adapter here invites every later solution to guess whether it should replace or extend that placeholder, instead of cleanly adding its own.
  - Fix: keep `run()` to config loading only; every extending solution adds its own construction call and, if it starts a long-running loop, converts `run()`'s body to an `errgroup.Group` at the point the *second* long-running loop is added (not before).
- Never place business logic in `main.go`.
  - Risk: the composition root starts enforcing business rules instead of just wiring adapters together.
  - Fix: keep business invariants in `internal/domain/services`; `main.go` only constructs and connects.

## SHOULD
- Pin the Go toolchain version in `go.mod` to the version the repository's CI actually runs, updating both together.

# Check list
- [ ] `go build ./...` succeeds against the empty baseline.
- [ ] `go vet ./...` reports nothing.
- [ ] `main.go` imports nothing beyond `config`, `log/slog`, and `os`.
