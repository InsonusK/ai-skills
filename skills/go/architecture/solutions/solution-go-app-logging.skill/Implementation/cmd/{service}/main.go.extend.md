---
description: Call logging.Init first in run(), before any adapter is constructed
project_name: "cmd/{service}"
name: main
element_kind: functions
change_kind: extend
tags:
  - solution/go-app-logging
  - element/cmd-service-main-go
---

# Implementation changes
```go
import (
	// ...
	"{module-path}/internal/logging"
)

func run() error {
	cfg, err := config.Load()
	if err != nil {
		return err
	}

	logging.Init(cfg.LogLevel)

	// ... every later constructor call follows, and may now log.
}
```

# Rule changes

## MUST
- `logging.Init(cfg.LogLevel)` must be the first call in `run()` after `config.Load()` returns successfully — before any adapter or client is constructed.
  - Risk: a constructor that runs before `Init` logs through `slog`'s zero-value default handler (unstructured, no level filtering), so its earliest — often most diagnostically important — logs look different from every later one.
  - Fix: call `logging.Init` immediately after the config-load error check, before any other line in `run()`.

# Check list
- [ ] `logging.Init` runs before the first adapter constructor in `run()`.
