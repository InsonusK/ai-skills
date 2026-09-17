---
description: The domain service depends on the new persistence port
project_name: internal/domain/services
name: "{Service}"
element_kind: struct
change_kind: extend
tags:
  - solution/persistent-db
  - element/internal-domain-services-service-go
---

# Implementation changes
```go
type {Service} struct {
	history interfaces.{StorePort} // added by this solution
}

func New{Service}(history interfaces.{StorePort}) *{Service} {
	return &{Service}{history: history}
}

func (s *{Service}) {Method}(ctx context.Context, input string) error {
	// ... produce {result} the normal way (and via cache/external port, if applied) ...

	return s.history.Record(ctx, interfaces.{Record}{Input: input, CheckedAt: time.Now()})
}
```

This catalog's own runnable examples record every `Check` call in `LinkHistory` and add a read method (e.g. `RecentChecks`) the HTTP/gRPC adapters expose — see `plateau-persistent-service`'s `example/`.

# Rule changes

## MUST
- Add the persistence port as an additional `New{Service}` constructor parameter, appended to whatever an earlier-applied solution already added (see [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md#MUST|solution-cached-db's own equivalent rule]]) — never a second constructor function.
- A `Record` failure must fail the request (unlike a cache-write failure) — the caller asked for a durable record; silently dropping it is a correctness bug, not a degraded optimization.
  - Risk: swallowing a `Record` error the same way a cache `Set` error is swallowed silently loses data the caller believes was saved.
  - Fix: return the `Record` error from `{Method}` unchanged (wrapped), never ignored.

# Check list
- [ ] A `Record` failure is returned to the caller, never silently ignored.

# Unittest TestCases
- [ ] WHEN `Record` fails THEN `{Method}` returns that error
