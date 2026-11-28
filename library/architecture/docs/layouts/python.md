# Python application layout

[Base capabilities](../base-capabilities.md) · [Catalog](../catalog.md)

This is the full catalog placement overlay. Optional directories appear only when
their capability is selected; repeated shared directories retain one owner.
Fixed code comes from the owning skill's templates. Entity clients, endpoints,
commands, domain behavior and adapting services are application-specific.

```text
src/{package}/
  main.py                                # composition and runner
  __main__.py                            # process entry
  api/
    core/{entry,rpcstatus}/
    startup/                             # base-only entry
    {http,grpc,cli}/
      pipeline/
      middleware/{name}/
      endpoint/
      routes.py | services.py | commands.py
    management/                          # health entry
  command/
    {operation}.py                       # frozen commands/results and handlers
    handlers.py                          # wrapped-handler registry
    factory/
    pipeline/
    middleware/{logging,unit_of_work,...}/
  domain/{entity,service}/
  service/                               # adaptation; Protocols owned by consumers
  client/
    core/
      outcomes.py
      {pipeline,httpclient,grpcclient}/
    middleware/retry/
    {api}/
      dto.py                             # logical API DTOs, no protocol imports
      {api}.py                           # protocol implementation
      mock/
      test/
  infrastructure/postgres/               # rows, selectors, UoW hook
    migrations/versions/                  # Alembic revisions
  log/masked/
  config/
  shared/errors/
  gen/{api,client}/{contract}/            # generated wire types/stubs
  migrate.py                             # PostgreSQL migration process
alembic.ini                              # migration configuration
docs/integration/{api,client}/{contract}/
```

## VP ownership and placement

Paths below are relative to the application package/root shown above. The source
links define behavior; the tree defines where its elements fit together.

| Capability | Placement | Contracts |
|---|---|---|
| Base entry, command/domain, settings, logger, runner and root | `api/startup, command, domain, config, log, main.py` | [base-plateau](../../spec/base-plateau.skill/base-plateau.skill.md) · [Python](../../python/base-plateau-in-python.skill/base-plateau-in-python.skill.md) |
| Command/API middleware contracts and registration | `command/pipeline, command/middleware, api/{transport}/middleware` | [base-plateau-extension](../../spec/base-plateau-extension.skill/base-plateau-extension.skill.md) · [Python](../../python/base-plateau-extension-in-python.skill/base-plateau-extension-in-python.skill.md) |
| HTTP entry, endpoints, middleware and contract/docs | `api/http` | [vp-http-api](../../spec/vp-http-api.skill/vp-http-api.skill.md) · [Python](../../python/vp-http-api-in-python.skill/vp-http-api-in-python.skill.md) |
| gRPC entry, endpoints, interceptors and proto | `api/grpc, gen/api` | [vp-grpc-api](../../spec/vp-grpc-api.skill/vp-grpc-api.skill.md) · [Python](../../python/vp-grpc-api-in-python.skill/vp-grpc-api-in-python.skill.md) |
| CLI entry and action registration | `api/cli` | [vp-cli-api](../../spec/vp-cli-api.skill/vp-cli-api.skill.md) · [Python](../../python/vp-cli-api-in-python.skill/vp-cli-api-in-python.skill.md) |
| Management entry and readiness contract | `api/management` | [vp-health](../../spec/vp-health.skill/vp-health.skill.md) · [Python](../../python/vp-health-in-python.skill/vp-health-in-python.skill.md) |
| Payload masking and request-id logging | `log/masked` | [vp-logging](../../spec/vp-logging.skill/vp-logging.skill.md) · [Python](../../python/vp-logging-in-python.skill/vp-logging-in-python.skill.md) |
| HTTP mechanics, entity client/DTOs, emulator, adapting service | `client/core/httpclient, client/{api}, service` | [vp-http-client](../../spec/vp-http-client.skill/vp-http-client.skill.md) · [Python](../../python/vp-http-client-in-python.skill/vp-http-client-in-python.skill.md) |
| gRPC mechanics/generated wire code, same logical DTO/service contract | `client/core/grpcclient, client/{api}, gen/client, service` | [vp-grpc-client](../../spec/vp-grpc-client.skill/vp-grpc-client.skill.md) · [Python](../../python/vp-grpc-client-in-python.skill/vp-grpc-client-in-python.skill.md) |
| Optional client middleware, settings and safe-operation marks | `client/middleware/retry` | [vp-client-retry](../../spec/vp-client-retry.skill/vp-client-retry.skill.md) · [Python](../../python/vp-client-retry-in-python.skill/vp-client-retry-in-python.skill.md) |
| Own-store repository, mappings/specifications, migrations and readiness | `infrastructure/postgres, service, infrastructure/postgres/migrations, migrate.py` | [vp-postgres](../../spec/vp-postgres.skill/vp-postgres.skill.md) · [Python](../../python/vp-postgres-in-python.skill/vp-postgres-in-python.skill.md) |
| Command transaction middleware and store interception | `command/middleware/unit_of_work, infrastructure/postgres` | [vp-unit-of-work](../../spec/vp-unit-of-work.skill/vp-unit-of-work.skill.md) · [Python](../../python/vp-unit-of-work-in-python.skill/vp-unit-of-work-in-python.skill.md) |
| Domain/store version declaration and atomic write check | `domain/entity, infrastructure/postgres` | [vp-row-version](../../spec/vp-row-version.skill/vp-row-version.skill.md) · [Python](../../python/vp-row-version-in-python.skill/vp-row-version-in-python.skill.md) |
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
