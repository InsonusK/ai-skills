---
name: project-version-in-go
description: The version of a Go application — var Version in internal/version/version.go, the one place it is recorded and the variable the program reports, overridden for a snapshot with -ldflags -X
whenToUse: when a Go program must report its version, when you add or review internal/version/version.go, or when a Go build must carry another version than the recorded one
updated: 20261024
tags:
  - stack/go
  - concern/coding
  - versioning
adr:
  - adr/version-source-file.md
---

# Goal
- **The version in Go** - A Go application that keeps every rule of [project-version](library/develop/core/project-version.skill.md); this skill details it for Go and restates none of its rules.

# Core Principle
- **Recorded in the code that reports it** - `go.mod` has no version field, so the number lives in the variable the program prints; a plain `go build` is right. Decision recorded in [version-source-file](./adr/version-source-file.md).

# Rule

## MUST

### Record the version in internal/version/version.go
Declare the version as `var Version = "MAJOR.MINOR.PATCH"` on one line of `internal/version/version.go`, package `version`.
- Violation: `const Version = "1.4.0"`; the variable in another package; `Version = "dev"` with the number in a `VERSION` file.
- Risk: a constant cannot be overridden for a snapshot; another path is not found by the tools that read it; with `"dev"` in source every build that forgets the flag reports `dev`.
- Fix: one `var` line in `internal/version/version.go`; delete the `VERSION` file.

### Report version.Version
Report the running version by reading `version.Version` (`logger.Info("starting", "version", version.Version)`).
- Violation: a second string with the number in `main.go` or a CLI command.
- Risk: the second string is not raised with the first.
- Fix: import `{module-path}/internal/version`.

### Override the version only for a snapshot
Pass `-ldflags "-X {module-path}/internal/version.Version={version}"` only when a build must carry a version other than the recorded one, and build without it otherwise.
- Violation: a `Makefile` or `Dockerfile` that always injects a version read from somewhere else.
- Risk: a second mechanism has to agree with the source, and a wrong package path in the flag is ignored silently.
- Fix: `VERSION ?=` in the `Makefile` and `ARG VERSION=` in the `Dockerfile`, with the flag added only when the value is not empty.

# Check list
- [ ] `internal/version/version.go` has exactly one `var Version = "MAJOR.MINOR.PATCH"` line; no `VERSION` file exists.
- [ ] The program reports `version.Version`.
- [ ] A build with `-ldflags "-X …/internal/version.Version=9.9.9"` reports `9.9.9`; a plain `go build` reports the recorded version.
