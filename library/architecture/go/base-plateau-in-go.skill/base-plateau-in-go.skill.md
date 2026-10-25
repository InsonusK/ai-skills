---
name: base-plateau-in-go
description: The base plateau in Go — the package layout of its four layers, the generic Handler contract, the command middleware list with the typed adapter Decorate and the logging middleware in internal/command/middleware/logging, the command registry command.Handlers, the error kinds, the startup entry, the runner of the entry points with the exit codes, the composition root with its three marked sections, and the scenarios that cover each layer
whenToUse: when you create a new Go service or Go CLI application from the architecture catalog, or when you attach a variation point to, or review, a Go application built on the base plateau
updated: 20261025
tags:
  - stack/go
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **The base plateau in Go** - A Go module that keeps every property of [base-plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md); this skill details that skill's implementation for Go and restates none of its rules.
- **Same shape in every Go application** - The [handler contract](./glossary/handler-contract.md), the [command registry](./glossary/command-registry.md), the [startup entry](./glossary/startup-entry.md), and the [composition-root](./glossary/composition-root.md) sections are identical across Go applications, so a variation point's instructions name the same file and the same section everywhere — how a variation point uses them is [base-plateau-extension-in-go](library/architecture/go/base-plateau-extension-in-go.skill/base-plateau-extension-in-go.skill.md).

# Core Principle
- **Layers per app-layers-in-go** - Packages, interfaces, entities, and fakes follow [app-layers-in-go](library/develop/go/app-layers-in-go.skill.md); this skill adds only what the base plateau itself contains.
- **Delivered, not retyped** - The fixed code (handler contract, logging middleware, startup entry, runner) is copied from templates; only the domain, the commands, the command registry, and the composition-root lines are written per application.

# Rule

## MUST

### Base packages
Create the packages [app-layers-in-go](library/develop/go/app-layers-in-go.skill.md) assigns to the api, command, and domain layers, with the startup entry as the only entry point and the base settings in their own package.

| Part | Package |
|---|---|
| startup entry | `internal/api/startup` |
| [runner of the entry points](./glossary/entry-runner.md): `Entry`, `RunAll`, `ExitCode` | `internal/api/core/entry` |
| commands — one file each | `internal/command` |
| [command registry](./glossary/command-registry.md): `Handlers`, `NewHandlers` | `internal/command/handlers.go` |
| [pipeline](./glossary/pipeline.md): handler contract, `HandlerFunc`, `Middleware` with `Decorate`, the error kinds re-exported | `internal/command/pipeline` |
| [command middleware](./glossary/command-middleware.md), one package each: `logging.Middleware` in the base plateau | `internal/command/middleware/{name}` |
| command factories, when an input needs one | `internal/command/factory` |
| entities, the error kinds (`kinds.go`) and the rule errors | `internal/domain/entity` |
| domain services | `internal/domain/service` |
| composition root | `cmd/{app}/main.go` |
| base settings (`LOG_LEVEL` only) | `internal/config` |
| [logging](./glossary/logging.md): `NewHandler`, `Payload`, `Failed`, request id | `internal/logging` |
| version of the build | `internal/version` — `var Version`, per `project-version-in-go` |

- Risk: a part in a package of its own choosing breaks every variation point's instruction that names the file to edit.
- Fix: move the code to the package in the table.

### Copy the handler contract, the startup entry, and the runner
Fill and copy [handler.go](./templates/internal/command/pipeline/handler.go) and [middleware.go](./templates/internal/command/pipeline/middleware.go) to `internal/command/pipeline/`, [logging.go](./templates/internal/command/middleware/logging/logging.go) to `internal/command/middleware/logging/`, [startup.go](./templates/internal/api/startup/startup.go) to `internal/api/startup/startup.go`, [entry.go](./templates/internal/api/core/entry/entry.go) to `internal/api/core/entry/entry.go`, and [logging.go](./templates/internal/logging/logging.go) to `internal/logging/logging.go`, replacing `{module-path}` with the module path from `go.mod`.
- Risk: a retyped contract drifts from the one every variation point's middleware and entry point is written against.
- Fix: copy the templates; after filling, searching the copied files for `{module-path}` returns nothing.

### Errors carry a kind of error-kinds
Copy `kinds.go` and `pipeline/errors.go` from the templates of [error-kinds-in-go](library/develop/go/error-kinds-in-go.skill/error-kinds-in-go.skill.md), and declare every business-rule error in `internal/domain/entity/errors.go` as `fmt.Errorf("%w: {what broke}", Err{Kind})`, the kind chosen by [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md)'s table — see the [error kind](./glossary/error-kind.md).
- Risk: an entry point comparing individual errors must change with every new rule; a rule error with the wrong kind tells the caller to fix an input that is correct.
- Fix: the example's `ErrAlreadyDone` wraps `ErrFailedPrecondition` (the input is valid, the item's state forbids it); the others wrap `ErrInvalid`.

### One file per command
Write a command, its result types, and its handler in `internal/command/{command_name}.go`: the command and result as structs (results with `json` tags), the handler as a struct with a `New{Command}Handler` constructor and a `Handle(ctx context.Context, cmd {Command}) ({Command}Result, error)` method; register it with a field and a line of the [command registry](./glossary/command-registry.md).
- Risk: a handler whose method differs from `Handler[TCommand, TResult]` cannot be wrapped or served without an adapter written for it alone.
- Fix: match the method to the contract; take every service the handler uses as a constructor parameter typed by an interface declared in `internal/command`.

