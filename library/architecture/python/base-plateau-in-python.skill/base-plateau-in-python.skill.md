---
name: base-plateau-in-python
description: The base plateau in Python — the package layout of its four layers under src/{package}/, the async Handler protocol, the Middleware list with decorate, the command registry build_handlers, the error kinds, the startup entry, the runner of the entry points with the exit codes, the stdlib logger writing one JSON line per record, the composition root with its three marked sections, and the pytest-bdd scenarios that cover each layer
whenToUse: when you create a new Python service or Python CLI application from the architecture catalog, or when you attach a variation point to, or review, a Python application built on the base plateau
updated: 20261025
tags:
  - stack/python
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/async-handlers.md
  - adr/stdlib-logging.md
---

# Goal
- **The base plateau in Python** - A Python package that keeps every property of [base-plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md); this skill details that skill's implementation for Python and restates none of its rules.
- **Same shape in every Python application** - The [handler contract](./glossary/handler-contract.md), the [command registry](./glossary/command-registry.md), the [runner of the entry points](./glossary/entry-runner.md), the [logging](./glossary/logging.md), and the composition-root sections are identical across Python applications, so a variation point's instructions name the same module and the same section everywhere — how a variation point uses them is [base-plateau-extension-in-python](library/architecture/python/base-plateau-extension-in-python.skill/base-plateau-extension-in-python.skill.md).

# Core Principle
- **Layers per app-layers-in-python** - Packages, protocols, entities, and fakes follow [app-layers-in-python](library/develop/python/app-layers-in-python.skill.md); this skill adds only what the base plateau itself contains.
- **Async from the entry to the handler** - Every handler is `async def handle`; an entry point runs as an asyncio task and stops when the event it is given is set ([ADR](./adr/async-handlers.md)).
- **Delivered, not retyped** - The fixed code is copied from templates; only the domain, the commands, the command registry, and the composition-root lines are written per application.

```
src/{package}/
├── main.py                        composition root: run(stdout, stderr, stopping) -> exit code
├── __main__.py                    python -m {package}
├── config/                        LOG_LEVEL
├── log/                           copied: attrs(), JsonFormatter, new_logger, request id
├── api/startup/                   copied: run(handler, command, out), env_list
├── api/core/entry/                copied: Entry, run_all, exit_code
├── command/                       one module per command: command, result, handler
│   └── handlers.py                the command registry: Handlers, build_handlers
├── command/pipeline/              copied: Handler, HandlerFunc, Middleware, decorate, the error kinds
├── command/middleware/logging/    copied: LoggingMiddleware - one package per command middleware
└── domain/entity, domain/service  entities, kinds.py and the rule errors; domain services
```

# Rule

## MUST

### Base packages
Create the packages [app-layers-in-python](library/develop/python/app-layers-in-python.skill.md) assigns to the api, command, and domain layers, with the startup entry as the only entry point and the base settings in their own package.

| Part | Package |
|---|---|
| startup entry | `api/startup` |
| [runner of the entry points](./glossary/entry-runner.md): `Entry`, `run_all`, `exit_code` | `api/core/entry` |
| commands — one module each | `command` |
| [command registry](./glossary/command-registry.md): `Handlers`, `build_handlers` | `command/handlers.py` |
| [handler contract](./glossary/handler-contract.md), `HandlerFunc`, `Middleware` with `decorate`, the error kinds re-exported | `command/pipeline` |
| command middleware, one package each: `LoggingMiddleware` in the base plateau | `command/middleware/{name}` |
| entities, the error kinds (`kinds.py`) and the rule errors | `domain/entity` |
| domain services | `domain/service` |
| composition root | `main.py` |
| base settings (`LOG_LEVEL` only) | `config` |
| [logging](./glossary/logging.md): `attrs`, `JsonFormatter`, `new_logger`, the request id | `log` |
| version of the build | `importlib.metadata.version` in `main.py`, no module of its own, per [project-version-in-python](library/develop/python/project-version-in-python.skill.md) |

- Risk: a part in a package of its own choosing breaks every variation point's instruction that names the module to edit.
- Fix: move the code to the package in the table.

### Copy the fixed code from templates
Copy [command/pipeline/](./templates/src/{package}/command/pipeline/), [command/middleware/](./templates/src/{package}/command/middleware/), [api/startup/](./templates/src/{package}/api/startup/), [api/core/entry/](./templates/src/{package}/api/core/entry/), and [log/](./templates/src/{package}/log/) to the same paths under `src/{package}/`, with the modules of [error-kinds-in-python](library/develop/python/error-kinds-in-python.skill/error-kinds-in-python.skill.md), replacing `{package}` with the import name of the package.
- Risk: a retyped contract drifts from the one every variation point's middleware and entry point is written against.
- Fix: copy the templates; after filling, searching the copied files for `{package}` returns nothing.

### One module per command
Write a command, its result types, and its handler in `command/{operation}.py` per [app-layers-in-python](library/develop/python/app-layers-in-python.skill.md), the handler satisfying `Handler[{Operation}Command, {Operation}Result]`, and register it with a field and a line of the [command registry](./glossary/command-registry.md).
- Risk: a handler whose method differs from the protocol cannot be wrapped or served without an adapter written for it alone.
- Fix: `async def handle(self, command)`; every service the handler uses is a constructor parameter typed by a protocol.

