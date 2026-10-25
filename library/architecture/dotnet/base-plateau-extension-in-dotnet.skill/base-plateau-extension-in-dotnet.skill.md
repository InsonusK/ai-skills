---
name: base-plateau-extension-in-dotnet
description: How a variation point extends the base plateau in dotnet — one registration method per module, API, and variation point called in order in the three sections of Program.cs (AddTodoModule, AddCommandMiddleware with the list of command middleware, AddHttpApi and AddGrpcApi with their lists of API middleware, every entry point registered as an Entry), every middleware in a folder of its own named {Name}Middleware — the command middleware as MediatR pipeline behaviors in the Middleware project, an API's in App.Host/Api/{Api}/Middleware inside its fixed RequestLogMiddleware and AnswerFailuresMiddleware — marks as interfaces of Shared a command implements, rejections as ErrorKindException, the variation point's own settings, and attach.allow, with a sample access variation point attached to the HTTP example
whenToUse: when you write, attach, or review a variation point of a dotnet application built on the base plateau, when you add a middleware around commands or around the requests or calls of an API, or when you add a command or a module to such an application
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
- **A sample to copy from** - [examples/](./examples/README.md) attaches a sample access variation point to the HTTP example of [vp-http-api-in-dotnet](library/architecture/dotnet/vp-http-api-in-dotnet.skill/vp-http-api-in-dotnet.skill.md): one HTTP middleware and two command middleware, with the attach check proving that only the composition root, the marked command with its project, and the readme and Makefile changed.

# Core Principle
- **Program.cs reads as a list of registrations** - Each module, API, and variation point brings one `Add{Part}()` extension method of `IServiceCollection`; `Program.cs` calls them in order in its three sections, and the order of a list argument is the order of wrapping ([ADR](./adr/registration-methods-in-program.md)).
- **A folder per middleware** - A command middleware is `src/Middleware/{Name}/{Name}Middleware.cs`, an open generic MediatR pipeline behavior; an API's is `src/App/App.Host/Api/{Api}/Middleware/{Name}/{Name}Middleware.cs`. The `Middleware` project knows no ASP.NET, so a command middleware works for every API.
- **MediatR is the command registry** - A module's `Add{Module}Module()` lets MediatR find every handler of its assembly, so a new command adds no line anywhere; `ISender` is what an entry point takes, and every middleware of the list wraps every handler.
- **A mark is an interface of Shared** - `Shared` references nothing, so a module's `Commands` project implements a mark (`IRestricted`) without referencing the variation point, and a middleware asks `command is IRestricted`.

```
src/
├── Shared/Access/              IRestricted, IBatch             the marks (sample VP)
├── BuildingBlocks/Pipeline/    AddCommandMiddleware            the registration of the list
├── Middleware/                 one folder per command middleware - no ASP.NET
│   ├── Logging/                LoggingMiddleware<,>            base plateau
│   ├── Access/                 AccessMiddleware<,>, settings   sample VP
│   └── BatchLimit/             BatchLimitMiddleware<,>         sample VP
├── Middleware.Tests/
└── App/App.Host/Api/Http/
    ├── Pipeline/               HttpServer: the fixed middleware around all, the list around the contract
    └── Middleware/
        ├── RequestLog/         RequestLogMiddleware            fixed, outermost
        ├── AnswerFailures/     AnswerFailuresMiddleware        fixed: every exception as google.rpc.Status
        └── Authentication/     AuthenticationMiddleware        sample VP

src/App/App.Host/Program.cs
  accessConfig = AccessConfig.Load()                          the variation point's own settings (in the try of AppConfig)
  // [services]  services.AddSingleton(accessConfig)
  // [handlers]  services.AddTodoModule()
                 services.AddCommandMiddleware(
                     typeof(LoggingMiddleware<,>),            every command      (base plateau)
                     typeof(AccessMiddleware<,>),             by the mark        IRestricted
                     typeof(BatchLimitMiddleware<,>))         by a value         IBatch
  // [entries]   services.AddHttpApi(middlewares: [typeof(AuthenticationMiddleware)])
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
- Risk: a middleware registered with the module wraps only some commands, or is registered twice once a second module arrives.
- Fix: the module registers handlers and services only; its infrastructure (`IListStore`) is registered in `// [services]`; the middleware are listed once in `AddCommandMiddleware`.

