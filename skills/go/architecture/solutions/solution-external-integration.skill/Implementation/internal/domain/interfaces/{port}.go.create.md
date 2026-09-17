---
description: The outbound port for calling the external service
project_name: internal/domain/interfaces
name: "{port}"
element_kind: functions
change_kind: create
tags:
  - solution/external-integration
  - element/internal-domain-interfaces-port-go
---

# Goals
- Declare what the domain needs from the external service, named for that need — never for the external service's own name or its transport.

# Core Principles
- One sentinel error per distinguishable failure mode the domain actually branches on (e.g. "unavailable" vs. any other failure) — not a 1:1 mirror of every gRPC status code.

# Naming convention
| use case | identifier name pattern | identifier name | file name pattern | file name |
| -------- | ------------------------ | ---------------- | ------------------- | --------- |
| the port | `{Concept}Checker`/`{Concept}Client` | `ReputationChecker` | `{concept}.go` | `reputation.go` |

# Implementation changes
```go
package interfaces

import (
	"context"
	"errors"
)

// ErrUnavailable is returned when {ExternalService} cannot be reached.
var ErrUnavailable = errors.New("interfaces: {external} unavailable")

// {Port} is the outbound port to {ExternalService}.
type {Port} interface {
	{Method}(ctx context.Context, input string) ({Result}, error)
}
```

This catalog's own runnable examples concretize this as:
```go
// ReputationChecker is the outbound port to the external reputation service.
type ReputationChecker interface {
	// CheckReputation reports whether a URL is flagged.
	CheckReputation(ctx context.Context, url string) (Reputation, error)
}

// Reputation is the external service's verdict on one URL.
type Reputation struct {
	Flagged bool
	Reason  string
}
```
— see `plateau-integrated-service`'s `example/`.

# Rule changes

## MUST
- Name the interface and its methods for the domain's need, never for the external service's own product name or its transport.
  - Violation: `type QuizAgentGrpcClient interface { ... }`.
  - Risk: a technology- or vendor-named port leaks an integration decision into the domain layer, so replacing the external service means renaming a domain-level type every caller already imports.
  - Fix: name it for what the domain needs (`ReputationChecker`), never for who provides it.
- Declare at most one sentinel error per failure mode the domain actually has different behavior for — never one sentinel per underlying transport status code.
  - Risk: a sentinel error per gRPC status code forces every adapter implementing this port (a future non-gRPC one included) to map onto codes that were never really distinguishable at the domain level.
  - Fix: ask "does the domain do something different for this failure?" before adding a new sentinel; if not, it falls under the generic wrapped error.

# Check list
- [ ] The interface has no method beyond what the domain service in this solution's own `{service}.go.extend.md` actually calls.
- [ ] Every declared sentinel error is used by the adapter's own translation logic.

# Unittest TestCases
- [ ] WHEN a fake implementation of this interface is given to the domain service THEN it compiles without importing anything from `internal/infrastructure`
