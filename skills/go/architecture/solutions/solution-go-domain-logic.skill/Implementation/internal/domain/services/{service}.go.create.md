---
description: One illustrative domain service, zero dependencies at this solution's baseline
project_name: internal/domain/services
name: "{Service}"
element_kind: struct
change_kind: create
tags:
  - solution/go-domain-logic
  - element/internal-domain-services-service-go
---

# Goals
- Demonstrate the shape every domain service in this catalog follows: a struct holding only its ports, a constructor, and methods with no transport or infrastructure type in their signatures.

# Core Principles
- At this solution's baseline the service has zero fields and zero dependencies — later solutions extend both the struct and the constructor as they add ports.

# Naming convention
| use case | struct name pattern | struct name | file name pattern | file name |
| -------- | -------------------- | ------------ | ------------------- | --------- |
| a domain service | `{Concept}Service` | `LinkCheckService` | `{concept}.go` | `linkcheck.go` |

# Implementation changes
```go
// Package services holds the module's business logic, independent of the
// transport that triggers it or the infrastructure it may rely on.
package services

import "context"

// {Service} holds no dependencies at this solution's baseline — later
// solutions extend this struct and New{Service} as they add ports.
type {Service} struct{}

func New{Service}() *{Service} {
	return &{Service}{}
}

// {Method} is a placeholder for this service's actual business logic.
func (s *{Service}) {Method}(ctx context.Context) error {
	return nil
}
```

This catalog's own runnable examples concretize `{Service}` as `LinkCheckService`, with `{Method}` becoming `Check(ctx, rawURL) (Result, error)` — validate and normalize a URL, returning `Result{URL, Normalized string}` — see `plateau-http-service`'s `example/`.

# Rule changes

## MUST
- Every exported method's first parameter must be `ctx context.Context`, even when unused at this solution's baseline.
  - Risk: adding cancellation or tracing later forces every call site's signature — and every adapter that calls it — to change at once.
  - Fix: accept `ctx context.Context` from the first method onward, even before anything inside the method uses it.
- Never let this struct hold a logger, a metrics client, or any other cross-cutting dependency directly.
  - Risk: threading cross-cutting concerns through every domain service's constructor couples business logic to observability choices that should be swappable independently.
  - Fix: keep the struct to business-relevant ports only; a cross-cutting concern is a Plateau Component attached at the composition root, not a constructor argument here.

# Check list
- [ ] `{Service}` has zero fields until a later solution adds a port.
- [ ] `New{Service}` takes zero arguments until a later solution adds a port.

# Unittest TestCases
- [ ] WHEN `{Method}` is called with valid input THEN it returns the expected result with no error
