---
name: version-source-file
description: Where a Go project records the one version that CI checks, tags, and the program reports
problem: Python, TypeScript, and .NET record the version in a manifest their build reads, but go.mod has no version field. Where does a Go project record its version so that a pull request can raise it, `make version` can print it, and the binary reports it?
decision: The version is the string of `var Version` in `internal/version/version.go`; there is no `VERSION` file, and `-ldflags -X` is used only to override it for a snapshot build.
tags:
  - stack/go
  - concern/ci
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The version check needs one comparable version per commit, recorded in the tree so that a pull request into `master` can raise it. `go.mod` names the module path and the Go language version, never the module's own release version. The first answer was a root `VERSION` file with `Version = "dev"` in Go source, filled at build time through `-ldflags`. That is one number but three places that must agree — the file, the variable, and the flag with the variable's full path in the `Makefile` and the `Dockerfile` — and a build that misses the flag reports `dev` without failing. Where should the version be recorded?

# Selected variant
[[#Variable in version.go]]
- Decided by the owner on 2026-10-10, replacing the root `VERSION` file selected before.

# Searched variants

## Variable in version.go

**Selected.**

### Description
`internal/version/version.go` declares `var Version = "1.4.0"`. `read-version.sh` prints that string. A snapshot build overrides it with `-ldflags "-X {module-path}/internal/version.Version=…"`; a release build passes no flag.

### Benefits
- One place: the number is in the variable the program reports.
- `go build`, `go install`, and a run from an editor all report the right version with no flag.
- A pull request raises the version by changing one reviewed line.

### Costs
- `read-version.sh` reads a line of Go source, so the declaration must keep its form and its path.
- The version is not readable by a tool that knows nothing of the project's layout.

## Root VERSION file

### Description
A plain-text `VERSION` file at the repository root; Go source holds `Version = "dev"` and every build injects the file's content through `-ldflags`.

### Benefits
- Trivial to read from any tooling.
- The same shape as the other stacks' manifests: one file, one field.

### Costs
- Every build path must carry the flag; one that does not reports `dev` and does not fail.
- Two files and a flag to keep in agreement; a test comparing the file with the code appears just to guard that.

## Git tag as the only version record

### Description
Never store the version in the tree; the latest `vX.Y.Z` tag is the version, read at build time or through `runtime/debug.ReadBuildInfo`.

### Benefits
- The most common convention in the Go ecosystem, and the only version the module proxy knows.
- Nothing to keep in agreement.

### Costs
- Nothing in the tree changes with the version, so a pull request cannot be required to raise it.
- A build from a checkout without tags — a shallow clone, a Docker context — has no version.
