---
name: plateau-cached-service--file-domain-interfaces-reputation-cache
description: internal/domain/interfaces/reputation_cache.go of the plateau-cached-service plateau
whenToUse: when creating or editing internal/domain/interfaces/reputation_cache.go, or reviewing the ReputationCache port's shape
domain: skill
type: template
plateau: plateau-cached-service
version: 20260917030000
tags:
  - skill/template/file
  - plateau/plateau-cached-service
created_by:
  - "[[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]"
---

# Goal
Declare a narrow, business-named port for caching reputation lookups — never a generic cache interface.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/interfaces/{cache-port}.go.create.md|reputation_cache.go]]

# Core Principles
- Apply ONE plateau template per file.
- A miss is expressed as `ok = false`, never an error.

# Implementation
```go
// Skill: file-domain-interfaces-reputation-cache
// Plateau: plateau-cached-service
// Version: 20260917030000

package interfaces

import "context"

// ReputationCache caches CheckReputation results by URL. A miss is
// signaled by ok=false, never an error.
type ReputationCache interface {
	Get(ctx context.Context, url string) (rep Reputation, ok bool, err error)
	Set(ctx context.Context, url string, rep Reputation) error
}
```
Verified against this plateau's own `example/internal/domain/interfaces/reputation_cache.go` — `go build`/`go vet` clean; exercised by 2 new godog scenarios (cache hit skips the external call, a fresh lookup is written to the cache) plus verified against a real Redis instance in the runtime smoke test.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/interfaces/{cache-port}.go.create.md|reputation_cache.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- `Get` returns `(value, ok=false, err=nil)` on a miss — never a sentinel error for "not found."

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/interfaces/{cache-port}.go.create.md#MUST|reputation_cache.go]]

# Check list
- [ ] `Get`'s signature returns `(value, ok bool, err error)`, never collapsing the miss case into `err`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/interfaces/{cache-port}.go.create.md|reputation_cache.go]]
