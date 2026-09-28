---
description: A data port whose write can trigger follow-up work takes the tasks next to the data
project_name: internal/domain/interfaces
name: "{port}"
element_kind: functions
change_kind: extend
tags:
  - solution/taskbox-in-go
  - element/internal-domain-interfaces-port-go
---

# Goals
- Let the domain hand follow-up tasks to the port that writes the data change, so both are stored in one transaction.

# Core Principles
- Only a write method that can trigger follow-ups grows the variadic `tasks ...Task`; read methods and writes that never trigger work stay as they are.
- The rationale and the rejected unit-of-work alternative: [[skills/common-workflow/architecture/solutions/solution-taskbox.skill/adr/enqueue-through-the-data-port.md|solution-taskbox's ADR]].

# Implementation changes
Applies to the port `solution-persistent-db` created (`{Port}` is its business name, e.g. `LinkHistory`).

### AS IS
```go
type {Port} interface {
	{Write}(ctx context.Context, entry {Entry}) error
	// ... read methods ...
}
```

### TO BE
```go
type {Port} interface {
	// {Write} stores entry and enqueues tasks in one transaction.
	{Write}(ctx context.Context, entry {Entry}, tasks ...Task) error
	// ... read methods unchanged ...
}
```

# Rule changes

## MUST

### Keep Task out of adapter types
Declare the port with `interfaces.Task` only — never a `taskbox` or `pgx` type.
- Risk: the domain starts depending on the TaskBox mechanism or the database driver, and its tests need them.
- Fix: the adapter converts `interfaces.Task` to `taskbox.NewTask` inside its transaction.

# Check list
- [ ] Every write method that can trigger follow-up work takes `tasks ...Task`; every stub implementing the port records them.
