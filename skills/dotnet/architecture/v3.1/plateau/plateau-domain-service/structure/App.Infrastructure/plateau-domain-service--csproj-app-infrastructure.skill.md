---
name: plateau-domain-service--csproj-app-infrastructure
description: Project App.Infrastructure in the plateau-domain-service plateau — the single home for outbound infrastructure (AppDbContext, generic repository, unit of work, concurrency version-resolver factory, gRPC client wrappers)
whenToUse: when adding or editing an outbound-integration implementation (persistence, a gRPC client wrapper, the version-resolver factory), or deciding whether infrastructure code belongs here
domain: skill
type: template
plateau: domain-service
version: 20260902000000
tags:
  - skill/template/csproj
  - plateau/domain-service
created_by:
  - "[[../../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]]"
  - "[[../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]]"
  - "[[../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]]"
  - "[[../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]]"
  - "[[../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]]"
---

# Goal
- Be the one project every outbound integration extends: the service's single `AppDbContext`, the generic `Repository<T>`, `UnitOfWork`, `EntityVersionResolverFactory`, and per-dependency gRPC client wrappers.
- Be referenced only by `App.Host`; no module `Application`/`Domain`/`Api` references it — they see only abstractions in `Shared`.

__Applied solutions:__
- [[../../../../solutions/solution-infrastructure-project.skill/solution-infrastructure-project.skill.md|solution-infrastructure-project]] - [[../../../../solutions/solution-infrastructure-project.skill/Implementation/App.Infrastructure.csproj.create.md|App.Infrastructure.csproj.create]]

# Core Principles
- `AppDbContext` (`/Persistence`) is the only class that knows EF's model API — applies every module's configs by assembly scan; sets server timestamps and bumps the concurrency token in `OnBeforeSaving`.
- `Repository<T>` (`/Repositories`) inherits Ardalis `RepositoryBase<T>`, implements `Shared.Repositories.IRepository<T>`, never calls `SaveChangesAsync`.
- `UnitOfWork` (`/UnitOfWork`) is the only place `DbContext.SaveChangesAsync` is called.
- `EntityVersionResolverFactory` (`/Concurrency`) maps a stable entity name to its `{Module}.Application` resolver by scanning Domain configs (`IEntityTypeConfiguration<T>` where `T : IVersioned`) and Application resolvers.
- gRPC client wrappers (`/Clients`, `/Protos`) turn a generated stub into an `I{Dependency}Client` returning `Result<T>`.
- Wired into the composition root through one `AddInfrastructure()` extension.

# Structure

## Solution place
```
/src/App/App.Infrastructure
```

## Project Structure
- /App.Infrastructure
  - /Persistence/[AppDbContext.cs](./classes/plateau-domain-service--class-app-dbcontext.skill.md), /Persistence/Configurations/{Module1}To{Module2}Config.cs
  - /Repositories/[Repository.cs](./classes/plateau-domain-service--class-repository.skill.md)
  - /UnitOfWork/[UnitOfWork.cs](./classes/plateau-domain-service--class-unit-of-work.skill.md)
  - /Concurrency/[EntityVersionResolverFactory.cs](./classes/plateau-domain-service--class-entity-version-resolver-factory.skill.md)
  - /Protos/{Dependency}.proto, /Clients/[{Dependency}GrpcClient.cs](./classes/plateau-domain-service--class-dependency-grpc-client.skill.md), /Clients/[GrpcStatusExtensions.cs](./classes/plateau-domain-service--class-grpc-status-extensions.skill.md)
  - App.Infrastructure.csproj

## Directory and class skills
| `Directory\|file` | Description | Pattern skill |
| --- | --- | --- |
| /Persistence/AppDbContext.cs | The service's single `DbContext` | [[./classes/plateau-domain-service--class-app-dbcontext.skill.md\|class-app-dbcontext]] |
| /Repositories/Repository.cs | Generic Ardalis-backed repository | [[./classes/plateau-domain-service--class-repository.skill.md\|class-repository]] |
| /UnitOfWork/UnitOfWork.cs | The single `SaveChangesAsync` call site | [[./classes/plateau-domain-service--class-unit-of-work.skill.md\|class-unit-of-work]] |
| /Concurrency/EntityVersionResolverFactory.cs | Name → resolver map (assembly scan) | [[./classes/plateau-domain-service--class-entity-version-resolver-factory.skill.md\|class-entity-version-resolver-factory]] |
| /Clients/{Dependency}GrpcClient.cs | Generated stub wrapped as `I{Dependency}Client` | [[./classes/plateau-domain-service--class-dependency-grpc-client.skill.md\|class-dependency-grpc-client]] |
| /Clients/GrpcStatusExtensions.cs | `RpcException` → `Result` mapping | [[./classes/plateau-domain-service--class-grpc-status-extensions.skill.md\|class-grpc-status-extensions]] |

## NuGet Packages
| Package | Purpose |
| --- | --- |
| Microsoft.EntityFrameworkCore (+ provider) | `AppDbContext`, `DbContext` |
| Ardalis.Specification.EntityFrameworkCore | `RepositoryBase<T>` |
| Grpc.Net.ClientFactory, Google.Protobuf, Grpc.Tools | generated gRPC client stubs |

## What Does NOT Belong Here
- Business logic, entities, domain services — belong to `{Module}.Domain`.
- Handlers, validators, specs — belong to `{Module}.Application`.
- Cross-cutting contracts (`IRepository`, `IUnitOfWork`, `I{Dependency}Client`) — belong to `Shared`.

## Allowed Dependencies
- `Shared`, `BuildingBlocks`, every `{Module}.Domain`, every `{Module}.Interfaces`

# Rules
MUST:
- Keep `App.Infrastructure` referenced only by `App.Host`; never let a module project reference it.
- Keep `AppDbContext` the only `DbContext`; apply configs only via `ApplyConfigurationsFromAssembly`.
- Keep `SaveChangesAsync` reachable only through `UnitOfWork` (itself invoked only by `UnitOfWorkBehavior`).
- Register the version-resolver factory as `Scoped` with explicit Domain + Application assembly lists — never `AppDomain.CurrentDomain.GetAssemblies()`, never a hardcoded resolver dictionary.
- Never put concern-specific code in `solution-infrastructure-project`'s own slice — each concern's solution adds its own folder.

__Applied solutions:__
- [[../../../../solutions/solution-repository-integration.skill/solution-repository-integration.skill.md|solution-repository-integration]] - [[../../../../solutions/solution-repository-integration.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[../../../../solutions/solution-unit-of-work.skill/solution-unit-of-work.skill.md|solution-unit-of-work]] - [[../../../../solutions/solution-unit-of-work.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[../../../../solutions/solution-entity-concurrency-change.skill/solution-entity-concurrency-change.skill.md|solution-entity-concurrency-change]] - [[../../../../solutions/solution-entity-concurrency-change.skill/Implementation/App.Infrastructure.csproj.extend.md|App.Infrastructure.csproj.extend]]
- [[../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]] - [[../../../../solutions/solution-grpc-client.skill/solution-grpc-client.skill.md|solution-grpc-client]]

# Check list
- [ ] `App.Infrastructure.csproj` referenced only by `App.Host`; references `Shared` + `BuildingBlocks` + every module's `Domain`/`Interfaces`.
- [ ] Exactly one `DbContext`; configs applied only by assembly scan.
- [ ] `Repository<T>` never calls `SaveChangesAsync`; `UnitOfWork` is the only call site.
- [ ] `EntityVersionResolverFactory` registered `Scoped`, no hardcoded map.
