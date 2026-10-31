---
name: app-layers-in-dotnet
description: The application layers in dotnet — one project per layer of each module (Domain, Commands, Application) plus Shared, BuildingBlocks, App.Host, and an Abstractions and an implementation project per infrastructure, project references that let the compiler hold the direction of the calls, entry points and other modules that see only a module's Commands, service interfaces declared beside their consumer, and entities that guard their state
whenToUse: when you design a dotnet application or feature, when you add or change a dotnet entry point, command, handler, entity, or domain service, or when you review such a change
updated: 20261030
tags:
  - stack/dotnet
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
adr:
  - adr/commands-project-and-service-interfaces.md
---

# Goal
- **Layers in dotnet projects** - A dotnet solution that keeps every rule of [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md); this skill details that skill's implementation for dotnet and restates none of its rules.
- **The compiler holds the boundaries** - A layer that may not call another has no project reference to it, so the wrong call does not build.

# Core Principle
- **A module is three projects** - `{Module}.Domain`, `{Module}.Commands`, `{Module}.Application`; what is the same for every module (error kinds, pipeline behaviors, logging) lives in `Shared` and `BuildingBlocks`, each infrastructure in `src/Infrastructure/{Infra}`, and `App.Host` joins them.
- **A module is entered only by its commands** - An entry point and another module both send a module's commands through `ISender`; nothing else of a module is visible outside it. Decision recorded in [commands-project-and-service-interfaces](./adr/commands-project-and-service-interfaces.md).

# Scope
- Covers every layer of app-layers and the composition root. What a database store looks like in dotnet (`DbContext`, repositories, specifications, the unit of work) is the DB variation point's.

# Rule

## MUST

### One project per layer
Place each layer of [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md) in its project and folder.

