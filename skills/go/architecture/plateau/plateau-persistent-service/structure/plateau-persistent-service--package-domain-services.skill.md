---
name: plateau-persistent-service--package-domain-services
description: internal/domain/services package of the plateau-persistent-service plateau
whenToUse: when adding or editing a file in internal/domain/services, or deciding whether new business logic belongs here
domain: skill
type: template
plateau: plateau-persistent-service
version: 20260917040000
tags:
  - skill/template/package
  - plateau/plateau-persistent-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]]"
---

# Goal
Hold the module's business logic, independent of the transport that triggers it or infrastructure it may later rely on.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/Package.create.md|internal/domain/services]]

# Core Principles
No type from `internal/api/*` or any specific `internal/infrastructure/*` adapter is ever imported here.

# Structure
## Repository place
```
internal/
  domain/
    services/
```
## Package Structure
```
internal/domain/services/
  linkcheck.go          ← LinkCheckService, see plateau-persistent-service--file-domain-services-linkcheck.skill.md
  features/
    check.feature
  test/
    world_test.go
    runner_test.go
    check_steps_test.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| linkcheck.go | `LinkCheckService`: validates and normalizes a URL | [[skills/go/architecture/plateau/plateau-persistent-service/structure/plateau-persistent-service--file-domain-services-linkcheck.skill.md]] |

# What Does NOT Belong Here
- Request/response DTOs for any specific transport.
- A concrete infrastructure type.

# Allowed Dependencies
- Standard library only, at this plateau (no `internal/domain/interfaces` yet — nothing here needs a port).

# Rules
MUST:
- A domain service's constructor takes only ports as arguments, never a concrete infrastructure type.
- Never import a type from `internal/api/*` here.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/Package.create.md#MUST|internal/domain/services]]

# Check list
- [ ] No import from `internal/api/*` or `internal/infrastructure/*` anywhere in this package.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/Package.create.md|internal/domain/services]]
