---
name: vp-http-client-in-python
description: The HTTP client variation point in Python — infrastructure/client/core copied from templates (outcomes.py with ApiUnavailableError, ApiStatusError, and StatusCode for every transport; httpclient with HttpConfig, load_config, and HttpClient.call on httpx2 with the timeout, JSON in and out, the call log, the request id, and the mock through MockTransport), one client per API in infrastructure/client/{api} with its operations, its mock, and its contract scenario, the service over it in service/, and the lines of main.py that build them
whenToUse: when a Python application built on the base plateau must call another service's HTTP API, when you call a further operation of such an API, or when you review a Python HTTP client
updated: 20261031
adr:
  - adr/httpx2-and-one-outcome-module.md
tags:
  - stack/python
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **The HTTP client variation point in Python** - A Python application that keeps every property of [vp-http-client](library/architecture/spec/vp-http-client.skill/vp-http-client.skill.md) and [api-client](library/develop/core/api-client.skill/api-client.skill.md), in the packages of [app-layers-in-python](library/develop/python/app-layers-in-python.skill.md); this skill details their implementation for Python and restates none of their rules.
- **Attached by the listed changes only** - Attaching it adds packages and files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-python](library/architecture/python/base-plateau-in-python.skill/base-plateau-in-python.skill.md)'s example.

# Core Principle
- **Copied once, written per operation** - `infrastructure/client/core/` is copied from templates and never edited; per API the application writes a client package whose operations are calls of `HttpClient.call`, the client's emulator, and the service over it.
- **The service sees outcomes, never a transport** - The service catches `ApiUnavailableError` and `ApiStatusError` of `core/outcomes.py`, the same for an HTTP and a gRPC client. Decision recorded in [httpx2-and-one-outcome-module](./adr/httpx2-and-one-outcome-module.md).

```
docs/integration/client/{api-name}/openapi.json   the provider's contract, copied unchanged
src/{package}/infrastructure/
├── client/                  every client of another service's API, and nothing else
│   ├── core/                copied from templates, never edited
│   │   ├── pipeline/        the client middleware around the attempts of a call - every transport
│   ├── outcomes.py      ApiUnavailableError, ApiStatusError, StatusCode (google.rpc.Code) - every transport
│   │   └── httpclient/      config.py: load_config(prefix): {API}_HOST, _HTTP_PORT, _TLS, _TIMEOUT; MOCK
│   │                        client.py: HttpClient.call - timeout, JSON, X-Request-Id, call log, outcomes
│   │                        features/ test/ - the fixed code's scenarios, with a test API of their own
│   └── {api}/               written per API (todo_api)
│       ├── {api}.py         PREFIX, OPERATIONS, the client with one method per operation, new(logger, middlewares)
│       ├── mock/{api}_mock.py   emulator(request) -> response: every operation with stable data ({API}_HOST=mock)
│       └── features/ test/  each operation against a server of the scenario + the contract scenario
└── {store}/                 a store (memory, postgres): beside client/, never in it
src/{package}/service/       the service over the client, its protocol, and its errors: outcomes -> kinds
```

# Rule

## MUST

### Copy the client code from templates
Copy [infrastructure/client/core/](./templates/src/{package}/infrastructure/client/core/) to `src/{package}/infrastructure/client/core/`, with the example's `features/` and `test/` of `httpclient/` and `pipeline/`, replacing `{package}` with the import name of the package, and add `httpx2==2.16.0` to the dependencies of `pyproject.toml`.
- Risk: a retyped client drifts in its timeout, its outcomes, or its log from every other Python client of the catalog.
- Fix: copy the files; after filling, searching them for `{package}` returns nothing; `make test` and `make lint` pass.

### One client package per API
Write each API as `src/{package}/infrastructure/client/{api}/` holding `PREFIX` (`"TODO_API"`), `OPERATIONS` (an `Operation` - method, path, and `retryable` - for every operation it calls), `new(logger, middlewares)` reading the settings, a client class taking `HttpConfig` with `aclose()`, and per operation one method that calls `HttpClient.call` with a private request dataclass and a private function reading the answer into the client's own dataclasses.

```python
async def plan_items(self, titles: list[str]) -> list[Item]:
    """POST /v1/items/plan: one open item per title, planned by the API."""
    return await self._http.call("POST", "/v1/items/plan", _PlanItemsRequest(titles=titles), _plan_items_response)
```

