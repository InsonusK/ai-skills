---
name: base-plateau-extension-in-go
description: How a variation point extends the base plateau in Go — every middleware a package of its own under internal/command/middleware/ or internal/api/{kind}/middleware/ with a function Middleware, the command middleware as pipeline.Middleware in the list main.go gives command.NewHandlers, wrapped around every typed handler by pipeline.Decorate, marks as methods a command has, each API's Entry factory with its list of API middleware (httpapi.Middleware, grpcapi.Interceptor) inside the fixed requestlog and answerfailures, rejections as errors of a kind, the variation point's own settings, and attach.allow — with a sample access variation point attached to the HTTP example
whenToUse: when you write, attach, or review a variation point of a Go application built on the base plateau, when you add a middleware around Go commands or around HTTP or gRPC requests, or when you add a command to such an application
updated: 20261025
tags:
  - stack/go
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/untyped-middleware-with-typed-adapter.md
---

# Goal
- **The extension points in Go** - A Go application that keeps every property of [base-plateau-extension](library/architecture/spec/base-plateau-extension.skill/base-plateau-extension.skill.md); this skill details that skill's implementation for Go and restates none of its rules.
- **A sample to copy from** - [examples/](./examples/README.md) attaches a sample access variation point to the HTTP example of [vp-http-api-in-go](library/architecture/go/vp-http-api-in-go.skill/vp-http-api-in-go.skill.md): one API middleware and two command middleware, with the attach check proving that only the composition root, the marked command, and the readme and Makefile changed.

# Core Principle
- **One package per middleware** - Each middleware is a package named after its step, with one function `Middleware` — `access.Middleware()`, `batchlimit.Middleware(limit)` — so the package name says what it does and the function name says what it is, without `access.AccessMiddleware`.
- **Typed handlers, untyped middleware** - A Go generic function cannot stand in a slice, so a [command middleware](library/architecture/go/base-plateau-in-go.skill/glossary/command-middleware.md) sees the command as `any`, and `pipeline.Decorate` — the one place with type assertions — turns the list back into a typed `Handler[TCommand, TResult]` ([ADR](./adr/untyped-middleware-with-typed-adapter.md)).
- **A mark is a method** - Go interfaces are implicit: a command carries a mark by having the mark's method, and the middleware asks `cmd.(Restricted)`; the command layer imports nothing of the variation point.

```
internal/command/                         internal/api/http/
├── pipeline/       Handler, Middleware,  ├── pipeline/       Server, Endpoint, Config
│                   Decorate, kinds       ├── middleware/
├── middleware/                           │   ├── requestlog/      fixed, outermost
│   ├── logging/        base plateau      │   ├── answerfailures/  fixed: kinds, panics, WriteError
│   ├── access/         sample VP         │   └── authentication/  sample VP
│   └── batchlimit/     sample VP         └── routes.go       Entry(handlers, logger, middlewares)
└── handlers.go     NewHandlers(middlewares, services...)

cmd/{app}/main.go
  // [services]  accessCfg, err := access.LoadConfig()                the variation point's own settings
  // [handlers]  command.NewHandlers([]pipeline.Middleware{
                     commandlogging.Middleware(logger),               every command      (base plateau)
                     access.Middleware(),                             by the mark        access.Restricted
                     batchlimit.Middleware(accessCfg.MaxBatch),       by a value         batchlimit.Batch
                 })
  // [entries]   httpapi.Entry(handlers, logger, []httpapi.Middleware{
                     authentication.Middleware(accessCfg.Tokens),     inside requestlog and answerfailures
                 })
```

# Rule

## MUST

### Register every command in command.NewHandlers
Give every command a field of `command.Handlers` and a line of `command.NewHandlers` in `internal/command/handlers.go`, the handler built with its services and wrapped by `pipeline.Decorate` with the command's name and the middleware.

```go
// internal/command/handlers.go
type Handlers struct {
	PlanItems pipeline.Handler[PlanItemsCommand, PlanItemsResult]
	AddItem   pipeline.Handler[AddItemCommand, AddItemResult]
}

func NewHandlers(middlewares []pipeline.Middleware, lists itemLists) Handlers {
	return Handlers{
		PlanItems: pipeline.Decorate("PlanItems", NewPlanItemsHandler(), middlewares),
		AddItem:   pipeline.Decorate("AddItem", NewAddItemHandler(lists), middlewares),
	}
}
```

