---
name: base-plateau-extension-in-dotnet
description: How a variation point extends the base plateau in dotnet — one registration method per module, API, and variation point called in order in the three sections of Program.cs (AddTodoModule, AddCommandDecorators with the list of MediatR pipeline behaviors, AddHttpApi and AddGrpcApi with their lists of middlewares and interceptors, every entry point registered as an Entry), marks as interfaces of Shared a command implements, rejections as ErrorKindException, the variation point's own project and settings, and attach.allow — with a sample access variation point attached to the HTTP example
whenToUse: when you write, attach, or review a variation point of a dotnet application built on the base plateau, when you add a pipeline behavior around commands or a middleware or interceptor around requests, or when you add a command or a module to such an application
updated: 20261025
tags:
  - stack/dotnet
  - concern/architecture
  - app-type/service
adr:
  - adr/registration-methods-in-program.md
---

# Goal
- **The extension points in dotnet** - A dotnet application that keeps every property of [base-plateau-extension](library/architecture/spec/base-plateau-extension.skill/base-plateau-extension.skill.md); this skill details that skill's implementation for dotnet and restates none of its rules.
- **A sample to copy from** - [examples/](./examples/README.md) attaches a sample access variation point to the HTTP example of [vp-http-api-in-dotnet](library/architecture/dotnet/vp-http-api-in-dotnet.skill/vp-http-api-in-dotnet.skill.md): one middleware and two pipeline behaviors, with the attach check proving that only the composition root, the project files, the marked command, and the readme and Makefile changed.

# Core Principle
- **Program.cs reads as a list of registrations** - Each module, API, and variation point brings one `Add{Part}()` extension method of `IServiceCollection`; `Program.cs` calls them in order in its three sections, and the order of a list argument is the order of wrapping ([ADR](./adr/registration-methods-in-program.md)).
- **MediatR is the command registry** - A module's `Add{Module}Module()` lets MediatR find every handler of its assembly, so a new command adds no line anywhere; `ISender` is what an entry point takes, and every behavior of the list wraps every handler.
- **A mark is an interface of Shared** - `Shared` references nothing, so a module's `Commands` project implements a mark (`IRestricted`) without referencing the variation point, and a behavior asks `command is IRestricted`.

```
src/App/App.Host/Program.cs
  accessConfig = AccessConfig.Load()                       the variation point's own settings (in the try of AppConfig)
  // [services]  services.AddSingleton(accessConfig)
  // [handlers]  services.AddTodoModule()                  Todo.Application/TodoModule.cs: handlers + module services
                 services.AddCommandDecorators(
                     typeof(LoggingBehavior<,>),           every command      (base plateau)
                     typeof(AccessCheckBehavior<,>),       by the mark        IRestricted
                     typeof(BatchLimitBehavior<,>))        by a value         IBatch
  // [entries]   services.AddHttpApi(middlewares: [typeof(Authentication)])   entry decorator (inside the request log)
  EntryRunner.RunAllAsync(logger, stopping, [.. provider.GetServices<Entry>()])
```

# Rule

## MUST

### Register a module with its own method
Give every module an `Add{Module}Module()` extension method in `{Module}.Application/{Module}Module.cs` that registers the handlers of its assembly with MediatR and the services they use, and call it once in `// [handlers]`.

```csharp
// Todo.Application/TodoModule.cs
public static IServiceCollection AddTodoModule(this IServiceCollection services) =>
    services
        .AddMediatR(mediatr => mediatr.RegisterServicesFromAssemblyContaining<PlanItemsHandler>())
        .AddSingleton<IItemLists, ItemListsService>();
```

- Violation: `services.AddMediatR(...)` in `Program.cs`; a handler registered by hand; a behavior given to `AddMediatR`'s configuration.
- Risk: a behavior registered with the module wraps only some commands, or is registered twice once a second module arrives.
- Fix: the module registers handlers and services only; its infrastructure (`IListStore`) is registered in `// [services]`; the behaviors are listed once in `AddCommandDecorators`.

### Write a command decorator as a pipeline behavior
Write a command decorator as an open generic `{Step}Behavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>` in its own file, taking what it needs from the container through its constructor; let it call `next(cancellationToken)` once, or throw an `ErrorKindException` of a factory method of its errors class to reject the command.

