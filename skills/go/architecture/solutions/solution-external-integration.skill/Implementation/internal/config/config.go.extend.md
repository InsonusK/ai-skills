---
description: Add the external service's address to Config
project_name: internal/config
name: config
element_kind: functions
change_kind: extend
tags:
  - solution/external-integration
  - element/internal-config-config-go
---

# Implementation changes
```go
type Config struct {
	{External}Addr string
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		{External}Addr: l.require("{EXTERNAL}_ADDR"),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}
```

# Check list
- [ ] `{EXTERNAL}_ADDR` is required — `Load` fails fast when it is unset, rather than the module starting and only failing on the first real call.