- Violation: a middleware applied to one handler in `main.go`; an entry point that calls `command.NewPlanItemsHandler()`.
- Risk: a command built outside the registry misses the middleware the others have.
- Fix: the services a handler uses are parameters of `NewHandlers` after the middleware, passed by `main.go` from `// [services]`; an API's resource takes its handlers from the registry in its `New{Resource}Handler(handlers command.Handlers)`.

### One package per middleware
Put each command middleware in `internal/command/middleware/{name}/` and each API middleware in `internal/api/{kind}/middleware/{name}/`, a package named after its step with a function `Middleware` that takes what the step needs, its marks and errors beside it, and its `features/` and `test/`.
- Violation: `WithAccessCheck` added to `internal/command/pipeline`; an HTTP middleware in `internal/api/http/pipeline`, which is copied from templates; two middleware in one package.
- Risk: a variation point edits a base file or a template, and a reader looking for a step finds it under another name.
- Fix: the sample's `command/middleware/access`, `command/middleware/batchlimit`, and `api/http/middleware/authentication`. A package named like another the composition root imports is imported under an alias: `commandlogging "{module-path}/internal/command/middleware/logging"` beside `internal/logging`.

### Write a command middleware as a pipeline.Middleware
Return a `pipeline.Middleware` from `Middleware`; let it call `next(ctx, cmd)` once with the command unchanged, or return an error wrapping a kind of `pipeline` to reject the command.

```go
// by the command's mark: internal/command/middleware/access/access.go
func Middleware() pipeline.Middleware {
	return func(ctx context.Context, name string, cmd any, next pipeline.Next) (any, error) {
		restricted, ok := cmd.(Restricted)
		if !ok {
			return next(ctx, cmd) // not marked: passes unchanged
		}
		identity, ok := IdentityOf(ctx)
		if !ok {
			return nil, fmt.Errorf("%w: %s needs a caller", pipeline.ErrUnauthenticated, name)
		}
		if !identity.HasRole(restricted.RequiredRole()) {
			return nil, fmt.Errorf("%w: %s needs the role %s", pipeline.ErrPermissionDenied, name, restricted.RequiredRole())
		}
		return next(ctx, cmd)
	}
}

// by a value of the command: internal/command/middleware/batchlimit/batchlimit.go
func Middleware(limit int) pipeline.Middleware {
	return func(ctx context.Context, name string, cmd any, next pipeline.Next) (any, error) {
		if batch, ok := cmd.(Batch); ok && batch.BatchSize() > limit {
			return nil, fmt.Errorf("%w: %s takes at most %d items at once, got %d",
				pipeline.ErrResourceExhausted, name, limit, batch.BatchSize())
		}
		return next(ctx, cmd)
	}
}
```

- Violation: a generic `Middleware[TCommand, TResult](next Handler[...]) Handler[...]` that wraps one handler; `switch cmd.(type) { case command.PlanItemsCommand: ... }`; a middleware that passes a new value of another type to `next`.
- Risk: a generic middleware cannot stand in the list; a middleware that names the command types imports `internal/command`, which a list cannot keep up with; `pipeline.Decorate` answers a value of another type with an error.
- Fix: the simple form — every command — is the base plateau's [logging middleware](library/architecture/go/base-plateau-in-go.skill/templates/internal/command/middleware/logging/logging.go).

### Mark a command with a method in its file
Give a command a mark by writing the mark's method beside its struct, in `internal/command/{command_name}.go`, with a comment naming the mark.

```go
// internal/command/plan_items.go
type PlanItemsCommand struct {
	Titles []string
}

// RequiredRole marks PlanItemsCommand access.Restricted: only a planner may plan.
func (PlanItemsCommand) RequiredRole() string { return "planner" }

// BatchSize marks PlanItemsCommand batchlimit.Batch: its size is the number of titles.
func (c PlanItemsCommand) BatchSize() int { return len(c.Titles) }
```

- Violation: the method on a pointer receiver (`func (*PlanItemsCommand)`); the method in a file of the middleware's package.
- Risk: the middleware receives the command as a value, so a pointer-receiver method leaves it unmarked and the check never runs — the command is served to everyone.
- Fix: value receivers; the mark's interface is declared in the middleware's package and named in the comment.

### List the command middleware in main.go
Give `command.NewHandlers` the list in `// [handlers]` of `cmd/{app}/main.go`, `commandlogging.Middleware(logger)` first, so every rejection of a middleware after it is logged as `command failed`.

```go
// [handlers] - the handler of every command, built by the command registry
// and wrapped by this command middleware, the first the outermost.
handlers := command.NewHandlers([]pipeline.Middleware{
	commandlogging.Middleware(logger),
	access.Middleware(),                       // by the command's mark: access.Restricted
	batchlimit.Middleware(accessCfg.MaxBatch), // by a value of the command: batchlimit.Batch
})
```

