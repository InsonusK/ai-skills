---
name: devops-project-version-in-go
description: Go implementation of devops-project-version — the version recorded in a root VERSION file, the read-version.sh that prints it, and the -ldflags injection that puts it into the binary
whenToUse: when a Go project needs `make version`/`make version-check`, when you add or review its `VERSION` file or `tools/version/read-version.sh`, or when a Go build must stamp the version into a binary or an image
updated: 20261010
tags:
  - stack/go
  - concern/ci
  - versioning
adr:
  - adr/version-source-file.md
---

# Goal
- A root `VERSION` file holding the project's version.
- `tools/version/read-version.sh`, an unchanged copy of this skill's asset.
- Every `go build` of a released binary injecting the output of `make version` through `-ldflags`.

# Core Principle
- This skill details [[skills/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]] for Go; apply both.
- `go.mod` has no version field, so the version lives in `VERSION`. Decision recorded in [[./adr/version-source-file.md|version-source-file]].

# Rule

## MUST

### Record the version in the root VERSION file
Keep the version as the only content of a `VERSION` file at the root of the module.
- Violation: a `const Version = "1.4.0"` in Go source, or the version taken from the latest git tag.
- Risk: `make version` finds nothing to read, and a pull request has no file whose change shows the bump.
- Fix: `VERSION` with one line, `1.4.0`.

### Copy read-version.sh verbatim
Copy [[./assets/tools/version/read-version.sh|read-version.sh]] verbatim to `tools/version/read-version.sh`; do not modify it.
- Risk: a hand-written reader keeps the trailing newline or a `v` prefix, and `make version` refuses the value.
- Fix: restore the file from the asset.

### Inject the version at build time
Build every released binary with `-ldflags "-X {module-path}/internal/version.Version=$(make -s version)"`, in the `Makefile` and — through `ARG VERSION` — in the `Dockerfile`.
- Violation: a version number written into `internal/version/version.go`.
- Risk: the binary reports a version that differs from its tag.
- Fix: leave the variable at `"dev"` in source and set it only through `-ldflags`.

# Check list
- [ ] `VERSION` exists at the module root and holds only `MAJOR.MINOR.PATCH`.
- [ ] `tools/version/read-version.sh` is byte-identical to this skill's asset.
- [ ] `make -s version` prints the content of `VERSION`.
- [ ] No Go source file holds the version number; released builds pass it through `-ldflags`.
