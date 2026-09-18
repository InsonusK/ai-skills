---
name: solution-cached-db
description: A narrow, business-named outbound port backed by a cache-capable store (Redis), with no durability guarantee assumed by the port itself
whenToUse: when the module needs to avoid repeating an expensive lookup or external call, or reviewing whether domain code depends on a generic Cache interface instead of a business-named one
domain: skill
type: architecture
version: 20260917000000
tags:
  - skill/architecture/solution
  - solution/cached-db
  - stack/go
  - concern/architecture
creates:
  - "internal/domain/interfaces/{cache-port}.go"
  - "internal/infrastructure/{cache}/"
extends:
  - "internal/domain/services/{service}.go"
  - "cmd/{service}/main.go"
  - "internal/config/config.go"
depends_on:
  - "[[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]"
built_on_plateau:
adr:
  - "[[adr/business-named-port-not-generic-cache.md|A business-named port, not a generic Cache interface]]"
---

# Goal
- Let the domain avoid repeating an expensive computation, lookup, or external call by depending on a narrow, business-named outbound port — demonstrated backed by Redis.

# Capabilities
- Fewer redundant external calls or recomputations for the same input.
- The caching technology (Redis here) stays fully swappable — an in-memory LRU, Memcached, or anything else satisfying the same narrow port needs no change anywhere outside `internal/infrastructure/{cache}`.

# Core Principles
- The port is named for the business data it holds (e.g. `ReputationCache`), never generically (`Cache`) — see the ADR.
- A cache miss is a normal, expected outcome the port's method signature expresses directly (an `ok bool` or a "not found" sentinel), never an error.
- Nothing about the port implies a durability guarantee — a caller that needs data to survive a restart depends on [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]'s port instead, or in addition.

# Adr
- [[adr/business-named-port-not-generic-cache.md|A business-named port, not a generic Cache interface]]
  - Selected variant: a narrow, business-purpose-named port (`ReputationCache`), matching the reference implementation's own `ChatStore` shape

# Requirements
SOLUTION:
- [[skills/go/architecture/solutions/solution-go-domain-ports.skill/solution-go-domain-ports.skill.md|solution-go-domain-ports]]
  - [[skills/go/architecture/solutions/solution-go-domain-ports.skill/Implementation/internal/domain/interfaces/Package.create.md|internal/domain/interfaces]] - the package this solution adds its port file to
GO MODULES:
- github.com/redis/go-redis/v9
  - `redis.NewClient`, `Client.Get`/`Client.Set` — the cache-store adapter

# Template Skill Mutations
FILES:
- [[./Implementation/internal/domain/interfaces/{cache-port}.go.create.md|internal/domain/interfaces/{cache-port}.go]] - create - the outbound cache port
- [[./Implementation/internal/domain/services/{service}.go.extend.md|internal/domain/services/{service}.go]] - extend - the domain service depends on the new port
- [[./Implementation/internal/infrastructure/{cache}/Package.create.md|internal/infrastructure/{cache}]] - create - the Redis-backed adapter package
- [[./Implementation/internal/infrastructure/{cache}/store.go.create.md|store.go]] - create - `Store` struct implementing the port
- [[./Implementation/cmd/{service}/main.go.extend.md|cmd/{service}/main.go]] - extend - construct the Redis client and pass it to the domain service
- [[./Implementation/internal/config/config.go.extend.md|internal/config/config.go]] - extend - add Redis connection settings

# Workflow

## Cache hit
1. The domain service calls the port's `Get`.
2. The adapter finds the key in Redis and returns the decoded value with `ok = true`.
3. The domain service uses it without repeating whatever produced it originally.

## Cache miss
1. The port's `Get` returns `ok = false` (no error).
2. The domain service produces the value the normal way and calls the port's `Set` to populate the cache for next time.

# Rules

## MUST
- [[./Implementation/internal/domain/interfaces/{cache-port}.go.create.md#MUST|internal/domain/interfaces/{cache-port}.go]]
- [[./Implementation/internal/infrastructure/{cache}/store.go.create.md#MUST|store.go]]

# Check list
- [ ] The port's `Get` method signals a miss without returning an error.
- [ ] `internal/domain/services` never imports `github.com/redis/go-redis/v9` directly.
