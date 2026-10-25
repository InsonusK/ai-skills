---
name: registration methods in Program
description: How the dotnet composition root registers modules, decorators, and entry points, so a variation point adds one call instead of editing another's lines
problem: Program.cs registered MediatR with the modules' assemblies and every behavior in one AddMediatR block, and built each entry point's endpoints from the sender before listing new Entry(...) for the runner; a new module, a behavior, or an API each edited lines another part owned. How does Program.cs stay a list a variation point adds one line to?
decision: One IServiceCollection extension method per part - Add{Module}Module() for a module's handlers and services, AddCommandDecorators(params Type[]) for the ordered list of pipeline behaviors, Add{Api}(params Type[]) per API registering its Entry with its list of middlewares or interceptors, AddStartupEntry and AddManagement - called in order in the three sections; the runner takes every registered Entry.
tags:
  - stack/dotnet
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The command registry and the decorator lists of base-plateau-extension must look like dotnet, where composition is `services.AddX()` calls. A behavior given inside `AddMediatR`'s configuration is registered with the modules; an entry point built after `BuildServiceProvider` needs the sender and the logger factory pulled out by hand in `Program.cs`.

# Selected variant
[[#One registration method per part]]
- Owner decision, 2026-10-24: extension methods per module, per API kind, and per variation point (`AddTodoModule`, `AddHttpApi`, `AddUnitOfWork`), with `Program.cs` keeping the explicit ordered calls in its three sections and the entry points registered in DI.

# Searched variants

## One registration method per part

**Selected.**

### Description
`AddTodoModule()` registers the module's handlers and services; `AddCommandDecorators(typeof(LoggingBehavior<,>), ...)` registers the behaviors in the order given, which is MediatR's order of wrapping; `AddHttpApi(middlewares: [...])` registers an `Entry` that builds the resources from `ISender` and the middlewares from the container when it runs; `RunAllAsync` gets `provider.GetServices<Entry>()`.

### Benefits
- `Program.cs` names no handler, resource, or endpoint; a new command or resource changes no line of it.
- A middleware or interceptor takes its dependencies (its settings) from the container, as any ASP.NET middleware does.
- The order of every list is visible in one call.

### Costs
- A list of `Type` is checked at start, not at build: `AddCommandDecorators` refuses a type that is no open generic pipeline behavior, and an entry refuses a middleware type that is no `IMiddleware`.
- The entries run in the order they were registered; the order is in `Program.cs` like any other.

## One AddMediatR block and new Entry(...) in Program.cs

### Description
The earlier shape: `AddMediatR(m => { m.RegisterServicesFromAssemblyContaining<...>(); m.AddOpenBehavior(...); })` and `new Entry("http", stopping => HttpApi.RunAsync(new HttpEndpoints(...), loggers, stopping))`.

### Benefits
- Everything visible in one file.

### Costs
- A new module and a behavior variation point edit the same block; every API variation point builds its resources in `Program.cs`, so a new resource edits it too.

## Behaviors with generic constraints as the filter

### Description
`AccessCheckBehavior<TCommand, TResult> where TCommand : IRestricted`, left to the container to skip for other commands.

### Benefits
- The filter is in the type.

### Costs
- Whether the container skips an open generic whose constraint a command does not meet depends on its version; the check is spread over the type system instead of one `is` in the behavior.
