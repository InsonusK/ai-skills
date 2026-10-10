---
name: plateau-gw009-001--file-version-version
description: internal/version/version.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when creating or editing internal/version/version.go
domain: skill
type: template
plateau: plateau-gw009-001
version: 20261010000000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]]"
---

# Goal
Record the project's version in one place and make the running binary's version observable.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/version/version.go.create.md|internal/version/version.go]]

# Core Principles
- Apply ONE plateau template per file.
- The version is recorded here and nowhere else; `make version` reads the `var Version` line.
- `-ldflags -X` only overrides it for a snapshot build; there is no `VERSION` file.

# Implementation
```go
// Skill: file-version-version
// Plateau: plateau-gw009-001
// Version: 20261010000000

package version

var Version = "0.1.0"
```
Verified against this plateau's own `examples/internal/version/version.go`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/version/version.go.create.md|internal/version/version.go]]

# Check list
- [ ] `Version` holds the project's version; `make -s version` prints it; no `VERSION` file exists.
- [ ] `go build -ldflags "-X {module-path}/internal/version.Version=9.9.9" ...` overrides `Version`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-repository-structure.skill/solution-go-repository-structure.skill.md|solution-go-repository-structure]] - [[skills/go/architecture/solutions/solution-go-repository-structure.skill/Implementation/internal/version/version.go.create.md|internal/version/version.go]]
