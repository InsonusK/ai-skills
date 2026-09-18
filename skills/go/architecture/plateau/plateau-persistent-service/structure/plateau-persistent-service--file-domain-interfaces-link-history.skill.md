---
name: plateau-persistent-service--file-domain-interfaces-link-history
description: internal/domain/interfaces/link_history.go of the plateau-persistent-service plateau
whenToUse: when creating or editing internal/domain/interfaces/link_history.go, or reviewing the LinkHistory port's shape
domain: skill
type: template
plateau: plateau-persistent-service
version: 20260917040000
tags:
  - skill/template/file
  - plateau/plateau-persistent-service
created_by:
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
---

# Goal
Declare a narrow, business-named port for durably storing and reloading recorded link checks — never a generic `Repository[T]`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/interfaces/{store-port}.go.create.md|link_history.go]]

# Core Principles
- Apply ONE plateau template per file.
- Named for the business record it stores (`LinkHistory`/`LinkHistoryEntry`), never generically.
- The port has no method beyond what `LinkCheckService` actually calls — a write (`Record`) and a read (`Recent`), nothing more.

# Implementation
```go
// Skill: file-domain-interfaces-link-history
// Plateau: plateau-persistent-service
// Version: 20260917040000

package interfaces

import (
	"context"
	"time"
)

// LinkHistoryEntry is one durably-stored record of a completed check.
type LinkHistoryEntry struct {
	Normalized string
	Flagged    bool
	Reason     string
	CheckedAt  time.Time
}

// LinkHistory is the outbound port for durable link-check history.
type LinkHistory interface {
	Record(ctx context.Context, entry LinkHistoryEntry) error
	Recent(ctx context.Context, limit int) ([]LinkHistoryEntry, error)
}
```
Verified against this plateau's own `example/internal/domain/interfaces/link_history.go` — `go build`/`go vet` clean; exercised by 2 new godog scenarios (a successful check is recorded, a history failure fails the check) plus verified against a real PostgreSQL instance in the runtime smoke test, including across a process restart.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/interfaces/{store-port}.go.create.md|link_history.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Name the port and its record type for the business concept stored, never generically (`Repository`, `Record[T]`).

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/interfaces/{store-port}.go.create.md#MUST|link_history.go]]

# Check list
- [ ] `LinkHistory` has exactly two methods: `Record` and `Recent`.
- [ ] Neither `LinkHistory` nor `LinkHistoryEntry` names a storage technology.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/interfaces/{store-port}.go.create.md|link_history.go]]

# Unittest TestCases
- [ ] WHEN a fake implementation of this interface is given to `LinkCheckService` THEN it compiles without importing anything from `internal/infrastructure`

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/interfaces/{store-port}.go.create.md|link_history.go]]
