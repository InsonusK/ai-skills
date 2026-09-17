---
description: The composition root — loads config, wires adapters in run(), no business logic
project_name: "cmd/{service}"
name: main
element_kind: functions
change_kind: create
tags:
  - solution/go-repository-structure
  - element/cmd-service-main-go
---

# Goals
- Give the module a single place where every concrete adapter gets constructed and connected.

# Core Principles
- `run()` returns `error`; `main()` is the only place that logs a fatal error and sets the process exit code.
- At this solution's own baseline, `run()` does nothing beyond loading config — every capability-adding solution extends it with its own construction call.

# Implementation changes
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

# Rule changes

## MUST
- `run()` must return an `error` and do nothing else at this solution's baseline — no adapter, no server, no goroutine.
  - Risk: adding a placeholder adapter here invites every later solution to guess whether it should replace or extend that placeholder, instead of cleanly adding its own.
  - Fix: keep `run()` to config loading only; every extending solution adds its own construction call and, if it starts a long-running loop, converts `run()`'s body to an `errgroup.Group` at the point the *second* long-running loop is added (not before).
- Never place business logic in `main.go`.
  - Risk: the composition root starts enforcing business rules instead of just wiring adapters together.
  - Fix: keep business invariants in `internal/domain/services`; `main.go` only constructs and connects.

# Check list
- [ ] `main.go` imports nothing beyond `config`, `log/slog`, and `os` at this solution's baseline.

# Unittest TestCases
- [ ] Not unit-tested directly — `run()`'s own behavior is exercised through each extending solution's addition; `main.go` itself stays thin enough that its correctness is verified by `go build` and the ground-truth example running.
