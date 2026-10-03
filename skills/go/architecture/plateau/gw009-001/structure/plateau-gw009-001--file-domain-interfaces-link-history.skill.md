---
name: plateau-gw009-001--file-domain-interfaces-link-history
description: internal/domain/interfaces/link_history.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when editing the LinkHistory port or adding a follow-up task to a history write
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]]"
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: internal/domain/interfaces/link_history.go
---

# Goal
Declare the durable link-history port; its write takes the follow-up tasks to store in the same transaction.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/interfaces/{store-port}.go.create.md|{store-port}.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Core Principles
- Apply ONE plateau template per file.
- `Record(ctx, entry, tasks ...Task)` — the domain hands follow-ups to the port that owns the transaction.
- Business-named, never a generic repository.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/interfaces/{store-port}.go.create.md|{store-port}.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Implementation
```go
// Skill: file-domain-interfaces-link-history
// Plateau: plateau-gw009-001
// Version: 20260928120000

package interfaces

import (
	"context"
	"time"
)

// LinkHistoryEntry is one durably-stored record of a checked URL.
type LinkHistoryEntry struct {
	Normalized string
	Flagged    bool
	Reason     string
	CheckedAt  time.Time
}

// LinkHistory is the outbound port for durable link-check storage.
type LinkHistory interface {
	// Record stores entry and enqueues tasks in one transaction.
	Record(ctx context.Context, entry LinkHistoryEntry, tasks ...Task) error
	Recent(ctx context.Context, limit int) ([]LinkHistoryEntry, error)
}
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/interfaces/{store-port}.go.create.md|{store-port}.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Never use a `taskbox` or `pgx` type in this port — only `interfaces.Task`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/interfaces/{store-port}.go.create.md|{store-port}.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]

# Check list
- [ ] Every stub of `LinkHistory` records the tasks passed with `Record`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-persistent-db.skill/solution-persistent-db.skill.md|solution-persistent-db]] - [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/domain/interfaces/{store-port}.go.create.md|{store-port}.go]]
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]
