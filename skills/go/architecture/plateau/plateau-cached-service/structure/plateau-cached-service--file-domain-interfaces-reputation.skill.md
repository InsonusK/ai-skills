---
name: plateau-cached-service--file-domain-interfaces-reputation
description: internal/domain/interfaces/reputation.go of the plateau-cached-service plateau
whenToUse: when creating or editing internal/domain/interfaces/reputation.go, or reviewing the ReputationChecker port's shape
domain: skill
type: template
plateau: plateau-cached-service
version: 20260917030000
tags:
  - skill/template/file
  - plateau/plateau-cached-service
created_by:
  - "[[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]"
---

# Goal
Declare what the domain needs from the external reputation service, named for that need.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/interfaces/{port}.go.create.md|reputation.go]]

# Core Principles
- Apply ONE plateau template per file.
- One sentinel error (`ErrUnavailable`) for the one failure mode the domain branches on.

# Implementation
```go
// Skill: file-domain-interfaces-reputation
// Plateau: plateau-cached-service
// Version: 20260917030000

package interfaces

import (
	"context"
	"errors"
)

// ErrUnavailable is returned when the reputation service cannot be reached.
var ErrUnavailable = errors.New("interfaces: reputation service unavailable")

// Reputation is the external service's verdict on one URL.
type Reputation struct {
	Flagged bool
	Reason  string
}

// ReputationChecker is the outbound port to the external reputation service.
type ReputationChecker interface {
	CheckReputation(ctx context.Context, url string) (Reputation, error)
}
```
Verified against this plateau's own `example/internal/domain/interfaces/reputation.go` — `go build`/`go vet` clean, exercised by 4 godog scenarios (flagged, clean, validation-fails-before-reputation-call, unavailable).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/interfaces/{port}.go.create.md|reputation.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Name the interface and its methods for the domain's need, never for the external service's own product name or its transport.
- Declare at most one sentinel error per failure mode the domain actually has different behavior for.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/interfaces/{port}.go.create.md#MUST|reputation.go]]

# Check list
- [ ] The interface has no method beyond what `LinkCheckService.Check` actually calls.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]] - [[skills/go/architecture/solutions/solution-external-integration.skill/Implementation/internal/domain/interfaces/{port}.go.create.md|reputation.go]]
