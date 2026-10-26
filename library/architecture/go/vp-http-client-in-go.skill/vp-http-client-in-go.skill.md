---
name: vp-http-client-in-go
description: The HTTP client variation point in Go — internal/infrastructure/httpclient copied from templates (the {API} settings, Client.Call with the timeout, JSON in and out, ErrUnavailable and StatusError, the call log, the request id, the mock against an emulator), one adapter per API in internal/infrastructure/{api} with its operations, its emulator, and its contract scenario, the service over it in internal/service, and the lines of main.go that build them
whenToUse: when a Go application built on the base plateau must call another service's HTTP API, when you call a further operation of such an API, or when you review a Go HTTP client
updated: 20261026
tags:
  - stack/go
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **The HTTP client variation point in Go** - A Go application that keeps every property of [vp-http-client](library/architecture/spec/vp-http-client.skill/vp-http-client.skill.md) and [api-client](library/develop/core/api-client.skill/api-client.skill.md); this skill details their implementation for Go and restates none of their rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-go](library/architecture/go/base-plateau-in-go.skill/base-plateau-in-go.skill.md)'s example.

# Core Principle
- **Copied once, written per operation** - `internal/infrastructure/httpclient` is copied from templates and never edited; per API the application writes an adapter package whose operations are calls of `httpclient.Client.Call`, the adapter's emulator, and the service over it.

```
docs/integration/client/{api-name}/openapi.json   the provider's contract, copied unchanged
internal/infrastructure/
├── httpclient/              copied from templates, never edited
│   ├── config.go            LoadConfig(prefix): {API}_HOST, _HTTP_PORT, _TLS, _TIMEOUT; Mock
│   ├── client.go            Client.Call: timeout, JSON, X-Request-Id, call log; ErrUnavailable, StatusError
│   └── features/  test/     the fixed code's scenarios, with a test API of their own
└── {api}/                   written per API (todoapi)
    ├── {api}.go             Prefix, Operations, Client with one method per operation and its own types
    ├── emulator.go          Emulator(): every operation answered with stable data (TODO_API_HOST=mock)
    └── features/  test/     each operation against an emulator of the scenario + the contract scenario
internal/service/{name}.go   the service over the adapter: technical outcomes → the application's kinds
```

# Rule

## MUST

### Copy the client code from templates
Fill and copy [config.go](./templates/internal/infrastructure/httpclient/config.go) and [client.go](./templates/internal/infrastructure/httpclient/client.go) to `internal/infrastructure/httpclient/`, with the example's `features/` and `test/`, replacing `{module-path}` with the module path from `go.mod`.
- Risk: a retyped client drifts in its timeout, its error report, or its log from every other Go client of the catalog.
- Fix: copy the files; after filling, searching them for `{module-path}` returns nothing; `make test` passes.

### One adapter package per API
Write each API as `internal/infrastructure/{api}/` holding `Prefix` (`"TODO_API"`), `Operations` (the method and path of every operation it calls), `New(logger)` reading the settings and `NewClient(cfg, logger)` taking them, and per operation one method of `Client` with an unexported request and response type and the mapping to the values the service uses.

```go
// PlanItems is POST /v1/items/plan: one open item per title, planned by the API.
func (c *Client) PlanItems(ctx context.Context, titles []string) ([]Item, error) {
	var answer planItemsResponse
	if err := c.http.Call(ctx, http.MethodPost, "/v1/items/plan", planItemsRequest{Titles: titles}, &answer); err != nil {
		return nil, err
	}
	items := make([]Item, len(answer.Items))
	for i, item := range answer.Items {
		items[i] = Item{Title: item.Title, Done: item.Done}
	}
	return items, nil
}
```

- Violation: `http.Client` used in the adapter beside `Call`; the adapter returning `entity` types or the application's kinds; an operation missing from `Operations`.
- Risk: a call that bypasses `Call` has no timeout, no log, and no request id; the contract scenario misses the operation.
- Fix: follow the example's [todoapi.go](./examples/internal/infrastructure/todoapi/todoapi.go); a new operation adds its method, its two types, its line of `Operations`, and its route in `emulator.go`.

### Answer every operation in the emulator
Write `Emulator()` in the adapter's package as an `http.Handler` answering every operation of `Operations` as the API does, with stable data and no state, and pass it to `httpclient.New`; `{API}_HOST=mock` runs the client against it.
- Risk: an operation the emulator does not answer fails with 404 under `mock`, and the service cannot run without the other service.
- Fix: the emulator's scenario runs the adapter mocked and checks its answer.

### Map the outcomes in the service
Write the service over the adapter in `internal/service/` with an unexported interface of the adapter's methods it uses, turning `httpclient.ErrUnavailable` into `entity.ErrUnavailable`, each `*httpclient.StatusError` code it expects into an error of the kind that fits, and anything else — another status, an answer it cannot use — into an error with no kind, wrapping the adapter's error with `%v`, never `%w`, where it carries a kind of its own.

```go
var status *httpclient.StatusError
switch {
case errors.Is(err, httpclient.ErrUnavailable):
	return nil, fmt.Errorf("%w: the todo service: %v", entity.ErrUnavailable, err)
case errors.As(err, &status) && status.Code == codeInvalidArgument:
	return nil, fmt.Errorf("%w: the todo service rejected the titles: %s", entity.ErrInvalid, status.Message)
case err != nil:
	return nil, fmt.Errorf("the todo service: %w", err)
}
```

- Violation: an entity built from the API's answer whose error is wrapped with `%w`.
- Risk: an item the other service got wrong is answered to this service's client as its own invalid input, 400 instead of 500.
- Fix: the scenario "Any other answer is a failure with no kind" of the example's `remote_planner.feature`.

### Build the adapter and the service in main.go
Build the adapter with `{api}.New(logger)` and the service over it in `// [services]` of `cmd/{app}/main.go`, stopping the start with its error, and pass the service to `command.NewHandlers`.

```go
// [services]
todoAPI, err := todoapi.New(logger)
if err != nil {
	logger.Error("startup failed", "error", err)
	return 1
}
remotePlanner := service.NewRemotePlanner(todoAPI)
```

- Risk: a client built inside a command or a service cannot be mocked or configured from one place.
- Fix: the command takes the service through an interface; the composition root builds both.

### Cover the client with scenarios
Copy the example's `internal/infrastructure/httpclient/features/` and `test/` unchanged but for the module path: they cover the settings, the call, the outcomes, the log, and the mock through a test API of their own. Write per adapter a feature calling each operation against an `httptest` emulator of the scenario and its emulator mocked, and its contract scenario reading `docs/integration/client/{api-name}/openapi.json`; per service a feature with the adapter faked; tag them `@type/tech-check` and `@type/service`.
- Risk: an adapter feature that repeats the outcomes of `Call` grows with every operation and tests nothing new.
- Fix: an adapter feature checks only its own request and mapping.

# Check list
- [ ] `internal/infrastructure/httpclient/{config,client}.go` are filled copies of the templates; no placeholder is left.
- [ ] Each API has its package with `Prefix`, `Operations`, `New`, `NewClient`, one method per operation, and `Emulator`; the contract scenario passes.
- [ ] The service maps `ErrUnavailable`, the expected `StatusError` codes, and nothing else to kinds.
- [ ] `main.go` builds the adapter and the service in `// [services]`.
- [ ] `make test` passes; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
- [examples/](./examples/README.md) — the base plateau's startup command planning with another service's todo API: `make run` against its emulator, or against the running HTTP example of [vp-http-api-in-go](library/architecture/go/vp-http-api-in-go.skill/vp-http-api-in-go.skill.md).
