---
name: vp-cli-api-in-python
description: The CLI API variation point in Python — api/cli/pipeline copied from templates (run building the click command line with --version and help at every level, usage errors as the invalid kind, a crashed action as a failure with no kind, the generic command adapter with --output json|text, the run log), one {Resource}Handler module per resource in api/cli/endpoint, commands.py with the entry factory, and the startup entry replaced by cli_api.entry in main.py
whenToUse: when a Python application built on the base plateau must be run from a command line, when you offer a new command as an action of such an application, or when you review its api/cli package
updated: 20261102
tags:
  - stack/python
  - concern/architecture
  - app-type/cli
adr:
  - adr/click-with-coroutine-actions.md
---

# Goal
- **The CLI variation point in Python** - A Python application that keeps every property of [vp-cli-api](library/architecture/spec/vp-cli-api.skill/vp-cli-api.skill.md); this skill details that skill's implementation for Python and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-python](library/architecture/python/base-plateau-in-python.skill/base-plateau-in-python.skill.md)'s example.

# Core Principle
- **Copied once, written per command** - `pipeline/` is copied from templates and never edited; per command the application writes, in its resource's module of `endpoint/`, a handler, an action line, and its `bind`, output type with `encode`, and `text`.
- **click parses, the loop runs** - click builds and parses the command line; each action returns a coroutine that `run()` awaits on the application's event loop, and every failure is answered by `run()` ([ADR](./adr/click-with-coroutine-actions.md)).

```
src/{package}/api/cli/
├── pipeline/             copied, never edited
│   ├── cli.py            Resource, run(): the click root, --version, usage errors → invalid, a crash → internal error
│   ├── command.py        Binding, command(): flags → command → handler → JSON line or text
│   └── run_log.py        the run log: received / answered
├── endpoint/             one module per resource: {Resource}Handler with resource(), `todo items plan`
└── commands.py           Endpoints with from_handlers, resources(), entry(), run()
```

# Rule

## MUST

### Copy the entry point from templates
Copy [api/cli/pipeline/](./templates/src/{package}/api/cli/pipeline/) with the example's `features/` and `test/`, replacing `{package}` with the import name of the package, and add `click==8.5.0` to the dependencies of `pyproject.toml`.
- Risk: a retyped command line drifts in its usage errors, exit codes, or output formats from every other CLI of the catalog, or lets click exit the process itself.
- Fix: copy the templates; after filling, searching them for `{package}` returns nothing; `make test` passes.

### One module per resource in the endpoint package
Write each resource as `api/cli/endpoint/{resource}.py` holding a `{Resource}Handler` class with one command handler per action, a `from_handlers(handlers)` classmethod taking them from the command registry, and a `resource()` method returning `Resource(name, short, actions)` with one `command(name, short, handler, bind, encode, text)` per command; beside it per action its `bind` returning a `Binding` of its click options and `read`, its output dataclass with `encode`, and its `text` (or `None` for indented JSON).

```python
def bind_plan_items() -> Binding[PlanItemsCommand]:
    """The flags of `todo items plan`."""
    return Binding(
        params=[click.Option(["--title", "titles"], multiple=True, help="the title of an item; repeat for several")],
        read=lambda flags: PlanItemsCommand(titles=list(flags["titles"])),
    )
```

- Violation: a `@click.command` of its own that calls a handler; an action registered outside `resources()`; the command's result returned by `encode` unchanged.
- Risk: an action of its own loses the usage-error kind, the output formats, and the run log; the published output follows the command layer.
- Fix: follow the example's [items.py](./examples/src/todo/api/cli/endpoint/items.py); a `ValueError` that `read` raises is a usage error (exit `2`); a repeated value is a `multiple=True` option, not a comma-separated string.

### Register the resources in commands.py
List every resource as a field of `Endpoints`, its line in `Endpoints.from_handlers`, and its `resource()` in `resources()` of `api/cli/commands.py`, beside `entry(...)`, the entry factory, and `run(...)`, which the scenarios call, and nothing else there.
- Risk: a resource missing from `resources()` is offered by nothing while its module and its scenarios pass.
- Fix: one field and one line per resource.

