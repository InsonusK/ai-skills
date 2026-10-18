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
- **Copied once, written per command** - Settings, the generic `Endpoint`, the server with `Run`, and the docs routes are copied from templates; per command the application writes only, in its resource's file, a handler field, a route line, a request and a response type, and their two mappings.

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

### One file per resource
Write each resource of the API — the first path segment after the version, `/v1/items/...` — in its own file `internal/api/http/{resource}.go`, the counterpart of an ASP.NET controller: an exported `{Resource}Handlers` struct with one `pipeline.Handler[TCommand, TResult]` field per command, an `{resource}Routes(h {Resource}Handlers) []Route` returning one `{"METHOD /v1/...", Endpoint(handler, decode, encode, status)}` per command, and per route its request and response types with a `decode` and an `encode` function. Keep in `routes.go` only `Handlers`, one field per resource, and `contractRoutes`, which concatenates the resources' routes.
- Violation: every route of the API in one file, a route registered outside `contractRoutes`, or a command's result type encoded as the response.
- Risk: one routes file grows with every command until no one reads it; a route outside the list escapes the contract scenario; an encoded command type ties the published contract to the command layer.
- Fix: follow the example's [items.go](./examples/internal/api/http/items.go) and [routes.go](./examples/internal/api/http/routes.go); decode a JSON body with `DecodeJSON[TBody](r)`; test each resource in its own feature (`features/{resource}.feature`).

### Hand-write the contract
Write the contract as `docs/integration/api/{contract-name}/openapi.json` (OpenAPI 3) with `embed.go` beside it, per [api-develop-in-go](library/common-workflow/develop/api-develop-in-go.skill/api-develop-in-go.skill.md)'s rules for a service with no proto contract, listing for every route its success status, 400 and 500 with the `Status` schema.
- Risk: a route missing from the spec is invisible in Swagger UI; a spec operation with no route answers 404.
- Fix: change the resource's file and `openapi.json` in the same change and bump `info.version`.

### Replace the startup entry in main.go
Replace the lines of the `// [entries]` section of `cmd/{app}/main.go` with one call of `httpapi.Run`, filling `Handlers` with the handlers built in `// [handlers]`, and delete `internal/api/startup/`.

```go
// [entries] - every entry point.
handlers := httpapi.Handlers{Items: httpapi.ItemHandlers{PlanItems: planItems}}
if err := httpapi.Run(ctx, handlers, logger); err != nil {
	logger.Error("http api stopped", "error", err)
	return 1
}
return 0
```

- Violation: the settings loaded or the server built in `main.go`, the server started outside the `// [entries]` section, or the startup entry kept beside it.
- Risk: every line of the entry point's start in `main.go` is a line each later variation point's instruction must step around; a kept startup entry runs a command on every start.
- Fix: import the package as `httpapi "{module-path}/internal/api/http"`; `Run` reads the settings, builds the server, and serves until `ctx` is cancelled; a service variation point adds its readiness checks as the last arguments of `Run`.

### Add the Swagger UI module
Add `github.com/swaggest/swgui` at `v1.8.9` with `go get` and run `go mod tidy`.
- Risk: an unpinned version builds a different Swagger UI on another machine.
- Fix: commit `go.mod` and `go.sum` with the version above.

### Cover the entry point with scenarios
Cover `internal/api/http` with the five features of the example — the API with faked handlers (`pipeline.HandlerFunc`), the health endpoints, the docs under a prefix and switched off, the settings, and the graceful stop — tagged `@type/api/http` and `@type/tech-check`, plus the contract scenario of api-develop-in-go.
- Risk: a variation point without scenarios lets a change in the error mapping or the shutdown reach production unnoticed.
- Fix: copy the example's `features/` and `test/` and adapt the API feature to the application's routes; `make test` passes.

## SHOULD

### Serve a new command in its resource's file
Serve a further command of an existing resource by adding its field, its route line, and its request and response types with `decode` and `encode` to the resource's file, its path to `openapi.json`, and its handler to `Handlers` in `main.go`; a new resource adds its file, one field of `Handlers`, and one line of `contractRoutes`.

# Check list
- [ ] `internal/api/http/{config,endpoint,server,docs}.go` are filled copies of the templates; no placeholder is left.
- [ ] Each resource has its own file with its handlers struct, its routes, and per route its request and response types with `decode` and `encode`; `routes.go` holds only `Handlers` and `contractRoutes`.
- [ ] `docs/integration/api/{contract-name}/openapi.json` and `embed.go` exist; the contract scenario passes.
- [ ] The `// [entries]` section of `main.go` is one `httpapi.Run` call; `internal/api/startup/` is deleted.
- [ ] `github.com/swaggest/swgui` is pinned at `v1.8.9`.
- [ ] `make test` passes with the five HTTP features.
- [ ] `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
- [examples/](./examples/README.md) — the base plateau's to-do planner with the HTTP API attached: `make run`, `make test`.