| Layer | Project / folder |
|---|---|
| api | `src/App/App.Host/Api/{Transport}` (`Startup`, `Http`, `Grpc`, ...), split per [Split each entry point](#split-each-entry-point-into-pipeline-endpoint-and-registration) |
| api — the fixed code the entry points share | `src/App/App.Host/Api/Core/{Part}` (`Entry`, `Errors`, `Hosting`), per [Keep the code the entry points share in Core](#keep-the-code-the-entry-points-share-in-core) |
| command — the command and its result types | `src/Modules/{Module}/{Module}.Commands/{Command}.cs`, one file per command |
| command — the handler | `src/Modules/{Module}/{Module}.Application/Commands/{Command}Handler.cs`, one file per handler |
| command factory | `src/Modules/{Module}/{Module}.Application/Factories` |
| command pipeline (behaviors), logging | `src/BuildingBlocks/Pipeline`, `src/BuildingBlocks/Logging` |
| error kinds | `src/Shared/Errors`, per [error-kinds-in-dotnet](library/develop/dotnet/error-kinds-in-dotnet.skill/error-kinds-in-dotnet.skill.md) |
| service — its interface, declared by its consumer | `src/Modules/{Module}/{Module}.Application/Services/I{Service}.cs` (a handler's); `{Module}.Domain/Services/I{Service}.cs` (a domain service's) |
| service — its implementation | `src/Modules/{Module}/{Module}.Application/Services/{Service}Service.cs` |
| infrastructure — its interface | `src/Infrastructure/{Infra}/{Infra}.Abstractions` (`Memory.Abstractions`, `Postgres.Abstractions`) |
| infrastructure — its implementation | `src/Infrastructure/{Infra}/{Infra}` |
| infrastructure — a client of another service's API | `src/Infrastructure/Client/{Api}/{Api}.Abstractions` and `{Api}`, per [api-client](library/develop/core/api-client.skill/api-client.skill.md); the fixed code of every client in `src/Infrastructure/Client/Core/` (`Client.Abstractions`, `Client.Pipeline`, `Client.Http`, `Client.Grpc`), each client middleware in `src/Infrastructure/Client/Middleware/{Name}` (`Retry`) |
| entity | `src/Modules/{Module}/{Module}.Domain/Entities` |
| domain service | `src/Modules/{Module}/{Module}.Domain/Services` |
| the module's rule errors | `src/Modules/{Module}/{Module}.Domain/Errors/{Module}Errors.cs` |
| composition root | `src/App/App.Host/Program.cs` |

- Violation: a handler in `{Module}.Commands`; an entity in `{Module}.Application`; a module's own `Behaviors` folder.
- Risk: a part in a project of its own choosing gets references its layer may not have, and the compiler no longer stops a call upward.
- Fix: move the type to the project of its layer; one namespace per folder (`Todo.Domain.Entities`).

### Reference only downward
Give each project only these references, so a call against [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md)'s table does not compile.

| Project | References |
|---|---|
| `Shared` | nothing — no project, no package |
| `{Module}.Domain` | `Shared` |
| `{Module}.Commands` | the package `MediatR.Contracts`; `Shared` for the log marks (`[LogMask]`, `[LogOmit]`) of a command's fields |
| `{Module}.Application` | `{Module}.Commands`, `{Module}.Domain`, `MediatR`; the `{Other}.Commands` of a module it calls; the `{Infra}.Abstractions` its services use |
| `{Infra}.Abstractions` | `Shared` at most — no package of the technique; an `{Api}.Abstractions` also `Client.Abstractions`, the outcomes every client reports |
| `{Infra}` | `{Infra}.Abstractions`, the packages of the technique (Npgsql, EF Core, a client SDK) |
| `BuildingBlocks` | `Shared`, `MediatR`, `Microsoft.Extensions.Logging.Abstractions` |
| `App.Host` | every project above |
| `{Project}.Tests`, beside `{Project}` | the project it tests (and what that project references) |

- Violation: `{Module}.Domain` referencing MediatR or Entity Framework; `{Module}.Commands` referencing `{Module}.Domain` to return an entity; `{Module}.Application` referencing `{Infra}` itself, or another module's `Application`.
- Risk: the domain depends on a framework and its rules can no longer be tested alone; a result carrying an entity lets an entry point change the domain outside a command.
- Fix: remove the reference and move the code that needed it to the layer that may have it.

### Entry points see only Commands
Write code under `App.Host/Api` against a module's `{Module}.Commands` types, `ISender`, `Shared`, and `BuildingBlocks` only; `App.Host` references `{Module}.Application` for `Program.cs` to register the handlers, and nothing in `Api/` uses its types or the domain's.
- Violation: an endpoint that creates a `PlanItemsHandler` or catches a module's error by its type.
- Risk: an entry point that reaches a handler or an entity skips the pipeline behaviors and depends on the domain.
- Fix: send the command through `ISender`; answer an exception by `ErrorKindException.KindOf`.

### Another module is reached only by its commands
Call another module by sending its command through `ISender` from a handler, with `{Module}.Application` referencing only `{Other}.Commands`; never through its services, handlers, or entities.
- Violation: `Orders.Application` referencing `Users.Application` to call `IUsers`; a service interface put in `{Module}.Commands` for other modules.
- Risk: a call past the other module's commands skips its pipeline behaviors (logging, the unit of work) and binds the two modules' insides together, so neither changes alone.
- Fix: add the command the caller needs to `{Other}.Commands`.

### Declare a service's interface beside its consumer
Declare the interface a handler uses to reach a service in `{Module}.Application/Services/`, beside its implementation, listing only the methods the module's handlers call; declare the interface a service uses to reach its infrastructure in `{Infra}.Abstractions`, in the technique's terms; build both implementations in `// [services]` of `Program.cs`.

```
Todo.Application/Services/IItemLists.cs        Add(listId, item), Items(listId)      — the application's language
Todo.Application/Services/ItemListsService.cs  over IListStore: "duplicate key" → conflict, "no rows" → not found
Infrastructure/Memory/Memory.Abstractions      IListStore: Insert(listId, row), Select(listId) — the technique's terms
Infrastructure/Memory/Memory                   MemoryListStore : IListStore
```

- Violation: `IItemLists` in `{Module}.Commands`, where entry points see it; a handler using `IListStore`; `new ItemListsService(...)` inside a handler.
- Risk: an entry point that reaches a service skips the command and its behaviors; a handler on the infrastructure's interface must change when the technique does; a service built by its consumer cannot be replaced or faked.
- Fix: the interface exists for the composition root to swap implementations (memory → a database) and for the handler's scenarios to fake it; it stays inside the module.

### Keep the code the entry points share in Core
Put the fixed code that serves several entry points and belongs to none of them — the runner of the entry points (`Entry`), the table of error kinds (`Errors/RpcStatus.cs`), the web host of every web entry point (`Hosting/WebHost.cs`) — in its own folder under `App.Host/Api/Core/`, never in a transport's folder or beside the transports.
- Violation: `RpcStatus.cs` inside `Api/Http/Pipeline`; `Api/Hosting` beside `Api/Http`.
- Risk: removing a transport removes code the other entry points still use; a shared folder beside the transports reads as one more entry point.
- Fix: `Core/` holds only code copied from templates — one folder to turn into a package later; each part appears with the base plateau (`Entry`) or with the first variation point that needs it (`Errors`, `Hosting`).

### Split each entry point into pipeline, endpoint, and registration
Lay out every entry point `App.Host/Api/{Transport}/` in three parts.

```
Api/{Transport}/
├── Pipeline/        the fixed code, copied from templates: settings, server, the generic
│   └── ...          adapter, one file per wrapper around every call (logging, recovery)
├── Endpoint/        one class per resource: {Resource}Handler (ItemsHandler), mapping its operations
└── Routes.cs        the registration: every {Resource}Handler, and the method that runs the entry
                     (Services.cs for gRPC)
```

- Violation: a resource's routes in `Pipeline/`; a wrapper added inside the server's file.
- Risk: application code mixed into the copied folder is lost when the templates are copied again; a wrapper hidden in another file is not seen in the list of files.
- Fix: `{Resource}Handler` takes `ISender` and maps its operations; the registration file collects the handlers; `Program.cs` builds them in `// [entries]`.

### Entities guard their state
Give an entity a constructor that throws its rule error for an invalid entity, properties with no public setter, and a method for each allowed state change that throws its rule error.
- Risk: a public setter lets any project set a state past every rule.
- Fix: `public bool Done { get; private set; }` changed only by `Complete()`.

## SHOULD

### Fake the layer below by hand
Write the fake of the layer below as a small class in the test project implementing the handler's interface, recording calls and returning what the scenario sets, rather than a mocking library.
- Risk: a mock configured per scenario repeats the interface's behaviour in setup code that no compiler checks.
- Fix: `sealed class FakeUsers : IUsers { public List<string> Added { get; } = []; ... }`.

# Check list
- [ ] Every layer is in its project and folder per [One project per layer](#one-project-per-layer); the code the entry points share is in `App.Host/Api/Core/`.
- [ ] Every project has only the references of [Reference only downward](#reference-only-downward).
- [ ] Another module is called only by its commands; no project outside a module references its `Application` or `Domain`.
- [ ] Each service interface is in its consumer's project (`Application/Services`, or `Domain/Services` for a domain service); each infrastructure interface is in `{Infra}.Abstractions`; implementations are built only in `// [services]`.
- [ ] Code under `App.Host/Api` uses only `{Module}.Commands`, `ISender`, `Shared`, and `BuildingBlocks`.
- [ ] Every entry point is split into `Pipeline/`, `Endpoint/`, and one registration file.
- [ ] Entities have no public setters, a validating constructor, and guarded state-change methods.
