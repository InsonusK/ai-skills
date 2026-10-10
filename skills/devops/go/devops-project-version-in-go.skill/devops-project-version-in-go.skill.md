---
name: devops-project-version-in-go
description: Go implementation of devops-project-version — the version recorded as the string of `var Version` in internal/version/version.go, the read-version.sh that prints it, and the -ldflags override a snapshot build uses
whenToUse: when a Go project needs `make version`/`make version-check`, when you add or review its `internal/version/version.go` or `tools/version/read-version.sh`, or when a Go build must carry a version other than the recorded one
updated: 20261010
tags:
  - stack/go
  - concern/ci
  - versioning
adr:
  - adr/version-source-file.md
---

# Goal
- `internal/version/version.go` declaring `var Version = "{version}"` — the one place the version is recorded.
- `tools/version/read-version.sh`, an unchanged copy of this skill's asset.
- No `VERSION` file and no second copy of the number.
- A plain `go build` producing a binary that reports the recorded version.

# Core Principle
- This skill details [[skills/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]] for Go; apply both.
- **Recorded in the code that reports it** - `go.mod` has no version field, so the number lives in the variable the program prints; a build needs no flag to be right. Decision recorded in [[./adr/version-source-file.md|version-source-file]].
- `-ldflags` only overrides: a snapshot build replaces the recorded version, a release build passes nothing.

# Rule

## MUST

### Record the version in internal/version/version.go
Declare the version as `var Version = "MAJOR.MINOR.PATCH"` on one line of `internal/version/version.go`, package `version`.
- Violation: `const Version = "1.4.0"`, the variable in another package, or `Version = "dev"` with the number in a `VERSION` file.
- Risk: a constant cannot be overridden for a snapshot; another path is not found by `read-version.sh`; with `"dev"` in source every build that forgets the flag reports `dev`.
- Fix: one `var` line in `internal/version/version.go`; delete the `VERSION` file.

### Copy read-version.sh verbatim
Copy [[./assets/tools/version/read-version.sh|read-version.sh]] verbatim to `tools/version/read-version.sh`; do not modify it.
- Risk: a reader pointed at another file reports a number the binary does not carry.
- Fix: restore the file from the asset.

### Override the version only for a snapshot
Pass `-ldflags "-X {module-path}/internal/version.Version={version}"` only when a build must carry a version other than the recorded one, and build without it otherwise.
- Violation: a `Makefile` or `Dockerfile` that always injects a version read from somewhere else.
- Risk: a second mechanism has to agree with the source, and a wrong package path in the flag is ignored silently.
- Fix: `VERSION ?=` in the `Makefile` and `ARG VERSION=` in the `Dockerfile`, with the flag added only when the value is not empty.

# Check list
- [ ] `internal/version/version.go` has exactly one `var Version = "MAJOR.MINOR.PATCH"` line.
- [ ] No `VERSION` file exists; no other file holds the version number.
- [ ] `tools/version/read-version.sh` is byte-identical to this skill's asset.
- [ ] `make -s version` prints what a binary built by plain `go build` reports.
- [ ] A build with `-ldflags "-X …/internal/version.Version=9.9.9"` reports `9.9.9`.
