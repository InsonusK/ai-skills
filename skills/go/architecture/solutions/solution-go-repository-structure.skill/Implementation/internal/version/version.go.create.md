---
description: Build-time version variable, set via -ldflags at build time
project_name: internal/version
name: version
element_kind: functions
change_kind: create
tags:
  - solution/go-repository-structure
  - element/internal-version-version-go
---

# Goals
- Make the running binary's version observable (logs, a health/status endpoint, etc.) without hand-editing a file per release.

# Core Principles
- The version is a build-time constant, never computed or read from a file at runtime.

# Implementation changes
```go
// Package version holds the build-time version string.
package version

// Version is overridden at build time via:
//   -ldflags "-X {module-path}/internal/version.Version=$(VERSION)"
var Version = "dev"
```

# Check list
- [ ] `go build -ldflags "-X {module-path}/internal/version.Version=1.2.3" ...` overrides `Version` at build time.
