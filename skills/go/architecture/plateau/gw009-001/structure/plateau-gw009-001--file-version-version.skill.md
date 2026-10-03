---
name: plateau-gw009-001--file-version-version
description: internal/version/version.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when creating or editing internal/version/version.go
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
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

# Implementation
```go
// Skill: file-version-version
// Plateau: plateau-gw009-001
// Version: 20260928120000

package version

var Version = "dev"
```
Verified against this plateau's own `example/internal/version/version.go`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/version/version.go.create.md|internal/version/version.go]]

# Check list
- [ ] `go build -ldflags "-X {module-path}/internal/version.Version=1.2.3" ...` overrides `Version`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/version/version.go.create.md|internal/version/version.go]]