- Violation: `httpx2` used in the client beside `call`; the client raising a `KindError`; an operation missing from `OPERATIONS`.
- Risk: a call that bypasses `call` has no timeout, no log, and no request id; the contract scenario misses the operation.
- Fix: follow the example's [todo_api.py](./examples/src/todo/infrastructure/client/todo_api/todo_api.py); a new operation adds its method, its request and its reading, its line of `OPERATIONS`, and its route in [todo_api_mock.py](./examples/src/todo/infrastructure/client/todo_api/mock/todo_api_mock.py).

### Answer every operation in the mock
Write the mock of each client as `src/{package}/infrastructure/client/{api}/mock/{api}_mock.py` with `emulator(request: httpx2.Request) -> httpx2.Response` answering every operation of `OPERATIONS` as the API does, with stable data and no state, and pass it to `HttpClient`; `{API}_HOST=mock` runs the client against it through `httpx2.MockTransport`. The file's name says it is a mock and of which client.
- Risk: an operation the emulator does not route answers 404 under `mock`, and the service cannot run without the other service.
- Fix: the mocked scenario of the client's feature runs it and checks its answer.

### Map the outcomes in the service
Write the service over the client in `src/{package}/service/` with a `Protocol` of the client's methods it uses and its errors in `service/errors.py`, turning `ApiUnavailableError` into an error of the kind unavailable, each `ApiStatusError` code it expects into an error of the kind that fits, raised `from` the outcome, and letting anything else pass with no kind; raise an entity's rule error caused by the API's answer as an error with no kind, `from None`.

```python
except ApiUnavailableError as error:
    raise TodoServiceUnavailableError() from error
except ApiStatusError as error:
    if error.code is StatusCode.INVALID_ARGUMENT:
        raise TitlesRejectedError(error.message) from error
    raise
```

- Violation: the `KindError` of an entity built from the answer raised `from error`, which `kind_of` follows.
- Risk: an item the other service got wrong is answered to this service's client as its own invalid input.
- Fix: the scenario "Any other answer is a failure with no kind" of the example's `remote_planner.feature`.

### Build the client and the service in main.py
Build the client middleware of the API in `# [client middleware]` before it - empty until a variation point (`vp-client-retry`) adds one - and pass it to `{api}.new(logger, middlewares)`; the client refuses a call of an operation not in its list, and `retryable` marks one safe to repeat, as [api-client](library/develop/core/api-client.skill/api-client.skill.md) requires. Build the client with `{api}.new(logger, middlewares)` in `# [services]` of `main.py`, stopping the start with "startup failed" on its `ValueError`, build the service over it, pass the service to `build_handlers` as the keyword parameter of the command that uses it, and close the client when `run_all` returns.
- Risk: a client built inside a command or a service cannot be mocked or configured from one place; one never closed leaves its connections open when the loop ends.
- Fix: the command's handler takes the service through its protocol; the composition root builds and closes the client.

### Cover the client with scenarios
Copy the example's `httpclient/features/` and `httpclient/test/` unchanged but for the package: they cover the settings, the call, the outcomes, the log, and the mock through a test API of their own on `ThreadingHTTPServer`. Write per client a feature calling each operation against a server of the scenario and its emulator mocked, and its contract scenario reading the copied `openapi.json`; per service and per command a feature with the layer below faked by hand; tag them `@type/tech-check` and `@type/service`.
- Risk: a client feature that repeats the outcomes of `call` grows with every operation and tests nothing new.
- Fix: a client feature checks only its own request and reading.

# Check list
- [ ] `src/{package}/infrastructure/client/core/` is a filled copy of the templates; no placeholder is left.
- [ ] Each API has its package with `PREFIX`, `OPERATIONS`, `new`, a client with `aclose` and one method per operation, and `mock/{api}_mock.py`; the contract scenario passes.
- [ ] The service maps `ApiUnavailableError`, the expected `ApiStatusError` codes, and nothing else to kinds.
- [ ] `main.py` builds the client and the service in `# [services]` and closes the client after `run_all`.
- [ ] `make test` and `make lint` pass; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
One call of the example, with its real classes:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

The classes that do it, by package, and how they relate:

@import "./diagrams/example-classes.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the base plateau's startup command planning with another service's todo API: `make run` against its emulator, or against a running HTTP example of [vp-http-api-in-python](library/architecture/python/vp-http-api-in-python.skill/vp-http-api-in-python.skill.md).
