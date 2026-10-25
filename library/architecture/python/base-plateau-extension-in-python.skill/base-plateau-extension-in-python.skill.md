---
name: base-plateau-extension-in-python
description: How a variation point extends the base plateau in Python — command decorators as async functions of the Decorator protocol in the list main.py gives build_handlers, wrapped around every typed handler by decorate, marks as runtime-checkable protocols a command satisfies, the HTTP entry factory http_api.entry with its list of middlewares, rejections as KindError, the variation point's own settings and packages, and attach.allow — with a sample access variation point attached to the HTTP example
whenToUse: when you write, attach, or review a variation point of a Python application built on the base plateau, when you add a decorator around Python commands or around HTTP requests, or when you add a command to such an application
updated: 20261025
tags:
  - stack/python
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **The extension points in Python** - A Python application that keeps every property of [base-plateau-extension](library/architecture/spec/base-plateau-extension.skill/base-plateau-extension.skill.md); this skill details that skill's implementation for Python and restates none of its rules.
- **A sample to copy from** - [examples/](./examples/README.md) attaches a sample access variation point to the HTTP example of [vp-http-api-in-python](library/architecture/python/vp-http-api-in-python.skill/vp-http-api-in-python.skill.md): one entry decorator and two command decorators, with the attach check proving that only the composition root, the marked command, and the readme and Makefile changed.

# Core Principle
- **Typed handlers, untyped decorators** - A decorator sees the command as `object`, so decorators of different steps stand in one list; `decorate` — the one place with casts — turns the list back into a typed `Handler[TCommand, TResult]`, so `mypy --strict` still checks every entry point ([command registry](library/architecture/python/base-plateau-in-python.skill/glossary/command-registry.md)).
- **A mark is a protocol** - A command carries a mark by having the protocol's method; the decorator asks `isinstance(command, Restricted)` of a `@runtime_checkable` protocol, and the command module imports nothing of the variation point.

```
src/{package}/main.py
  # [services]  access_settings = access.load_config()               the variation point's own settings
  # [handlers]  handlers = build_handlers([                          command/handlers.py
                    with_logging(logger),                            every command      (base plateau)
                    access.with_access_check(),                      by the mark        access.Restricted
                    access.with_batch_limit(access_settings.max_batch),  by a value     access.Batch
                ])
  # [entries]   await run_all(logger, stopping,
                    http_api.entry(handlers, logger, [
                        http_access.authenticate(access_settings.tokens),  entry decorator (inside the request log)
                    ]),
                )
```

# Rule

## MUST

### Register every command in build_handlers
Give every command a field of `Handlers` and a line of `build_handlers` in `command/handlers.py`, the handler built with its services and wrapped by `decorate` with the command's name and the decorators.

```python
# command/handlers.py
@dataclass(frozen=True)
class Handlers:
    plan_items: Handler[PlanItemsCommand, PlanItemsResult]


def build_handlers(decorators: Sequence[Decorator]) -> Handlers:
    return Handlers(
        plan_items=decorate("PlanItems", PlanItemsHandler(), decorators),
    )
```

- Violation: a handler wrapped in `main.py`; an entry point that constructs `PlanItemsHandler()`.
- Risk: a command built outside the registry misses the decorators the others have.
- Fix: the services a handler uses are keyword parameters of `build_handlers` after the decorators, passed by `main.py` from `# [services]`; an API's resource takes its handlers in `{Resource}Handler.from_handlers(handlers)`.

### Write a command decorator as a Decorator
Write a command decorator as a function that takes what the step needs and returns an async function `(name, command, call_next) -> object`; let it await `call_next(command)` once with the command unchanged, or raise a `KindError` subclass declared beside it to reject the command.

```python
# by the command's mark: command/access/access_check.py
@runtime_checkable
class Restricted(Protocol):
    def required_role(self) -> str: ...


class MissingRoleError(KindError):
    def __init__(self, name: str, role: str) -> None:
        super().__init__(Kind.PERMISSION_DENIED, f"{name} needs the role {role}")


def with_access_check() -> Decorator:
    async def check_access(name: str, command: object, call_next: Next) -> object:
        if not isinstance(command, Restricted):
            return await call_next(command)  # not marked: passes unchanged
        identity = identity_of()
        if identity is None:
            raise NoCallerError(name)
        if not identity.has_role(command.required_role()):
            raise MissingRoleError(name, command.required_role())
        return await call_next(command)

    return check_access


# by a value of the command: command/access/batch_limit.py
def with_batch_limit(limit: int) -> Decorator:
    async def limit_batch(name: str, command: object, call_next: Next) -> object:
        if isinstance(command, Batch) and command.batch_size() > limit:
            raise BatchTooLargeError(name, limit, command.batch_size())
        return await call_next(command)

    return limit_batch
```

- Violation: a generic `with_{step}(next_handler: Handler[TCommand, TResult]) -> Handler[...]` wrapping one handler; `if isinstance(command, PlanItemsCommand)`; `if name == "PlanItems"`.
- Risk: a decorator that wraps one handler cannot stand in the list; one that names the commands misses the next command that needs it.
- Fix: one function per step returning a `Decorator`; the simple form — every command — is the base plateau's [with_logging](library/architecture/python/base-plateau-in-python.skill/templates/src/{package}/command/pipeline/logging.py).

### Mark a command with a method in its module
Give a command a mark by writing the protocol's method in the command's dataclass, with a docstring naming the mark.

