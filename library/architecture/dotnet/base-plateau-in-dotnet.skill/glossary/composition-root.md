# Composition root

The **composition root** is `Program.RunAsync` in `src/App/App.Host/Program.cs`: the one place where every concrete type is registered and the parts are wired together. No other project creates a service, a handler, or an entry point for itself.

## Why it exists

A variation point attaches by adding lines, never by editing another file. That works only when there is one known place to add them, cut into sections every variation point names the same way.

## How it works

```
Main                     SIGINT/SIGTERM → cancel "stopping"; return RunAsync(...)
RunAsync
  AppConfig.Load()       LOG_LEVEL
  AddLogging(...)        the one provider (the logging VP replaces this line)
  // [services]          infrastructure and the services over it (DB, clients)
  // [handlers]          AddTodoModule(): the module's handlers and services
                         AddCommandMiddleware(typeof(LoggingMiddleware<,>), ...): first = outermost
  // [entries]           AddStartupEntry(...) / AddHttpApi(middlewares: [...]) / AddManagement(): one Entry each
  BuildServiceProvider()
  EntryRunner.RunAllAsync(logger, stopping, [.. provider.GetServices<Entry>()]) → ExitCode
```

| Section | A variation point adds |
|---|---|
| `[services]` | its infrastructure: `services.AddSingleton<IListStore, MemoryListStore>()` |
| `[handlers]` | one element of the `AddCommandMiddleware` list: `typeof(UnitOfWorkMiddleware<,>)`; a module adds its `Add{Module}Module()` |
| `[entries]` | `services.AddHttpApi(middlewares: [])` replacing `AddStartupEntry`; `services.AddManagement()` beside it; an element of an API's list of middlewares |

## Example

```csharp
// [handlers] - the handler of every command of each module, and the
// middleware wrapped around every one of them, the first the outermost.
services.AddTodoModule();
services.AddCommandMiddleware(
    typeof(LoggingMiddleware<,>));

// [entries] - every entry point, registered as an Entry and run together by EntryRunner.
services.AddStartupEntry(() => new PlanItemsCommand(StartupEntry.EnvList("STARTUP_TITLES")), stdout);
```

## Related concepts

- [Entry runner](./entry-runner.md) — what runs the entries `[entries]` registers.
- [base-plateau-extension-in-dotnet](library/architecture/dotnet/base-plateau-extension-in-dotnet.skill/base-plateau-extension-in-dotnet.skill.md) — how a variation point uses the sections.
- [Command middleware](./command-middleware.md) — what `[handlers]` registers.

## Sources

- Mark Seemann, Composition Root: https://blog.ploeh.dk/2011/07/28/CompositionRoot/
- Dependency injection in .NET: https://learn.microsoft.com/dotnet/core/extensions/dependency-injection
