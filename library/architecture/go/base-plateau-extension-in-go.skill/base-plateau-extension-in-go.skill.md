---
name: base-plateau-extension-in-go
description: How a variation point extends the base plateau in Go — command decorators as pipeline.Decorator in the list main.go gives command.NewHandlers, wrapped around every typed handler by pipeline.Decorate, marks as methods a command has, each API's Entry factory with its list of middlewares (httpapi.Middleware) or interceptors (grpcapi.Interceptor), rejections as errors of a kind, the variation point's own settings and packages, and attach.allow — with a sample access variation point attached to the HTTP example
whenToUse: when you write, attach, or review a variation point of a Go application built on the base plateau, when you add a decorator around Go commands or around HTTP or gRPC requests, or when you add a command to such an application
updated: 20261025
tags:
  - stack/go
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/untyped-decorator-with-typed-adapter.md
---

# Goal
- **The extension points in Go** - A Go application that keeps every property of [base-plateau-extension](library/architecture/spec/base-plateau-extension.skill/base-plateau-extension.skill.md); this skill details that skill's implementation for Go and restates none of its rules.
- **A sample to copy from** - [examples/](./examples/README.md) attaches a sample access variation point to the HTTP example of [vp-http-api-in-go](library/architecture/go/vp-http-api-in-go.skill/vp-http-api-in-go.skill.md): one entry decorator and two command decorators, with the attach check proving that only the composition root, the marked command, and the readme and Makefile changed.

# Core Principle
- **Typed handlers, untyped decorators** - A Go generic function cannot stand in a slice, so a [decorator](library/architecture/go/base-plateau-in-go.skill/glossary/handler-decorator.md) sees the command as `any`, and `pipeline.Decorate` — the one place with type assertions — turns the list back into a typed `Handler[TCommand, TResult]` ([ADR](./adr/untyped-decorator-with-typed-adapter.md)).
- **A mark is a method** - Go interfaces are implicit: a command carries a mark by having the mark's method, and the decorator asks `cmd.(Restricted)`; the command layer imports nothing of the variation point.

```
cmd/{app}/main.go
  // [services]  accessCfg, err := access.LoadConfig()             the variation point's own settings
  // [handlers]  handlers := command.NewHandlers([]pipeline.Decorator{   internal/command/handlers.go
                     pipeline.WithLogging(logger),                 every command      (base plateau)
                     access.WithAccessCheck(),                     by the mark        access.Restricted
                     access.WithBatchLimit(accessCfg.MaxBatch),    by a value         access.Batch
                 })
  // [entries]   entry.RunAll(ctx, logger,
                     httpapi.Entry(handlers, logger, []httpapi.Middleware{
                         httpaccess.Authenticate(accessCfg.Tokens),  entry decorator  (inside the request log)
                     }),
                 )
```

# Rule

## MUST

### Register every command in command.NewHandlers
Give every command a field of `command.Handlers` and a line of `command.NewHandlers` in `internal/command/handlers.go`, the handler built with its services and wrapped by `pipeline.Decorate` with the command's name and the decorators.

```go
// internal/command/handlers.go
type Handlers struct {
	PlanItems pipeline.Handler[PlanItemsCommand, PlanItemsResult]
	AddItem   pipeline.Handler[AddItemCommand, AddItemResult]
}

func NewHandlers(decorators []pipeline.Decorator, lists itemLists) Handlers {
	return Handlers{
		PlanItems: pipeline.Decorate("PlanItems", NewPlanItemsHandler(), decorators),
		AddItem:   pipeline.Decorate("AddItem", NewAddItemHandler(lists), decorators),
	}
}
```

- Violation: `pipeline.WithLogging(...)` applied to one handler in `main.go`; an entry point that calls `command.NewPlanItemsHandler()`.
- Risk: a command built outside the registry misses the decorators the others have.
- Fix: the services a handler uses are parameters of `NewHandlers` after the decorators, passed by `main.go` from `// [services]`; an API's resource takes its handlers from the registry in its `New{Resource}Handler(handlers command.Handlers)`.

### Write a command decorator as a pipeline.Decorator
Write a command decorator as a function that takes what the step needs and returns a `pipeline.Decorator`; let it call `next(ctx, cmd)` once with the command unchanged, or return an error wrapping a kind of `pipeline` to reject the command.

```go
// by the command's mark: internal/command/access/access_check.go
func WithAccessCheck() pipeline.Decorator {
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

// by a value of the command: internal/command/access/batch_limit.go
func WithBatchLimit(limit int) pipeline.Decorator {
	return func(ctx context.Context, name string, cmd any, next pipeline.Next) (any, error) {
		if batch, ok := cmd.(Batch); ok && batch.BatchSize() > limit {
			return nil, fmt.Errorf("%w: %s takes at most %d items at once, got %d",
				pipeline.ErrResourceExhausted, name, limit, batch.BatchSize())
		}
		return next(ctx, cmd)
	}
}
```

