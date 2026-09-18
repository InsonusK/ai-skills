---
name: plateau-cached-service--package-infrastructure-reputationcache
description: internal/infrastructure/reputationcache package of the plateau-cached-service plateau
whenToUse: when editing the reputation-cache adapter, or deciding whether new caching code belongs in internal/infrastructure/reputationcache
domain: skill
type: template
plateau: plateau-cached-service
version: 20260917030000
tags:
  - skill/template/package
  - plateau/plateau-cached-service
created_by:
  - "[[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]"
---

# Goal
Implement the domain's `ReputationCache` port using Redis.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/infrastructure/{cache}/Package.create.md|internal/infrastructure/reputationcache]]

# Core Principles
Translates `redis.Nil` into the port's `ok=false` return — never lets a `go-redis` type escape this package.

# Structure
## Repository place
```
internal/
  infrastructure/
    reputationcache/
```
## Package Structure
```
internal/infrastructure/reputationcache/
  store.go      ← Store, see plateau-cached-service--file-infrastructure-reputationcache-store.skill.md
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| store.go | `Store` implementing `ReputationCache` via `github.com/redis/go-redis/v9` | [[plateau-cached-service--file-infrastructure-reputationcache-store.skill.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/redis/go-redis/v9 | >= 9.22 | Redis client |

# Allowed Dependencies
- `internal/domain/interfaces`
- `github.com/redis/go-redis/v9`

# Rules
MUST:
- Never let `redis.Nil` or any other `go-redis` error type cross this package's boundary.
- Never import a sibling `internal/infrastructure/*` package from here.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/infrastructure/{cache}/Package.create.md#MUST|internal/infrastructure/reputationcache]]

# Check list
- [ ] No `redis.Nil` or `*redis.Client` type appears outside this package.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/infrastructure/{cache}/Package.create.md|internal/infrastructure/reputationcache]]
