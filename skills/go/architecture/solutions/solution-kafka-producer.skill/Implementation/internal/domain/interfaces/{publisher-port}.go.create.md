---
description: Shape-only outbound publish port
project_name: internal/domain/interfaces
name: "{publisher-port}"
element_kind: functions
change_kind: create
tags:
  - solution/kafka-producer
  - element/internal-domain-interfaces-publisher-port-go
---

> Draft contract — shape only.

# Goals
- Declare a narrow, business-named port for publishing one kind of event.

# Implementation changes
```go
package interfaces

import "context"

// {Port} is the outbound port for publishing {event} to other services.
type {Port} interface {
	Publish(ctx context.Context, event {Event}) error
}
```

# Check list
- [ ] A real application of this solution names the port and event for the actual thing published, not left as `{Port}`/`{Event}`.
