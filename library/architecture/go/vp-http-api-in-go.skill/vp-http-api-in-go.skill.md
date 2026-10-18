---
name: vp-http-api-in-go
description: The HTTP API variation point in Go — the internal/api/http package copied from templates (settings, the generic Endpoint, the server with graceful stop, the docs), the routes and their request and response types written per application, the hand-written OpenAPI contract, and the startup entry replaced in the entries section of main.go
whenToUse: when a Go application built on the base plateau must be served over HTTP, when you serve a new command over HTTP in such an application, or when you review its internal/api/http package
updated: 20261018
tags:
  - stack/go
  - concern/architecture
  - app-type/service
---

# Goal
- **The HTTP variation point in Go** - A Go application that keeps every property of [vp-http-api](library/architecture/spec/vp-http-api.skill/vp-http-api.skill.md); this skill details that skill's implementation for Go and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-go](library/architecture/go/base-plateau-in-go.skill/base-plateau-in-go.skill.md)'s example.

# Core Principle
- **Copied once, written per command** - `internal/api/http/` holds only code copied from templates — settings, the generic `Endpoint`, the server with `Run`, the docs routes — and is never edited; everything that grows with the API lives in `internal/api/http/endpoint/`, where per command the application writes, in its resource's file, a handler field, a route line, a request and a response type, and their two mappings.

```
internal/api/rpcstatus/        copied from error-kinds-in-go; the same in every network entry point
internal/api/http/            copied from templates, never edited
├── config.go  endpoint.go  server.go  docs.go
├── features/  test/          the fixed code's scenarios, with a test command of their own
└── endpoint/                 written per application, grows with the API
    ├── routes.go             Handlers (one field per resource) + Routes
    ├── items.go              one file per resource ≈ an ASP.NET controller
    └── features/  test/      one feature per resource + the contract scenario
```

# Rule

## MUST