### One folder per middleware
Put each command middleware in `src/Middleware/{Name}/` with the settings, identity, and errors it uses, its scenarios in `src/Middleware.Tests/`, and each API middleware in `src/App/App.Host/Api/{Api}/Middleware/{Name}/`, the class named `{Name}Middleware` and the namespace `Middleware.{Name}` or `App.Host.Api.{Api}.Middleware.{Name}`; put the marks in `src/Shared/{VariationPoint}/`.
- Violation: `AccessMiddleware` added to `BuildingBlocks/Pipeline`; an HTTP middleware in `Api/Http/Pipeline`, which is copied from templates; a project of its own per variation point.
- Risk: a variation point edits a base project or a template, and a reader looking for a step finds it under another name.
- Fix: a new folder needs no project edit — the SDK compiles every file of the project's folders; a middleware that needs a package the `Middleware` project lacks adds it there, as a listed change.

### Write a command middleware as a pipeline behavior
Write a command middleware as an open generic `{Name}Middleware<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>`, taking what it needs from the container through its constructor; let it call `next(cancellationToken)` once, or throw an `ErrorKindException` of a factory method of its errors class to reject the command.

```csharp
// by the command's mark: src/Middleware/Access/AccessMiddleware.cs
public sealed class AccessMiddleware<TCommand, TResult> : IPipelineBehavior<TCommand, TResult>
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

// by a value of the command: src/Middleware/BatchLimit/BatchLimitMiddleware.cs
public sealed class BatchLimitMiddleware<TCommand, TResult>(AccessConfig config) : IPipelineBehavior<TCommand, TResult>
    where TCommand : notnull
{
    private static readonly string Name = typeof(TCommand).Name.Replace("Command", "");

    public Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken) =>
        command is IBatch batch && batch.BatchSize > config.MaxBatch
            ? throw AccessErrors.BatchTooLarge(Name, config.MaxBatch, batch.BatchSize)
            : next(cancellationToken);
}
```

- Violation: `where TCommand : IRestricted` as the filter; `if (command is PlanItemsCommand)`; a middleware that answers a rejection with a result.
- Risk: a constraint the container cannot satisfy fails the resolution of every other command, depending on the container's version; a middleware that names the commands misses the next one that needs it.
- Fix: one middleware per step that passes every unmarked command on; the simple form — every command — is the base plateau's [LoggingMiddleware](library/architecture/dotnet/base-plateau-in-dotnet.skill/templates/src/Middleware/Logging/LoggingMiddleware.cs).

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

- Violation: the mark's interface in the `Middleware` project; a mark without `[JsonIgnore]`.
- Risk: a `Commands` project referencing the middleware pulls MediatR's behaviors into every entry point; a mark without `[JsonIgnore]` is logged as data of the command.
- Fix: marks in `src/Shared/{VariationPoint}/`, as the logging variation point's `[LogMask]` is.

### List the command middleware in AddCommandMiddleware
Give every command middleware as one element of the `AddCommandMiddleware` call in `// [handlers]` of `Program.cs`, `typeof(LoggingMiddleware<,>)` first, so every rejection of a middleware after it is logged as `command failed`.

```csharp
// [handlers] - the handler of every command of each module, and the
// command middleware wrapped around every one of them, the first the outermost.
services.AddTodoModule();
services.AddCommandMiddleware(
    typeof(LoggingMiddleware<,>),
    typeof(AccessMiddleware<,>),      // by the command's mark: IRestricted
    typeof(BatchLimitMiddleware<,>)); // by a value of the command: IBatch
```

- Violation: `services.AddTransient(typeof(IPipelineBehavior<,>), ...)` of its own; a second `AddCommandMiddleware` call.
- Risk: the order of the middleware depends on where a line happens to stand, and a middleware placed before the logging rejects calls that leave no trace.
- Fix: one list; `AddCommandMiddleware` refuses a type that is no open generic pipeline behavior at start.

### Give an API's middleware to its registration
Give an API's middleware as the list argument of its registration in `// [entries]`: `AddHttpApi(middlewares: [...])` with ASP.NET `IMiddleware` types, `AddGrpcApi(interceptors: [...])` with `Interceptor` types, each built from the application's services; let one reject by throwing an `ErrorKindException` and pass a value to the command layer through an `AsyncLocal` it begins around `next`.