```python
# command/plan_items.py
@dataclass(frozen=True)
class PlanItemsCommand:
    titles: list[str]

    def required_role(self) -> str:
        """Marks PlanItemsCommand access.Restricted: only a planner may plan."""
        return "planner"

    def batch_size(self) -> int:
        """Marks PlanItemsCommand access.Batch: its size is the number of titles."""
        return len(self.titles)
```

- Violation: the protocol without `@runtime_checkable`; the mark as a class attribute the protocol does not declare.
- Risk: `isinstance` against a protocol that is not runtime-checkable raises `TypeError` on the first call; a mark the protocol does not describe is never seen, and the command is served to everyone.
- Fix: methods only, declared by a `@runtime_checkable` protocol of the variation point.

### List the decorators in main.py
Give `build_handlers` the list in `# [handlers]` of `main.py`, `with_logging(logger)` first, so every rejection of a decorator after it is logged as `command failed`.

```python
# [handlers] - the handler of every command, built by the command registry
# and wrapped by these decorators, the first the outermost.
handlers = build_handlers(
    [
        with_logging(logger),
        access.with_access_check(),  # by the command's mark: access.Restricted
        access.with_batch_limit(access_settings.max_batch),  # by a value of the command: access.Batch
    ]
)
```

- Risk: a decorator placed before the logging rejects calls that leave no trace in the log.
- Fix: insert the variation point's decorator at the place its skill names; a check that is cheaper than the next one goes first.

### Give the HTTP API's decorators to its entry
Give the HTTP entry decorators as the last argument of `http_api.entry` in `# [entries]`: a list of `Middleware`, async functions `(request, call_next) -> Response` of the shape of FastAPI's `@app.middleware("http")`; let a middleware reject by raising a `KindError` and pass a value to the command layer through a `ContextVar` set around `call_next`.

```python
# api/http/access/authenticate.py
def authenticate(tokens: dict[str, Identity]) -> Middleware:
    async def identify(request: Request, call_next: CallNext) -> Response:
        header = request.headers.get("authorization")
        if header is None:
            return await call_next(request)  # no credentials: the command decides
        scheme, _, token = header.partition(" ")
        identity = tokens.get(token)
        if scheme != "Bearer" or identity is None:
            raise UnknownCallerError()
        with caller(identity):  # the ContextVar the access check reads
            return await call_next(request)

    return identify


# main.py, [entries]
http_api.entry(
    handlers,
    logger,
    [
        http_access.authenticate(access_settings.tokens),
    ],
),
```

- Violation: `JSONResponse(status_code=401, ...)` returned by a middleware; `app.add_middleware` for an entry decorator; a dependency on the resources' routers.
- Risk: the rejection is answered outside `google.rpc.Status`, or the docs need a token.
- Fix: the HTTP server runs only the requests of contract routes through the list, inside the request log and `AnswerFailures` ([vp-http-api-in-python](library/architecture/python/vp-http-api-in-python.skill/vp-http-api-in-python.skill.md)).

### Put each decorator in its own module of its own package
Put a variation point's command decorators in `command/{vp}/{step}.py` with the marks they select by and the errors they raise, and its entry decorators in `api/{transport}/{vp}/{step}.py`; keep `command/pipeline` for the base plateau's.
- Violation: `with_access_check` added to `command/pipeline/logging.py`; a middleware in `api/http/pipeline`, which is copied from templates.
- Risk: a variation point edits a base file or a template, and two variation points edit the same one.
- Fix: the sample's `command/access` and `api/http/access`, each re-exporting its names from its `__init__.py`.

### Read the settings in the variation point's package
Read a variation point's settings with `{vp}.load_config()` in its own package, called at the top of `# [services]`; a `ValueError` stops the start with `startup failed` and exit code `1`.

```python
# [services]
try:
    access_settings = access.load_config()  # ACCESS_TOKENS, ACCESS_MAX_BATCH
except ValueError as error:
    logger.error("startup failed", exc_info=error)
    return 1
```

- Risk: a field in `config` belongs to the base plateau and stays when the variation point goes.
- Fix: the error names its variable (`access config: ACCESS_MAX_BATCH '0' is not a positive number`).

### Name every changed base file in attach.allow
Give the variation point's example an `attach.allow` beside its `examples/`, naming its base example and every base file it changes or deletes.

```
# skill folder / attach.allow
base library/architecture/python/vp-http-api-in-python.skill/examples
change src/todo/main.py
change src/todo/command/plan_items.py
change Makefile
change README.md
```

- Risk: a change outside the list is a hidden dependency of the variation point.
- Fix: `bash test/check-vp-attach.sh` in this repository passes.

# Check list
- [ ] Every command is a field and a line of `command/handlers.py`; `main.py` calls `build_handlers` once.
- [ ] Every command decorator returns a `Decorator`, awaits `call_next` with the command unchanged, and rejects with a `KindError` subclass.
- [ ] A filtered decorator selects by a `@runtime_checkable` protocol; the marked command has the method in its own module.
- [ ] `with_logging(logger)` is the first decorator of the list.
- [ ] `http_api.entry` gets the list of middlewares; a middleware rejects by raising a `KindError`.
- [ ] The variation point's decorators and settings live in its own packages.
- [ ] `attach.allow` names every changed base file; `bash test/check-vp-attach.sh`, `make test`, and `make lint` pass.

# Example
- [examples/](./examples/README.md) — the HTTP example with the sample access variation point attached: `make init`, `make run`, then the `curl` calls of its readme; `make test`, `make lint`.
