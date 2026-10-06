---
name: plateau-dual-api-service--file-version-version
description: internal/version/version.go of the plateau-dual-api-service plateau
whenToUse: when creating or editing internal/version/version.go
domain: skill
type: template
plateau: plateau-dual-api-service
version: 20260917010000
tags:
  - skill/template/file
  - plateau/plateau-dual-api-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
---

# Goal
Make the running binary's version observable, set at build time via `-ldflags`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/version/version.go.create.md|internal/version/version.go]]

# Core Principles
- Apply ONE plateau template per file.
- The version is a build-time constant, never computed at runtime.
- The value comes only from the root `VERSION` file via `-ldflags -X`; never write a version number in Go source.

# Implementation
```go
// Skill: file-version-version
// Plateau: plateau-dual-api-service
// Version: 20260917010000

package version

var Version = "dev"
```
Verified against this plateau's own `example/internal/version/version.go`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/version/version.go.create.md|internal/version/version.go]]

# Check list
- [ ] `go build -ldflags "-X {module-path}/internal/version.Version=1.2.3" ...` overrides `Version`.
- [ ] `Version` is `"dev"` in source; no Go file holds a version number.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/version/version.go.create.md|internal/version/version.go]]