```csharp
// src/App/App.Host/Api/Http/Middleware/Authentication/AuthenticationMiddleware.cs
public sealed class AuthenticationMiddleware(AccessConfig config) : IMiddleware
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
services.AddHttpApi(middlewares: [typeof(AuthenticationMiddleware)]);
```

- Violation: `context.Response.StatusCode = 401` written by the middleware; `app.UseMiddleware` in an API's fixed code; `IHttpContextAccessor` read in a command middleware; `typeof(RequestLogMiddleware)` given in the list.
- Risk: the rejection is answered outside `google.rpc.Status` or outside the request log; a command middleware that reads the HTTP context does nothing for gRPC; a fixed middleware given twice logs every request twice.
- Fix: the HTTP server puts `RequestLogMiddleware` outermost and `AnswerFailuresMiddleware` inside it, and runs the list only for the contract's routes; the gRPC server runs `CallLogMiddleware` and `AnswerFailuresMiddleware` before the list.

### Keep a middleware without state, a scoped dependency from a scope of its own
Keep an API middleware (`IMiddleware`, `Interceptor`) and a command middleware without fields written per call — `HttpServer.Activate` and `GrpcServer.Activate` build an API middleware once from the application's container, and it serves every request at the same time — and take a dependency registered as scoped per call, from a scope it opens through the application's `IServiceScopeFactory`, never from `context.RequestServices`.

```csharp
// an API middleware with a scoped dependency: one scope per request
public sealed class AuditMiddleware(IServiceScopeFactory scopes) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        await using var scope = scopes.CreateAsyncScope();
        var audit = scope.ServiceProvider.GetRequiredService<IAuditLog>(); // lives as long as this request
        await next(context);
        await audit.WriteAsync(context.Request.Path, context.Response.StatusCode);
    }
}
```

- Violation: `context.RequestServices.GetRequiredService<IAuditLog>()`; a scoped service (`DbContext`) taken in the constructor; a field set in `InvokeAsync`.
- Risk: `context.RequestServices` is the web host's own container (`WebHost.CreateBuilder`), which holds none of the application's services, so the call fails at run time; a scoped service taken in the constructor lives as long as the process and is shared by every request at once — a `DbContext` is not safe for that; a field is overwritten by the request served beside it.
- Fix: the constructor takes singletons and `IServiceScopeFactory` only; the scope a middleware opens is its own, not the command's — a unit of work shared with the handler is the DB variation point's scope per command (README backlog).

### Read the settings in the variation point's folder
Read a variation point's settings with `{VariationPoint}Config.Load()` from one of its folders, called in the `try` of `AppConfig.Load()`, and register the result in `// [services]`; a `FormatException` stops the start with `startup failed` and exit code `1`.

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
change src/Modules/Todo/Todo.Commands/Todo.Commands.csproj
change src/Modules/Todo/Todo.Commands/PlanItemsCommand.cs
change Makefile
change README.md
```

- Risk: a change outside the list is a hidden dependency of the variation point.
- Fix: `bash test/check-vp-attach.sh` in this repository passes.

# Check list
- [ ] Every module has its `Add{Module}Module()`; `Program.cs` has no `AddMediatR`.
- [ ] Every middleware is in a folder of its own, named `{Name}Middleware`: a command middleware in `src/Middleware/{Name}/`, an API's in `Api/{Api}/Middleware/{Name}/`.
- [ ] Every command middleware is an open generic pipeline behavior that passes every unmarked command on and rejects with an `ErrorKindException`.
- [ ] A filtered middleware selects by an interface of `Shared`; the marked command implements it with `[JsonIgnore]` properties.
- [ ] `AddCommandMiddleware` lists every command middleware, `LoggingMiddleware<,>` first.
- [ ] Every entry point is registered as an `Entry`; each API's registration gets its list of middleware; no fixed middleware is in a list.
- [ ] No middleware keeps a field written per call; a scoped dependency comes from a scope opened through `IServiceScopeFactory`, never from `context.RequestServices`.
- [ ] The variation point's settings are loaded with `AppConfig` and registered in `// [services]`.
- [ ] `attach.allow` names every changed base file; `bash test/check-vp-attach.sh` and `make test` pass.

# Example
- [examples/](./examples/README.md) — the HTTP example with the sample access variation point attached: `make run`, then the `curl` calls of its readme; `make test`.
