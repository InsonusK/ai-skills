---
description: Add GRPCListenPort to Config
project_name: internal/config
name: config
element_kind: functions
change_kind: extend
tags:
  - solution/grpc-api
  - element/internal-config-config-go
---

# Implementation changes
```go
type Config struct {
	GRPCListenPort string
}

func Load() (*Config, error) {
	l := &loader{}
	cfg := &Config{
		GRPCListenPort: l.get("GRPC_LISTEN_PORT", "50051"),
	}
	if l.err != nil {
		return nil, l.err
	}
	return cfg, nil
}
```

# Check list
- [ ] `GRPC_LISTEN_PORT` defaults to `50051` when unset.
