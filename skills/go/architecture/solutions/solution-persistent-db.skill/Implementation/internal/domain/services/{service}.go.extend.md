---
description: The domain service depends on the new persistence port
project_name: internal/domain/services
name: "{Service}"
element_kind: struct
change_kind: extend
tags:
  - solution/persistent-db
  - element/internal-domain-services-service-go
---

# Implementation changes
```go
type {Service} struct {
	history interfaces.{StorePort} // added by this solution
}

func New{Service}(history interfaces.{StorePort}) *{Service} {
	return &{Service}{history: history}
}

func (s *{Service}) {Method}(ctx context.Context, input string) error {
	// ... produce {result} the normal way (and via cache/external port, if applied) ...

	return s.history.Record(ctx, interfaces.{Record}{Input: input, CheckedAt: time.Now()})
}

// {ReadMethod} exposes the durably-stored records back out of the domain
// service — the read half of the port this solution adds. Without this
// method, the port's own Recent capability is unreachable from any adapter.
func (s *{Service}) {ReadMethod}(ctx context.Context, limit int) ([]interfaces.{Record}, error) {
	return s.history.Recent(ctx, limit)
}
```

This catalog's own runnable examples record every `Check` call in `LinkHistory` and expose the read side as `RecentChecks`, which [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/http/server.go.extend.md|the HTTP adapter]] and (when `solution-grpc-api` is applied) [[skills/go/architecture/solutions/solution-persistent-db.skill/Implementation/internal/api/grpc/server.go.extend.md|the gRPC adapter]] both surface — see `plateau-persistent-service`'s `example/`.

# Rule changes

## MUST
- Add the persistence port as an additional `New{Service}` constructor parameter, appended to whatever an earlier-applied solution already added (see [[skills/go/architecture/solutions/solution-cached-db.skill/Implementation/internal/domain/services/{service}.go.extend.md#MUST|solution-cached-db's own equivalent rule]]) — never a second constructor function.
- A `Record` failure must fail the request (unlike a cache-write failure) — the caller asked for a durable record; silently dropping it is a correctness bug, not a degraded optimization.
  - Risk: swallowing a `Record` error the same way a cache `Set` error is swallowed silently loses data the caller believes was saved.
  - Fix: return the `Record` error from `{Method}` unchanged (wrapped), never ignored.
- Add a `{ReadMethod}` on `{Service}` that calls the port's `Recent`, and extend every applied inbound adapter (see this solution's own `internal/api/http/server.go.extend.md` and `internal/api/grpc/server.go.extend.md`) to expose it.
  - Risk: a port with an unreachable read side means the data is durably stored but no caller can ever retrieve it back out — the solution's own stated Capability ("data ... survives a restart") is unverifiable from outside the process.
  - Fix: add the read method here in the same change that adds `history`, and wire an endpoint/RPC for it in every applied adapter.

# Check list
- [ ] A `Record` failure is returned to the caller, never silently ignored.
- [ ] `{Service}` has a `{ReadMethod}` calling the port's `Recent`, and every applied adapter exposes it.

# Unittest TestCases
- [ ] WHEN `Record` fails THEN `{Method}` returns that error
- [ ] WHEN `{ReadMethod}` is called THEN it returns exactly what the port's `Recent` returned
