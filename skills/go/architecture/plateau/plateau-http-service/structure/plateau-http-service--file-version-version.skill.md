---
name: plateau-http-service--file-version-version
description: internal/version/version.go of the plateau-http-service plateau
whenToUse: when creating or editing internal/version/version.go
domain: skill
type: template
plateau: plateau-http-service
version: 20261010000000
tags:
  - skill/template/file
  - plateau/plateau-http-service
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
// Plateau: plateau-http-service
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
