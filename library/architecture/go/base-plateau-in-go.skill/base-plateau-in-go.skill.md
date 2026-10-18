---
name: base-plateau-in-go
description: The base plateau in Go — the package layout of its four layers, the generic Handler contract with its decorators, the error kinds, the startup entry and its exit codes, the composition root with its three marked sections, and the scenarios that cover each layer
whenToUse: when you create a new Go service or Go CLI application from the architecture catalog, or when you attach a variation point to, or review, a Go application built on the base plateau
updated: 20261018
tags:
  - stack/go
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **The base plateau in Go** - A Go module that keeps every property of [base-plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md); this skill details that skill's implementation for Go and restates none of its rules.
- **Same shape in every Go application** - The [handler contract](./glossary/handler-contract.md), the [startup entry](./glossary/startup-entry.md), and the [composition-root](./glossary/composition-root.md) sections are identical across Go applications, so a variation point's instructions name the same file and the same section everywhere.

# Core Principle
- **Layers per app-layers-in-go** - Packages, interfaces, entities, and fakes follow [app-layers-in-go](library/develop/go/app-layers-in-go.skill.md); this skill adds only what the base plateau itself contains.
- **Delivered, not retyped** - The fixed code (handler contract, startup entry) is copied from templates; only the domain, the commands, and the composition-root lines are written per application.

# Rule

## MUST

### Base packages
Create the packages [app-layers-in-go](library/develop/go/app-layers-in-go.skill.md) assigns to the api, command, and domain layers, with the startup entry as the only api package and the base settings in their own package.

| Part | Package |
|---|---|
| startup entry | `internal/api/startup` |
| commands — one file each, nothing else | `internal/command` |
| [pipeline](./glossary/pipeline.md): handler contract, `HandlerFunc`, decorators, the error kinds re-exported | `internal/command/pipeline` |
| command factories, when an input needs one | `internal/command/factory` |
| entities, the error kinds (`kinds.go`) and the rule errors | `internal/domain/entity` |
| domain services | `internal/domain/service` |
| composition root | `cmd/{app}/main.go` |
| base settings (`LOG_LEVEL` only) | `internal/config` |
| version of the build | `internal/version` — `var Version`, per `devops-project-version-in-go` |

- Risk: a part in a package of its own choosing breaks every variation point's instruction that names the file to edit.
- Fix: move the code to the package in the table.

### Copy the handler contract and the startup entry
Fill and copy [handler.go](./templates/internal/command/pipeline/handler.go) and [logging.go](./templates/internal/command/pipeline/logging.go) to `internal/command/pipeline/`, and [startup.go](./templates/internal/api/startup/startup.go) to `internal/api/startup/startup.go`, replacing `{module-path}` with the module path from `go.mod`.
- Risk: a retyped contract drifts from the one every variation point's decorator and entry point is written against.
- Fix: copy the templates; after filling, searching the copied files for `{module-path}` returns nothing.

### One file per decorator
Put each decorator in its own file of `internal/command/pipeline` — `logging.go` for `WithLogging`, `{step}.go` for `With{Step}` — and never add one to `handler.go` or to another decorator's file.
- Violation: `WithUnitOfWork` appended to `logging.go` or to `handler.go`.
- Risk: a variation point that brings a decorator edits a base file, and two such variation points edit the same one.
- Fix: the variation point adds `internal/command/pipeline/{step}.go` and wraps the handlers in the `// [handlers]` section.

### Errors carry a kind of error-kinds
Copy `kinds.go` and `pipeline/errors.go` from the templates of [error-kinds-in-go](library/develop/go/error-kinds-in-go.skill/error-kinds-in-go.skill.md), and declare every business-rule error in `internal/domain/entity/errors.go` as `fmt.Errorf("%w: {what broke}", Err{Kind})`, the kind chosen by [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md)'s table — see the [error kind](./glossary/error-kind.md).
- Risk: an entry point comparing individual errors must change with every new rule; a rule error with the wrong kind tells the caller to fix an input that is correct.
- Fix: the example's `ErrAlreadyDone` wraps `ErrFailedPrecondition` (the input is valid, the item's state forbids it); the others wrap `ErrInvalid`.

### One file per command
Write a command, its result types, and its handler in `internal/command/{command_name}.go`: the command and result as structs (results with `json` tags), the handler as a struct with a `New{Command}Handler` constructor and a `Handle(ctx context.Context, cmd {Command}) ({Command}Result, error)` method.
- Risk: a handler whose method differs from `Handler[TCommand, TResult]` cannot be wrapped or served without an adapter written for it alone.
- Fix: match the method to the contract; take every service the handler uses as a constructor parameter typed by an interface declared in `internal/command`.

### Compose in the three marked sections
Build every concrete type in `run` of `cmd/{app}/main.go` under the comments `// [services]`, `// [handlers]`, and `// [entries]`, in this order, with `main` only creating a context cancelled on `SIGINT`/`SIGTERM` (`signal.NotifyContext`) and exiting with `run(ctx, os.Stdout, os.Stderr)`.
- Risk: a variation point whose instruction says "add after `// [handlers]`" finds no such line and guesses where its code goes.
- Fix: keep the three comments even when a section is empty; type each handler variable as `pipeline.Handler[TCommand, TResult]` so a [decorator](./glossary/handler-decorator.md) can rewrap it.

### Cover each layer with scenarios
Cover `internal/domain/entity`, `internal/domain/service`, `internal/command`, `internal/command/pipeline`, `internal/api/startup`, and `internal/config` with Cucumber scenarios per [cucumber-testing-in-go](library/testing/go/cucumber-testing-in-go.skill.md), tagged `@type/domain`, `@type/service` (commands), `@type/tech-check` (decorators, settings), and `@type/api/cli` (startup entry), run by `go test ./...`.
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
- [ ] `internal/command/pipeline/{handler,logging}.go` and `internal/api/startup/startup.go` are filled copies of the templates; none contains `{module-path}`.
- [ ] Every business-rule error wraps one kind of `kinds.go`; code outside the domain checks kinds only through `pipeline`.
- [ ] Every command lives in its own file in `internal/command` with a handler matching `Handler[TCommand, TResult]`.
- [ ] Each decorator is in its own file of `internal/command/pipeline`.
- [ ] `run` in `cmd/{app}/main.go` has the `// [services]`, `// [handlers]`, `// [entries]` sections in this order.
- [ ] The example holds no testing tooling; its `Makefile` runs `go test ./...`.
- [ ] Type parameters are named by role, and every exported generic type or function has a comment explaining them.
- [ ] `make test` passes, with scenarios in every package listed in [Cover each layer with scenarios](#cover-each-layer-with-scenarios).

# Example
- [examples/](./examples/README.md) — the base plateau as a runnable to-do planner: `make run`, `make test`.
- [glossary/](./glossary/README.md) — how the handler contract, `HandlerFunc`, decorators, the error kind, the composition root, and the startup entry work.
