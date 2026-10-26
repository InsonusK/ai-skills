---
name: base-plateau-extension-in-python
description: How a variation point extends the base plateau in Python — every middleware a package of its own under command/middleware/ or api/{kind}/middleware/ with a {Name}Middleware class, the command middleware of the Middleware protocol in the list main.py gives build_handlers, wrapped around every typed handler by decorate, marks as runtime-checkable protocols a command satisfies, the HTTP entry factory http_api.entry with its list of API middleware inside the fixed RequestLogMiddleware and AnswerFailuresMiddleware, rejections as KindError, the variation point's own settings, and attach.allow — with a sample access variation point attached to the HTTP example
whenToUse: when you write, attach, or review a variation point of a Python application built on the base plateau, when you add a middleware around Python commands or around HTTP requests, or when you add a command to such an application
updated: 20261026
tags:
  - stack/python
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **The extension points in Python** - A Python application that keeps every property of [base-plateau-extension](library/architecture/spec/base-plateau-extension.skill/base-plateau-extension.skill.md); this skill details that skill's implementation for Python and restates none of its rules.
- **A sample to copy from** - [examples/](./examples/README.md) attaches a sample access variation point to the HTTP example of [vp-http-api-in-python](library/architecture/python/vp-http-api-in-python.skill/vp-http-api-in-python.skill.md): one API middleware and two command middleware, with the attach check proving that only the composition root, the marked command, and the readme and Makefile changed.

# Core Principle
- **One package per middleware** - Each middleware is a package `middleware/{name}/` holding `middleware.py` with its `{Name}Middleware` class, re-exported by the package's `__init__.py`, with its `features/` and `test/` beside it.
- **Typed handlers, untyped middleware** - A command middleware sees the command as `object`, so middleware of different steps stand in one list; `decorate` — the one place with casts — turns the list back into a typed `Handler[TCommand, TResult]`, so `mypy --strict` still checks every entry point ([command registry](library/architecture/python/base-plateau-in-python.skill/glossary/command-registry.md)).
- **A mark is a protocol** - A command carries a mark by having the protocol's method; the middleware asks `isinstance(command, Restricted)` of a `@runtime_checkable` protocol, and the command module imports nothing of the variation point.

```
src/{package}/command/                    src/{package}/api/http/
├── pipeline/      Handler, Middleware,   ├── pipeline/       Server, errors, docs, config
│                  decorate, kinds        ├── middleware/
├── middleware/                           │   ├── request_log/     fixed, outermost
│   ├── logging/       base plateau       │   ├── answer_failures/ fixed: status_response, failures
│   ├── access/        sample VP          │   └── authentication/  sample VP
│   └── batch_limit/   sample VP          └── routes.py       entry(handlers, logger, middlewares)
└── handlers.py    build_handlers(middlewares, *, services...)

src/{package}/main.py
  # [services]  access_settings = load_access_config()              the variation point's own settings
  # [handlers]  build_handlers([
                    LoggingMiddleware(logger),                       every command      (base plateau)
                    AccessMiddleware(),                              by the mark        Restricted
                    BatchLimitMiddleware(access_settings.max_batch), by a value         Batch
                ])
  # [entries]   http_api.entry(handlers, logger, [
                    AuthenticationMiddleware(access_settings.tokens),  inside the request log and answer failures
                ])
```

# Rule

## MUST

### Register every command in build_handlers
Give every command a field of `Handlers` and a line of `build_handlers` in `command/handlers.py`, the handler built with its services and wrapped by `decorate` with the command's name and the middleware.

```python
# command/handlers.py
@dataclass(frozen=True)
class Handlers:
    plan_items: Handler[PlanItemsCommand, PlanItemsResult]


def build_handlers(middlewares: Sequence[Middleware]) -> Handlers:
    return Handlers(
        plan_items=decorate("PlanItems", PlanItemsHandler(), middlewares),
    )
```

