---
description: Repository root — go.mod, Makefile (build/run/lint), .gitignore
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
    main.go        ← own file-tier Implementation file, see ./cmd/{service}/main.go.create.md
```

## Directory and package skills
| Directory | file | Description |
| --------- | ---- | ----------- |
| cmd/{service} | main.go | Composition root — its own file-tier element, not repo-tier content; see [[./cmd/{service}/main.go.create.md]] |

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

`.gitignore`:
```
/bin/
```

# Rule

## MUST
- The `unit-test`/`mutation-test`/`test-report`/`test-and-report` Makefile targets are never defined here — [[skills/go/testing/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md|solution-conformance-testing-in-go]] extends this `Makefile` with the full four-target testing contract; this solution only establishes `build`/`run`/`lint`.
  - Risk: duplicating or pre-guessing the testing targets here would drift from the shared `solution-conformance-testing` contract the moment that solution defines them for real.
  - Fix: leave testing entirely to the extending solution; this Makefile stays lifecycle-only.

## SHOULD
- Pin the Go toolchain version in `go.mod` to the version the repository's CI actually runs, updating both together.

# Check list
- [ ] `go build ./...` succeeds against the empty baseline.
- [ ] `go vet ./...` reports nothing.
