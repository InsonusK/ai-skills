---
description: Construct the external-service client and pass it to the domain service
project_name: "cmd/{service}"
name: main
element_kind: functions
change_kind: extend
tags:
  - solution/external-integration
  - element/cmd-service-main-go
---

# Implementation changes
```go
import (
	// ...
	"{module-path}/internal/infrastructure/{adapter}"
)

func run() error {
	// ... config load, logging.Init unchanged ...

	client, err := {adapter}.Dial(cfg.{External}Addr)
	if err != nil {
		return err
	}
	defer func() { _ = client.Close() }()

	domainService := services.New{Service}(client) // client satisfies interfaces.{Port}

	// ... server construction unchanged, now using domainService ...
}
```

# Rule changes

## MUST
- Dial the external client before constructing the domain service, and pass it in — never construct the domain service first and set the port afterward.
  - Risk: a domain service with a settable-after-construction port can be started with the port still unset if a later line is reordered.
  - Fix: pass every port as a `New{Service}` constructor argument; never add a setter.
- Close the client on shutdown via `defer`.
  - Risk: an un-closed gRPC connection leaks a file descriptor and a background goroutine every time the process restarts under a process manager that doesn't fully reap children.
  - Fix: `defer func() { _ = client.Close() }()` immediately after a successful `Dial`.

# Check list
- [ ] `{adapter}.Dial` is called before `services.New{Service}`, and its result is passed directly into that call.
- [ ] `client.Close()` is deferred immediately after a successful `Dial`.