### Copy the entry point from templates
Fill and copy [config.go](./templates/internal/api/http/config.go), [endpoint.go](./templates/internal/api/http/endpoint.go), [server.go](./templates/internal/api/http/server.go), and [docs.go](./templates/internal/api/http/docs.go) to `internal/api/http/` and `rpcstatus.go` of [error-kinds-in-go](library/develop/go/error-kinds-in-go.skill/error-kinds-in-go.skill.md) with its scenarios (the example's `internal/api/rpcstatus/`) unless another entry point already put them there, replacing:

| Placeholder | Substitute |
|---|---|
| `{module-path}` | the module path from `go.mod` |
| `{contract-name}` | the contract's folder name under `docs/integration/api/` (kebab-case) |
| `{contract-package}` | `{contract-name}` without hyphens — the package name of its `embed.go` |
| `{api-title}` | the title shown in Swagger UI |

- Risk: a retyped endpoint or server drifts in its error mapping or shutdown from every other Go service of the catalog.
- Fix: copy the files; after filling, searching them for `{module-path}`, `{contract-name}`, `{contract-package}`, and `{api-title}` returns nothing.

### One file per resource in the endpoint package
Write each resource of the API — the first path segment after the version, with its collection and item paths together (`/v1/users` and `/v1/users/{id}` in `users.go`) — in its own file `internal/api/http/endpoint/{resource}.go`, the counterpart of an ASP.NET controller: an exported `{Resource}Handlers` struct with one `pipeline.Handler[TCommand, TResult]` field per command, an `{resource}Routes(h {Resource}Handlers) []httpapi.Route` returning one `{Pattern: "METHOD /v1/...", Handler: httpapi.Endpoint(handler, decode, encode, status)}` per command, and per route its request and response types with a `decode` and an `encode` function. Keep in `endpoint/routes.go` only `Handlers`, one field per resource, and `Routes`, which concatenates the resources' routes.
- Violation: a resource's routes written in `internal/api/http/`, every route in one file, a route registered outside `Routes`, or a command's result type encoded as the response.
- Risk: application code mixed into the copied package is overwritten or lost when the templates are copied again; one routes file grows with every command until no one reads it; a route outside the list escapes the contract scenario; an encoded command type ties the published contract to the command layer.
- Fix: follow the example's [items.go](./examples/internal/api/http/endpoint/items.go) and [routes.go](./examples/internal/api/http/endpoint/routes.go); decode a JSON body with `httpapi.DecodeJSON[TBody](r)`.

### Hand-write the contract
Write the contract as `docs/integration/api/{contract-name}/openapi.json` (OpenAPI 3) with `embed.go` beside it, per [api-develop-in-go](library/common-workflow/develop/api-develop-in-go.skill/api-develop-in-go.skill.md)'s rules for a service with no proto contract, listing for every route its success status, 400 and 500 with the `Status` schema.
- Risk: a route missing from the spec is invisible in Swagger UI; a spec operation with no route answers 404.
- Fix: change the resource's file and `openapi.json` in the same change and bump `info.version`.

### Replace the startup entry in main.go
Replace the startup `entry.Entry` in the `// [entries]` list of `cmd/{app}/main.go` with an `http` one calling `httpapi.Run` with `endpoint.Routes`, filling `endpoint.Handlers` with the handlers built in `// [handlers]`, and delete `internal/api/startup/`.

```go
// [entries] - every entry point, one Entry each, run together by entry.RunAll.
routes := endpoint.Routes(endpoint.Handlers{Items: endpoint.ItemHandlers{PlanItems: planItems}})
err = entry.RunAll(ctx, logger,
	entry.Entry{Name: "http", Run: func(ctx context.Context) error {
		return httpapi.Run(ctx, routes, logger)
	}},
)
return entry.ExitCode(err)
```

- Violation: the settings loaded or the server built in `main.go`, `httpapi.Run` called outside `entry.RunAll`, or the startup entry kept beside it.
- Risk: every line of the entry point's start in `main.go` is a line each later variation point's instruction must step around; a server started outside the list is not stopped when another entry point fails.
- Fix: import `httpapi "{module-path}/internal/api/http"` and `"{module-path}/internal/api/http/endpoint"`; `Run` reads the settings, builds the server, and serves until `ctx` is cancelled; other entries of the list (health, gRPC) stay as they are.

### Add the Swagger UI module
Add `github.com/swaggest/swgui` at `v1.8.9` with `go get` and run `go mod tidy`.
- Risk: an unpinned version builds a different Swagger UI on another machine.
- Fix: commit `go.mod` and `go.sum` with the version above.

### Cover the entry point with scenarios
Copy the example's `internal/api/http/features/` and `test/` unchanged but for the module path and the contract's package (`todo`) in their imports: they cover the fixed code — the error mapping through a test command of their own, the docs under a prefix and switched off, the settings, and `Run` with its graceful stop. In `internal/api/http/endpoint/` write one feature per resource with its handlers faked (`pipeline.HandlerFunc`), and the contract scenario of api-develop-in-go; tag them `@type/api/http` and `@type/tech-check`.
- Risk: a variation point without scenarios lets a change in the error mapping or the shutdown reach production unnoticed; a resource feature that repeats the error mapping grows the suite with every resource and tests nothing new.
- Fix: a resource feature checks only its own decoding and encoding; `make test` passes.

## SHOULD

### Serve a new command in its resource's file
Serve a further command of an existing resource by adding its field, its route line, and its request and response types with `decode` and `encode` to the resource's file in `endpoint/`, its path to `openapi.json`, its scenario to the resource's feature, and its handler to `endpoint.Handlers` in `main.go`; a new resource adds its file and feature, one field of `Handlers`, and one line of `Routes`.

# Check list
- [ ] `internal/api/http/{config,endpoint,server,docs}.go` and `internal/api/rpcstatus/rpcstatus.go` are filled copies of the templates; no placeholder is left.
- [ ] `internal/api/http/` holds only filled template files and their copied scenarios; each resource has its own file in `endpoint/` with its handlers struct, its routes, and per route its request and response types with `decode` and `encode`; `endpoint/routes.go` holds only `Handlers` and `Routes`.
- [ ] `docs/integration/api/{contract-name}/openapi.json` and `embed.go` exist; the contract scenario passes.
- [ ] The `// [entries]` list of `main.go` has an `http` entry calling `httpapi.Run`; `internal/api/startup/` is deleted.
- [ ] `github.com/swaggest/swgui` is pinned at `v1.8.9`.
- [ ] `make test` passes with the fixed code's features and one feature per resource.
- [ ] `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
- [examples/](./examples/README.md) — the base plateau's to-do planner with the HTTP API attached: `make run`, `make test`.
