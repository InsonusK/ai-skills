# Command middleware

MediatR gives the dotnet base plateau its **handler contract** and its **command middleware**: every handler implements `IRequestHandler<TCommand, TResult>`, and every cross-cutting step is a MediatR pipeline behavior, an `IPipelineBehavior<TCommand, TResult>` that wraps all handlers at once. Each is a `{Name}Middleware<TCommand, TResult>` in a folder of its own of the `Middleware` project — `src/Middleware/Logging/LoggingMiddleware.cs` in the base plateau.

## How it works

```
entry point ── sender.Send(new PlanItemsCommand(...)) ──► LoggingMiddleware ──► (UnitOfWorkMiddleware, later) ──► PlanItemsHandler
                                                       ▲ outermost first, the order of the AddCommandMiddleware list
```

| Go base plateau | dotnet |
|---|---|
| `Handler[TCommand, TResult]` | `IRequestHandler<TCommand, TResult>` |
| `commandlogging.Middleware(logger)` in the list given to `command.NewHandlers` | `typeof(LoggingMiddleware<,>)` in the list given to `AddCommandMiddleware` |
| `command.Handlers`, the registry built by `command.NewHandlers` | `ISender` over the handlers `AddTodoModule()` registered, which finds the handler by the command's type |

`TCommand` is the command record (`PlanItemsCommand`), `TResult` the result it declares with `IRequest<TResult>` (`PlanItemsResult`). MediatR closes the open middleware for every command, so a middleware is written once.

## Example

```csharp
public async Task<TResult> Handle(TCommand command, RequestHandlerDelegate<TResult> next, CancellationToken cancellationToken)
{
    logger.Write(LogLevel.Information, "command called", LogAttr.Of("command", typeof(TCommand).Name), LogAttr.Payload(command));
    var result = await next(cancellationToken); // the next behavior, or the handler
    ...
}
```

## Related concepts

- [Composition root](./composition-root.md) — where the middleware are listed (`AddCommandMiddleware`).
- [base-plateau-extension-in-dotnet](library/architecture/dotnet/base-plateau-extension-in-dotnet.skill/base-plateau-extension-in-dotnet.skill.md) — a behavior for some commands, selected by a mark.
- [Logging](./logging.md) — what `LoggingMiddleware` writes.

## Sources

- MediatR behaviors: https://github.com/jbogard/MediatR/wiki/Behaviors
