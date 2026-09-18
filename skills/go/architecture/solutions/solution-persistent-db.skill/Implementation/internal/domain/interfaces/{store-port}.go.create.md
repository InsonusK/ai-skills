---
description: The outbound port for durable storage
project_name: internal/domain/interfaces
name: "{store-port}"
element_kind: functions
change_kind: create
tags:
  - solution/persistent-db
  - element/internal-domain-interfaces-store-port-go
---

# Goals
- Declare a narrow, business-named port for durably storing and reloading one kind of record — never a generic `Repository[T]`.

# Naming convention
| use case | identifier name pattern | identifier name | file name pattern | file name |
| -------- | ------------------------ | ---------------- | ------------------- | --------- |
| the port | `{Concept}History`/`{Concept}Store` | `LinkHistory` | `{concept}.go` | `link_history.go` |

# Implementation changes
```go
package interfaces

import (
	"context"
	"time"
)

// {Record} is one durably-stored {concept} record.
type {Record} struct {
	Input     string
	CheckedAt time.Time
}

// {Port} is the outbound port for durable {concept} storage.
type {Port} interface {
	Record(ctx context.Context, rec {Record}) error
	Recent(ctx context.Context, limit int) ([]{Record}, error)
}
```

This catalog's own runnable examples concretize this as `LinkHistory` recording/listing `HistoryEntry{URL, Normalized, CheckedAt}` — see `plateau-persistent-service`'s `example/`.

# Rule changes

## MUST
- Name the port and its record type for the business concept stored, never generically (`Repository`, `Record[T]`).
  - Risk: a generic repository port invites every future persisted concept to share one interface shaped around the first concept's needs, coupling unrelated data through a shared abstraction.
  - Fix: one narrow port per durably-stored concept, matching [[skills/go/architecture/solutions/solution-cached-db.skill/solution-cached-db.skill.md|solution-cached-db]]'s own ADR reasoning.

# Check list
- [ ] The port has no method beyond what the domain service in this solution's own `{service}.go.extend.md` actually calls.

# Unittest TestCases
- [ ] WHEN a fake implementation of this interface is given to the domain service THEN it compiles without importing anything from `internal/infrastructure`
