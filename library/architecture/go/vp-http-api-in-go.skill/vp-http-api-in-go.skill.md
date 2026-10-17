---
name: vp-http-api-in-go
description: The HTTP API variation point in Go — the internal/api/http package copied from templates (settings, the generic Endpoint, the server with health endpoints and graceful stop, the docs), the routes and their request and response types written per application, the hand-written OpenAPI contract, and the startup entry replaced in the entries section of main.go
whenToUse: when a Go application built on the base plateau must be served over HTTP, when you serve a new command over HTTP in such an application, or when you review its internal/api/http package
updated: 20261017
tags:
  - stack/go
  - concern/architecture
  - app-type/service
---

# Goal
- **The HTTP variation point in Go** - A Go application that keeps every property of [vp-http-api](library/architecture/spec/vp-http-api.skill/vp-http-api.skill.md); this skill details that skill's implementation for Go and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-go](library/architecture/go/base-plateau-in-go.skill/base-plateau-in-go.skill.md)'s example.

# Core Principle
- **Copied once, written per command** - Settings, the generic `Endpoint`, the server, and the docs routes are copied from templates; per command the application writes only a `Handlers` field, a route line, a request and a response type, and their two mappings.

# Rule

## MUST

### Copy the entry point from templates
Fill and copy [config.go](./templates/internal/api/http/config.go), [endpoint.go](./templates/internal/api/http/endpoint.go), [server.go](./templates/internal/api/http/server.go), and [docs.go](./templates/internal/api/http/docs.go) to `internal/api/http/`, replacing:

| Placeholder | Substitute |
|---|---|
| `{module-path}` | the module path from `go.mod` |
| `{contract-name}` | the contract's folder name under `docs/integration/api/` (kebab-case) |
| `{contract-package}` | `{contract-name}` without hyphens — the package name of its `embed.go` |
| `{api-title}` | the title shown in Swagger UI |

- Risk: a retyped endpoint or server drifts in its error mapping, health endpoints, or shutdown from every other Go service of the catalog.
- Fix: copy the four files; after filling, searching them for `{module-path}`, `{contract-name}`, `{contract-package}`, and `{api-title}` returns nothing.

### Write the routes in routes.go
Write `internal/api/http/routes.go` with a `Handlers` struct holding one `command.Handler[TCommand, TResult]` field per served command, a `contractRoutes(h Handlers) []Route` returning one `{"METHOD /v1/...", Endpoint(handler, decode, encode, status)}` per command, and, per route, its request and response types with a `decode` and an `encode` function.
- Violation: a route registered outside `contractRoutes`, or a command's result type encoded as the response.
- Risk: a route outside the list escapes the contract scenario; an encoded command type ties the published contract to the command layer.
- Fix: follow the shape of the example's [routes.go](./examples/internal/api/http/routes.go); decode a JSON body with `DecodeJSON[TBody](r)`.

### Hand-write the contract
Write the contract as `docs/integration/api/{contract-name}/openapi.json` (OpenAPI 3) with `embed.go` beside it, per [api-develop-in-go](library/common-workflow/develop/api-develop-in-go.skill/api-develop-in-go.skill.md)'s rules for a service with no proto contract, listing for every route its success status, 400 and 500 with the `Status` schema.
- Risk: a route missing from the spec is invisible in Swagger UI; a spec operation with no route answers 404.
- Fix: change `routes.go` and `openapi.json` in the same change and bump `info.version`.

### Replace the startup entry in main.go
Replace the lines of the `// [entries]` section of `cmd/{app}/main.go` with these, filling `Handlers` with the handlers built in `// [handlers]`, and delete `internal/api/startup/`.

```go
// [entries] - every entry point.
httpConfig, err := httpapi.LoadConfig()
if err != nil {
	logger.Error("startup failed", "error", err)
	return 1
}
server := httpapi.NewServer(httpConfig, version.Version, httpapi.Handlers{PlanItems: planItems}, logger)
if err := server.Serve(ctx); err != nil {
	logger.Error("http server failed", "error", err)
	return 1
}
return 0
```

- Violation: the server started outside the `// [entries]` section, or the startup entry kept beside it.
- Risk: the next variation point's instruction names a section that no longer holds the entries; a kept startup entry runs a command on every start.
- Fix: import the package as `httpapi "{module-path}/internal/api/http"`; a service variation point adds its readiness checks as the last arguments of `NewServer`.

### Add the Swagger UI module
Add `github.com/swaggest/swgui` at `v1.8.9` with `go get` and run `go mod tidy`.
- Risk: an unpinned version builds a different Swagger UI on another machine.
- Fix: commit `go.mod` and `go.sum` with the version above.

### Cover the entry point with scenarios
Cover `internal/api/http` with the five features of the example — the API with faked handlers (`command.HandlerFunc`), the health endpoints, the docs under a prefix and switched off, the settings, and the graceful stop — tagged `@type/api/http` and `@type/tech-check`, plus the contract scenario of api-develop-in-go.
- Risk: a variation point without scenarios lets a change in the error mapping or the shutdown reach production unnoticed.
- Fix: copy the example's `features/` and `test/` and adapt the API feature to the application's routes; `make test-kind-unit` passes.

## SHOULD

### Serve a new command in four edits
Serve a further command by adding its field to `Handlers`, its line to `contractRoutes`, its request and response types with `decode` and `encode` in `routes.go`, and its path in `openapi.json`, and by passing its handler in `main.go`.

# Check list
- [ ] `internal/api/http/{config,endpoint,server,docs}.go` are filled copies of the templates; no placeholder is left.
- [ ] `routes.go` holds `Handlers`, `contractRoutes`, and per route its request and response types with `decode` and `encode`.
- [ ] `docs/integration/api/{contract-name}/openapi.json` and `embed.go` exist; the contract scenario passes.
- [ ] The `// [entries]` section of `main.go` starts the server; `internal/api/startup/` is deleted.
- [ ] `github.com/swaggest/swgui` is pinned at `v1.8.9`.
- [ ] `make test-kind-unit` passes with the five HTTP features.
- [ ] `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
- [examples/](./examples/README.md) — the base plateau's to-do planner with the HTTP API attached: `make run`, `make test-kind-unit`.