- Violation: a handler wrapped in `main.py`; an entry point that constructs `PlanItemsHandler()`.
- Risk: a command built outside the registry misses the middleware the others have.
- Fix: the services a handler uses are keyword parameters of `build_handlers` after the middleware, passed by `main.py` from `# [services]`; an API's resource takes its handlers in `{Resource}Handler.from_handlers(handlers)`.

### Pass a nested command's decorated handler
Give a handler that runs another command a constructor parameter `Handler[{Nested}Command, {Nested}Result]`, and in `build_handlers` decorate the nested handler first and pass that value, never `{Nested}Handler(...)`.

```python
def build_handlers(middlewares: Sequence[Middleware], *, lists: ItemLists) -> Handlers:
    add_item = decorate("AddItem", AddItemHandler(lists), middlewares)
    return Handlers(
        add_item=add_item,
        import_items=decorate("ImportItems", ImportItemsHandler(add_item), middlewares),
    )
```

- Violation: `ImportItemsHandler(AddItemHandler(lists))`; a handler that takes `Handlers` and picks its nested handler itself.
- Risk: an undecorated nested call skips every command middleware; a handler holding the whole registry hides which commands it runs.
- Fix: one parameter per nested command, the same decorated value the field of `Handlers` holds.

### One package per middleware
Put each command middleware in `command/middleware/{name}/` and each API middleware in `api/{kind}/middleware/{name}/`: `middleware.py` with its `{Name}Middleware` class, the marks and errors it uses beside it, an `__init__.py` re-exporting them, and its `features/` and `test/`.
- Violation: `AccessMiddleware` added to `command/pipeline`; a middleware in `api/http/pipeline`, which is copied from templates; two middleware in one package.
- Risk: a variation point edits a base file or a template, and a reader looking for a step finds it under another name.
- Fix: the sample's `command/middleware/access`, `command/middleware/batch_limit`, and `api/http/middleware/authentication`.

### Write a command middleware as a Middleware
Write a command middleware as a class whose constructor takes what the step needs and whose `async __call__(self, name, command, call_next)` awaits `call_next(command)` once with the command unchanged, or raises a `KindError` subclass declared beside it to reject the command.

```python
# by the command's mark: command/middleware/access/middleware.py
@runtime_checkable
class Restricted(Protocol):
    def required_role(self) -> str: ...


class MissingRoleError(KindError):
    def __init__(self, name: str, role: str) -> None:
        super().__init__(Kind.PERMISSION_DENIED, f"{name} needs the role {role}")


class AccessMiddleware:
    async def __call__(self, name: str, command: object, call_next: Next) -> object:
        if not isinstance(command, Restricted):
            return await call_next(command)  # not marked: passes unchanged
        identity = identity_of()
        if identity is None:
            raise NoCallerError(name)
        if not identity.has_role(command.required_role()):
            raise MissingRoleError(name, command.required_role())
        return await call_next(command)


# by a value of the command: command/middleware/batch_limit/middleware.py
class BatchLimitMiddleware:
    def __init__(self, limit: int) -> None:
        self._limit = limit

    async def __call__(self, name: str, command: object, call_next: Next) -> object:
        if isinstance(command, Batch) and command.batch_size() > self._limit:
            raise BatchTooLargeError(name, self._limit, command.batch_size())
        return await call_next(command)
```

- Violation: a generic middleware `(next_handler: Handler[TCommand, TResult]) -> Handler[...]` wrapping one handler; `if isinstance(command, PlanItemsCommand)`; `if name == "PlanItems"`.
- Risk: a middleware that wraps one handler cannot stand in the list; one that names the commands misses the next command that needs it.
- Fix: the simple form — every command — is the base plateau's [LoggingMiddleware](library/architecture/python/base-plateau-in-python.skill/templates/src/{package}/command/middleware/logging/middleware.py).

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
        """Marks PlanItemsCommand batch_limit.Batch: its size is the number of titles."""
        return len(self.titles)
