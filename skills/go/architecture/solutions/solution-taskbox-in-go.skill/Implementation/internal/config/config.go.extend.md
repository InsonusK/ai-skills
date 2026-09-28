---
description: Worker settings for TaskBox, and the follow-up delay the domain schedules with
project_name: internal/config
name: config
element_kind: functions
change_kind: extend
tags:
  - solution/taskbox-in-go
  - element/internal-config-config-go
---

# Goals
- Make the worker pool's size, lease, and poll interval configurable per deployment (the contract makes lease and backoff per-service settings).

# Implementation changes
Adds four fields to `Config` and a `getDuration` helper to the loader.

```go
type Config struct {
	// ... existing fields ...
	TaskWorkers      int
	TaskLease        time.Duration
	TaskPollInterval time.Duration
	{FollowUpDelay}  time.Duration // e.g. RecheckAfter; only when a task type is delayed
}

// in Load():
		TaskWorkers:      l.getInt("TASKBOX_WORKERS", 2),
		TaskLease:        l.getDuration("TASKBOX_LEASE", 5*time.Minute),
		TaskPollInterval: l.getDuration("TASKBOX_POLL_INTERVAL", time.Second),
		{FollowUpDelay}:  l.getDuration("{FOLLOW_UP_DELAY}", time.Hour),

// getDuration is like get, but parses the value as a time.Duration ("90s").
func (l *loader) getDuration(key string, def time.Duration) time.Duration {
	v := l.get(key, "")
	if l.err != nil || v == "" {
		return def
	}
	d, err := time.ParseDuration(v)
	if err != nil {
		l.err = fmt.Errorf("%s: %w", key, err)
		return def
	}
	return d
}
```

# Check list
- [ ] `TASKBOX_LEASE` defaults to 5 minutes and exceeds the slowest handler.
