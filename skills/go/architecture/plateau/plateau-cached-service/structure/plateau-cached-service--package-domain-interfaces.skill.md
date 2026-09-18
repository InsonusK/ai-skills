---
name: plateau-cached-service--package-domain-interfaces
description: internal/domain/interfaces package of the plateau-cached-service plateau
whenToUse: when adding a new outbound port, or creating or editing a file in internal/domain/interfaces
domain: skill
type: template
plateau: plateau-cached-service
version: 20260917030000
tags:
  - skill/template/package
  - plateau/plateau-cached-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
  - "[[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]"
---

# Goal
Give the domain a package to declare what it needs from the outside world, without knowing which infrastructure provides it. Created empty by `solution-go-domain-ports`; `solution-external-integration` was the first solution to add a port to it, `solution-cached-db` the second.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]] - [[skills/go/architecture/solutions/solution-go-domain-ports.skill/Implementation/internal/domain/interfaces/Package.create.md|internal/domain/interfaces]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/interfaces/{port}.go.create.md|reputation.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/interfaces/{cache-port}.go.create.md|reputation_cache.go]]

# Core Principles
Declares interfaces (and the value types/sentinel errors they use) only — never an implementation.

# Structure
## Repository place
```
internal/
  domain/
    interfaces/
```
## Package Structure
```
internal/domain/interfaces/
  reputation.go        ← ReputationChecker port, see plateau-cached-service--file-domain-interfaces-reputation.skill.md
  reputation_cache.go  ← ReputationCache port, see plateau-cached-service--file-domain-interfaces-reputation-cache.skill.md
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| reputation.go | `ReputationChecker` port + `Reputation` value type + `ErrUnavailable` | [[skills/go/architecture/plateau/plateau-cached-service/structure/plateau-cached-service--file-domain-interfaces-reputation.skill.md]] |
| reputation_cache.go | `ReputationCache` port | [[skills/go/architecture/plateau/plateau-cached-service/structure/plateau-cached-service--file-domain-interfaces-reputation-cache.skill.md]] |

# What Does NOT Belong Here
- Any concrete type from an infrastructure library — belongs to the adapter package that implements the port.
- Business logic — belongs to `internal/domain/services`.

# Allowed Dependencies
- Standard library only (`context`, `errors`).

# Rules
MUST:
- Every interface added to this package must be named for the domain's need, never for the technology that will implement it.
- Never let this package import anything under `internal/infrastructure` or `internal/api`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]] - [[skills/go/architecture/solutions/solution-go-domain-ports.skill/Implementation/internal/domain/interfaces/Package.create.md#MUST|internal/domain/interfaces]]

# Check list
- [ ] No import outside the standard library.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]] - [[skills/go/architecture/solutions/solution-go-domain-ports.skill/Implementation/internal/domain/interfaces/Package.create.md|internal/domain/interfaces]]
