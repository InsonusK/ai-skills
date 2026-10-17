---
name: base-plateau-in-go
description: The base plateau in Go — the package layout of its four layers, the generic Handler contract with its decorators, the invalid error kind, the startup entry and its exit codes, the composition root with its three marked sections, and the scenarios that cover each layer
whenToUse: when you create a new Go service or Go CLI application from the architecture catalog, or when you attach a variation point to, or review, a Go application built on the base plateau
updated: 20261017
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
- **Interfaces belong to their consumer** - A handler declares the interface it needs from a service in its own package, as Go code does; a variation point implements it.
- **Delivered, not retyped** - The fixed code (handler contract, startup entry) is copied from templates; only the domain, the commands, and the composition-root lines are written per application.

# Rule

## MUST

### Lay out the packages
Place each layer of [base-plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md) in its package.

| Layer | Package |
|---|---|
| Domain | `internal/domain` |
| Application | `internal/application` |
| Entry point | `internal/api/startup` — each API variation point adds `internal/api/{transport}` |
| Composition root | `cmd/{app}/main.go`; the base settings in `internal/config` |

- Risk: a layer in a package of its own choosing breaks every variation point's instruction that names the file to edit.
- Fix: move the code to the package in the table; a service variation point adds `internal/infrastructure/{kind}`.

### Copy the handler contract and the startup entry
Fill and copy [handler.go](./templates/internal/application/handler.go) to `internal/application/handler.go` and [startup.go](./templates/internal/api/startup/startup.go) to `internal/api/startup/startup.go`, replacing `{module-path}` with the module path from `go.mod`.
- Risk: a retyped contract drifts from the one every variation point's decorator and entry point is written against.
- Fix: copy the templates; after filling, searching both files for `{module-path}` returns nothing.

### Rule errors wrap domain.ErrInvalid
Make `ErrInvalid` the [error kind](./glossary/error-kind.md) of every business-rule error: declare `var ErrInvalid = errors.New("invalid")` in `internal/domain`, declare every business-rule error as `fmt.Errorf("%w: {what broke}", ErrInvalid)`, and check the kind with `errors.Is(err, application.ErrInvalid)` outside the domain.
- Risk: an entry point comparing individual errors must change with every new rule.
- Fix: wrap the kind in each rule error; the template re-exports it as `application.ErrInvalid`.

### Entities guard their fields
Give an entity unexported fields, a `New{Entity}` constructor that rejects an invalid entity, and a method for each allowed state change that returns its rule error.
- Risk: an exported field can be set past every rule, from any package.
- Fix: unexport the field; add a getter for what results must show.

### One file per command
Write a command, its result types, and its handler in `internal/application/{command_name}.go`: the command and result as structs (results with `json` tags), the handler as a struct with a `New{Command}Handler` constructor and a `Handle(ctx context.Context, cmd {Command}) ({Command}Result, error)` method.
- Risk: a handler whose method differs from `Handler[TCommand, TResult]` cannot be wrapped or served without an adapter written for it alone.
- Fix: match the method to the contract; take every service the handler uses as a constructor parameter typed by an interface declared in `internal/application`.

### Compose in the three marked sections
Build every concrete type in `run` of `cmd/{app}/main.go` under the comments `// [services]`, `// [handlers]`, and `// [entries]`, in this order, with `main` doing nothing but `os.Exit(run(context.Background(), os.Stdout, os.Stderr))`.
- Risk: a variation point whose instruction says "add after `// [handlers]`" finds no such line and guesses where its code goes.
- Fix: keep the three comments even when a section is empty; type each handler variable as `application.Handler[TCommand, TResult]` so a [decorator](./glossary/handler-decorator.md) can rewrap it.

### Cover each layer with scenarios
Cover `internal/domain`, `internal/application`, `internal/api/startup`, and `internal/config` with Cucumber scenarios per [cucumber-testing-in-go](library/testing/go/cucumber-testing-in-go.skill.md), tagged `@type/domain`, `@type/service` (commands), `@type/tech-check` (decorators, settings), and `@type/api/cli` (startup entry), with the project's testing contract per [solution-conformance-testing-in-go](library/testing/go/solution-conformance-testing-in-go.skill/solution-conformance-testing-in-go.skill.md).
- Risk: a layer without scenarios lets a variation point break it unnoticed, since the variation point's example reruns the base scenarios as its regression check.
- Fix: one `features/` and `test/` pair per package; `make test-kind-unit` passes.

## SHOULD

### Name type parameters by their role
Name a type parameter after what it stands for (`TCommand`, `TResult`, `TEntity`), never by a single letter, and comment every exported generic type and function with what each type parameter is and an example of it.
- Risk: `Handler[C, R]` forces a reader to open the definition to learn what `C` and `R` are, in exactly the shared code every variation point builds on.
- Fix: rename the parameter; add a comment naming each parameter's role with a concrete type from the application.

### Base settings read in one Load
Read the base settings from environment variables in `config.Load() (Config, error)`, returning an error that names the variable it could not parse.

# Check list
- [ ] Packages follow [Lay out the packages](#lay-out-the-packages).
- [ ] `internal/application/handler.go` and `internal/api/startup/startup.go` are filled copies of the templates; neither contains `{module-path}`.
- [ ] Every business-rule error wraps `domain.ErrInvalid`; code outside the domain checks `errors.Is(err, application.ErrInvalid)`.
- [ ] Entities have unexported fields, a validating constructor, and guarded state-change methods.
- [ ] Every command lives in its own file in `internal/application` with a handler matching `Handler[TCommand, TResult]`.
- [ ] `run` in `cmd/{app}/main.go` has the `// [services]`, `// [handlers]`, `// [entries]` sections in this order.
- [ ] Type parameters are named by role, and every exported generic type or function has a comment explaining them.
- [ ] `make test-kind-unit` passes, with scenarios in every package listed in [Cover each layer with scenarios](#cover-each-layer-with-scenarios).

# Example
- [examples/](./examples/README.md) — the base plateau as a runnable to-do planner: `make run`, `make test-kind-unit`.
- [glossary/](./glossary/README.md) — how the handler contract, `HandlerFunc`, decorators, the error kind, the composition root, and the startup entry work.