### Compose in the three marked sections
Build every concrete type in `run` of `cmd/{app}/main.go` under the comments `// [services]`, `// [handlers]`, and `// [entries]`, in this order, with `main` only creating a context cancelled on `SIGINT`/`SIGTERM` (`signal.NotifyContext`) and exiting with `run(ctx, os.Stdout, os.Stderr)`.
- Risk: a variation point whose instruction says "add after `// [handlers]`" finds no such line and guesses where its code goes.
- Fix: keep the three comments even when a section is empty; `// [handlers]` is one `command.NewHandlers` call with the list of [command middleware](./glossary/command-middleware.md), `commandlogging.Middleware(logger)` first.

```go
// [handlers] - the handler of every command, built by the command registry
// and wrapped by this command middleware, the first the outermost.
handlers := command.NewHandlers([]pipeline.Middleware{
	commandlogging.Middleware(logger), // commandlogging "{module-path}/internal/command/middleware/logging"
})
```

### Build the logger from logging.NewHandler
Build the one `*slog.Logger` in `run` as `slog.New(logging.NewHandler(stderr, cfg.LogLevel))`, pass it to every middleware and entry point, and log the data of a call only as `logging.Payload(v)` with `logging.Failed(err != nil)` on the record that ends it.
- Violation: `slog.NewJSONHandler` in `main.go`; `logger.Info("called", "body", body)`; a second logger built in a package.
- Risk: data logged under another key escapes the base handler's drop and the logging variation point's mask.
- Fix: the logging variation point replaces only this line of `main.go`.

### One Entry per entry point in [entries]
End `run` with one `entry.RunAll` call listing an `entry.Entry{Name, Run}` per entry point, and return `entry.ExitCode(err)`.

```go
// [entries] - every entry point, one Entry each, run together by entry.RunAll.
err = entry.RunAll(ctx, logger,
	entry.Entry{Name: "startup", Run: func(ctx context.Context) error {
		return startup.Run(ctx, handlers.PlanItems, command.PlanItemsCommand{Titles: startup.EnvList("STARTUP_TITLES")}, stdout)
	}},
)
return entry.ExitCode(err)
```

- Violation: an entry point's `Run` called outside `entry.RunAll`; an exit code computed in `main.go`.
- Risk: the next variation point's entry point is never started, or the process keeps running with one entry point dead.
- Fix: a variation point adds its `Entry` to the list (health) or replaces the startup `Entry` with its own (an API), built by its entry factory (`httpapi.Entry(handlers, logger, middlewares)`); `RunAll` and `ExitCode` stay as they are. `RunAll` logs the error of an entry, unless the entry sets `ReportsErrors: true` because it shows the error itself (the CLI).

### Cover each layer with scenarios
Cover `internal/domain/entity`, `internal/domain/service`, `internal/command`, `internal/command/pipeline`, `internal/command/middleware/logging`, `internal/api/startup`, `internal/api/core/entry`, `internal/logging`, and `internal/config` with Cucumber scenarios per [cucumber-testing-in-go](library/testing/go/cucumber-testing-in-go.skill.md), tagged `@type/domain`, `@type/service` (commands), `@type/tech-check` (decorators, settings, the runner), and `@type/api/cli` (startup entry), run by `go test ./...`.
- Risk: a layer without scenarios lets a variation point break it unnoticed, since the variation point's example reruns the base scenarios as its regression check.
- Fix: one `features/` and `test/` pair per package; `make test` passes.

### Run the scenarios with make test
Give the example's `Makefile` a `test` target running `go test ./...`, and leave the testing contract (kinds, reports, mutation) to [solution-conformance-testing-in-go](library/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md), as base-plateau's "Examples show the build, not the testing contract" requires.
- Risk: a copied `tools/testing` must be updated in every example whenever the testing skills change.
- Fix: no `tools/`, `report-template/`, or mutation tool in the example.

## SHOULD

### Name type parameters by their role
Name a type parameter after what it stands for (`TCommand`, `TResult`, `TEntity`), never by a single letter, and comment every exported generic type and function with what each type parameter is and an example of it.
- Risk: `Handler[C, R]` forces a reader to open the definition to learn what `C` and `R` are, in exactly the shared code every variation point builds on.
- Fix: rename the parameter; add a comment naming each parameter's role with a concrete type from the application.

### Base settings read in one Load
Read the base settings from environment variables in `config.Load() (Config, error)`, returning an error that names the variable it could not parse.

# Check list
- [ ] Packages follow [Base packages](#base-packages) and [app-layers-in-go](library/develop/go/app-layers-in-go.skill.md).
- [ ] `internal/command/pipeline/{handler,middleware}.go`, `internal/command/middleware/logging/logging.go`, `internal/api/startup/startup.go`, `internal/api/core/entry/entry.go`, and `internal/logging/logging.go` are filled copies of the templates; none contains `{module-path}`.
- [ ] Every business-rule error wraps one kind of `kinds.go`; code outside the domain checks kinds only through `pipeline`.
- [ ] Every command lives in its own file in `internal/command` with a handler matching `Handler[TCommand, TResult]`, and has a field and a line in `internal/command/handlers.go`.
- [ ] `run` in `cmd/{app}/main.go` has the `// [services]`, `// [handlers]`, `// [entries]` sections in this order; `[handlers]` is one `command.NewHandlers` call; `[entries]` is one `entry.RunAll` call returning `entry.ExitCode(err)`.
- [ ] The example holds no testing tooling; its `Makefile` runs `go test ./...`.
- [ ] Type parameters are named by role, and every exported generic type or function has a comment explaining them.
- [ ] `make test` passes, with scenarios in every package listed in [Cover each layer with scenarios](#cover-each-layer-with-scenarios).

# Example
- [examples/](./examples/README.md) — the base plateau as a runnable to-do planner: `make run`, `make test`.
- [glossary/](./glossary/README.md) — how the handler contract, `HandlerFunc`, the command middleware, the command registry, the error kind, the composition root, and the startup entry work.
