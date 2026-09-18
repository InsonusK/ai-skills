---
name: plateau-dual-api-service--file-domain-services-linkcheck
description: internal/domain/services/linkcheck.go of the plateau-dual-api-service plateau
whenToUse: when creating or editing internal/domain/services/linkcheck.go, or reviewing the LinkCheckService's validation rules
domain: skill
type: template
plateau: plateau-dual-api-service
version: 20260917010000
tags:
  - skill/template/file
  - plateau/plateau-dual-api-service
created_by:
  - "[[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]]"
---

# Goal
Validate and normalize a URL — this plateau's concrete realization of `solution-go-domain-logic`'s illustrative `{Service}`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]

# Core Principles
- Apply ONE plateau template per file.
- Zero dependencies at this plateau — no port exists yet.

# Implementation
```go
// Skill: file-domain-services-linkcheck
// Plateau: plateau-dual-api-service
// Version: 20260917010000

package services

import (
	"context"
	"errors"
	"net/url"
	"strings"
)

var ErrInvalidURL = errors.New("services: invalid url")

type Result struct {
	URL        string
	Normalized string
}

type LinkCheckService struct{}

func NewLinkCheckService() *LinkCheckService {
	return &LinkCheckService{}
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
	return Result{URL: trimmed, Normalized: normalized}, nil
}
```
Verified against this plateau's own `example/internal/domain/services/linkcheck.go` — 5 godog scenarios green (`internal/domain/services/features/check.feature`), `go vet` clean, `gremlins` run (3 killed / 4 survived / 20 not-covered — the HTTP-adapter path isn't scenario-covered at this plateau, expected).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Every exported method's first parameter is `ctx context.Context`.
- Never let this struct hold a logger, metrics client, or other cross-cutting dependency directly.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md#MUST|{service}.go]]

# Check list
- [ ] `LinkCheckService` has zero fields at this plateau.
- [ ] `Check` lowercases scheme and host but leaves the path unchanged.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]

# Unittest TestCases
- [ ] WHEN given `"https://Example.com/Path"` THEN the check succeeds and normalizes to `"https://example.com/Path"`
- [ ] WHEN given a URL with a non-http(s) scheme (e.g. `"ftp://example.com/file"`) THEN `ErrInvalidURL` is returned
- [ ] WHEN given an empty or unparsable string THEN `ErrInvalidURL` is returned

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-go-domain-logic.skill/solution-go-domain-logic.skill.md|solution-go-domain-logic]] - [[skills/go/architecture/solutions/solution-go-domain-logic.skill/Implementation/internal/domain/services/{service}.go.create.md|{service}.go]]
