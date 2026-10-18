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
| 1b–d | API: HTTP, gRPC, CLI | | | |
| 2 | clients: HTTP, gRPC | | | |
| 3 | DB | | | |
| 4 | TaskBox, Inbox, Outbox | | | |

## Library candidates

Code that is the same in every project and could become a library the agent only installs.
To be reviewed with the owner before any is created.

| Candidate | Where it lives now | Why a library |
|---|---|---|
| Go handler contract + startup entry (`Handler[TCommand, TResult]`, `HandlerFunc`, `WithLogging`, `startup.Run`, `ExitCode`) | templates of `base-plateau-in-go` | identical in every Go application; a variation point's decorator or entry point would import one stable package |
| Testing tooling (`tools/testing`, `tools/normalize_*`, `tools/livingdoc`, `report-template`) | copied verbatim into every example | ~1,200 lines repeated per example; a version bump would update every project at once |
| TaskBox | `taskbox-spec` published; `taskbox-go` code proven, not yet packaged | already decided as a library (ADR `contract-vp-realized-by-libraries`) |

## Decisions waiting for their variation point

Agreed with the owner; written into the variation point's skill and ADR when it is built.

| Decision | Goes to | Agreed |
|---|---|---|
| **Unit of work at the command.** Optional, available once the DB variation point is attached; the outermost command opens it, nested commands and notification handlers join, services never commit. A simple service may skip it; once any command changes data in several places at once (nested commands, notification handlers, outbox task), every command is wrapped. Rejected: each service method owning its transaction (breaks atomicity across handlers); mandatory for every application. dotnet: pipeline behavior + EF change tracking (one short transaction at the end). Go: handler decorator, unit of work in `context.Context`, transaction opened on the first write, every storage method takes its executor from the context. | DB (step 3) | 2026-10-17 |
| **In-process notifications in Go** (the counterpart of MediatR notifications), joining the command's unit of work. | DB (step 3) | 2026-10-17 |

## Backlog

| Task | When |
|---|---|
| Rename Client → Service and Integration → Infrastructure in `solution-integration-client-layering` (old Client = today's service, old Integration = today's infrastructure) | after the API variation points are designed |
| Move `library/common-workflow/develop/*` to `library/develop/{core,stack}` | later pass |
