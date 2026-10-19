---
name: vp-http-api-in-go
description: The HTTP API variation point in Go — the internal/api/http package copied from templates (settings, the generic Endpoint, the server with graceful stop, the docs), the routes written per application in internal/api/http/endpoint, the contract either hand-written in OpenAPI or, as the HTTP form of a gRPC contract, generated from the proto file's google.api.http annotations with the routes over the generated messages, and one http Entry in main.go
whenToUse: when a Go application built on the base plateau must be served over HTTP, when a Go service with a gRPC contract needs its HTTP form, when you serve a new command over HTTP in such an application, or when you review its internal/api/http package
updated: 20261019
tags:
  - stack/go
  - concern/architecture
  - app-type/service
---

# Goal
- **The HTTP variation point in Go** - A Go application that keeps every property of [vp-http-api](library/architecture/spec/vp-http-api.skill/vp-http-api.skill.md); this skill details that skill's implementation for Go and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-go](library/architecture/go/base-plateau-in-go.skill/base-plateau-in-go.skill.md)'s example; as the HTTP form of a proto contract, only those [attach-proto.allow](./attach-proto.allow) names against [vp-grpc-api-in-go](library/architecture/go/vp-grpc-api-in-go.skill/vp-grpc-api-in-go.skill.md)'s example.

# Core Principle
- **One contract, two forms** - With a gRPC contract, the proto file is the only contract: the routes read and write its generated messages and the spec is generated from its annotations, so the HTTP and gRPC forms cannot describe different operations or shapes.
- **Copied once, written per command** - `internal/api/http/` holds only code copied from templates — settings, the generic `Endpoint`, the server with `Run`, the docs routes — and is never edited; everything that grows with the API lives in `internal/api/http/endpoint/`, where per command the application writes, in its resource's file, a handler field, a route line, a request and a response type, and their two mappings.

```
internal/api/rpcstatus/        copied from error-kinds-in-go; the same in every network entry point
internal/api/http/            copied from templates, never edited
├── config.go  endpoint.go  server.go  docs.go
├── proto.go                  only for a proto contract: DecodeProto, ProtoJSON, Unimplemented
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

### Hand-write the contract when there is no proto file
Write the contract as `docs/integration/api/{contract-name}/openapi.json` (OpenAPI 3) with `embed.go` beside it, per [api-develop-in-go](library/common-workflow/develop/api-develop-in-go.skill/api-develop-in-go.skill.md)'s rules for a service with no proto contract, listing for every route its success status, 400 and 500 with the `Status` schema.
- Risk: a route missing from the spec is invisible in Swagger UI; a spec operation with no route answers 404.
- Fix: change the resource's file and `openapi.json` in the same change and bump `info.version`.

### Generate the spec of a proto contract from its annotations
Give every rpc of the proto file a `google.api.http` annotation and an `openapiv2_operation` listing its success status, each error status it can answer, and `default`, all with `.google.rpc.Status` as the error schema; add the googleapis and grpc-gateway dependencies to `buf.yaml` and the `protoc-gen-openapiv2` plugin with `disable_default_responses=true` and `enable_rpc_deprecation=true` to `buf.gen.yaml`, as the [proto example](./examples-proto/docs/integration/api/todo/todo.proto) and its [buf.gen.yaml](./examples-proto/buf.gen.yaml) do; commit `{file}.swagger.json` and name it in `embed.go`.
- Violation: a hand-written `openapi.json` beside the proto file; a 201 method whose spec also lists the 200 the generator adds by default; a deprecated rpc not marked deprecated in the spec.
- Risk: two descriptions of one API drift; Swagger UI promises a status the method never answers.
- Fix: one spec, generated by `make proto-gen` (`go tool protoc-gen-openapiv2` as a `tool` line of `go.mod`), per [api-develop-in-go](library/common-workflow/develop/api-develop-in-go.skill/api-develop-in-go.skill.md); `docs.go` sets `basePath` in it under `HTTP_BASE_PATH`.

### Map the generated messages with proto.go
Copy [proto.go](./templates/internal/api/http/proto.go) beside the other templates, and in each route decode the body with `httpapi.DecodeProto(r, req)` into the rpc's generated request message, read each path parameter with `r.PathValue` under the name the generated spec gives it, and encode with `httpapi.ProtoJSON(resp)`.

```go
// POST /v1/lists/{listId}/items - rpc AddItem, field list_id in the path
func decodeAddItem(r *http.Request) (command.AddItem, error) {
	req := &todo.AddItemRequest{}
	if err := httpapi.DecodeProto(r, req); err != nil {
		return command.AddItem{}, err
	}
	return command.AddItem{ListID: r.PathValue("listId"), Title: req.GetTitle()}, nil
}
```

- Violation: a struct of the route's own beside a generated message; `encoding/json` on a generated message; a path parameter named `{list_id}` in the route while the spec says `{listId}`.
- Risk: `encoding/json` writes proto names and `null` and rejects the lowerCamelCase a gateway accepts; a route pattern that differs from the spec fails the contract scenario, or, unchecked, answers 404 for what Swagger UI shows.
- Fix: proto-JSON in and out; the generator names a path parameter in lowerCamelCase (`list_id` → `listId`), and the route uses that name. A field at its default is left out of a response (`"done": false` is absent), as the standard mapping allows.

### Keep each path parameter a whole segment
Write every `google.api.http` path with each variable as a whole segment (`/v1/lists/{list_id}/clear`), never a custom-method suffix (`/v1/lists/{list_id}:clear`).
- Risk: `http.ServeMux` rejects a wildcard that is not a whole segment, so the route cannot be registered.
- Fix: `/clear` in place of `:clear`.

### Answer a deprecated rpc 501 in both forms
Mark an rpc that is kept but no longer served `option deprecated = true`, answer it with `httpapi.Unimplemented(message)` as its route's handler and `grpcapi.Unimplemented(message)` from its gRPC method, with the same message, and call no command.
- Risk: a removed route answers 404 and a removed method fails the build of every client that still has it; two messages for one method confuse the client.
- Fix: keep the rpc until the next major version; 501 / `UNIMPLEMENTED` with `"{Rpc} is deprecated since {version}"`.

### Serve both forms from the same handlers
In a service with the gRPC variation point, add the `http` entry beside the `grpc` one in `// [entries]`, both filled with the same handlers built in `// [handlers]`.