```

- Violation: the protocol without `@runtime_checkable`; the mark as a class attribute the protocol does not declare.
- Risk: `isinstance` against a protocol that is not runtime-checkable raises `TypeError` on the first call; a mark the protocol does not describe is never seen, and the command is served to everyone.
- Fix: methods only, declared by a `@runtime_checkable` protocol of the middleware's package.

### List the command middleware in main.py
Give `build_handlers` the list in `# [handlers]` of `main.py`, `LoggingMiddleware(logger)` first, so every rejection of a middleware after it is logged as `command failed`.

```python
# [handlers] - the handler of every command, built by the command registry
# and wrapped by this command middleware, the first the outermost.
handlers = build_handlers(
    [
        LoggingMiddleware(logger),
        AccessMiddleware(),  # by the command's mark: Restricted
        BatchLimitMiddleware(access_settings.max_batch),  # by a value of the command: Batch
    ]
)
```

- Risk: a middleware placed before the logging rejects calls that leave no trace in the log.
- Fix: insert the variation point's middleware at the place its skill names; a check that is cheaper than the next one goes first.

### Give the HTTP API's middleware to its entry
Give the HTTP API middleware as the last argument of `http_api.entry` in `# [entries]`: a list of `Middleware`, callables `(request, call_next) -> Response` of the shape of FastAPI's `@app.middleware("http")`, here `{Name}Middleware` instances; let a middleware reject by raising a `KindError` and pass a value to the command layer through a `ContextVar` set around `call_next`.

```python
# api/http/middleware/authentication/middleware.py
class AuthenticationMiddleware:
    def __init__(self, tokens: dict[str, Identity]) -> None:
        self._tokens = tokens

    async def __call__(self, request: Request, call_next: CallNext) -> Response:
        header = request.headers.get("authorization")
        if header is None:
            return await call_next(request)  # no credentials: the command decides
        scheme, _, token = header.partition(" ")
        identity = self._tokens.get(token)
        if scheme != "Bearer" or identity is None:
            raise UnknownCallerError()
        with caller(identity):  # the ContextVar the access check reads
            return await call_next(request)


# main.py, [entries]
http_api.entry(
    handlers,
    logger,
    [
        AuthenticationMiddleware(access_settings.tokens),
    ],
),
```

- Violation: `JSONResponse(status_code=401, ...)` returned by a middleware; `app.add_middleware` for an application's middleware; `RequestLogMiddleware` given in the list.
- Risk: the rejection is answered outside `google.rpc.Status`, or the docs need a token; a fixed middleware given twice logs every request twice.
- Fix: the HTTP server adds `RequestLogMiddleware` outermost and `AnswerFailuresMiddleware` inside it, and runs only the requests of contract routes through the list ([vp-http-api-in-python](library/architecture/python/vp-http-api-in-python.skill/vp-http-api-in-python.skill.md)).

### Read the settings in the variation point's package
Read a variation point's settings with `load_config()` of one of its own packages, called at the top of `# [services]`; a `ValueError` stops the start with `startup failed` and exit code `1`.

```python
# [services]
try:
    access_settings = load_access_config()  # ACCESS_TOKENS, ACCESS_MAX_BATCH
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
- [ ] A handler that runs another command takes that command's decorated handler, built first in `build_handlers`.
- [ ] Every middleware is a package of its own under `command/middleware/` or `api/{kind}/middleware/` with its `{Name}Middleware`.
- [ ] Every command middleware awaits `call_next` with the command unchanged and rejects with a `KindError` subclass.
- [ ] A filtered middleware selects by a `@runtime_checkable` protocol; the marked command has the method in its own module.
- [ ] `LoggingMiddleware(logger)` is the first of the command middleware.
- [ ] `http_api.entry` gets the list of API middleware; a middleware rejects by raising a `KindError`; no fixed middleware is in the list.
- [ ] `attach.allow` names every changed base file; `bash test/check-vp-attach.sh`, `make test`, and `make lint` pass.

# Example
- [examples/](./examples/README.md) — the HTTP example with the sample access variation point attached: `make init`, `make run`, then the `curl` calls of its readme; `make test`, `make lint`.
