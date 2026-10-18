# Architecture catalog (in progress)

A new way to describe applications: **one base plateau + variation points that attach to it**,
instead of one plateau per combination of features. It is built here, beside the old catalogs
(`library/{stack}/architecture/`, `library/common-workflow/architecture/design/plateau-map/`),
which stay untouched until this one replaces them.

## Model

The layers and who may call whom are defined once in `library/develop/core/app-layers`
(Go: `library/develop/go/app-layers-in-go`). The base plateau fills api, command and domain;
variation points add entries above and services below.

```
          ┌──────────── API variation points (entries) ─────────────┐
          │ Startup* │ CLI │ HTTP │ gRPC │ Kafka/RabbitMQ listener    │
          └────┬─────────────────────────────────────────────────────┘
               │ plain parameters → command
               ▼
┌──────────────────────── Base plateau ─────────────────────────────┐
│ command (handlers, optional factory) ──► domain: entities,        │
│      │                                     domain services        │
│      │ through interfaces                       │                 │
└──────┼──────────────────────────────────────────┼─────────────────┘
       ▼                                          ▼
          ┌──────────── Service variation points ───────────────────┐
          │ service (business language) ──► infrastructure (technique)│
          │ DB │ HTTP/gRPC client │ Kafka/RabbitMQ producer │ TaskBox │
          └─────────────────────────────────────────────────────────┘
  * Startup is part of the base plateau only: an API variation point replaces it.
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

| Layer | Go | Python (draft) | dotnet |
|---|---|---|---|
| Entity | `internal/domain/entity` | `src/{package}/domain/entity` | `src/Modules/{Module}/{Module}.Domain/Entities` |
| Domain service | `internal/domain/service` | `src/{package}/domain/service` | `src/Modules/{Module}/{Module}.Domain/Services` |
| Commands + handlers | `internal/command` (contract and decorators in `internal/command/pipeline`) | `src/{package}/command` | `src/Modules/{Module}/{Module}.Application` (command types in `{Module}.Interfaces`) |
| API (entries) | `internal/api/{transport}` | `src/{package}/api/{transport}` | `src/App/App.Host/Api/{Transport}` |
| Service | `internal/service` | `src/{package}/service` | to be decided with the dotnet base |
| Infrastructure | `internal/infrastructure/{kind}` | `src/{package}/infrastructure/{kind}` | `src/Infrastructure/{Kind}` |
| Composition root | `cmd/{app}/main.go` | `src/{package}/main.py` | `src/App/App.Host/Program.cs` |

## Variation points by application type

| Variation point | Service | CLI |
|---|:-:|:-:|
| CLI API | — | ✅ |
| HTTP / gRPC API | ✅ | — |
| Kafka / RabbitMQ listener, producer | ✅ | ⏳ allowed, not built |
| DB, HTTP / gRPC client | ✅ | ✅ |
| TaskBox, Inbox, Outbox | ✅ | — |

## Status

| Step | Content | Go | dotnet | Python |
|---|---|:-:|:-:|:-:|
| 1a | base plateau (on `app-layers`) | ✅ | | |
| 1b | API: HTTP (`vp-http-api`) | ✅ | | |
| 1c-1 | API: gRPC (`vp-grpc-api`) | ✅ | | |
| — | error kinds (`library/develop/core/error-kinds`) | ✅ | | |
| — | health (`vp-health`, `:8081`) | | | |
| 1c-2 | HTTP + gRPC in one service | | | |
| 1d | API: CLI | | | |
| 2 | clients: HTTP, gRPC | | | |
| 3 | DB | | | |
| 4 | TaskBox, Inbox, Outbox | | | |

## Library candidates

Code that is the same in every project and could become a library the agent only installs.
To be reviewed with the owner before any is created — agreed to revisit the whole list once the
variation points are formed (2026-10-18).

| Candidate | Where it lives now | Why a library |
|---|---|---|
| Go handler contract + startup entry (`Handler[TCommand, TResult]`, `HandlerFunc`, `WithLogging`, `startup.Run`, `ExitCode`) | templates of `base-plateau-in-go` | identical in every Go application; a variation point's decorator or entry point would import one stable package |
| Go HTTP entry point (`Endpoint`, `Server` with health and graceful stop, `LoadConfig`, docs routes) | templates of `vp-http-api-in-go` | identical in every Go HTTP service except the module path and the contract's name; the docs routes could take the embedded spec as a parameter |
| Go gRPC entry point (`Call`, `Server` with reflection, panic recovery, graceful stop, `LoadConfig`) | templates of `vp-grpc-api-in-go` | identical in every Go gRPC service except the module path |
| Go error kinds (`entity/kinds.go`, `pipeline/errors.go` with `KindOf`, `api/rpcstatus`) | templates of `error-kinds-in-go` | identical in every Go application; a service would import the kinds and the table instead of copying them; the skill would keep only when to choose each kind |
| Testing tooling (`tools/testing`, `tools/normalize_*`, `tools/livingdoc`, `report-template`) | example of `solution-conformance-testing-in-go`; not copied into plateau or variation-point examples | ~2,000 lines copied into every real project; a version bump would update every project at once |
| TaskBox | `taskbox-spec` published; `taskbox-go` code proven, not yet packaged | already decided as a library (ADR `contract-vp-realized-by-libraries`) |

## Decisions waiting for their variation point

Agreed with the owner; written into the variation point's skill and ADR when it is built.

| Decision | Goes to | Agreed |
|---|---|---|
| **Unit of work at the command.** Optional, available once the DB variation point is attached; the outermost command opens it, nested commands and notification handlers join, services never commit. A simple service may skip it; once any command changes data in several places at once (nested commands, notification handlers, outbox task), every command is wrapped. Rejected: each service method owning its transaction (breaks atomicity across handlers); mandatory for every application. dotnet: pipeline behavior + EF change tracking (one short transaction at the end). Go: handler decorator, unit of work in `context.Context`, transaction opened on the first write, every storage method takes its executor from the context. | DB (step 3) | 2026-10-17 |
| **In-process notifications in Go** (the counterpart of MediatR notifications), joining the command's unit of work. | DB (step 3) | 2026-10-17 |
| **HTTP form of a gRPC contract: HTTP endpoint files over the generated messages** (`protojson`), the spec generated from the proto's `google.api.http` annotations, no grpc-gateway (ADR `http-form-over-generated-messages`). Must cover the owner's example: path parameters (`/v1/users/{user_id}/books`), 201 and 409 documented through `openapiv2_operation`, a deprecated method answering 501 `UNIMPLEMENTED`. | HTTP + gRPC (step 1c-2) | 2026-10-18 |
| **Health is HTTP only, the same in every long-running service** (HTTP, gRPC, listener): `/health/live`, `/health/ready`, `/health` from a health variation point; no `grpc.health.v1` (a Docker `healthcheck` cannot call it; one kind of health check per system). Its own port: `MANAGEMENT_LISTEN_PORT` (8081), so ingress never publishes `/health` and the later `/metrics`; HTTP API on 8080, gRPC on 9090. | health (before 1c-2) | 2026-10-18 |
| **Several entry points in one `[entries]` section**: an `errgroup` runs every `Run`; one stops → all stop. | HTTP + gRPC (step 1c-2) | 2026-10-18 |

## Backlog

| Task | When |
|---|---|
| Rename Client → Service and Integration → Infrastructure in `solution-integration-client-layering` (old Client = today's service, old Integration = today's infrastructure) | after the API variation points are designed |
| Move `library/common-workflow/develop/*` to `library/develop/{core,stack}` | later pass |
| `api-develop-in-go` still says the contract's routes are one list in `internal/api/http/server.go` exposed as `ContractRoutes()`; the HTTP variation point now keeps them in `internal/api/http/endpoint` (`Routes`) | with the move of `library/common-workflow/develop/*` |
