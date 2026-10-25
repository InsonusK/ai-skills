---
name: vp-http-api-in-python
description: The HTTP variation point in Python — the api/http/pipeline package copied from templates (FastAPI app with binding by pydantic models, every error answered as google.rpc.Status through exception handlers and one ASGI layer, the request log with X-Request-Id, the committed OpenAPI contract and Swagger UI served without a CDN, uvicorn on its own socket stopped by the entry's event, the application's middlewares around the contract's routes), one module per resource in api/http/endpoint, routes.py with the entry factory, and the startup entry replaced in main.py
whenToUse: when a Python application built on the base plateau must be reachable over HTTP, when you serve a new command over HTTP in a Python service, or when you review a Python service's HTTP entry point
updated: 20261025
tags:
  - stack/python
  - concern/architecture
  - app-type/service
  - framework/fastapi
adr:
  - adr/fastapi-with-binding-and-error-handlers.md
  - adr/contract-read-from-the-project-tree.md
  - adr/middlewares-by-route-match.md
---

# Goal
- **The HTTP variation point in Python** - A Python application that keeps every property of [vp-http-api](library/architecture/spec/vp-http-api.skill/vp-http-api.skill.md); this skill details that skill's implementation for Python and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-python](library/architecture/python/base-plateau-in-python.skill/base-plateau-in-python.skill.md)'s example.

# Core Principle
- **FastAPI binds, the pipeline answers** - A route declares its request type as a pydantic model and returns its response type; FastAPI decodes and encodes, and the copied pipeline turns every error — of a command, of binding, of the framework itself — into one `google.rpc.Status` body ([ADR](./adr/fastapi-with-binding-and-error-handlers.md)).
- **The contract is the committed file** - FastAPI's generated OpenAPI is off; the service serves `docs/integration/api/{contract-name}/openapi.json` as written, and one scenario holds the routes against it ([ADR](./adr/contract-read-from-the-project-tree.md)).

```
src/{package}/api/
├── core/rpcstatus/          copied (error-kinds-in-python): kind → google.rpc.Code → HTTP status
└── http/
    ├── pipeline/            copied, never edited
    │   ├── config.py        HTTP_LISTEN_PORT, HTTP_BASE_PATH, HTTP_API_DOCS_ENABLED
    │   ├── server.py        Server: the app, uvicorn on its own socket, graceful stop; run();
    │   │                    Middleware, ContractMiddlewares: the middlewares around the contract's routes
    │   ├── errors.py        exception handlers + AnswerFailures: every error as google.rpc.Status
    │   ├── logging.py       RequestLog: two records per request, X-Request-Id
    │   └── docs.py          /openapi.json, Swagger UI at /swagger/
    ├── endpoint/            one module per resource: {Resource}Handler with router()
    └── routes.py            Endpoints with from_handlers, routes(), entry()
docs/integration/api/{contract-name}/openapi.json   the hand-written contract
```

# Rule

## MUST

### Copy the entry point from templates
Copy [api/http/pipeline/](./templates/src/{package}/api/http/pipeline/) and, from [error-kinds-in-python](library/develop/python/error-kinds-in-python.skill/error-kinds-in-python.skill.md), `api/core/rpcstatus/`, with the example's `features/` and `test/` of both, replacing `{package}` with the import name of the package and `{contract-name}` with the contract's folder name; add `fastapi==0.161.0`, `uvicorn==0.54.0`, and `swagger-ui-bundle==1.1.0` to the dependencies and `httpx2==2.16.0` to the dev dependencies of `pyproject.toml`.
- Risk: a retyped pipeline answers some error with FastAPI's `{"detail": ...}` body or lets uvicorn take the process's signals.
- Fix: copy the templates; after filling, searching them for `{package}` and `{contract-name}` returns nothing; `make test` passes.

### One module per resource in the endpoint package
Write each resource — the first path segment after the version — as `api/http/endpoint/{resource}.py` holding a `{Resource}Handler` class that takes the command handlers of its routes, a `from_handlers(handlers)` classmethod taking them from the command registry, a `router()` method returning an `APIRouter` with one decorated function per route, and per route a pydantic request model and response model with a `classmethod` mapping the command's result.

```python
@router.post("/v1/items/plan", status_code=200)  # 201 created, 202 accepted, 200 otherwise
async def plan_items(body: PlanItemsRequest) -> PlanItemsResponse:
    result = await self._plan_items.handle(PlanItemsCommand(titles=body.titles))
    return PlanItemsResponse.of(result)
```

- Violation: a route returning the command's result dataclass; a route catching a `KindError` to answer it itself; `HTTPException` raised by a route.
- Risk: renaming a field of the result changes the published contract unnoticed; an error answered in the route answers differently from every other.
- Fix: the route only maps and awaits; every error propagates to the pipeline.

### Register the resources in routes.py
List every resource as a field of `Endpoints`, its line in `Endpoints.from_handlers`, and its router in `routes()` of `api/http/routes.py`, beside `entry(handlers, logger, middlewares)`, the entry factory, and nothing else there.
- Risk: a router missing from `routes()` is served by no one, and the contract scenario reports its operations as missing.
- Fix: one field and one line per resource.

### Hand-write the contract
Write `docs/integration/api/{contract-name}/openapi.json` by hand, per [api-develop](library/common-workflow/develop/api-develop.skill/api-develop.skill.md), with one operation per route, and keep the contract scenario listing them.
- Violation: FastAPI's `openapi_url` turned back on; a contract generated from the routes and committed.
- Risk: the contract follows the code instead of the code following the contract, and a field renamed in a model changes it silently.
- Fix: the contract scenario fails until the file and the routes list the same operations.

### Replace the startup entry in main.py
Replace the startup `Entry` in `# [entries]` of `main.py` with `http_api.entry(handlers, logger, middlewares)`, given the command registry and the list of middlewares — empty until a variation point adds one, as [base-plateau-extension-in-python](library/architecture/python/base-plateau-extension-in-python.skill/base-plateau-extension-in-python.skill.md) describes — and delete `api/startup/`.

```python
# [entries]
ended_by = await run_all(
    logger,
    stopping,
    # the HTTP API: its contract routes wrapped by these middlewares, the
    # first the outermost, inside the request log
    http_api.entry(handlers, logger, []),
)
return exit_code(ended_by)
```

- Risk: a second way of starting the server bypasses the runner of the entry points and its exit code.
- Fix: `from todo.api import http as http_api` — the package name `http` is the transport's, so it is imported under that alias. The server runs every request of a contract route through the middlewares, inside the request log and the error answers; the docs go past them ([ADR](./adr/middlewares-by-route-match.md)).

### Ship the contract with the service
Run the service from its project tree — the image copies the project and installs it with `pip install -e .`, or runs it from the tree — so `docs/integration/api/{contract-name}/openapi.json` is found above the pipeline's module.
- Risk: a wheel installed without the tree has no contract; with the docs enabled, the server stops at start saying so.
- Fix: keep the tree in the image; with the docs disabled the contract is never read.

### Cover the entry point with scenarios
Copy the pipeline's and rpcstatus's scenarios — the middlewares in their order, a rejected request, and the docs outside them among them — and add per resource a feature with its fake command handlers and the contract feature.
- Risk: a resource without its scenario lets a route call the wrong command or map a field wrongly.
- Fix: a fake class per command in the step module, recording what it received; the steps the pipeline's features share are in its `test/conftest.py`.

# Check list
- [ ] `api/http/pipeline/` and `api/core/rpcstatus/` are filled copies of the templates; no placeholder is left.
- [ ] Each resource has its module in `endpoint/` with its `{Resource}Handler`, its `router()`, and per route its request and response models; `routes.py` holds only `Endpoints`, `routes`, and `entry`.
- [ ] `docs/integration/api/{contract-name}/openapi.json` is hand-written; the contract scenario passes.
- [ ] The `# [entries]` list of `main.py` has `http_api.entry(handlers, logger, [...])`; `api/startup/` is deleted.
- [ ] `make test` and `make lint` pass; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow).

# Example
- [examples/](./examples/README.md) — the base plateau with the HTTP variation point: `make run`, then `POST /v1/items/plan`; Swagger UI at `/swagger/`.