- Violation: a generic `With{Step}[TCommand, TResult](next Handler[...]) Handler[...]` that wraps one handler; `switch cmd.(type) { case command.PlanItemsCommand: ... }`; a decorator that passes a new value of another type to `next`.
- Risk: a generic decorator cannot stand in the list; a decorator that names the command types imports `internal/command`, which imports `pipeline` — an import cycle when it lives in `pipeline`, and a list that drifts when it does not; `pipeline.Decorate` answers a value of another type with an error.
- Fix: one function per step returning `pipeline.Decorator`; the simple form — every command — is the base plateau's [pipeline.WithLogging](library/architecture/go/base-plateau-in-go.skill/templates/internal/command/pipeline/logging.go).

### Mark a command with a method in its file
Give a command a mark by writing the mark's method beside its struct, in `internal/command/{command_name}.go`, with a comment naming the mark.

```go
// internal/command/plan_items.go
type PlanItemsCommand struct {
	Titles []string
}

// RequiredRole marks PlanItemsCommand access.Restricted: only a planner may plan.
func (PlanItemsCommand) RequiredRole() string { return "planner" }

// BatchSize marks PlanItemsCommand access.Batch: its size is the number of titles.
func (c PlanItemsCommand) BatchSize() int { return len(c.Titles) }
```

- Violation: the method on a pointer receiver (`func (*PlanItemsCommand)`); the method in a file of the variation point's package.
- Risk: the decorator receives the command as a value, so a pointer-receiver method leaves it unmarked and the check never runs — the command is served to everyone.
- Fix: value receivers; the mark's interface is declared in the variation point's package and named in the comment.

### List the decorators in main.go
Give `command.NewHandlers` the list in `// [handlers]` of `cmd/{app}/main.go`, `pipeline.WithLogging(logger)` first, so every rejection of a decorator after it is logged as `command failed`.

```go
// [handlers] - the handler of every command, built by the command registry
// and wrapped by these decorators, the first the outermost.
handlers := command.NewHandlers([]pipeline.Decorator{
	pipeline.WithLogging(logger),
	access.WithAccessCheck(),                  // by the command's mark: access.Restricted
	access.WithBatchLimit(accessCfg.MaxBatch), // by a value of the command: access.Batch
})
```

- Risk: a decorator placed before the logging rejects calls that leave no trace in the log.
- Fix: insert the variation point's decorator at the place its skill names; a check that is cheaper than the next one goes first (the access check before the batch limit).

### Give an API's decorators to its Entry
Give each API's entry decorators as the last argument of its `Entry` in `// [entries]`: `[]httpapi.Middleware` (`func(http.Handler) http.Handler`) for HTTP, `[]grpcapi.Interceptor` (`grpc.UnaryServerInterceptor`) for gRPC; let an HTTP middleware reject with `httppipeline.WriteError(w, err)` and a gRPC interceptor by returning an error of a kind.

```go
// internal/api/http/access/authenticate.go
func Authenticate(tokens map[string]access.Identity) httppipeline.Middleware {
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
				httppipeline.WriteError(w, fmt.Errorf("%w: the bearer token names no known caller", pipeline.ErrUnauthenticated))
				return
			}
			next.ServeHTTP(w, r.WithContext(access.WithIdentity(r.Context(), identity)))
		})
	}
}

// cmd/{app}/main.go, [entries]
httpapi.Entry(handlers, logger, []httpapi.Middleware{
	httpaccess.Authenticate(accessCfg.Tokens),
}),
grpcapi.Entry(handlers, logger, []grpcapi.Interceptor{}),
```

- Violation: `w.WriteHeader(401)` with a text body; `status.Error(codes.Unauthenticated, ...)` built by hand; a middleware wrapped around `httpapi.Entry`'s handler in `main.go`.
- Risk: the rejection is answered outside google.rpc.Status, or outside the request log.
- Fix: the Entry factories put the request log (HTTP) or the call log, panic recovery, and the answer by kind (gRPC) outside the list, and the list around the contract's routes only — the docs stay open. The CLI has no entry decorators: a step on every action is a command decorator.

### Put each decorator in its own file of its own package
Put a variation point's command decorators in `internal/command/{vp}/{step}.go` with the marks they select by, and its entry decorators in `internal/api/{transport}/{vp}/{step}.go`; keep `internal/command/pipeline` for the base plateau's.
- Violation: `WithAccessCheck` appended to `pipeline/logging.go`; an HTTP middleware in `internal/api/http/pipeline`, which is copied from templates.
- Risk: a variation point edits a base file or a template, and two variation points edit the same one.
- Fix: the sample's `internal/command/access` and `internal/api/http/access`.

### Read the settings in the variation point's package
Read a variation point's settings with `{vp}.LoadConfig()` in its own package, called at the top of `// [services]`; an unusable value stops the start with `startup failed` and exit code `1`.

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
- [ ] Every command decorator returns a `pipeline.Decorator`, passes the command on unchanged, and rejects with an error of a kind.
- [ ] A filtered decorator selects by a mark interface; the marked command has the method on a value receiver in its own file.
- [ ] `pipeline.WithLogging(logger)` is the first decorator of the list.
- [ ] Each API's `Entry` gets its list of middlewares or interceptors; a middleware rejects with `httppipeline.WriteError`.
- [ ] The variation point's decorators and settings live in its own packages.
- [ ] `attach.allow` names every changed base file; `bash test/check-vp-attach.sh` and `make test` pass.

# Example
- [examples/](./examples/README.md) — the HTTP example with the sample access variation point attached: `make run`, then the `curl` calls of its readme; `make test`.
