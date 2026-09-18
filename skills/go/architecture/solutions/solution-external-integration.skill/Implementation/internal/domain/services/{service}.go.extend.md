---
description: The domain service depends on the new outbound port
project_name: internal/domain/services
name: "{Service}"
element_kind: struct
change_kind: extend
tags:
  - solution/external-integration
  - element/internal-domain-services-service-go
---

# Naming convention
No change — extends the existing `{Service}` struct and `New{Service}` constructor.

# Implementation changes
```go
type {Service} struct {
	port interfaces.{Port} // added by this solution
}

func New{Service}(port interfaces.{Port}) *{Service} {
	return &{Service}{port: port}
}

func (s *{Service}) {Method}(ctx context.Context, input string) error {
	result, err := s.port.{Method}(ctx, input)
	if err != nil {
		return err
	}
	_ = result
	return nil
}
```

This catalog's own runnable examples extend `LinkCheckService.Check` to also call `ReputationChecker.CheckReputation` and fold its verdict into the returned `Result` — see `plateau-integrated-service`'s `example/`.

# Rule changes

## MUST
- Add the new port as a constructor parameter — never construct the adapter (or reach for a concrete infrastructure type) inside the domain service itself.
  - Risk: constructing a concrete adapter inside the domain service reintroduces the exact coupling the port exists to prevent, and makes the service untestable without the real external service reachable.
  - Fix: accept `interfaces.{Port}` in `New{Service}`; `main.go` passes the concrete adapter that satisfies it.
- If another solution already extended `{Service}`'s constructor with its own port, add this port as an additional parameter to the same constructor — never a second `New{Service}`-like function.
  - Risk: two constructors for one type invite half the callers to use the older one and silently miss whichever port it doesn't take.
  - Fix: every port-adding solution extends the same `New{Service}` signature, appending its own parameter.

# Check list
- [ ] `{Service}` takes every port it depends on through `New{Service}`, with no default/zero-value fallback that would let it run without one.

# Unittest TestCases
- [ ] WHEN the port returns its unavailable sentinel error THEN `{Method}` returns that error wrapped, unchanged in kind