```csharp
// by the command's mark: src/Access/AccessCheckBehavior.cs
public sealed class AccessCheckBehavior<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    private static readonly string Name = typeof(TCommand).Name.Replace("Command", "");

    public Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
    {
        if (command is not IRestricted restricted)
        {
            return next(cancellationToken); // not marked: passes unchanged
        }
        var caller = CallerIdentity.Current ?? throw AccessErrors.NoCaller(Name);
        if (!caller.HasRole(restricted.RequiredRole))
        {
            throw AccessErrors.MissingRole(Name, restricted.RequiredRole);
        }
        return next(cancellationToken);
    }
}

// by a value of the command: src/Access/BatchLimitBehavior.cs
public sealed class BatchLimitBehavior<TCommand, TResult>(AccessConfig config) : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    private static readonly string Name = typeof(TCommand).Name.Replace("Command", "");

    public Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken) =>
        command is IBatch batch && batch.BatchSize > config.MaxBatch
            ? throw AccessErrors.BatchTooLarge(Name, config.MaxBatch, batch.BatchSize)
            : next(cancellationToken);
}
```

- Violation: `where TCommand : IRestricted` as the filter; `if (command is PlanItemsCommand)`; a behavior that answers a rejection with a result.
- Risk: a constraint the container cannot satisfy fails the resolution of every other command, depending on the container's version; a behavior that names the commands misses the next one that needs it.
- Fix: one behavior per step that passes every unmarked command on; the simple form — every command — is the base plateau's [LoggingBehavior](library/architecture/dotnet/base-plateau-in-dotnet.skill/templates/src/BuildingBlocks/Pipeline/LoggingBehavior.cs).

### Mark a command with an interface of Shared
Give a command a mark by implementing the mark's interface of `Shared` on its record, with the property's body and `[JsonIgnore]`, and a reference from the module's `Commands` project to `Shared`.

```csharp
// Todo.Commands/PlanItemsCommand.cs
public sealed record PlanItemsCommand(IReadOnlyList<string> Titles) : IRequest<PlanItemsResult>, IRestricted, IBatch
{
    /// <summary>Marks PlanItemsCommand <see cref="IRestricted"/>: only a planner may plan.</summary>
    [JsonIgnore]
    public string RequiredRole => "planner";

    /// <summary>Marks PlanItemsCommand <see cref="IBatch"/>: its size is the number of titles.</summary>
    [JsonIgnore]
    public int BatchSize => Titles.Count;
}
```

- Violation: the mark's interface in the variation point's project; a mark without `[JsonIgnore]`.
- Risk: a `Commands` project referencing a variation point pulls its packages into every entry point; a mark without `[JsonIgnore]` is logged as data of the command.
- Fix: marks in `src/Shared/{VariationPoint}/`, as the logging variation point's `[LogMask]` is.

### List the behaviors in AddCommandDecorators
Give every pipeline behavior as one element of the `AddCommandDecorators` call in `// [handlers]` of `Program.cs`, `typeof(LoggingBehavior<,>)` first, so every rejection of a behavior after it is logged as `command failed`.

```csharp
// [handlers] - the handler of every command of each module, and the
// decorators wrapped around every one of them, the first the outermost.
services.AddTodoModule();
services.AddCommandDecorators(
    typeof(LoggingBehavior<,>),
    typeof(AccessCheckBehavior<,>), // by the command's mark: IRestricted
    typeof(BatchLimitBehavior<,>)); // by a value of the command: IBatch
```

- Violation: `services.AddTransient(typeof(IPipelineBehavior<,>), ...)` of its own; a second `AddCommandDecorators` call.
- Risk: the order of the behaviors depends on where a line happens to stand, and a behavior placed before the logging rejects calls that leave no trace.
- Fix: one list; `AddCommandDecorators` refuses a type that is no open generic pipeline behavior at start.

### Give an API's decorators to its registration
Give an API's entry decorators as the list argument of its registration in `// [entries]`: `AddHttpApi(middlewares: [...])` with ASP.NET `IMiddleware` types, `AddGrpcApi(interceptors: [...])` with `Interceptor` types, each built from the application's services; let one reject by throwing an `ErrorKindException` and pass a value to the command layer through an `AsyncLocal` it begins around `next`.

