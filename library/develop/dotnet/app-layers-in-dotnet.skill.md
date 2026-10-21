---
name: app-layers-in-dotnet
description: The application layers in dotnet — one project per layer of each module (Domain, Interfaces, Application) plus Shared, BuildingBlocks, and App.Host, project references that let the compiler hold the direction of the calls, entry points that see only a module's Interfaces, and entities that guard their state
whenToUse: when you design a dotnet application or feature, when you add or change a dotnet entry point, command, handler, entity, or domain service, or when you review such a change
updated: 20261021
tags:
  - stack/dotnet
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
---

# Goal
- **Layers in dotnet projects** - A dotnet solution that keeps every rule of [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md); this skill details that skill's implementation for dotnet and restates none of its rules.
- **The compiler holds the boundaries** - A layer that may not call another has no project reference to it, so the wrong call does not build.

# Core Principle
- **A module is three projects** - `{Module}.Domain`, `{Module}.Interfaces`, `{Module}.Application`; what is the same for every module (error kinds, pipeline behaviors, logging) lives in `Shared` and `BuildingBlocks`, and `App.Host` joins them.

# Scope
- Covers the api, command, and domain layers and the composition root. Where the service and infrastructure layers go — and which project declares the interface a handler uses to reach a service — is decided with the first service variation point (step 2 of the architecture catalog).

# Rule

## MUST

### One project per layer
Place each layer of [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md) in its project and folder.

| Layer | Project / folder |
|---|---|
| api | `src/App/App.Host/Api/{Transport}` (`Startup`, `Http`, `Grpc`, ...), split per [Split each entry point](#split-each-entry-point-into-pipeline-endpoint-and-registration) |
| command — the command and its result types | `src/Modules/{Module}/{Module}.Interfaces/Commands/{Command}.cs`, one file per command |
| command — the handler | `src/Modules/{Module}/{Module}.Application/Commands/{Command}Handler.cs`, one file per handler |
| command factory | `src/Modules/{Module}/{Module}.Application/Factories` |
| command pipeline (behaviors), logging | `src/BuildingBlocks/Pipeline`, `src/BuildingBlocks/Logging` |
| error kinds | `src/Shared/Errors`, per [error-kinds-in-dotnet](library/develop/dotnet/error-kinds-in-dotnet.skill/error-kinds-in-dotnet.skill.md) |
| entity | `src/Modules/{Module}/{Module}.Domain/Entities` |
| domain service | `src/Modules/{Module}/{Module}.Domain/Services` |
| the module's rule errors | `src/Modules/{Module}/{Module}.Domain/Errors/{Module}Errors.cs` |
| composition root | `src/App/App.Host/Program.cs` |

- Violation: a handler in `{Module}.Interfaces`; an entity in `{Module}.Application`; a module's own `Behaviors` folder.
- Risk: a part in a project of its own choosing gets references its layer may not have, and the compiler no longer stops a call upward.
- Fix: move the type to the project of its layer; one namespace per folder (`Todo.Domain.Entities`).

### Reference only downward
Give each project only these references, so a call against [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md)'s table does not compile.

| Project | References |
|---|---|
| `Shared` | nothing — no project, no package |
| `{Module}.Domain` | `Shared` |
| `{Module}.Interfaces` | the package `MediatR.Contracts` only |
| `{Module}.Application` | `{Module}.Interfaces`, `{Module}.Domain`, `MediatR` |
| `BuildingBlocks` | `Shared`, `MediatR`, `Microsoft.Extensions.Logging.Abstractions` |
| `App.Host` | every project above |
| `tests/{Project}.Tests` | the project it tests (and what that project references) |

- Violation: `{Module}.Domain` referencing MediatR or Entity Framework; `{Module}.Interfaces` referencing `{Module}.Domain` to return an entity.
- Risk: the domain depends on a framework and its rules can no longer be tested alone; a result carrying an entity lets an entry point change the domain outside a command.
- Fix: remove the reference and move the code that needed it to the layer that may have it.

### Entry points see only Interfaces
Write code under `App.Host/Api` against a module's `{Module}.Interfaces` types, `ISender`, `Shared`, and `BuildingBlocks` only; `App.Host` references `{Module}.Application` for `Program.cs` to register the handlers, and nothing in `Api/` uses its types or the domain's.
- Violation: an endpoint that creates a `PlanItemsHandler` or catches a module's error by its type.
- Risk: an entry point that reaches a handler or an entity skips the pipeline behaviors and depends on the domain.
- Fix: send the command through `ISender`; answer an exception by `ErrorKindException.KindOf`.

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
- [ ] Every layer is in its project and folder per [One project per layer](#one-project-per-layer).
- [ ] Every project has only the references of [Reference only downward](#reference-only-downward).
- [ ] Code under `App.Host/Api` uses only `{Module}.Interfaces`, `ISender`, `Shared`, and `BuildingBlocks`.
- [ ] Every entry point is split into `Pipeline/`, `Endpoint/`, and one registration file.
- [ ] Entities have no public setters, a validating constructor, and guarded state-change methods.
