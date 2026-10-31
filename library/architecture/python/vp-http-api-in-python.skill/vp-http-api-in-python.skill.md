---
name: vp-http-api-in-python
description: The HTTP variation point in Python — the api/http/pipeline package copied from templates (FastAPI app with binding by pydantic models, every error answered as google.rpc.Status through exception handlers and one ASGI layer, the request log with X-Request-Id, the committed OpenAPI contract and Swagger UI served without a CDN, uvicorn on its own socket stopped by the entry's event, the application's middlewares around the contract's routes), one module per resource in api/http/endpoint, routes.py with the entry factory, the contract either hand-written in OpenAPI or, as the HTTP form of a gRPC contract, generated from the proto file's google.api.http annotations with the routes over the generated messages (proto.py), and the entry listed in main.py
whenToUse: when a Python application built on the base plateau must be reachable over HTTP, when you serve a new command over HTTP in a Python service, or when you review a Python service's HTTP entry point
updated: 20261102
tags:
  - stack/python
  - concern/architecture
  - app-type/service
  - framework/fastapi
adr:
  - adr/fastapi-with-binding-and-error-handlers.md
  - adr/contract-read-from-the-project-tree.md
  - adr/middlewares-by-route-match.md
  - adr/http-form-by-routes-over-generated-messages.md
  - adr/annotations-imported-at-run-time.md
---

# Goal
- **The HTTP variation point in Python** - A Python application that keeps every property of [vp-http-api](library/architecture/spec/vp-http-api.skill/vp-http-api.skill.md); this skill details that skill's implementation for Python and restates none of its rules.
- **Attached by the listed changes only** - Attaching it adds files and changes only the base files [attach.allow](./attach.allow) names, which the repository's attach check verifies against [base-plateau-in-python](library/architecture/python/base-plateau-in-python.skill/base-plateau-in-python.skill.md)'s example; as the HTTP form of a proto contract, only those [attach-proto.allow](./attach-proto.allow) names against [vp-grpc-api-in-python](library/architecture/python/vp-grpc-api-in-python.skill/vp-grpc-api-in-python.skill.md)'s example.

# Core Principle
- **FastAPI binds, the pipeline answers** - A route declares its request type as a pydantic model and returns its response type; FastAPI decodes and encodes, and the copied pipeline turns every error — of a command, of binding, of the framework itself — into one `google.rpc.Status` body ([ADR](./adr/fastapi-with-binding-and-error-handlers.md)).
- **The contract is the committed file** - FastAPI's generated OpenAPI is off; the service serves the one spec of `docs/integration/api/{contract-name}/` as committed — `openapi.json` written by hand, or `{file}.swagger.json` generated from the proto file — and one scenario holds the routes against it ([ADR](./adr/contract-read-from-the-project-tree.md)).
- **One contract, two forms** - With a gRPC contract, the proto file is the only contract: the routes read and write its generated messages as proto-JSON and the spec is generated from its annotations, so the HTTP and gRPC forms cannot describe different operations or shapes ([ADR](./adr/http-form-by-routes-over-generated-messages.md)).

```
src/{package}/api/
├── core/rpcstatus/          copied (error-kinds-in-python): kind → google.rpc.Code → HTTP status
└── http/
    ├── pipeline/            copied, never edited
    │   ├── config.py        HTTP_LISTEN_PORT, HTTP_BASE_PATH, HTTP_API_DOCS_ENABLED
    │   ├── server.py        Server: the app, uvicorn on its own socket, graceful stop; run();
    │   │                    Middleware, ContractMiddlewares: the middlewares around the contract's routes
    │   ├── errors.py        exception handlers: KindError, binding, the framework's 404/405
    │   ├── docs.py          /openapi.json, Swagger UI at /swagger/
    │   └── proto.py         only for a proto contract: decode_proto, proto_json, unimplemented
    ├── middleware/          one package per middleware
    │   ├── request_log/     copied, fixed, outermost: RequestLogMiddleware, X-Request-Id
    │   ├── answer_failures/ copied, fixed: AnswerFailuresMiddleware, status_response
    │   └── {name}/          an application's {Name}Middleware, given in entry()'s list
    ├── endpoint/            one module per resource: {Resource}Handler with router()
    └── routes.py            Endpoints with from_handlers, routes(), entry()
docs/integration/api/{contract-name}/openapi.json   the hand-written contract, or, for a proto contract:
docs/integration/api/{contract-name}/{file}.proto + {file}.swagger.json   written by hand + generated
```

# Rule

## MUST

### Copy the entry point from templates
Copy [api/http/pipeline/](./templates/src/{package}/api/http/pipeline/) and [api/http/middleware/](./templates/src/{package}/api/http/middleware/) and, from [error-kinds-in-python](library/develop/python/error-kinds-in-python.skill/error-kinds-in-python.skill.md), `api/core/rpcstatus/`, with the example's `features/` and `test/` of both — of [examples-proto/](./examples-proto/README.md) for a proto contract, which add [proto.py](./templates/src/{package}/api/http/pipeline/proto.py) and `proto.feature` — replacing `{package}` with the import name of the package and `{contract-name}` with the contract's folder name; add `fastapi==0.161.0`, `uvicorn==0.54.0`, and `swagger-ui-bundle==1.1.0` to the dependencies and `httpx2==2.16.0` to the dev dependencies of `pyproject.toml`.
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
- Fix: the route only maps and awaits; every error propagates to the pipeline. For a proto contract the generated messages take the place of the pydantic models ([Map the generated messages with proto.py](#map-the-generated-messages-with-protopy)).

### Register the resources in routes.py
List every resource as a field of `Endpoints`, its line in `Endpoints.from_handlers`, and its router in `routes()` of `api/http/routes.py`, beside `entry(handlers, logger, middlewares)`, the entry factory, and nothing else there.
- Risk: a router missing from `routes()` is served by no one, and the contract scenario reports its operations as missing.
- Fix: one field and one line per resource.

### Hand-write the contract when there is no proto file
Write `docs/integration/api/{contract-name}/openapi.json` by hand, per [api-develop](library/common-workflow/develop/api-develop.skill/api-develop.skill.md), with one operation per route, and keep the contract scenario listing them.
- Violation: FastAPI's `openapi_url` turned back on; a contract generated from the routes and committed.
- Risk: the contract follows the code instead of the code following the contract, and a field renamed in a model changes it silently.
- Fix: the contract scenario fails until the file and the routes list the same operations.

### Generate the spec of a proto contract from its annotations
Give every rpc of the proto file a `google.api.http` annotation and an `openapiv2_operation` listing its success status, each error status it can answer, and `default`, all with `.google.rpc.Status` as the error schema, as the [proto example](./examples-proto/docs/integration/api/todo/todo.proto) does; vendor the four annotation files under `third_party/proto/` and generate with the `proto-gen` target of the example's [Makefile](./examples-proto/Makefile): grpc-gateway's `protoc-gen-openapiv2` v2.31.0 with `disable_default_responses=true` and `enable_rpc_deprecation=true`, the generator and options of the Go and dotnet catalogs, writing `{file}.swagger.json` beside the proto file; commit it.
- Violation: a hand-written `openapi.json` beside the proto file; a 201 method whose spec also lists the 200 the generator adds by default; a deprecated rpc not marked deprecated in the spec.
- Risk: two descriptions of one API drift; Swagger UI promises a status the method never answers; a spec generated otherwise differs from the Go service's of the same contract.
- Fix: one spec, generated by `make proto-gen` (the binary is downloaded once into the git-ignored `tools/bin/`); `docs.py` serves it and sets its `basePath` under `HTTP_BASE_PATH`.

### Import the annotations at run time
Add `googleapis-common-protos==1.75.5` to the dependencies, and let `make proto-gen` generate the openapiv2 annotations from `third_party/proto/` into `src/protoc_gen_openapiv2/` with their type stubs, committed beside the contract's code ([ADR](./adr/annotations-imported-at-run-time.md)).
- Risk: the generated `{file}_pb2.py` imports `google.api.annotations_pb2` and `protoc_gen_openapiv2.options.annotations_pb2`; without them neither entry point starts.
- Fix: the second top-level package is installed with the application's (`packages.find` under `src/`); nothing else imports it.

### Map the generated messages with proto.py
In each route of a proto contract decode the body with `await decode_proto(request, {Rpc}Request())` into the rpc's generated request message, take each path parameter by the name the generated spec gives it (`Path(alias="listId")`), and answer `proto_json({Rpc}Response(...), status)` with the success status of the rpc's `openapiv2_operation`.

```python
type ListId = Annotated[str, Path(alias="listId")]

# 201: the item is created and kept.
@router.post("/v1/lists/{listId}/items")
async def add_item(list_id: ListId, request: Request) -> Response:
    message = await decode_proto(request, todo_pb2.AddItemRequest())
    result = await self._add_item.handle(AddItemCommand(list_id=list_id, title=message.title))
    item = todo_pb2.Item(title=result.item.title, done=result.item.done)
    return proto_json(todo_pb2.AddItemResponse(item=item), 201)
```

- Violation: a pydantic model of the route's own beside a generated message; `json.dumps` of `MessageToDict(..., preserving_proto_field_name=True)`; a path parameter named `{list_id}` in the route while the spec says `{listId}`.
- Risk: a model of the route's own drifts from the message; proto names in the body are not what a gateway writes; a route pattern that differs from the spec fails the contract scenario, or, unchecked, answers 404 for what Swagger UI shows.
- Fix: proto-JSON in and out: `decode_proto` accepts a field by its lowerCamelCase or its proto name and ignores an unknown one; `proto_json` writes lowerCamelCase and leaves a field at its default out (`"done": false` is absent), as the standard mapping allows.

### Keep each path parameter a whole segment
Write every `google.api.http` path with each variable as a whole segment (`/v1/lists/{list_id}/clear`), never a custom-method suffix (`/v1/lists/{list_id}:clear`).
- Risk: the same proto file is served by Go services, whose `http.ServeMux` rejects a wildcard that is not a whole segment.
- Fix: `/clear` in place of `:clear`.

### Answer a deprecated rpc 501 in both forms
Mark an rpc that is kept but no longer served `option deprecated = true`, answer its route with `unimplemented(message)` and its gRPC method with `KindError(Kind.UNIMPLEMENTED, message)`, with the same message, and call no command.
- Risk: a client of one form sees 404 or a call that still works while the other answers `UNIMPLEMENTED`.
- Fix: the lists feature and the gRPC items feature each hold the answer of `ClearList`.

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
- Fix: `from todo.api import http as http_api` — the package name `http` is the transport's, so it is imported under that alias. As the HTTP form of a proto contract, the startup entry is gone already: add `http_api.entry(...)` after `grpc_api.entry(...)`; both serve the same handlers. The server runs every request of a contract route through the middlewares, inside the request log and the error answers; the docs go past them ([ADR](./adr/middlewares-by-route-match.md)).

### Ship the contract with the service
Run the service from its project tree — the image copies the project and installs it with `pip install -e .`, or runs it from the tree — so the spec in `docs/integration/api/{contract-name}/` — `openapi.json`, or the generated `{file}.swagger.json` — is found above the pipeline's module.
- Risk: a wheel installed without the tree has no contract; with the docs enabled, the server stops at start saying so.
- Fix: keep the tree in the image; with the docs disabled the contract is never read.

### Keep the fixed middleware inside the server
Let `Server.app()` add its fixed middleware from the logger `http_api.entry` gives it — `RequestLogMiddleware` outermost, then `AnswerFailuresMiddleware`, with the exception handlers answering `KindError`, binding errors, 404, and 405 — around the whole application, and the list of `entry` inside them, around the contract's routes only (`ContractMiddlewares`), as [fixed-middleware-inside-the-entry](library/architecture/spec/base-plateau-extension.skill/adr/fixed-middleware-inside-the-entry.md) requires.

```python
# api/http/pipeline/server.py, Server.app
app.add_middleware(ContractMiddlewares, routes=contract, middlewares=self._middlewares)
app.add_middleware(AnswerFailuresMiddleware, logger=self._logger)
app.add_middleware(RequestLogMiddleware, logger=self._logger)  # added last: the outermost
```

- Violation: `RequestLogMiddleware` or `AnswerFailuresMiddleware` given in `http_api.entry`'s list; a middleware of the list that returns a `JSONResponse` with an error.
- Risk: a request the list does not reach — the docs, an unknown path — is unlogged; an exception of a middleware reaches Starlette's own plain answer.
- Fix: the copied `server.py`; the scenarios of `middleware.feature` and `endpoint.feature` hold the order, the failures, and the 404 and 405, each logged.

### Cover the entry point with scenarios
Copy the pipeline's and rpcstatus's scenarios — the middlewares in their order, a rejected request, and the docs outside them among them — and add per resource a feature with its fake command handlers and the contract feature.
- Risk: a resource without its scenario lets a route call the wrong command or map a field wrongly.
- Fix: a fake class per command in the step module, recording what it received; the steps the pipeline's features share are in its `test/conftest.py`.

# Check list
- [ ] `api/http/pipeline/` and `api/core/rpcstatus/` are filled copies of the templates; no placeholder is left.
- [ ] Each resource has its module in `endpoint/` with its `{Resource}Handler`, its `router()`, and per route its request and response models; `routes.py` holds only `Endpoints`, `routes`, and `entry`.
- [ ] `docs/integration/api/{contract-name}/` holds one spec: the hand-written `openapi.json`, or, for a proto contract, the generated `{file}.swagger.json`; the contract scenario passes.
- [ ] For a proto contract: every rpc has `google.api.http` and `openapiv2_operation`; `make proto-gen` leaves no diff; the routes use the generated messages through `proto.py` and the spec's path-parameter names; every path variable is a whole segment; a deprecated rpc answers 501 / `UNIMPLEMENTED` in both forms.
- [ ] The `# [entries]` list of `main.py` has `http_api.entry(handlers, logger, [...])`; `api/startup/` is deleted.
- [ ] `make test` and `make lint` pass; `bash test/check-vp-attach.sh` in this repository reports no base file changed beyond [attach.allow](./attach.allow) and [attach-proto.allow](./attach-proto.allow).

# Example
One call of the example, each part with its class and file:

@import "./diagrams/example-flow.mmd" {as="mermaid"}

- [examples/](./examples/README.md) — the base plateau with the HTTP variation point: `make run`, then `POST /v1/items/plan`; Swagger UI at `/swagger/`.
- [examples-proto/](./examples-proto/README.md) — the HTTP form of the gRPC example's proto contract, with a path parameter, 201, 404, 409, and a deprecated rpc: `make run`, `make proto-gen`, `make test`.
