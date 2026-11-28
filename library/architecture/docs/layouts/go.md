# Go application layout

[Base capabilities](../base-capabilities.md) · [Catalog](../catalog.md)

This is the full catalog placement overlay. Optional directories appear only when
their capability is selected; repeated shared directories retain one owner.
Fixed code comes from the owning skill's templates. Entity clients, endpoints,
commands, domain behavior and adapting services are application-specific.

```text
cmd/{app}/main.go                         # composition and entry runner
cmd/migrate/                             # PostgreSQL migration job
internal/
  api/
    core/{entry,rpcstatus}/               # shared entry mechanics
    startup/                             # base-only entry
    {http,grpc,cli}/
      pipeline/                          # fixed transport code
      middleware/{name}/                 # fixed and application middleware
      endpoint/                          # resource handlers
      routes.go | services.go | commands.go
    management/                          # health entry
  command/
    {operation}.go                       # command, result, handler
    handlers.go                          # wrapped-handler registry
    factory/                             # optional state-based selection
    pipeline/                            # handler and middleware contracts
    middleware/{logging,unitofwork,...}/
  domain/
    entity/                              # invariants and row-version declaration
    service/                             # reusable domain behavior
  service/                               # API/store adaptation; consumer-owned ports
  client/
    core/{outcome,pipeline,httpclient,grpcclient}/
    middleware/retry/
    {api}/                               # logical DTOs and protocol client
      contract/dto.go
      mock/
      test/                              # emulator and contract scenarios
  infrastructure/postgres/               # database, repositories, rows, selector,
                                         # specifications, migrations and UoW hook
  config/
  logging/masked/                        # masking extends the base logger
  shared/errorkind/
  version/
gen/client/{api}/                        # generated client wire types/stubs
gen/api/{contract}/                      # generated entry wire types/stubs
docs/integration/{api,client}/{contract}/ # published/consumed wire contracts
```

## VP ownership and placement

Paths below are relative to the application package/root shown above. The source
links define behavior; the tree defines where its elements fit together.

| Capability | Placement | Contracts |
|---|---|---|
| Base entry, command/domain, settings, logger, runner and root | `api/startup, command, domain, config, logging, cmd/{app}` | [base-plateau](../../spec/base-plateau.skill/base-plateau.skill.md) · [Go](../../go/base-plateau-in-go.skill/base-plateau-in-go.skill.md) |
| Command/API middleware contracts and registration | `command/pipeline, command/middleware, api/{transport}/middleware` | [base-plateau-extension](../../spec/base-plateau-extension.skill/base-plateau-extension.skill.md) · [Go](../../go/base-plateau-extension-in-go.skill/base-plateau-extension-in-go.skill.md) |
| HTTP entry, endpoints, middleware and contract/docs | `api/http` | [vp-http-api](../../spec/vp-http-api.skill/vp-http-api.skill.md) · [Go](../../go/vp-http-api-in-go.skill/vp-http-api-in-go.skill.md) |
| gRPC entry, endpoints, interceptors and proto | `api/grpc, gen/api` | [vp-grpc-api](../../spec/vp-grpc-api.skill/vp-grpc-api.skill.md) · [Go](../../go/vp-grpc-api-in-go.skill/vp-grpc-api-in-go.skill.md) |
| CLI entry and action registration | `api/cli` | [vp-cli-api](../../spec/vp-cli-api.skill/vp-cli-api.skill.md) · [Go](../../go/vp-cli-api-in-go.skill/vp-cli-api-in-go.skill.md) |
| Management entry and readiness contract | `api/management` | [vp-health](../../spec/vp-health.skill/vp-health.skill.md) · [Go](../../go/vp-health-in-go.skill/vp-health-in-go.skill.md) |
| Payload masking and request-id logging | `logging/masked` | [vp-logging](../../spec/vp-logging.skill/vp-logging.skill.md) · [Go](../../go/vp-logging-in-go.skill/vp-logging-in-go.skill.md) |
| HTTP mechanics, entity client/DTOs, emulator, adapting service | `client/core/httpclient, client/{api}, service` | [vp-http-client](../../spec/vp-http-client.skill/vp-http-client.skill.md) · [Go](../../go/vp-http-client-in-go.skill/vp-http-client-in-go.skill.md) |
| gRPC mechanics/generated wire code, same logical DTO/service contract | `client/core/grpcclient, client/{api}, gen/client, service` | [vp-grpc-client](../../spec/vp-grpc-client.skill/vp-grpc-client.skill.md) · [Go](../../go/vp-grpc-client-in-go.skill/vp-grpc-client-in-go.skill.md) |
| Optional client middleware, settings and safe-operation marks | `client/middleware/retry` | [vp-client-retry](../../spec/vp-client-retry.skill/vp-client-retry.skill.md) · [Go](../../go/vp-client-retry-in-go.skill/vp-client-retry-in-go.skill.md) |
| Own-store repository, mappings/specifications, migrations and readiness | `infrastructure/postgres, service, cmd/migrate` | [vp-postgres](../../spec/vp-postgres.skill/vp-postgres.skill.md) · [Go](../../go/vp-postgres-in-go.skill/vp-postgres-in-go.skill.md) |
| Command transaction middleware and store interception | `command/middleware/unitofwork, infrastructure/postgres` | [vp-unit-of-work](../../spec/vp-unit-of-work.skill/vp-unit-of-work.skill.md) · [Go](../../go/vp-unit-of-work-in-go.skill/vp-unit-of-work-in-go.skill.md) |
| Domain/store version declaration and atomic write check | `domain/entity, infrastructure/postgres` | [vp-row-version](../../spec/vp-row-version.skill/vp-row-version.skill.md) · [Go](../../go/vp-row-version-in-go.skill/vp-row-version-in-go.skill.md) |
| Caller identity validation and table primary-key policy | `No stack realization; entry validation and own-store table policy` | [vp-external-id](../../spec/vp-external-id.skill/vp-external-id.skill.md) |

## Planned capability placement

These capabilities have no current stack realization linked here. Proposed roots
identify structural ownership, not completed code or a new selectable composition.

| Capability | Proposed owner / placement |
|---|---|
| SQLite | Own-store infrastructure alternative to PostgreSQL; exact driver/mapping paths belong to its VP |
| Edit timestamps | Domain/table metadata and own-store mappings; no separate integration root |
| Idempotency | Entry/command boundary plus an own-store key/answer record; exact contract not selected |
| Kafka/RabbitMQ inbound | API entry/listener under the transport's entry directory |
| Kafka/RabbitMQ outbound | Broker infrastructure behind its providing integration service |
| TaskBox, Inbox, Outbox | Own-store infrastructure and worker/entry registration; claim transaction contract must be settled |
| Data cache | Composition unresolved under the no-service-to-service constraint; no wrapper call edge is implied |

## Composition and conflicts

- An API entry replaces Startup; health adds its own entry. HTTP and gRPC can share
  a logical contract through separate entries. CLI follows its application shape.
- Own-store variants replace infrastructure implementations; they do not force
  entity clients or API entry code into infrastructure.
- Client DTOs and technical outcomes have one logical contract per API/resource.
  Service alone constructs Domain Model and composes clients of its one system.
- Consumer-owned ports keep concrete dependencies in the composition root. No
  service calls another service; cross-system work belongs to command/domain logic.
- Layer layouts, templates, examples and these ownership rows must change together
  when an element is added or moved.
