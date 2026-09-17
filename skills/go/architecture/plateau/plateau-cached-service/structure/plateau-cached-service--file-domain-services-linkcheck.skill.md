---
name: plateau-cached-service--file-domain-services-linkcheck
description: internal/domain/services/linkcheck.go of the plateau-cached-service plateau
whenToUse: when creating or editing internal/domain/services/linkcheck.go, or reviewing the LinkCheckService's validation/reputation/caching rules
domain: skill
type: template
plateau: plateau-cached-service
version: 20260917030000
tags:
  - skill/template/file
  - plateau/plateau-cached-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
  - "[[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]"
registry:
  - "[[../registry/internal-domain-services-service-go.md|internal-domain-services-service-go]]"
---

# Goal
Validate and normalize a URL, check the reputation cache first, and only call the external reputation service on a miss — populating the cache afterward.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Core Principles
- Apply ONE plateau template per file.
- Validation happens before either the cache or the reputation call.
- A cache-store failure (not a miss) degrades to computing the value normally — never fails the request.

# Implementation
```go
// Skill: file-domain-services-linkcheck
// Plateau: plateau-cached-service
// Version: 20260917030000

package services

import (
	"context"
	"errors"
	"net/url"
	"strings"

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
}

func NewLinkCheckService(reputation interfaces.ReputationChecker, cache interfaces.ReputationCache) *LinkCheckService {
	return &LinkCheckService{reputation: reputation, cache: cache}
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

	return Result{URL: trimmed, Normalized: normalized, Flagged: rep.Flagged, Reason: rep.Reason}, nil
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
Verified against this plateau's own `example/internal/domain/services/linkcheck.go` — 11 godog scenarios green (9 unchanged + 2 new: cache hit skips the external call, a fresh lookup is written to the cache), using in-memory stub `ReputationChecker`/`ReputationCache` in the unit-test suite. Also verified against a **real Redis instance**: 3 HTTP requests for the same URL produced exactly one call to a throwaway fake reputation server; a subsequent gRPC request for the same URL hit the cache too (same domain-service instance shared across transports).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Every exported method's first parameter is `ctx context.Context`.
- Add a new port as an additional constructor parameter, appended to whatever an earlier-applied solution already added — never a second constructor function.
- A cache-store failure degrades to "compute without caching," never fails the request.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md#MUST|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md#MUST|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md#MUST|{service}.go]]

# Check list
- [ ] An invalid URL never reaches either the cache or the reputation service.
- [ ] `New{Service}` takes every port the service depends on, with no zero-value fallback.
- [ ] A cache read/write error never fails a request that would otherwise have succeeded.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Unittest TestCases
- [ ] WHEN given a well-formed URL and the reputation service reports it flagged THEN the result carries `Flagged: true` and the reason
- [ ] WHEN given a well-formed URL and the reputation service does not flag it THEN the result carries `Flagged: false`
- [ ] WHEN given a malformed URL THEN neither the cache nor the reputation service is consulted
- [ ] WHEN the reputation service is unavailable THEN `Check` returns that error, unchanged in kind
- [ ] WHEN the cache already has an entry for the URL THEN the reputation service is never called
- [ ] WHEN the cache misses THEN the freshly-computed reputation is written to the cache

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]] - [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
