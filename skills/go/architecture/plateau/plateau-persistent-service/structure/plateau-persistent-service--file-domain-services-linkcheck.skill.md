---
name: plateau-persistent-service--file-domain-services-linkcheck
description: internal/domain/services/linkcheck.go of the plateau-persistent-service plateau
whenToUse: when creating or editing internal/domain/services/linkcheck.go, or reviewing the LinkCheckService's validation/reputation/caching/history rules
domain: skill
type: template
plateau: plateau-persistent-service
version: 20260917040000
tags:
  - skill/template/file
  - plateau/plateau-persistent-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
  - "[[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]"
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
registry:
  - "[[../registry/internal-domain-services-service-go.md|internal-domain-services-service-go]]"
---

# Goal
Validate and normalize a URL, check the reputation cache first, call the external reputation service on a miss, durably record every check in `LinkHistory`, and expose the recorded history back out via `RecentChecks`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Core Principles
- Apply ONE plateau template per file.
- Validation happens before the cache, the reputation call, or the history record.
- A cache-store failure (not a miss) degrades to computing the value normally — never fails the request.
- A history-record failure **does** fail the request (unlike the cache) — the caller asked for a durable record; silently dropping it would be a correctness bug, not a degraded optimization.
- `history.Record` runs strictly after the reputation/cache result is known — it is a pure append to `Check`'s existing body, never a wrap of the cache-aside logic (`reputationWithCache` stays untouched by this solution; see [[../registry/internal-domain-services-service-go.md|the registry entry]] for why this stays canonical `FMN` rather than the borderline case `{external-integration, cached-db}` needed).

# Implementation
```go
// Skill: file-domain-services-linkcheck
// Plateau: plateau-persistent-service
// Version: 20260917040000

package services

import (
	"context"
	"errors"
	"net/url"
	"strings"
	"time"

	"{module-path}/internal/domain/interfaces"
)

var ErrInvalidURL = errors.New("services: invalid url")

type Result struct {
	URL        string
	Normalized string
	Flagged    bool
	Reason     string
}

type LinkCheckService struct {
	reputation interfaces.ReputationChecker
	cache      interfaces.ReputationCache
	history    interfaces.LinkHistory
}

func NewLinkCheckService(reputation interfaces.ReputationChecker, cache interfaces.ReputationCache, history interfaces.LinkHistory) *LinkCheckService {
	return &LinkCheckService{reputation: reputation, cache: cache, history: history}
}

func (s *LinkCheckService) Check(ctx context.Context, rawURL string) (Result, error) {
	trimmed := strings.TrimSpace(rawURL)
	u, err := url.Parse(trimmed)
	if err != nil || u.Host == "" {
		return Result{}, ErrInvalidURL
	}
	scheme := strings.ToLower(u.Scheme)
	if scheme != "http" && scheme != "https" {
		return Result{}, ErrInvalidURL
	}
	normalized := scheme + "://" + strings.ToLower(u.Host) + u.Path

	rep, err := s.reputationWithCache(ctx, normalized)
	if err != nil {
		return Result{}, err
	}

	checkedAt := time.Now().UTC()
	if err := s.history.Record(ctx, interfaces.LinkHistoryEntry{
		Normalized: normalized,
		Flagged:    rep.Flagged,
		Reason:     rep.Reason,
		CheckedAt:  checkedAt,
	}); err != nil {
		return Result{}, err
	}

	return Result{URL: trimmed, Normalized: normalized, Flagged: rep.Flagged, Reason: rep.Reason}, nil
}

// RecentChecks returns the most recently recorded checks, most recent first.
func (s *LinkCheckService) RecentChecks(ctx context.Context, limit int) ([]interfaces.LinkHistoryEntry, error) {
	return s.history.Recent(ctx, limit)
}

func (s *LinkCheckService) reputationWithCache(ctx context.Context, normalized string) (interfaces.Reputation, error) {
	if cached, ok, err := s.cache.Get(ctx, normalized); err == nil && ok {
		return cached, nil
	}

	rep, err := s.reputation.CheckReputation(ctx, normalized)
	if err != nil {
		return interfaces.Reputation{}, err
	}

	_ = s.cache.Set(ctx, normalized, rep)
	return rep, nil
}
```
Verified against this plateau's own `example/internal/domain/services/linkcheck.go` — 13 godog scenarios green (11 unchanged + 2 new: a successful check is recorded, a history-record failure fails the check), using in-memory stub `ReputationChecker`/`ReputationCache`/`LinkHistory` in the unit-test suite. Also verified against a **real PostgreSQL instance**: two checks recorded via HTTP, read back identically via HTTP, gRPC, and a direct `psql` query, and confirmed to survive a full process restart.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Every exported method's first parameter is `ctx context.Context`.
- Add a new port as an additional constructor parameter, appended to whatever an earlier-applied solution already added — never a second constructor function.
- A cache-store failure degrades to "compute without caching," never fails the request.
- A `history.Record` failure fails `Check` — returned unchanged, never swallowed.
- `RecentChecks` calls `history.Recent` directly — no extra business logic hides between the port and this method.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md#MUST|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md#MUST|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md#MUST|{service}.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md#MUST|{service}.go]]

# Check list
- [ ] An invalid URL never reaches the cache, the reputation service, or the history store.
- [ ] `New{Service}` takes every port the service depends on, with no zero-value fallback.
- [ ] A cache read/write error never fails a request that would otherwise have succeeded.
- [ ] A history-record error always fails the request.
- [ ] `RecentChecks` exists and delegates to the history port's `Recent`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Unittest TestCases
- [ ] WHEN given a well-formed URL and the reputation service reports it flagged THEN the result carries `Flagged: true` and the reason
- [ ] WHEN given a well-formed URL and the reputation service does not flag it THEN the result carries `Flagged: false`
- [ ] WHEN given a malformed URL THEN neither the cache, the reputation service, nor the history store is consulted
- [ ] WHEN the reputation service is unavailable THEN `Check` returns that error, unchanged in kind
- [ ] WHEN the cache already has an entry for the URL THEN the reputation service is never called
- [ ] WHEN the cache misses THEN the freshly-computed reputation is written to the cache
- [ ] WHEN a check succeeds THEN exactly one entry is recorded in history
- [ ] WHEN the history store fails to record THEN `Check` returns that error and the result is discarded

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
