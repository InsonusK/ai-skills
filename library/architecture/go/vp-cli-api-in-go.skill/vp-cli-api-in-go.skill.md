---
name: vp-cli-api-in-go
description: The CLI API variation point in Go — internal/api/cli/pipeline copied from templates (Run building the cobra command line with --output, --version, usage errors as the invalid kind, the generic Command adapter, the run log), one {Resource}Handler per resource in internal/api/cli/endpoint, the registration in internal/api/cli/commands.go with each action's flags, JSON output, and text form, and the startup entry replaced by cliapi.Entry in main.go
whenToUse: when a Go application built on the base plateau must be run from a command line, when you offer a new command as an action of such an application, or when you review its internal/api/cli package
updated: 20261025
tags:
  - stack/go
  - concern/architecture
  - app-type/cli
---

# Goal
- **The CLI variation point in Go** - A Go application that keeps every property of [vp-cli-api](library/architecture/spec/vp-cli-api.skill/vp-cli-api.skill.md); this skill details that skill's implementation for Go and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-go](library/architecture/go/base-plateau-in-go.skill/base-plateau-in-go.skill.md)'s example.

# Core Principle
- **Copied once, written per command** - The entry point is split as [app-layers-in-go](library/develop/go/app-layers-in-go.skill.md#split-each-entry-point-into-pipeline-endpoint-and-registration) requires: `pipeline/` is copied from templates and never edited; per command the application writes, in its resource's file of `endpoint/`, a handler field, an action line, and its `bind`, `encode`, and `text` functions.

```
internal/api/cli/
├── pipeline/                  copied from templates, never edited (clipipeline)
│   ├── cli.go                 Run: the cobra root, --output, --version, usage errors → invalid
│   ├── command.go             Command: flags → command → handler → JSON line or text
│   ├── logging.go             the run log: received / answered
│   └── features/  test/       the fixed code's scenarios, with a test resource of their own
├── endpoint/                  written per application, grows with the command line
│   ├── items.go               ItemsHandler: one file per resource, `todo items plan`
│   └── features/  test/       one feature per resource
└── commands.go                Endpoints, NewEndpoints, Resources, Entry, Run (cliapi)
```

# Rule

## MUST

### Copy the entry point from templates
Fill and copy [cli.go](./templates/internal/api/cli/pipeline/cli.go), [command.go](./templates/internal/api/cli/pipeline/command.go), and [logging.go](./templates/internal/api/cli/pipeline/logging.go) to `internal/api/cli/pipeline/`, with the example's `pipeline/features/` and `pipeline/test/`, replacing `{module-path}` with the module path from `go.mod`.
- Risk: a retyped command line drifts in its usage errors, exit codes, or output formats from every other Go CLI of the catalog.
- Fix: copy the files; after filling, searching them for `{module-path}` returns nothing.

### One file per resource in the endpoint package
Write each resource in its own file `internal/api/cli/endpoint/{resource}.go`: an exported `{Resource}Handler` struct (`ItemsHandler`) with one `pipeline.Handler[TCommand, TResult]` field per command, filled from the command registry by `New{Resource}Handler(handlers command.Handlers)`, its method `Resource() clipipeline.Resource` returning `{Name, Short, Actions}` with one `clipipeline.Command(use, short, handler, bind, encode, text)` per command, and per action its `bind`, its output type with `encode`, and its `text` (or `nil` for indented JSON). Keep in `internal/api/cli/commands.go` only `Endpoints`, one field per resource, `NewEndpoints(handlers command.Handlers)`, `Resources(e Endpoints)`, `Entry`, the entry factory, and `Run`, which the scenarios call.

```go
// bind declares the flags and returns how to read them into the command.
func bindPlanItems(flags *pflag.FlagSet) func() (command.PlanItemsCommand, error) {
	titles := flags.StringArray("title", nil, "the title of an item; repeat for several")
	return func() (command.PlanItemsCommand, error) {
		return command.PlanItemsCommand{Titles: *titles}, nil
	}
}
```

- Violation: a `cobra.Command` with its own `RunE` that calls a handler; an action registered outside `Resources`; the command's result passed to `encode` unchanged.
- Risk: an action with its own `RunE` loses the usage-error kind and the output formats; the published output follows the command layer.
- Fix: follow the example's [items.go](./examples/internal/api/cli/endpoint/items.go); an error `read` returns is a usage error (exit `2`); a repeated value is a `StringArray`, not a comma-separated string.

### Replace the startup entry in main.go
Replace the startup `entry.Entry` in the `// [entries]` list of `cmd/{app}/main.go` with `cliapi.Entry`, given the command registry, the logger, the application's name, the version, `os.Args[1:]`, and the output streams, and delete `internal/api/startup/`.

```go
// [entries] - every entry point, one Entry each, run together by entry.RunAll.
err = entry.RunAll(ctx, logger,
	cliapi.Entry(handlers, logger, "todo", version.Version, os.Args[1:], stdout, stderr),
)
return entry.ExitCode(err)
```

- Violation: `cobra.Command.Execute` called in `main.go`, an `os.Exit` inside the CLI package, or the startup entry kept beside the CLI.
- Risk: an exit code computed outside `entry.ExitCode` ignores the error kind; an `os.Exit` skips the graceful stop of every other entry.
- Fix: import `cliapi "{module-path}/internal/api/cli"`; the entry prints the error as `Error: ...` and returns it, its `ReportsErrors: true` keeps `RunAll` from logging it a second time, and `entry.ExitCode` turns it into the exit code. The logs of the command's run stay on stderr.

### Add the cobra module
Add `github.com/spf13/cobra` at `v1.10.2` with `go get` and run `go mod tidy`.
- Risk: another version beside the one `godog` already requires (v1.10.2) downgrades `godog` in `go.mod`.
- Fix: commit `go.mod` and `go.sum` with this version.

### Cover the entry point with scenarios
Copy the example's `internal/api/cli/pipeline/features/` and `test/` unchanged but for the module path: they cover the fixed code through a test resource of their own. In `internal/api/cli/endpoint/` write one feature per resource, running `cliapi.Run` with `cliapi.Endpoints` over faked handlers (`pipeline.HandlerFunc`), checking the flags the handler received and the JSON and text output; tag them `@type/api/cli`.
- Risk: a resource feature that repeats the usage errors grows with every resource and tests nothing new.
- Fix: a resource feature checks only its own flags and output; `make test` passes.

## SHOULD

### Offer a new command in its resource's file
Offer a further command of an existing resource by adding its field, its line of `New{Resource}Handler`, its action line, and its `bind`, `encode`, and `text` to the resource's file, and its scenario to the resource's feature; the command itself joins the command registry; `main.go` does not change. A new resource adds its file and feature, one field of `Endpoints`, and one line each of `NewEndpoints` and `Resources`. The command line has no API middleware: a step on every action is a command middleware ([base-plateau-extension-in-go](library/architecture/go/base-plateau-extension-in-go.skill/base-plateau-extension-in-go.skill.md)).

# Check list
- [ ] `internal/api/cli/pipeline/{cli,command,logging}.go` are filled copies of the templates; no placeholder is left.
- [ ] Each resource has its own file in `endpoint/` with its `{Resource}Handler` and `Resource()`, its actions built by `clipipeline.Command`, and per action its `bind`, `encode`, and `text`; `commands.go` holds only `Endpoints`, `NewEndpoints`, `Resources`, `Entry`, and `Run`.
- [ ] The `// [entries]` list of `main.go` has `cliapi.Entry(handlers, ..., os.Args[1:], ...)`; `internal/api/startup/` is deleted.
- [ ] `github.com/spf13/cobra` is pinned at `v1.10.2`.
- [ ] `make test` passes with the fixed code's feature and one feature per resource.
- [ ] `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
- [examples/](./examples/README.md) — the base plateau's to-do planner as a command line: `make run`, `make test`, `./bin/todo --help`.
