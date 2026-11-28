# .NET application layout

[Base capabilities](../base-capabilities.md) · [Catalog](../catalog.md)

This is the full catalog placement overlay. Optional directories appear only when
their capability is selected; repeated shared directories retain one owner.
Fixed code comes from the owning skill's templates. Entity clients, endpoints,
commands, domain behavior and adapting services are application-specific.

```text
src/
  App/
    App.Host/
      Program.cs                         # composition and service wiring
      Api/
        Core/{Entry,Errors,Hosting}/
        Startup/                         # base-only entry
        {Http,Grpc,Cli}/
          Pipeline/
          Middleware/{Name}/
          Endpoint/
          Routes.cs | Services.cs
        Management/                      # health entry
    App.Migrate/                         # own-store migration process
  Modules/{Module}/
    {Module}.Commands/                   # command/result contracts
    {Module}.Application/
      Commands/                          # handlers
      Factories/                         # optional selection
      Services/I{Service}.cs             # consumer-owned service ports
      Specifications/                    # own-store queries
    {Module}.Domain/
      Entities/
      Services/                          # domain behavior and its service ports
      Configurations/                    # entity mappings owned by the module
      Errors/
  Service/{Module}/{Module}.Service/      # concrete integration services
                                         # references consumer ports, Domain,
                                         # Client abstractions, never handlers
  Client/
    Core/{Client.Abstractions,Client.Pipeline,Client.Http,Client.Grpc}/
    Middleware/Retry/
    {Api}/{Api}.Abstractions/             # logical API DTOs and client contract
    {Api}/{Api}/                         # protocol implementation and Mock/
  Infrastructure/Postgres/Postgres/      # own-store context, repositories,
                                         # migrations, readiness and UoW hook
  Shared.Persistence/                    # own-store repository contracts
  Shared/{Errors,Logging,UnitOfWork}/
  BuildingBlocks/{Pipeline,Logging}/
  Middleware/{Logging,StoreFailures,UnitOfWork,...}/
  MaskedLogging/
docs/integration/{api,client}/{contract}/ # generated proto at build time
{Project}.Tests/                         # beside each subject project;
                                         # service tests beside Module.Service
```

## VP ownership and placement

Paths below are relative to the application package/root shown above. The source
links define behavior; the tree defines where its elements fit together.

| Capability | Placement | Contracts |
|---|---|---|
| Base entry, command/domain, settings, logger, runner and root | `App.Host/Api/Startup, Modules, BuildingBlocks, Shared` | [base-plateau](../../spec/base-plateau.skill/base-plateau.skill.md) · [.NET](../../dotnet/base-plateau-in-dotnet.skill/base-plateau-in-dotnet.skill.md) |
| Command/API middleware contracts and registration | `BuildingBlocks/Pipeline, Middleware, App.Host/Api` | [base-plateau-extension](../../spec/base-plateau-extension.skill/base-plateau-extension.skill.md) · [.NET](../../dotnet/base-plateau-extension-in-dotnet.skill/base-plateau-extension-in-dotnet.skill.md) |
| HTTP entry, endpoints, middleware and contract/docs | `App.Host/Api/Http` | [vp-http-api](../../spec/vp-http-api.skill/vp-http-api.skill.md) · [.NET](../../dotnet/vp-http-api-in-dotnet.skill/vp-http-api-in-dotnet.skill.md) |
| gRPC entry, endpoints, interceptors and proto | `App.Host/Api/Grpc` | [vp-grpc-api](../../spec/vp-grpc-api.skill/vp-grpc-api.skill.md) · [.NET](../../dotnet/vp-grpc-api-in-dotnet.skill/vp-grpc-api-in-dotnet.skill.md) |
| CLI entry and action registration | `App.Host/Api/Cli` | [vp-cli-api](../../spec/vp-cli-api.skill/vp-cli-api.skill.md) · [.NET](../../dotnet/vp-cli-api-in-dotnet.skill/vp-cli-api-in-dotnet.skill.md) |
| Management entry and readiness contract | `App.Host/Api/Management` | [vp-health](../../spec/vp-health.skill/vp-health.skill.md) · [.NET](../../dotnet/vp-health-in-dotnet.skill/vp-health-in-dotnet.skill.md) |
| Payload masking and request-id logging | `MaskedLogging, BuildingBlocks/Logging` | [vp-logging](../../spec/vp-logging.skill/vp-logging.skill.md) · [.NET](../../dotnet/vp-logging-in-dotnet.skill/vp-logging-in-dotnet.skill.md) |
| HTTP mechanics, entity client/DTOs, emulator, adapting service | `Client/Core/Client.Http, Client/{Api}, Service/{Module}` | [vp-http-client](../../spec/vp-http-client.skill/vp-http-client.skill.md) · [.NET](../../dotnet/vp-http-client-in-dotnet.skill/vp-http-client-in-dotnet.skill.md) |
| gRPC mechanics/generated wire code, same logical DTO/service contract | `Client/Core/Client.Grpc, Client/{Api}, Service/{Module}` | [vp-grpc-client](../../spec/vp-grpc-client.skill/vp-grpc-client.skill.md) · [.NET](../../dotnet/vp-grpc-client-in-dotnet.skill/vp-grpc-client-in-dotnet.skill.md) |
| Optional client middleware, settings and safe-operation marks | `Client/Middleware/Retry` | [vp-client-retry](../../spec/vp-client-retry.skill/vp-client-retry.skill.md) · [.NET](../../dotnet/vp-client-retry-in-dotnet.skill/vp-client-retry-in-dotnet.skill.md) |
| Own-store repository, mappings/specifications, migrations and readiness | `Infrastructure/Postgres, Shared.Persistence, Domain/Configurations, Application/Specifications, App.Migrate` | [vp-postgres](../../spec/vp-postgres.skill/vp-postgres.skill.md) · [.NET](../../dotnet/vp-postgres-in-dotnet.skill/vp-postgres-in-dotnet.skill.md) |
| Command transaction middleware and store interception | `Middleware/UnitOfWork, Infrastructure/Postgres, Shared/UnitOfWork` | [vp-unit-of-work](../../spec/vp-unit-of-work.skill/vp-unit-of-work.skill.md) · [.NET](../../dotnet/vp-unit-of-work-in-dotnet.skill/vp-unit-of-work-in-dotnet.skill.md) |
| Domain/store version declaration and atomic write check | `Domain/Entities, Infrastructure/Postgres` | [vp-row-version](../../spec/vp-row-version.skill/vp-row-version.skill.md) · [.NET](../../dotnet/vp-row-version-in-dotnet.skill/vp-row-version-in-dotnet.skill.md) |
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