```csharp
// src/App/App.Host/Api/Http/Access/Authentication.cs
public sealed class Authentication(AccessConfig config) : IMiddleware
{
    private const string Scheme = "Bearer ";

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var header = context.Request.Headers.Authorization.ToString();
        if (header == "")
        {
            await next(context); // no credentials: the command decides
            return;
        }
        if (!header.StartsWith(Scheme, StringComparison.Ordinal) || !config.Tokens.TryGetValue(header[Scheme.Length..], out var caller))
        {
            throw AccessErrors.UnknownCaller();
        }
        using (CallerIdentity.Begin(caller)) // the AsyncLocal the access check reads
        {
            await next(context);
        }
    }
}

// Program.cs, [entries]
services.AddHttpApi(middlewares: [typeof(Authentication)]);
```

- Violation: `context.Response.StatusCode = 401` written by the middleware; `app.UseMiddleware` in an API's fixed code; `IHttpContextAccessor` read in a behavior.
- Risk: the rejection is answered outside `google.rpc.Status` or outside the request log; a behavior that reads the HTTP context does nothing for gRPC.
- Fix: the HTTP server runs the middlewares only for the contract's routes, inside the request log and the error answers; the gRPC server runs the interceptors after the call log and the error answers.

### Put the variation point in its own project
Put a variation point's behaviors, settings, identity, and errors in `src/{VariationPoint}/` with its tests in `src/{VariationPoint}.Tests/`, its marks in `src/Shared/{VariationPoint}/`, and its transport half in `src/App/App.Host/Api/{Transport}/{VariationPoint}/`.
- Violation: `AccessCheckBehavior` added to `BuildingBlocks/Pipeline`; a middleware in `Api/Http/Pipeline`, which is copied from templates.
- Risk: a variation point edits a base project or a template, and two variation points edit the same one.
- Fix: the sample's `src/Access`, `src/Shared/Access`, and `Api/Http/Access`; the solution and `App.Host.csproj` reference the new project.

### Read the settings in the variation point's project
Read a variation point's settings with `{VariationPoint}Config.Load()` in its own project, called in the `try` of `AppConfig.Load()`, and register the result in `// [services]`; a `FormatException` stops the start with `startup failed` and exit code `1`.

```csharp
AppConfig config;
AccessConfig accessConfig;
try
{
    config = AppConfig.Load();
    accessConfig = AccessConfig.Load(); // ACCESS_TOKENS, ACCESS_MAX_BATCH
}
// ...
// [services]
services.AddSingleton(accessConfig);
```

- Violation: a property added to `AppConfig`; `services.AddSingleton(_ => AccessConfig.Load())`.
- Risk: a setting of `AppConfig` stays when the variation point goes; a setting loaded on first use fails a request instead of the start.
- Fix: the error names its variable (`access config: ACCESS_MAX_BATCH '0' is not a positive number`).

### Name every changed base file in attach.allow
Give the variation point's example an `attach.allow` beside its `examples/`, naming its base example and every base file it changes or deletes.

```
# skill folder / attach.allow
base library/architecture/dotnet/vp-http-api-in-dotnet.skill/examples
change src/App/App.Host/Program.cs
change src/App/App.Host/App.Host.csproj
change src/Modules/Todo/Todo.Commands/Todo.Commands.csproj
change src/Modules/Todo/Todo.Commands/PlanItemsCommand.cs
change Todo.slnx
change Makefile
change README.md
```

- Risk: a change outside the list is a hidden dependency of the variation point.
- Fix: `bash test/check-vp-attach.sh` in this repository passes.

# Check list
- [ ] Every module has its `Add{Module}Module()`; `Program.cs` has no `AddMediatR`.
- [ ] Every command decorator is an open generic pipeline behavior in its own file that passes every unmarked command on and rejects with an `ErrorKindException`.
- [ ] A filtered behavior selects by an interface of `Shared`; the marked command implements it with `[JsonIgnore]` properties.
- [ ] `AddCommandDecorators` lists every behavior, `LoggingBehavior<,>` first.
- [ ] Every entry point is registered as an `Entry`; each API's registration gets its list of middlewares or interceptors.
- [ ] The variation point lives in its own project; its settings are loaded with `AppConfig` and registered in `// [services]`.
- [ ] `attach.allow` names every changed base file; `bash test/check-vp-attach.sh` and `make test` pass.

# Example
- [examples/](./examples/README.md) — the HTTP example with the sample access variation point attached: `make run`, then the `curl` calls of its readme; `make test`.
