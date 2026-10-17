# Architecture catalog (in progress)

A new way to describe applications: **one base plateau + variation points that attach to it**,
instead of one plateau per combination of features. It is built here, beside the old catalogs
(`library/{stack}/architecture/`, `library/common-workflow/architecture/design/plateau-map/`),
which stay untouched until this one replaces them.

## Model

```
          ┌──────────── API variation points (entries) ─────────────┐
          │ Startup* │ CLI │ HTTP │ gRPC │ Kafka/RabbitMQ listener    │
          └────┬─────────────────────────────────────────────────────┘
               │ only commands
               ▼
┌──────────────────────── Base plateau ─────────────────────────────┐
│ Application: commands + handlers ──► Domain: entities, services   │
│      │ a handler uses a service through an interface              │
└──────┼────────────────────────────────────────────────────────────┘
       ▼
          ┌──────────── Service variation points ───────────────────┐
          │ DB │ HTTP/gRPC client │ Kafka/RabbitMQ producer │ TaskBox │
          └─────────────────────────────────────────────────────────┘
  * Startup is part of the base plateau: it runs one command and exits.
```

## Layout

| Folder | Holds |
|---|---|
| `spec/` | what a plateau or variation point is and what it guarantees — stack-agnostic; names its stack skills in plain text |
| `{stack}/` | how it is built in that stack, with a runnable example and tests; links its `spec/` skill |

A project takes one stack with a single `subpath` in `ai-skills.yaml` (`library/architecture/go`);
the `spec/` skills come along because every stack skill links its spec.

## Layer names per stack

| Layer | Go | dotnet | Python |
|---|---|---|---|
| Domain | `internal/domain` | `{Module}.Domain` | `domain/` |
| Application | `internal/application` | `{Module}.Application` | `application/` |
| API (entries) | `internal/api/{transport}` | `App.Host/Api/{Transport}` | `api/{transport}/` |
| Infrastructure (services) | `internal/infrastructure/{kind}` | `Infrastructure/{Kind}` | `infrastructure/{kind}/` |
| Composition root | `cmd/{app}/main.go` | `Program.cs` | `main.py` |

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
| 1a | base plateau | ✅ | | |
| 1b–d | API: HTTP, gRPC, CLI | | | |
| 2 | clients: HTTP, gRPC | | | |
| 3 | DB | | | |
| 4 | TaskBox, Inbox, Outbox | | | |

## Library candidates

Code that is the same in every project and could become a library the agent only installs.
To be reviewed with the owner before any is created.

| Candidate | Where it lives now | Why a library |
|---|---|---|
| Go handler contract + startup entry (`Handler[C, R]`, `HandlerFunc`, `WithLogging`, `startup.Run`, `ExitCode`) | templates of `base-plateau-in-go` | identical in every Go application; a variation point's decorator or entry point would import one stable package |
| Testing tooling (`tools/testing`, `tools/normalize_*`, `tools/livingdoc`, `report-template`) | copied verbatim into every example | ~1,200 lines repeated per example; a version bump would update every project at once |
| TaskBox | `taskbox-spec` published; `taskbox-go` code proven, not yet packaged | already decided as a library (ADR `contract-vp-realized-by-libraries`) |
