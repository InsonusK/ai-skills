---
name: plateau-gw009-001--file-domain-interfaces-task
description: internal/domain/interfaces/task.go of the plateau-gw009-001 (GW009.001) plateau
whenToUse: when the domain needs to ask for deferred follow-up work together with a data change
domain: skill
type: template
plateau: plateau-gw009-001
version: 20260928120000
tags:
  - skill/template/file
  - plateau/plateau-gw009-001
created_by:
  - "[[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]]"
source: internal/domain/interfaces/task.go
---

# Goal
Declare `Task`, the plain value a data port takes to enqueue follow-up work.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/domain/interfaces/task.go.create.md|task.go]]

# Core Principles
- Apply ONE plateau template per file.
- Plain data: type, JSON-encodable payload, group, `RunAt` — no TaskBox or store type.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/domain/interfaces/task.go.create.md|task.go]]

# Implementation
```go
// Skill: file-domain-interfaces-task
// Plateau: plateau-gw009-001
// Version: 20260928120000

package interfaces

import "time"

// Task is deferred work the domain asks for together with a data change: a
// data port that takes tasks writes them in the same transaction as the
// change, so both commit or neither does. The domain never sees how or where
// the task is stored.
type Task struct {
	Type    string    // stable task type name, e.g. "recheck-flagged-link"
	Payload any       // JSON-encoded by the adapter; its shape is owned by the task type
	Group   string    // tasks with the same Group run one at a time, in order; "" = unordered
	RunAt   time.Time // not before this time; zero = as soon as possible
}
```

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/domain/interfaces/task.go.create.md|task.go]]

# Rules
MUST:
- Never apply several plateau templates per file.
- Never add a store- or TaskBox-specific field to `Task`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/domain/interfaces/task.go.create.md|task.go]]

# Check list
- [ ] `task.go` imports only `time`.

__Applied solutions:__
- [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/solution-taskbox-in-go.skill.md|solution-taskbox-in-go]] - [[skills/go/architecture/solutions/solution-taskbox-in-go.skill/Implementation/internal/domain/interfaces/task.go.create.md|task.go]]
