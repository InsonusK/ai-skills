---
description: The Redis-backed cache adapter package
name: "internal/infrastructure/{cache}"
element_kind: package
change_kind: create
tags:
  - solution/cached-db
  - element/internal-infrastructure-cache
---

# Goals
- Implement the domain's cache port using Redis.

# Core Principles
- Translates `redis.Nil` (the go-redis "key does not exist" sentinel) into the port's `ok=false` return — never lets a `github.com/redis/go-redis/v9` type or error escape this package.

# Structure

## Repository place
```
internal/
  infrastructure/
    {cache}/
```

## Package Structure
```
internal/infrastructure/{cache}/
  store.go
```

## Directory and file skills
| Directory\|file | Description | Pattern skill |
| --------------- | ----------- | -------------- |
| store.go | `Store` struct implementing the domain's cache port | [[./store.go.create.md]] |

# Go Dependencies
| Module | Version constraint | Purpose |
| ------ | ------------------- | ------- |
| github.com/redis/go-redis/v9 | >= 9.22 | Redis client |

# What Does NOT Belong Here
- Business logic — belongs to `internal/domain/services`.
- Any concept this module durably owns as a system of record — belongs to [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]'s adapter instead.

# Allowed Dependencies
- `internal/domain/interfaces` (the port this package implements)
- `github.com/redis/go-redis/v9`

# Rules

## MUST
- Never let `redis.Nil` or any other `go-redis` error type cross this package's boundary.
  - Risk: a caller catching `redis.Nil` directly couples domain-adjacent code to this one adapter's library choice.
  - Fix: translate `redis.Nil` to the port's `ok=false`; translate every other Redis error to a real `err`.

# Check list
- [ ] No `redis.Nil` or `*redis.Client` type appears outside this package.