```go
err = entry.RunAll(ctx, logger,
	entry.Entry{Name: "grpc", Run: func(ctx context.Context) error { return grpcapi.Run(ctx, services, logger) }},
	entry.Entry{Name: "http", Run: func(ctx context.Context) error { return httpapi.Run(ctx, routes, logger) }},
)
```

- Risk: a handler built twice gets two decorator chains, and the two forms behave differently.
- Fix: one handler variable per command, passed to both `endpoint` packages.

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
Copy the example's `internal/api/http/features/` and `test/` — of [examples-proto/](./examples-proto/README.md) for a proto contract, which add `proto.feature` for `proto.go` — unchanged but for the module path and the contract's package (`todo`) in their imports: they cover the fixed code — the error mapping through a test command of their own, the docs under a prefix and switched off, the settings, and `Run` with its graceful stop. In `internal/api/http/endpoint/` write one feature per resource with its handlers faked (`pipeline.HandlerFunc`), and the contract scenario of api-develop-in-go; tag them `@type/api/http` and `@type/tech-check`.
- Risk: a variation point without scenarios lets a change in the error mapping or the shutdown reach production unnoticed; a resource feature that repeats the error mapping grows the suite with every resource and tests nothing new.
- Fix: a resource feature checks only its own decoding and encoding; `make test` passes.

## SHOULD

### Serve a new command in its resource's file
Serve a further command of an existing resource by adding its field, its route line, and its request and response types with `decode` and `encode` to the resource's file in `endpoint/`, its path to `openapi.json`, its scenario to the resource's feature, and its handler to `endpoint.Handlers` in `main.go`; a new resource adds its file and feature, one field of `Handlers`, and one line of `Routes`.

# Check list
- [ ] `internal/api/http/{config,endpoint,server,docs}.go` and `internal/api/rpcstatus/rpcstatus.go` are filled copies of the templates; no placeholder is left.
- [ ] `internal/api/http/` holds only filled template files and their copied scenarios; each resource has its own file in `endpoint/` with its handlers struct, its routes, and per route its request and response types with `decode` and `encode`; `endpoint/routes.go` holds only `Handlers` and `Routes`.
- [ ] `docs/integration/api/{contract-name}/` holds `embed.go` and one spec: the hand-written `openapi.json`, or, for a proto contract, the generated `{file}.swagger.json`; the contract scenario passes.
- [ ] For a proto contract: every rpc has `google.api.http` and `openapiv2_operation`; the routes use the generated messages through `proto.go` and the spec's path-parameter names; every path variable is a whole segment; a deprecated rpc answers 501 / `UNIMPLEMENTED` in both forms.
- [ ] The `// [entries]` list of `main.go` has an `http` entry calling `httpapi.Run`; `internal/api/startup/` is deleted.
- [ ] `github.com/swaggest/swgui` is pinned at `v1.8.9`.
- [ ] `make test` passes with the fixed code's features and one feature per resource.
- [ ] `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow) and [attach-proto.allow](./attach-proto.allow).

# Example
- [examples/](./examples/README.md) — the base plateau's to-do planner with the HTTP API attached: `make run`, `make test`.
- [examples-proto/](./examples-proto/README.md) — the HTTP form of the gRPC example's proto contract, with a path parameter, 201, 404, 409, and a deprecated rpc: `make run`, `make proto-gen`, `make test`.
