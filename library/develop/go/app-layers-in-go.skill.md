---
name: app-layers-in-go
description: The application layers in Go — one package per layer under internal/, interfaces declared by their consumer so that only cmd/{app}/main.go imports both sides, entities with unexported fields, and hand-written fakes for the layer below
whenToUse: when you design a Go application or feature, when you add or change a Go entry point, command, entity, domain service, service, or infrastructure adapter, or when you review such a change
updated: 20261022
tags:
  - stack/go
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
---

# Goal
- **Layers in Go packages** - A Go module that keeps every rule of [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md); this skill details that skill's implementation for Go and restates none of its rules.
- **Layers joined only in main** - A consumer never constructs what it calls through an interface; `cmd/{app}/main.go` builds every implementation and passes it in.

# Core Principle
- **The consumer owns the interface** - In Go an interface is declared where it is used and lists only the methods used there; the implementing struct declares none for itself.

# Rule

## MUST

### One package per layer
Place each layer of [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md) in its package.

| Layer | Package |
|---|---|
| api | `internal/api/{transport}` (`http`, `grpc`, `cli`, `kafka`, ...), split per [Split each entry point](#split-each-entry-point-into-pipeline-endpoint-and-registration) |
| api — the fixed code the entry points share | `internal/api/core/{part}` (`entry`, `rpcstatus`), per [Keep the code the entry points share in core](#keep-the-code-the-entry-points-share-in-core) |
| command | `internal/command` — one file per command, nothing else |
| command pipeline (handler contract, decorators, error kind) | `internal/command/pipeline` |
| command factory | `internal/command/factory` |
| entity | `internal/domain/entity` |
| domain service | `internal/domain/service`, imported as `domainservice` |
| service | `internal/service` |
| infrastructure | `internal/infrastructure/{kind}` (`postgres`, `userapi`, ...) |
| composition root | `cmd/{app}/main.go` |

- Violation: entities and domain services in one `internal/domain` package, or `internal/domain/service` imported without the `domainservice` name.
- Risk: a layer in a package of its own choosing hides which rules apply to it, and an import check cannot tell a downward call from an upward one; in one package, an entity can reach a domain service's interface to a service unnoticed; an unaliased import reads as `internal/service`.
- Fix: move the code into the package of its layer, and import the domain-service package as `domainservice "{module-path}/internal/domain/service"`.

### Split each entry point into pipeline, endpoint, and registration
Lay out every entry point `internal/api/{kind}/` (`http`, `grpc`, `cli`, later `kafka`) in three parts, and import them under these names.

```
internal/api/{kind}/
├── pipeline/            the fixed code, copied from templates: config, server or runner,
│   ├── ...              the generic adapter (http.Endpoint, grpc.Call, cli.Command),
│   └── logging.go       and one file per wrapper around every call (logging.go, recover.go)
├── endpoint/            one file per resource or service: {Resource}Handler (or listener/ for a broker)
└── routes.go            the registration: Endpoints (one field per resource), the list, Run
                         (services.go for gRPC, commands.go for the CLI)
```

| Package | Imported as |
|---|---|
| `internal/command/pipeline` | `pipeline` |
| `internal/api/{kind}/pipeline` | `{kind}pipeline` — `httppipeline`, `grpcpipeline`, `clipipeline`; inside it, the command layer's pipeline is `commandpipeline` |
| `internal/api/{kind}` | `{kind}api` — `httpapi`, `grpcapi`, `cliapi` |
| `internal/api/{kind}/endpoint` | `endpoint`; `{kind}endpoint` where `main.go` imports two |

- Violation: a resource file in `pipeline/`; a wrapper added to `server.go`; two packages named `pipeline` imported without an alias.
- Risk: application code mixed into the copied package is lost when the templates are copied again; a wrapper hidden in another file is not seen in the list of files; an unaliased `pipeline` reads as the command layer's.
- Fix: `{Resource}Handler` (`ItemsHandler`) holds the command handlers of one resource and lists its operations (`Routes()`, `Service()`, `Resource()`); the registration file collects them in `Endpoints` and starts the entry point with `Run`; `main.go` fills `Endpoints` and calls `{kind}api.Run`.

### Keep the code the entry points share in core
Put the fixed code that serves several entry points and belongs to none of them — the runner of the entry points (`entry`), the table of error kinds (`rpcstatus`) — in its own package under `internal/api/core/`, never in a transport's package or beside the transports.
- Violation: `rpcstatus` inside `internal/api/http/pipeline`; `internal/api/entry` beside `internal/api/http`.
- Risk: removing a transport removes code the other entry points still use; a shared package beside the transports reads as one more entry point.
- Fix: `core/` holds only code copied from templates; each part appears with the base plateau (`entry`) or with the first variation point that needs it (`rpcstatus`).

### Declare an interface in the consumer's package
Declare the interface a command, factory, or domain service uses to reach a service in that consumer's package, and the interface a service uses to reach its infrastructure in `internal/service`, each listing only the methods that consumer calls.

```go
// internal/command/add_user.go - the command declares what it needs
type users interface {
	Exists(ctx context.Context, name string) (bool, error)
	Add(ctx context.Context, name string) (domain.UserID, error)
}

// internal/service/user_service.go - a struct; no interface declared for itself
type UserService struct{ api userAPI } // userAPI: declared here, implemented in internal/infrastructure/userapi
```

- Violation: `internal/service` declaring `type UserServiceInterface interface {...}` next to `UserService`, imported by commands.
- Risk: an interface declared by the implementation grows to every method any caller uses, and the caller's package must import the implementation's package.
- Fix: one small interface per consumer; the struct satisfies it implicitly.

### Wire implementations only in main
Import `internal/service` only from `cmd/{app}/main.go`, and `internal/infrastructure/{kind}` only from `main.go` and from the one service over it — for the types its methods return — and pass each implementation to its consumer's constructor in `main.go`.
- Violation: `internal/command` importing `internal/service` to construct a `UserService`, or a service constructing its HTTP client itself.
- Risk: the consumer depends on the concrete implementation, so it cannot be tested with a fake or given another implementation without editing it.
- Fix: the consumer's constructor takes the interface; `main.go` builds the struct and passes it.

### Entities guard their fields
Give an entity unexported fields, a `New{Entity}` constructor that rejects an invalid entity, and a method for each allowed state change that returns its rule error.
- Risk: an exported field can be set past every rule, from any package.
- Fix: unexport the field and add a getter for what results must show.

### Fake the layer below by hand
Write the fake of the layer below as a small struct in the test package that implements the consumer's interface, recording calls and returning what the scenario sets.
- Violation: a command test that starts the real service with an HTTP stub server behind it.
- Risk: the test depends on two layers' contracts and breaks on a change below the service.
- Fix: `type fakeUsers struct{ exists bool; added []string }` implementing the command's `users` interface.

# Check list
- [ ] Every layer is in its package per [One package per layer](#one-package-per-layer); the code the entry points share is in `internal/api/core/`.
- [ ] Every interface to a service or an infrastructure adapter is declared in its consumer's package and lists only the methods that consumer calls.
- [ ] `internal/service` is imported only by `main.go`; `internal/infrastructure/{kind}` only by `main.go` and the one service over it.
- [ ] Entities have unexported fields, a validating constructor, and guarded state-change methods.
- [ ] Tests fake the layer below with hand-written structs implementing the consumer's interface.
