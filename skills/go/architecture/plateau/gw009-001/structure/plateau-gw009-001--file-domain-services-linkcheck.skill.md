---
name: plateau-gw009-001--file-domain-services-linkcheck
description: internal/domain/services/linkcheck.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when editing LinkCheckService — Check, Recheck, RecentChecks — or deciding when a checked link needs a follow-up re-check
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
  - "[[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]"
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: internal/domain/services/linkcheck.go
---

# Goal
Validate, normalize, and reputation-check URLs; record every check; schedule a delayed re-check of a flagged URL in the same write; re-check on request of its task.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Core Principles
- Apply ONE plateau template per file.
- Validation happens before the cache, the reputation call, or the history record.
- A cache-store failure (not a miss) degrades to computing the value normally — never fails the request.
- A history-record failure **does** fail the request (unlike the cache) — the caller asked for a durable record; silently dropping it would be a correctness bug, not a degraded optimization.
- `history.Record` runs strictly after the reputation/cache result is known — it is a pure append to `Check`'s existing body, never a wrap of the cache-aside logic (`reputationWithCache` stays untouched by this solution; see [[skills/go/architecture/registry/internal-domain-services-service-go.md|the registry entry]] for why this stays canonical `FMN` rather than the borderline case `{external-integration, cached-db}` needed).
- A flagged verdict passes a `recheck-flagged-link` task (group = the normalized URL, `RunAt` = now + `recheckAfter`) with the history write — the decision is the domain's, the atomicity the adapter's.
- `Recheck` bypasses the cache (the verdict may have changed), refreshes it, and records the fresh verdict without scheduling another re-check.
- The task type name and payload (`TaskRecheckFlaggedLink`, `RecheckFlaggedLink`) are declared here — the domain owns the task type.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Implementation
```go
// Skill: file-domain-services-linkcheck
// Plateau: plateau-gw009-001
// Version: 20260928120000

// Package services holds the module's business logic, independent of the
// transport that triggers it or the infrastructure it may later rely on.
package services

import (
	"context"
	"errors"
	"net/url"
	"strings"
	"time"

	"{module-path}/internal/domain/interfaces"
)

// ErrInvalidURL is returned when Check is given a URL that is not
// well-formed or not http(s).
var ErrInvalidURL = errors.New("services: invalid url")

// Result is the outcome of checking one URL.
type Result struct {
	URL        string
	Normalized string
	Flagged    bool
	Reason     string
}

// TaskRecheckFlaggedLink is the task type that asks the reputation service
// again about a URL it flagged; its payload is RecheckFlaggedLink.
const TaskRecheckFlaggedLink = "recheck-flagged-link"

// RecheckFlaggedLink is the payload of TaskRecheckFlaggedLink. Add fields
// only as optional: stored tasks keep the shape they were enqueued with.
type RecheckFlaggedLink struct {
	URL string `json:"url"`
}

// LinkCheckService validates, normalizes, and reputation-checks URLs.
type LinkCheckService struct {
	reputation   interfaces.ReputationChecker
	cache        interfaces.ReputationCache
	history      interfaces.LinkHistory
	recheckAfter time.Duration
}

// NewLinkCheckService builds the service; a URL the reputation service flags
// is checked again recheckAfter later.
func NewLinkCheckService(reputation interfaces.ReputationChecker, cache interfaces.ReputationCache, history interfaces.LinkHistory, recheckAfter time.Duration) *LinkCheckService {
	return &LinkCheckService{reputation: reputation, cache: cache, history: history, recheckAfter: recheckAfter}
}

// Check validates rawURL, normalizes it (lowercase scheme and host, path
// unchanged; only http and https schemes are accepted), then asks the
// external reputation service whether it is flagged - checking the cache
// first, and populating it after a real lookup.
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
	var followUps []interfaces.Task
	if rep.Flagged {
		followUps = append(followUps, interfaces.Task{
			Type:    TaskRecheckFlaggedLink,
			Payload: RecheckFlaggedLink{URL: normalized},
			Group:   normalized,
			RunAt:   checkedAt.Add(s.recheckAfter),
		})
	}
	if err := s.history.Record(ctx, interfaces.LinkHistoryEntry{
		Normalized: normalized,
		Flagged:    rep.Flagged,
		Reason:     rep.Reason,
		CheckedAt:  checkedAt,
	}, followUps...); err != nil {
		return Result{}, err
	}

	return Result{URL: trimmed, Normalized: normalized, Flagged: rep.Flagged, Reason: rep.Reason}, nil
}

// Recheck asks the reputation service again about an already-normalized URL
// that was flagged, bypassing the cache, refreshes the cache, and records the
// fresh verdict. It returns ErrInvalidURL for a URL that is not normalized
// http(s), and the reputation port's error (ErrUnavailable) unchanged.
func (s *LinkCheckService) Recheck(ctx context.Context, normalized string) (Result, error) {
	u, err := url.Parse(normalized)
	if err != nil || u.Host == "" || (u.Scheme != "http" && u.Scheme != "https") {
		return Result{}, ErrInvalidURL
	}

	rep, err := s.reputation.CheckReputation(ctx, normalized)
	if err != nil {
		return Result{}, err
	}
	_ = s.cache.Set(ctx, normalized, rep)

	if err := s.history.Record(ctx, interfaces.LinkHistoryEntry{
		Normalized: normalized,
		Flagged:    rep.Flagged,
		Reason:     rep.Reason,
		CheckedAt:  time.Now().UTC(),
	}); err != nil {
		return Result{}, err
	}
	return Result{URL: normalized, Normalized: normalized, Flagged: rep.Flagged, Reason: rep.Reason}, nil
}

// RecentChecks returns the most recently recorded checks, most recent first.
func (s *LinkCheckService) RecentChecks(ctx context.Context, limit int) ([]interfaces.LinkHistoryEntry, error) {
	return s.history.Recent(ctx, limit)
}

// reputationWithCache checks the cache first; a cache-store failure (not a
// miss) degrades to computing the value normally, never fails the request.
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

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Never apply several plateau templates per file.
- Every exported method's first parameter is `ctx context.Context`.
- Add a new port as an additional constructor parameter, appended to whatever an earlier-applied solution already added — never a second constructor function.
- A cache-store failure degrades to "compute without caching," never fails the request.
- A `history.Record` failure fails `Check` — returned unchanged, never swallowed.
- `RecentChecks` calls `history.Recent` directly — no extra business logic hides between the port and this method.
- Never decide in an adapter whether a re-check is needed.
- Add fields to `RecheckFlaggedLink` only as optional — stored tasks keep their shape.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Check list
- [ ] An invalid URL never reaches the cache, the reputation service, or the history store.
- [ ] `New{Service}` takes every port the service depends on, with no zero-value fallback.
- [ ] A cache read/write error never fails a request that would otherwise have succeeded.
- [ ] A history-record error always fails the request.
- [ ] `RecentChecks` exists and delegates to the history port's `Recent`.
- [ ] A scenario asserts the task passed with a flagged check, and one asserts none for a clean check.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Unittest TestCases
- [ ] WHEN a URL is flagged THEN `Record` receives one `recheck-flagged-link` task, group = normalized URL, run after = `recheckAfter`
- [ ] WHEN `Recheck` meets an unavailable reputation service THEN it returns `ErrUnavailable` and records nothing
- [ ] WHEN `Recheck` is given a non-http(s) URL THEN it returns `ErrInvalidURL` without calling the reputation service

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
