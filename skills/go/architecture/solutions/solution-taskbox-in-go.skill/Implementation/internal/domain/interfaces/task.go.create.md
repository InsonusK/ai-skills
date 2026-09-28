---
description: The domain value a data port takes to enqueue follow-up work with a data change
project_name: internal/domain/interfaces
name: task
element_kind: functions
change_kind: create
verbatim_of: internal/domain/interfaces/task.go
tags:
  - solution/taskbox-in-go
  - element/internal-domain-interfaces-task-go
---

# Goals
- Let the domain ask for a follow-up task without knowing TaskBox, transactions, or stores.

# Core Principles
- `Task` is plain data; the adapter that owns the transaction turns it into a TaskBox task.

# Implementation changes
Create `internal/domain/interfaces/task.go` exactly as below (`{module-path}` = the service's Go module path, `{store}` = the package owning the service's migrations). The code is proven by the conformance feature on PostgreSQL in this catalog's plateau built with this solution.

```go
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
