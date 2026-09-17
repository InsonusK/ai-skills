---
description: The outbound port for a cache-backed lookup
project_name: internal/domain/interfaces
name: "{cache-port}"
element_kind: functions
change_kind: create
tags:
  - solution/cached-db
  - element/internal-domain-interfaces-cache-port-go
---

# Goals
- Declare a narrow, business-named port for reading and writing one cached concept — never a generic cache abstraction (see this solution's own ADR).

# Core Principles
- A miss is expressed as `ok = false`, never an error — a cache miss is an expected outcome, not a failure.

# Naming convention
| use case | identifier name pattern | identifier name | file name pattern | file name |
| -------- | ------------------------ | ---------------- | ------------------- | --------- |
| the port | `{Concept}Cache` | `ReputationCache` | `{concept}.go` | `reputation_cache.go` |

# Implementation changes
```go
package interfaces

import "context"

// {Port} is the outbound port for a cache-backed {concept} lookup. A miss
// is signaled by ok=false, never an error.
type {Port} interface {
	Get(ctx context.Context, key string) (value {Value}, ok bool, err error)
	Set(ctx context.Context, key string, value {Value}) error
}
```

This catalog's own runnable examples concretize this as:
```go
// ReputationCache caches CheckReputation results by URL.
type ReputationCache interface {
	Get(ctx context.Context, url string) (rep Reputation, ok bool, err error)
	Set(ctx context.Context, url string, rep Reputation) error
}
```
— see `plateau-cached-service`'s `example/`.

# Rule changes

## MUST
- `Get` must return `(value, ok=false, err=nil)` on a miss — never a sentinel error for "not found."
  - Violation: `Get` returning `interfaces.ErrNotFound` on a cache miss.
  - Risk: a caller then has to `errors.Is`-branch on something that is not really a failure, and every future cache-backed port has to remember the same special case.
  - Fix: use the three-value return `(value, ok, err)`; reserve `err` for a real adapter failure (e.g. the store unreachable).

# Check list
- [ ] `Get`'s signature returns `(value, ok bool, err error)`, never collapsing the miss case into `err`.

# Unittest TestCases
- [ ] WHEN a fake implementation of this interface is given to the domain service THEN it compiles without importing anything from `internal/infrastructure`
