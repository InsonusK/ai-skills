---
description: Construct the Redis-backed cache store and pass it to the domain service
project_name: "cmd/{service}"
name: main
element_kind: functions
change_kind: extend
tags:
  - solution/cached-db
  - element/cmd-service-main-go
---

# Implementation changes
```go
import (
	// ...
	"net"

	"{module-path}/internal/infrastructure/{cache}"
)

func run() error {
	// ... config load, logging.Init unchanged ...

	cacheAddr := net.JoinHostPort(cfg.RedisHost, cfg.RedisPort)
	cache := {cache}.New(cacheAddr, cfg.RedisPassword, cfg.RedisDB)
	defer func() { _ = cache.Close() }()

	domainService := services.New{Service}(cache) // appends to any port(s) another applied solution already added
}
```

# Rule changes

## MUST
- Close the cache client on shutdown via `defer`, immediately after construction.
  - Risk: an un-closed Redis connection leaks a socket every process restart.
  - Fix: `defer func() { _ = cache.Close() }()` right after `{cache}.New(...)`.

# Check list
- [ ] `cache.Close()` is deferred immediately after construction.
