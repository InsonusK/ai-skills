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
  // [handlers]          AddMediatR: the modules' handlers + AddOpenBehavior, outermost first
  BuildServiceProvider()
  // [entries]           EntryRunner.RunAllAsync(new Entry(...), ...) → ExitCode
```

| Section | A variation point adds |
|---|---|
| `[services]` | `services.AddSingleton<IUsers, UserService>()` and its infrastructure |
| `[handlers]` | `mediatr.AddOpenBehavior(typeof(UnitOfWorkBehavior<,>))` |
| `[entries]` | `new Entry("http", ...)` replacing the startup entry; `new Entry("management", ...)` beside it |

## Example

```csharp
// [handlers] - every command handler of the modules, and the pipeline
// behaviors that wrap each of them, outermost first.
services.AddMediatR(mediatr =>
{
    mediatr.RegisterServicesFromAssemblyContaining<PlanItemsHandler>();
    mediatr.AddOpenBehavior(typeof(LoggingBehavior<,>));
});
```

## Related concepts

- [Entry runner](./entry-runner.md) — what `[entries]` calls.
- [Pipeline behavior](./pipeline-behavior.md) — what `[handlers]` registers.

## Sources

- Mark Seemann, Composition Root: https://blog.ploeh.dk/2011/07/28/CompositionRoot/
- Dependency injection in .NET: https://learn.microsoft.com/dotnet/core/extensions/dependency-injection