### Compose in the three marked sections
Build every concrete type in `run(stdout, stderr, stopping)` of `main.py` under the comments `# [services]`, `# [handlers]`, and `# [entries]`, in this order, with `main` only running it under `asyncio.run` with `stopping` set on `SIGINT`/`SIGTERM` and exiting with its code.
- Risk: a variation point whose instruction says "add after `# [handlers]`" finds no such line and guesses where its code goes.
- Fix: keep the three comments even when a section is empty; `# [handlers]` is one `build_handlers` call with the list of middleware, `LoggingMiddleware(logger)` first.

```python
# [handlers] - the handler of every command, built by the command registry
# and wrapped by this command middleware, the first the outermost.
handlers = build_handlers(
    [
        LoggingMiddleware(logger),
    ]
)
```

### Build the logger with new_logger
Build the one logger in `run` as `new_logger(stderr, settings.log_level)`, pass it to every middleware and entry point, and log every value as an attribute — `logger.info("command called", extra=attrs(command=name, payload=command))` — with the data of a call only under `payload` and `failed` on the record that ends it.
- Violation: `logging.getLogger(__name__)` in a module; `logging.basicConfig` in `main.py`; `logger.info(f"called with {command}")`.
- Risk: a record outside the composition root's logger escapes its handler, and a value rendered into the message escapes the base handler's drop and the logging variation point's mask.
- Fix: the logging variation point replaces only the formatter of this line ([ADR](./adr/stdlib-logging.md)).

### One Entry per entry point in [entries]
End `run` with one `run_all` call listing an `Entry(name, run)` per entry point, and return `exit_code(...)` of what it returned.

```python
# [entries] - every entry point, one Entry each, run together by run_all.
ended_by = await run_all(
    logger,
    stopping,
    Entry("startup", lambda _: startup.run(handlers.plan_items, PlanItemsCommand(titles=startup.env_list("STARTUP_TITLES")), stdout)),
)
return exit_code(ended_by)
```

- Violation: an entry point awaited outside `run_all`; an exit code computed in `main.py`.
- Risk: the next variation point's entry point is never started, or the process keeps running with one entry point dead.
- Fix: a variation point adds its `Entry` to the list (health) or replaces the startup `Entry` with its own (an API), built by its entry factory (`http_api.entry(handlers, logger, middlewares)`); `run_all` and `exit_code` stay as they are.

### Cover each layer with scenarios
Cover `domain/entity`, `domain/service`, `command`, `command/pipeline`, `command/middleware/logging`, `api/startup`, `api/core/entry`, `log`, and `config` with Cucumber scenarios per [cucumber-testing-in-python](library/testing/python/cucumber-testing-in-python.skill/cucumber-testing-in-python.skill.md), tagged `@type/domain`, `@type/service` (commands), `@type/tech-check` (middleware, settings, the runner, the logger), and `@type/api/cli` (startup entry), run by one `pytest`.
- Risk: a layer without scenarios lets a variation point break it unnoticed, since the variation point's example reruns the base scenarios as its regression check.
- Fix: one `features/` and `test/` pair per package; a step module runs a coroutine with `asyncio.run`; `make test` passes.

### Run the scenarios with make test
Give the example's `Makefile` the targets `init` (a `.venv` with `pip install -e '.[dev]'`), `run`, `test` (`pytest`), and `lint` (`mypy --strict`), and leave the testing contract (kinds, reports, coverage, mutation) to [solution-conformance-testing-in-python](library/testing/python/solution-conformance-testing-in-python.skill/solution-conformance-testing-in-python.skill.md), as base-plateau's "Examples show the build, not the testing contract" requires.
- Risk: copied testing tooling must be updated in every example whenever the testing skills change.
- Fix: `pyproject.toml` lists only `pytest`, `pytest-bdd`, and `mypy` as dev dependencies.

## SHOULD

### Name type parameters by their role
Name a type parameter after what it stands for (`TCommand`, `TResult`), never by a single letter, and document every public generic class and function with what each type parameter is and an example of it.
- Risk: `Handler[C, R]` forces a reader to open the definition to learn what `C` and `R` are, in exactly the shared code every variation point builds on.
- Fix: rename the parameter; add a docstring naming each parameter's role with a concrete type from the application.

# Check list
- [ ] Packages follow [Base packages](#base-packages) and [app-layers-in-python](library/develop/python/app-layers-in-python.skill.md).
- [ ] `command/pipeline`, `command/middleware/logging`, `api/startup`, `api/core/entry`, `log`, and the error-kinds modules are filled copies of the templates; none contains `{package}`.
- [ ] Every business-rule error is a `KindError` subclass; code outside the domain checks kinds only through `command.pipeline`.
- [ ] Every command lives in its own module in `command` with a handler matching `Handler[TCommand, TResult]`, and has a field and a line in `command/handlers.py`.
- [ ] `run` in `main.py` has the `# [services]`, `# [handlers]`, `# [entries]` sections in this order; `[handlers]` is one `build_handlers` call; `[entries]` is one `run_all` call returning `exit_code(...)`.
- [ ] Every record is written through the logger of `new_logger` with fixed text and `attrs(...)`.
- [ ] The example holds no testing tooling; `make test` and `make lint` pass.

# Example
- [examples/](./examples/README.md) — the base plateau as a runnable to-do planner: `make init`, `make run`, `make test`, `make lint`.
- [glossary/](./glossary/README.md) — how the handler contract, the middleware with the command registry, the runner of the entry points, and the logging work.
