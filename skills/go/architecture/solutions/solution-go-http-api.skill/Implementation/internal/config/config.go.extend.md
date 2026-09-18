---
description: Add HTTPListenPort to Config
project_name: internal/config
name: config
element_kind: functions
change_kind: extend
tags:
  - solution/go-http-api
  - element/internal-config-config-go
---

# Implementation changes
```go
type Config struct {
	HTTPListenPort string
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		HTTPListenPort: l.get("HTTP_LISTEN_PORT", "8080"),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}
```

# Check list
- [ ] `HTTP_LISTEN_PORT` defaults to `8080` when unset.
