---
description: Add the database DSN to Config
project_name: internal/config
name: config
element_kind: functions
change_kind: extend
tags:
  - solution/persistent-db
  - element/internal-config-config-go
---

# Implementation changes
```go
type Config struct {
	DatabaseDSN string
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		DatabaseDSN: l.require("DATABASE_DSN"),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}
```

# Check list
- [ ] `DATABASE_DSN` is required — `Load` fails fast when it is unset.
