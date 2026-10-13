---
description: Add LogLevel to Config
project_name: internal/config
name: config
element_kind: functions
change_kind: extend
tags:
  - solution/go-app-logging
  - element/internal-config-config-go
---

# Implementation changes
```go
type Config struct {
	LogLevel string
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		LogLevel: l.get("LOG_LEVEL", "info"),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}
```

# Check list
- [ ] `LOG_LEVEL` defaults to `"info"` when unset.