- Risk: a middleware placed before the logging rejects calls that leave no trace in the log.
- Fix: insert the variation point's middleware at the place its skill names; a check that is cheaper than the next one goes first (the access check before the batch limit).

### Give an API's middleware to its Entry
Give each API's middleware as the last argument of its `Entry` in `// [entries]`: `[]httpapi.Middleware` (`func(http.Handler) http.Handler`) for HTTP, `[]grpcapi.Interceptor` (`grpc.UnaryServerInterceptor`) for gRPC; let an HTTP middleware reject with `answerfailures.WriteError(w, err)` and a gRPC one by returning an error of a kind.

```go
// internal/api/http/middleware/authentication/authentication.go
func Middleware(tokens map[string]access.Identity) httppipeline.Middleware {
	return func(next http.Handler) http.Handler {
		return http.HandlerFunc(func(w http.ResponseWriter, r *http.Request) {
			header := r.Header.Get("Authorization")
			if header == "" {
				next.ServeHTTP(w, r) // no credentials: the command decides
				return
			}
			token, isBearer := strings.CutPrefix(header, "Bearer ")
			identity, known := tokens[token]
			if !isBearer || !known {
				answerfailures.WriteError(w, fmt.Errorf("%w: the bearer token names no known caller", pipeline.ErrUnauthenticated))
				return
			}
			next.ServeHTTP(w, r.WithContext(access.WithIdentity(r.Context(), identity)))
		})
	}
}

// cmd/{app}/main.go, [entries]
httpapi.Entry(handlers, logger, []httpapi.Middleware{
	authentication.Middleware(accessCfg.Tokens),
}),
grpcapi.Entry(handlers, logger, []grpcapi.Interceptor{}),
```

- Violation: `w.WriteHeader(401)` with a text body; `status.Error(codes.Unauthenticated, ...)` built by hand; `requestlog.Middleware` given in the list.
- Risk: the rejection is answered outside `google.rpc.Status`, or outside the request log; a fixed middleware given twice logs every request twice.
- Fix: the Entry factories put the fixed middleware — `requestlog` (HTTP) or `calllog` (gRPC) outermost, then `answerfailures` — outside the list, and the HTTP list around the contract's routes only — the docs stay open. The CLI has no API middleware: a step on every action is a command middleware.

### Read the settings in the variation point's package
Read a variation point's settings with `{name}.LoadConfig()` in one of its own packages, called at the top of `// [services]`; an unusable value stops the start with `startup failed` and exit code `1`.

```go
// [services]
accessCfg, err := access.LoadConfig() // ACCESS_TOKENS, ACCESS_MAX_BATCH
if err != nil {
	logger.Error("startup failed", "error", err)
	return 1
}
```

- Risk: a field in `internal/config` belongs to the base plateau and stays when the variation point goes.
- Fix: the error names its variable (`access config: ACCESS_MAX_BATCH "0" is not a positive number`).

### Name every changed base file in attach.allow
Give the variation point's example an `attach.allow` beside its `examples/`, naming its base example and every base file it changes or deletes.

```
# skill folder / attach.allow
base library/architecture/go/vp-http-api-in-go.skill/examples
change cmd/todo/main.go
change internal/command/plan_items.go
change Makefile
change README.md
```

- Risk: a change outside the list is a hidden dependency of the variation point.
- Fix: `bash test/check-vp-attach.sh` in this repository passes.

# Check list
- [ ] Every command is a field and a line of `internal/command/handlers.go`; `main.go` calls `command.NewHandlers` once.
- [ ] Every middleware is a package of its own under `internal/command/middleware/` or `internal/api/{kind}/middleware/` with a function `Middleware`.
- [ ] Every command middleware returns a `pipeline.Middleware`, passes the command on unchanged, and rejects with an error of a kind.
- [ ] A filtered middleware selects by a mark interface; the marked command has the method on a value receiver in its own file.
- [ ] `commandlogging.Middleware(logger)` is the first of the command middleware.
- [ ] Each API's `Entry` gets its list of middleware; an HTTP middleware rejects with `answerfailures.WriteError`; no fixed middleware is in a list.
- [ ] `attach.allow` names every changed base file; `bash test/check-vp-attach.sh` and `make test` pass.

# Example
- [examples/](./examples/README.md) — the HTTP example with the sample access variation point attached: `make run`, then the `curl` calls of its readme; `make test`.
