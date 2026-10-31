# Architecture catalog (in progress)

A new way to describe applications: **one base plateau + variation points that attach to it**,
instead of one plateau per combination of features. It is built here, beside the old catalogs
(`library/{stack}/architecture/`, `library/common-workflow/architecture/design/plateau-map/`),
which stay untouched until this one replaces them.

## Model

The layers and who may call whom are defined once in `library/develop/core/app-layers`
(Go: `library/develop/go/app-layers-in-go`, dotnet: `library/develop/dotnet/app-layers-in-dotnet`, Python: `library/develop/python/app-layers-in-python`). The base plateau fills api, command and domain;
variation points add entries above and services below.

@import "./diagrams/model.mmd" {as="mermaid"}

\* Startup is part of the base plateau only: an API variation point replaces it.

Every entry point is one `Entry` in the `[entries]` list of the composition root, run together
by the base plateau's runner (Go: `entry.RunAll`, dotnet: `EntryRunner.RunAllAsync`, Python: `run_all`): an API variation point replaces the startup
entry with its own, the health variation point adds one, so variation points combine by
listing, with no code written for a combination.

How a variation point attaches — the extension points, the command registry, and the two
ordered lists of middleware (around commands, around an API's requests), every middleware
in a directory of its own, `middleware/{name}/` — is
`spec/base-plateau-extension` with its stack skills, each with a sample access variation point:

```
request ─▶ request log ─▶ answer failures ─▶ API middleware ─▶ route ─▶ command middleware ─▶ handler
           (fixed, the API's)                 (the API's list)           (the registry's list)
```

## Layout

| Folder | Holds |
|---|---|
| `spec/` | what a plateau or variation point is and what it guarantees — stack-agnostic; names its stack skills in plain text |
| `{stack}/` | how it is built in that stack, with a runnable example and tests; links its `spec/` skill |

Every `{stack}/vp-*` skill holds `attach.allow`: the base example it attaches to and the base files it may change or delete. `test/check-vp-attach.sh` (run by `test/test.sh`) compares the examples and fails on any other edit.

A project takes one stack with a single `subpath` in `ai-skills.yaml` (`library/architecture/go`);
the `spec/` skills come along because every stack skill links its spec.

## Layer names per stack

| Layer | Go | Python | dotnet |
|---|---|---|---|
| Entity | `internal/domain/entity` | `src/{package}/domain/entity` | `src/Modules/{Module}/{Module}.Domain/Entities` |
| Domain service | `internal/domain/service` | `src/{package}/domain/service` | `src/Modules/{Module}/{Module}.Domain/Services` |
| Commands + handlers | `internal/command` (contract in `internal/command/pipeline`, middleware in `internal/command/middleware/{name}`) | `src/{package}/command` (contract in `src/{package}/command/pipeline`, middleware in `src/{package}/command/middleware/{name}`) | `src/Modules/{Module}/{Module}.Application/Commands` (command types in `{Module}.Commands`, command middleware in `src/Middleware/{Name}`) |
| API (entries) | `internal/api/{transport}`: `pipeline/` (fixed), `middleware/{name}/` (fixed request log and answer failures, an application's), `endpoint/` (`{Resource}Handler` per file), `routes.go` / `services.go` / `commands.go` (registration) | `src/{package}/api/{transport}`: `pipeline/`, `middleware/{name}/`, `endpoint/`, `routes.py` / `services.py` / `commands.py`; generated gRPC code in `src/{package}/gen/api/{contract}` | `src/App/App.Host/Api/{Transport}`: `Pipeline/` (fixed), `Middleware/{Name}/`, `Endpoint/` (`{Resource}Handler` per file), `Routes.cs` / `Services.cs` (registration) |
| API — shared by the entries | `internal/api/core/{entry,rpcstatus}` | `src/{package}/api/core/{entry,rpcstatus}` | `src/App/App.Host/Api/Core/{Entry,Errors,Hosting}` |
| Service | `internal/service` | `src/{package}/service` | `src/Modules/{Module}/{Module}.Application/Services` (interface beside its consumer) |
| Infrastructure | `internal/infrastructure/{kind}`; clients of other services' APIs in `internal/infrastructure/client/{api}`, their fixed code in `client/core/` | `src/{package}/infrastructure/{kind}`; clients in `infrastructure/client/{api}` | `src/Infrastructure/{Infra}/{Infra}.Abstractions` + `{Infra}` |
| Composition root | `cmd/{app}/main.go` | `src/{package}/main.py` | `src/App/App.Host/Program.cs` (`RunAsync`) |
| Error kinds | `internal/domain/entity/kinds.go` | `src/{package}/domain/entity/kinds.py` (exceptions) | `src/Shared/Errors` |

## Variation points by application type

| Variation point | Service | CLI |
|---|:-:|:-:|
| CLI API | — | ✅ |
| HTTP / gRPC API | ✅ | — |
| Health (management port) | ✅ | — |
| Logging (payload, masking, request id) | ✅ | ✅ |
| Kafka / RabbitMQ listener, producer | ✅ | ⏳ allowed, not built |
| DB, HTTP / gRPC client | ✅ | ✅ |
| TaskBox, Inbox, Outbox | ✅ | — |

## Status

| Step | Content | Go | dotnet | Python |
|---|---|:-:|:-:|:-:|
| 1a | base plateau (on `app-layers`) | ✅ | ✅ | ✅ |
| 1b | API: HTTP (`vp-http-api`) | ✅ | ✅ | ✅ (FastAPI) |
| 1c-1 | API: gRPC (`vp-grpc-api`) | ✅ | ✅ | ✅ (grpc.aio) |
| — | error kinds (`library/develop/core/error-kinds`) | ✅ | ✅ (exceptions) | ✅ (exceptions) |
| — | health (`vp-health`, `:8081`) + one runner for every entry point (`entry.RunAll`) | ✅ | ✅ | ✅ (asyncio, no dependency) |
| 1c-2 | HTTP form of a gRPC contract (`vp-http-api`, proto mode) | ✅ | ✅ | ✅ |
| 1d | API: CLI (`vp-cli-api`; Go cobra, Python click) | ✅ | | ✅ |
| — | logging: one logger, payload attribute, base handler drops it; every entry point logs its calls; `vp-logging` masks and writes the payload | ✅ | base ✅ | ✅ |
| — | extension of the base (`base-plateau-extension`): command registry, middleware lists for commands and entries, one directory per middleware, sample access VP | ✅ | ✅ | ✅ |
| 2 | clients (`develop/core/api-client`): HTTP (`vp-http-client`), gRPC (`vp-grpc-client`) | HTTP ✅, gRPC ✅ (buf managed mode, bufconn mock; one `client/core/outcome` for every transport) | HTTP ✅, gRPC ✅ (one `Client.Abstractions` for every transport; mock through a call invoker) | HTTP ✅ (httpx2, `MockTransport`; one `outcomes.py` for every transport), gRPC ✅ (`grpc.aio`, the mock on a loopback server) |
| 2b | client retry (`vp-client-retry`): client middleware list in every client, operations marked retryable, `{API}_RETRY_ATTEMPTS`/`_BACKOFF` | ✅ | ✅ (`Client.Pipeline`; the wait ends with the caller's cancellation) | ✅ (`core/pipeline`; the cancellation passes through) |
| 3 | DB | | | |
| 4 | TaskBox, Inbox, Outbox | | | |
| 5 | SQLite (`vp-sqlite`): one more persistent DB of step 3, an SQLite file - the driver per stack, the file's path setting, the same migrations and stores; fits a CLI that keeps its data between runs, and an application with an API within its limits (below) | | | |
| 6 | data cache (`vp-cache-data`): with a DB VP and a client VP attached, a service of its own that wraps the client's service behind the same interface, answering its cacheable methods from a store first and calling the client's service only on a miss, passing every other method on unchanged | | | |

## Library candidates

Code that is the same in every project and could become a library the agent only installs.
To be reviewed with the owner before any is created — agreed to revisit the whole list once the
variation points are formed (2026-10-18).

| Candidate | Where it lives now | Why a library |
|---|---|---|
| Go handler contract + startup entry (`Handler[TCommand, TResult]`, `HandlerFunc`, `Middleware`, `Decorate`, `logging.Middleware`, `startup.Run`, `ExitCode`) | templates of `base-plateau-in-go` | identical in every Go application; a variation point's middleware or entry point would import one stable package |
| Go HTTP entry point (`Endpoint`, `Server` with graceful stop, `LoadConfig`, docs routes, `proto.go` for a proto contract) | templates of `vp-http-api-in-go` | identical in every Go HTTP service except the module path and the contract's name; the docs routes could take the embedded spec as a parameter |
| Go gRPC entry point (`Call`, `Server` with reflection, panic recovery, graceful stop, `LoadConfig`) | templates of `vp-grpc-api-in-go` | identical in every Go gRPC service except the module path |
| Go CLI entry point (`Run`, `Command` over cobra, `--output`, usage errors as invalid) | templates of `vp-cli-api-in-go` | identical in every Go CLI except the module path |
| Go logging (`internal/logging`, `internal/logging/masked`: payload attribute, masked handler) | templates of `base-plateau-in-go` and `vp-logging-in-go` | identical in every Go application; one masking for all services |
| Go error kinds (`entity/kinds.go`, `pipeline/errors.go` with `KindOf`, `api/rpcstatus`) | templates of `error-kinds-in-go` | identical in every Go application; a service would import the kinds and the table instead of copying them; the skill would keep only when to choose each kind |
| Testing tooling (`tools/testing`, `tools/normalize_*`, `tools/livingdoc`, `report-template`) | example of `solution-conformance-testing-in-go`; not copied into plateau or variation-point examples | ~2,000 lines copied into every real project; a version bump would update every project at once |
| dotnet fixed code (`Shared`: error kinds; `BuildingBlocks`: `AddCommandMiddleware`, `JsonLogProvider`, `LogAttr`, `RequestId`; `Middleware`: `LoggingMiddleware`; `Api/Core/Entry`, `Api/Startup`; `Api/Core/Errors/RpcStatus`, `Api/Core/Hosting/WebHost`, `Api/Http/Pipeline` with `ProtoJson`, `Api/Grpc/Pipeline`, `Api/Management`) | templates of `base-plateau-in-dotnet`, `error-kinds-in-dotnet`, `vp-http-api-in-dotnet`, `vp-grpc-api-in-dotnet`, `vp-health-in-dotnet` | no placeholder at all: one NuGet package (or two: `Shared` with no dependency, the rest over MediatR) |
| TaskBox | `taskbox-spec` published; `taskbox-go` code proven, not yet packaged | already decided as a library (ADR `contract-vp-realized-by-libraries`) |

## Decisions waiting for their variation point

Agreed with the owner; written into the variation point's skill and ADR when it is built.

| Decision | Goes to | Agreed |
|---|---|---|
| **dotnet logging VP: own masking or Serilog + `Destructurama.Attributed`** — the base has its own `JsonLogProvider`; the logging variation point replaces only its `AddProvider` line, so the choice is made there (ADR `own-json-log-provider`). | dotnet logging (B4) | 2026-10-21 |
| **Python stack**: async handlers and entry points (asyncio), FastAPI for the HTTP variation point, the standard library's `logging` with a formatter of its own (ADRs `async-handlers`, `stdlib-logging` of `base-plateau-in-python`). | Python | 2026-10-22 |
| **Order of B**: B1 dotnet base → B2 HTTP + health → B3 gRPC + HTTP form → B4 logging VP; then Python base → HTTP → gRPC → CLI → health → logging. dotnet has no CLI. | B | 2026-10-21 |
| **Unit of work at the command.** Optional, available once the DB variation point is attached; the outermost command opens it, nested commands and notification handlers join, services never commit. A simple service may skip it; once any command changes data in several places at once (nested commands, notification handlers, outbox task), every command is wrapped. Rejected: each service method owning its transaction (breaks atomicity across handlers); mandatory for every application. dotnet: pipeline behavior + EF change tracking (one short transaction at the end). Go: command middleware, unit of work in `context.Context`, transaction opened on the first write, every storage method takes its executor from the context. | DB (step 3) | 2026-10-17 |
| **In-process notifications in Go** (the counterpart of MediatR notifications), joining the command's unit of work. | DB (step 3) | 2026-10-17 |

## Decisions and open questions of the queued VPs

| VP | Decided (2026-10-29) | Left to the VP |
|---|---|---|
| `vp-cache-data` | A separate wrapper service, not a rewrite of the client's service (a VP that rewrites it is harder to attach and to remove): it implements the client service's interface, caches some of its methods, and passes the others on | how the wrapper fits app-layers (a service over a service and a store); the mark of a cacheable method and the shape of the key |
| `vp-cache-data` | What is cached and for how long, and whether expired data answers while the API is unavailable, depend on the business case of each method: the VP gives the mechanism, the application decides per method | the default settings, if any |
| `vp-sqlite` | One more kind of persistent DB, not limited to CLI applications; its skill states the limits for an application that serves an API: the DB is not shared between pods and is lost when the container restarts without a volume | variant of the DB VP chosen by a setting, or a VP of its own; the driver per stack (Go `modernc.org/sqlite` without cgo, dotnet `Microsoft.Data.Sqlite`, Python `sqlite3`) |

## Backlog

| Task | When |
|---|---|
| Diagrams of `vp-diagrams` for every other variation point (base plateau, its extension, HTTP, gRPC, CLI APIs, health, logging), in the spec skills and all stacks; the client and retry variation points are the sample | after the owner accepts the sample |
| **Refinement: a middleware decides itself where it is registered.** *Task:* let a middleware decide whether it is applied to a given API endpoint or command at all. *Goal:* as the middleware of the command or API list grow, every call passes through more middleware that only hand it on unchanged; a middleware known not to apply should not be wrapped around that endpoint or command. *Proposal:* every middleware declares a decorating method; it is called once when the chain is built for an endpoint or a command, checks what it is applied to (the command's type and its marks, the route), and either wraps it or returns it unwrapped. A value-filtered middleware (batch limit) keeps its check of the value at call time; only the mark check moves to build time. Per stack: Go `Decorate` can ask with the zero value of `TCommand`; Python with the command's class; dotnet needs a registration that MediatR resolves per closed command type (a constraint, or a factory of `IPipelineBehavior<TCommand,TResult>`) | after #253, before the list grows (DB unit of work) |
| The new catalog's clients follow `library/develop/core/api-client` (the `{API}` group with `{API}_TIMEOUT`, the mock as an emulator inside the process); `api-client-develop` and `client-mocking` (`common-workflow/develop`) stay for the old catalogs and are removed with them | when the old catalogs are replaced |
| A row read back from a store, or an item answered by another service, that breaks an entity's rule carries the entity's kind invalid up to the caller (Go `service/item_lists.go` of the proto example wraps it with `%w`; dotnet and Python likewise): it is a failure of this service, not the caller's invalid input; `vp-http-client-in-go` and `vp-grpc-client-in-go` wrap with `%v` | next change of the proto examples |
| The proto examples keep their lists in memory (Go `internal/infrastructure/memory`, dotnet `src/Infrastructure/Memory`, scenarios tagged `@type/tech-check` since the tag vocabulary has no in-memory store); the DB variation point replaces it | DB (step 3) |
| The verification `curl` of the `devops-service-deploy` guides calls `/health/ready` on the published API port; for a service with the management port it needs that port published or port-forwarded | with the next change of `devops-service-deploy` |
| Review [Library candidates](#library-candidates) with the owner: turn fixed Go code (handler contract, error kinds + `rpcstatus`, HTTP/gRPC entry points) into libraries services import instead of copying templates | after the last variation point (step 4) |
| Move `library/common-workflow/develop/*` to `library/develop/{core,stack}` | later pass |
| The old dotnet catalog (`library/dotnet/architecture/...`: plateau examples, `*.Tests.csproj.create.md`) still keeps test projects in a root `tests/`; `cucumber-testing-in-dotnet` now puts each beside its project (ADR `test-location`) | when the old dotnet catalog is replaced |
| dotnet: one DI scope per command, for a scoped `DbContext` — entries send through a sender that opens a scope per call; today they take `ISender` from the root provider | DB (step 3) |
| `api-develop-in-go` still says the contract's routes are one list in `internal/api/http/server.go` exposed as `ContractRoutes()`; the HTTP variation point now keeps them in `internal/api/http/endpoint` (`Routes`) | with the move of `library/common-workflow/develop/*` |
