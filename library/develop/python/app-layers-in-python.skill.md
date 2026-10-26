---
name: app-layers-in-python
description: The application layers in Python — one package per layer under src/{package}/, protocols declared by their consumer so that only main.py imports both sides, entities guarding their state behind properties and methods, and hand-written fakes for the layer below
whenToUse: when you design a Python application or feature, when you add or change a Python entry point, command, entity, domain service, service, or infrastructure adapter, or when you review such a change
updated: 20261026
tags:
  - stack/python
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
---

# Goal
- **Layers in Python packages** - A Python package that keeps every rule of [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md); this skill details that skill's implementation for Python and restates none of its rules.
- **Layers joined only in main** - A consumer never constructs what it calls through a protocol; `src/{package}/main.py` builds every implementation and passes it in.

# Core Principle
- **The consumer owns the protocol** - A `typing.Protocol` is declared where it is used and lists only the methods used there; the implementing class names no protocol, and the type checker matches it by its methods.
- **A package re-exports its names** - Each package's `__init__.py` imports the public names of its modules, so a caller writes `from {package}.command import PlanItemsCommand`, never the module inside.

# Rule

## MUST

### One package per layer
Place each layer of [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md) in its package under `src/{package}/`.

| Layer | Package |
|---|---|
| api | `api/{transport}` (`http`, `grpc`, `cli`, `kafka`, ...), split per [Split each entry point](#split-each-entry-point-into-pipeline-endpoint-and-registration) |
| api — the fixed code the entry points share | `api/core/{part}` (`entry`, `rpcstatus`), per [Keep the code the entry points share in core](#keep-the-code-the-entry-points-share-in-core) |
| command | `command` — one module per command, nothing else |
| command pipeline (handler contract, decorators, error kinds) | `command/pipeline` |
| command factory | `command/factory` |
| entity | `domain/entity` |
| domain service | `domain/service` |
| service | `service` |
| infrastructure | `infrastructure/{kind}` (`postgres`, `memory`, ...); a client of another service's API in `infrastructure/client/{api}` (`user_api`) |
| composition root | `main.py`, run by `__main__.py` and the `[project.scripts]` entry |

- Violation: entities and domain services in one `domain` module; a `utils` package holding a service.
- Risk: a layer in a package of its own choosing hides which rules apply to it, and an import check cannot tell a downward call from an upward one.
- Fix: move the code into the package of its layer.

### Split each entry point into pipeline, endpoint, and registration
Lay out every entry point `api/{kind}/` (`http`, `grpc`, `cli`, later `kafka`) in three parts.

```
api/{kind}/
├── pipeline/            the fixed code, copied from templates: settings, server or runner,
│   ├── ...              the generic adapter, and one module per wrapper around every call
│   └── logging.py
├── endpoint/            one module per resource or service: a {Resource}Handler class
└── routes.py            the registration: the endpoints, the list, run
                         (services.py for gRPC, commands.py for the CLI)
```

- Violation: a resource module in `pipeline/`; a wrapper added to the server module.
- Risk: application code mixed into the copied package is lost when the templates are copied again; a wrapper hidden in another module is not seen in the list of files.
- Fix: `{Resource}Handler` holds the command handlers of one resource and lists its operations; the registration module collects them and starts the entry point with `run`; `main.py` builds the endpoints and calls it.

### Keep the code the entry points share in core
Put the fixed code that serves several entry points and belongs to none of them — the runner of the entry points (`entry`), the table of error kinds (`rpcstatus`) — in its own package under `api/core/`, never in a transport's package or beside the transports.
- Risk: removing a transport removes code the other entry points still use.
- Fix: `core/` holds only code copied from templates; each part appears with the base plateau (`entry`) or with the first variation point that needs it (`rpcstatus`).

### Declare a protocol in the consumer's module
Declare the `Protocol` a command, factory, or domain service uses to reach a service in that consumer's module, and the protocol a service uses to reach its infrastructure in `service`, each listing only the methods that consumer calls.

```python
# command/add_user.py - the command declares what it needs
class Users(Protocol):
    async def exists(self, name: str) -> bool: ...
    async def add(self, name: str) -> UserId: ...


# service/user_service.py - a class; no protocol declared for itself
class UserService:
    def __init__(self, api: UserApi) -> None:  # UserApi: declared here, implemented in infrastructure/client/user_api
        self._api = api
```

- Violation: `service` declaring an `IUserService` abstract base class next to `UserService`, imported by commands.
- Risk: an interface declared by the implementation grows to every method any caller uses, and the caller's package must import the implementation's.
- Fix: one small protocol per consumer; `mypy --strict` checks that the class passed in `main.py` satisfies it.

### Wire implementations only in main
Import `service` only from `main.py`, and `infrastructure/{kind}` only from `main.py` and from the one service over it — for the types its methods return — and pass each implementation to its consumer's constructor in `main.py`.
- Violation: a command module importing `service` to construct a `UserService`, or a service constructing its HTTP client itself.
- Risk: the consumer depends on the concrete implementation, so it cannot be tested with a fake or given another implementation without editing it.
- Fix: the consumer's constructor takes the protocol; `main.py` builds the class and passes it.

### Entities guard their state
Give an entity private attributes (`_title`) listed in `__slots__`, read-only properties for what results must show, a constructor that raises the rule error of an invalid entity, and a method for each allowed state change that raises its rule error.
- Violation: a `@dataclass` entity with public fields; a property with a setter.
- Risk: a public attribute can be set past every rule, from any module.
- Fix: a property without a setter; the change goes through a method.

### Commands and results are frozen dataclasses
Declare a command and its result types as `@dataclass(frozen=True)` with plain fields, and its handler as a class with an `async def handle(self, command: {Operation}Command) -> {Operation}Result` method.
- Risk: a mutable command can be changed by a decorator or an entry point after it was logged; a handler with another method shape cannot be wrapped or served.
- Fix: the handler matches the `Handler` protocol of the command pipeline; every service it uses is a constructor parameter typed by a protocol.

### Fake the layer below by hand
Write the fake of the layer below as a small class in the test folder that satisfies the consumer's protocol, recording calls and returning what the scenario sets.
- Violation: `unittest.mock.MagicMock()` passed as a service; a command test that starts the real service with a stub server behind it.
- Risk: a mock accepts any call and any name, so a renamed method passes the test and fails in production; two layers' contracts in one test break on a change below the service.
- Fix: `class FakeUsers` with `exists` and `add` and the list of added names, typed against the command's `Users` protocol.

### Type-check strictly
Check `src/` — the tests included — with `mypy --strict` in the `lint` target.
- Risk: a protocol is satisfied only by its methods' names and types; without the check a handler of the wrong shape, or a fake that drifted from its protocol, fails only at run time.
- Fix: annotate every function; a `type: ignore` names its error code and says why.

# Check list
- [ ] Every layer is in its package per [One package per layer](#one-package-per-layer); the code the entry points share is in `api/core/`.
- [ ] Every protocol to a service or an infrastructure adapter is declared in its consumer's module and lists only the methods that consumer calls.
- [ ] `service` is imported only by `main.py`; `infrastructure/{kind}` only by `main.py` and the one service over it.
- [ ] Entities keep private slotted attributes, read-only properties, a validating constructor, and guarded state-change methods.
- [ ] Commands and results are frozen dataclasses; tests fake the layer below with hand-written classes.
- [ ] `mypy --strict` passes over `src/`.
