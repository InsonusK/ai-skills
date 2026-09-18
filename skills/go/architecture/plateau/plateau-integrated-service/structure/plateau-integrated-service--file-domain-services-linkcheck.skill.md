---
name: plateau-integrated-service--file-domain-services-linkcheck
description: internal/domain/services/linkcheck.go of the plateau-integrated-service plateau
whenToUse: when creating or editing internal/domain/services/linkcheck.go, or reviewing the LinkCheckService's validation/reputation rules
domain: skill
type: template
plateau: plateau-integrated-service
version: 20260917020000
tags:
  - skill/template/file
  - plateau/plateau-integrated-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]]"
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
registry:
  - "[[skills/go/architecture/registry/internal-domain-services-service-go.md|internal-domain-services-service-go]]"
---

# Goal
Validate and normalize a URL, then ask the external reputation service whether it is flagged.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Core Principles
- Apply ONE plateau template per file.
- Validation happens before the reputation call — an invalid URL never reaches the external service.
- The reputation port's method is called with the *normalized* URL, not the raw input.

# Implementation
```go
// Skill: file-domain-services-linkcheck
// Plateau: plateau-integrated-service
// Version: 20260917020000

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
}

func NewLinkCheckService(reputation interfaces.ReputationChecker) *LinkCheckService {
	return &LinkCheckService{reputation: reputation}
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

	rep, err := s.reputation.CheckReputation(ctx, normalized)
	if err != nil {
		return Result{}, err
	}

	return Result{URL: trimmed, Normalized: normalized, Flagged: rep.Flagged, Reason: rep.Reason}, nil
}
```
Verified against this plateau's own `example/internal/domain/services/linkcheck.go` — 9 godog scenarios green (5 from `plateau-http-service` unchanged + 4 new: flagged, clean, validation-fails-before-reputation-call, reputation-unavailable).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Every exported method's first parameter is `ctx context.Context`.
- Add a new port as a constructor parameter — never construct the adapter inside the domain service itself.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md#MUST|{service}.go]]
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md#MUST|{service}.go]]

# Check list
- [ ] An invalid URL never reaches `s.reputation.CheckReputation`.
- [ ] `New{Service}` takes every port the service depends on, with no zero-value fallback.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]

# Unittest TestCases
- [ ] WHEN given a well-formed URL and the reputation service reports it flagged THEN the result carries `Flagged: true` and the reason
- [ ] WHEN given a well-formed URL and the reputation service does not flag it THEN the result carries `Flagged: false`
- [ ] WHEN given a malformed URL THEN the reputation service is never called
- [ ] WHEN the reputation service is unavailable THEN `Check` returns that error, unchanged in kind

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/services/{service}.go.extend.md|{service}.go]]
