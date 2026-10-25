# Pipeline

The **pipeline** (`internal/command/pipeline`) is the path every command takes from an entry point to its handler, and the package that holds that path's parts: the [handler contract](./handler-contract.md), [HandlerFunc](./handler-func.md), the [decorators](./handler-decorator.md) with the adapter `Decorate`, and the [error kind](./error-kind.md). It holds no command.

## Why it exists

Two kinds of code meet around a command. One is the commands themselves — `PlanItemsCommand` and its handler — which change with the business. The other is the machinery every command passes through — the contract, logging, later a unit of work — which is the same for all of them and rarely changes. Kept in one folder, the machinery hides among the commands and a reader cannot tell which file is a command. The pipeline package separates them: `internal/command` lists only commands.

## How it works

A call goes through the pipeline like water through a pipe with fittings: the entry point puts the command in, each decorator does its step around the call, and the handler at the end does the work.

```mermaid
flowchart LR
    E["Entry point<br/>(startup, HTTP)"] -->|command| L["WithLogging"]
    L --> U["WithUnitOfWork<br/>(a store variation point)"]
    U --> H["PlanItemsHandler<br/>internal/command"]
    H -. result or error .-> E
```

The fittings are listed in the composition root's `// [handlers]` section and fitted to every handler by the [command registry](./command-registry.md); the entry point and the handler never know which ones are there.

## How it is structured

| Part | In `pipeline` | Used by |
|---|---|---|
| `Handler[TCommand, TResult]` | the contract | every handler (implements), every entry point and decorator (calls) |
| `HandlerFunc` | a function as a handler | decorators, test stubs |
| `Decorator`, `Next`, `Decorate` (`decorator.go`) | a step around every command, and the adapter that wraps a typed handler with a list of them | decorators (`Decorator`), the command registry (`Decorate`) |
| `WithLogging` (`logging.go`) | the base plateau's decorator; a variation point's live in its own package | the composition root |
| `ErrInvalid`, `ErrNotFound`, ..., `KindOf` | the error kinds | entry points, to choose their response |

## Example

```go
// internal/command/handlers.go - every handler wrapped by the list main.go gives
PlanItems: pipeline.Decorate("PlanItems", NewPlanItemsHandler(), decorators),

// cmd/todo/main.go
handlers := command.NewHandlers([]pipeline.Decorator{pipeline.WithLogging(logger)})
```

In dotnet the same role is played by MediatR: `IRequestHandler<TRequest, TResponse>` is the contract and an `IPipelineBehavior` is a decorator — hence the name.

## Related concepts

- [Handler contract](./handler-contract.md)
- [Handler decorator](./handler-decorator.md)
- [Error kind](./error-kind.md)

## Sources

- [handler.go](../templates/internal/command/pipeline/handler.go), [decorator.go](../templates/internal/command/pipeline/decorator.go), [logging.go](../templates/internal/command/pipeline/logging.go)
- MediatR pipeline behaviors: https://github.com/jbogard/MediatR/wiki/Behaviors
