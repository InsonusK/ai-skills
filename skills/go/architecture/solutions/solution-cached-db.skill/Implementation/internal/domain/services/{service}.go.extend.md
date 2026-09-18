---
description: The domain service depends on the new cache port
project_name: internal/domain/services
name: "{Service}"
element_kind: struct
change_kind: extend
tags:
  - solution/cached-db
  - element/internal-domain-services-service-go
---

# Implementation changes
```go
type {Service} struct {
	cache interfaces.{CachePort} // added by this solution
}

func New{Service}(cache interfaces.{CachePort}) *{Service} {
	return &{Service}{cache: cache}
}

func (s *{Service}) {Method}(ctx context.Context, input string) error {
	if cached, ok, err := s.cache.Get(ctx, input); err == nil && ok {
		_ = cached
		return nil
	}

	// ... compute the result the normal way ...

	_ = s.cache.Set(ctx, input, {result})
	return nil
}
```

If [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] is also applied on the same plateau, this cache sits in front of *that* solution's port call specifically — check the cache before calling the external service, populate it after — not in front of every operation the domain service performs.

This catalog's own runnable examples cache `ReputationChecker.CheckReputation`'s result by URL in `LinkCheckService.Check` — see `plateau-cached-service`'s `example/`.

# Rule changes

## MUST
- Add the cache port as an additional `New{Service}` constructor parameter, appended to whatever parameters an earlier-applied solution already added — never a second constructor function.
  - Risk: a second `New{Service}`-shaped function invites half the call sites to use the one that is missing this port.
  - Fix: every port-adding solution extends the same constructor signature.
- Treat a `Get` error (not a miss) as "proceed without the cache," never as a request failure.
  - Risk: failing the whole request because the cache store is unreachable turns an optimization into a hard dependency, defeating the point of caching.
  - Fix: on `err != nil` from `Get`, fall through to computing the value normally, the same as a miss — a `Set` failure afterward is likewise best-effort and should not fail the request either.

# Check list
- [ ] A cache-store failure degrades to "compute without caching," never to a failed request.

# Unittest TestCases
- [ ] WHEN the cache has a hit THEN the underlying computation/external call is not made
- [ ] WHEN the cache returns an error (not a miss) THEN the method still succeeds by computing the value directly