### Replace the startup entry in main.py
Replace the startup `Entry` in `# [entries]` of `main.py` with `cli_api.entry(handlers, logger, name, version, sys.argv[1:], stdout, stderr)`, and delete `api/startup/`.

```python
# [entries]
ended_by = await run_all(
    logger,
    stopping,
    cli_api.entry(handlers, logger, "todo", version("todo"), sys.argv[1:], stdout, stderr),
)
return exit_code(ended_by)
```

- Violation: `click.Group.main` with `standalone_mode=True` or `sys.exit` inside the CLI package; the startup entry kept beside the CLI.
- Risk: an exit code computed outside `exit_code` ignores the error kind; a `sys.exit` skips the stop of every other entry.
- Fix: `from todo.api import cli as cli_api`; the entry prints the error as `Error: ...` and raises it, its `reports_errors=True` keeps `run_all` from logging it a second time, and `exit_code` turns it into the exit code. The logs of the run stay on stderr.

### Keep the run log and the answer of every failure inside run
Let `pipeline.run` write the run log from the logger `cli_api.entry` gives it and answer every failure itself — a usage error as invalid, an error of the handler as raised (its kind, or none with its message), and an action that crashed outside its handler as `Error: internal error` with no kind (exit code `1`), the crash logged as "cli action crashed" with its stack — as [fixed-middleware-inside-the-entry](library/architecture/spec/base-plateau-extension.skill/adr/fixed-middleware-inside-the-entry.md) requires; the command line takes no list of middleware.

```python
# api/cli/pipeline/cli.py, run
except Exception as crash:
    logger.error("cli action crashed", exc_info=crash)
    error = InternalError()
```

- Violation: an exception of an action that reaches the interpreter; a `try` written in a resource's module.
- Risk: an exception that escapes `run` reaches `run_all`, which leaves the error of an entry that reports its own errors unprinted: the person sees no `Error:` line, and the run leaves no answered record.
- Fix: the copied `cli.py`; the scenario "An action that crashes is answered as a failure with no kind and logged". A step on every action is a command middleware ([base-plateau-extension-in-python](library/architecture/python/base-plateau-extension-in-python.skill/base-plateau-extension-in-python.skill.md)).

### Cover the entry point with scenarios
Copy the example's `api/cli/pipeline/features/` and `test/`: they cover the fixed code through a test resource of their own — the output formats, the kinds, a crash, every usage error, help at every level, `--version`, and the run log. In `api/cli/endpoint/` write one feature per resource, running `cli_api.run` with `Endpoints` over faked handlers and checking the command the handler received and the JSON and text output; tag them `@type/api/cli`.
- Risk: a resource feature that repeats the usage errors grows with every resource and tests nothing new.
- Fix: a resource feature checks only its own flags and output, with a fake class per command recording what it received; `make test` passes.

## SHOULD

### Offer a new command in its resource's module
Offer a further command of an existing resource by adding its handler, its line of `from_handlers`, its action line, and its `bind`, output type, `encode`, and `text` to the resource's module, and its scenario to the resource's feature; the command itself joins the command registry; `main.py` does not change. A new resource adds its module and feature, one field of `Endpoints`, and one line each of `from_handlers` and `resources()`.

# Check list
- [ ] `api/cli/pipeline/` is a filled copy of the templates; no placeholder is left; `click==8.5.0` is a dependency.
- [ ] Each resource has its module in `endpoint/` with its `{Resource}Handler`, `from_handlers`, and `resource()`, its actions built by `command()`, and per action its `bind`, output type, `encode`, and `text`; `commands.py` holds only `Endpoints`, `resources`, `entry`, and `run`.
- [ ] The `# [entries]` list of `main.py` has `cli_api.entry(handlers, ..., sys.argv[1:], ...)`; `api/startup/` is deleted.
- [ ] `make test` and `make lint` pass; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
One call of the example, each part with its class and file:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the base plateau's to-do planner as a command line: `make run`, `make test`, `.venv/bin/todo --help`.
