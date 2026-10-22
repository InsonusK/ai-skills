# Pipeline behavior

MediatR gives the dotnet base plateau its **handler contract** and its **decorator**: every handler implements `IRequestHandler<TCommand, TResult>`, and every cross-cutting step is an `IPipelineBehavior<TCommand, TResult>` that wraps all handlers at once.

## How it works

```
entry point ── sender.Send(new PlanItemsCommand(...)) ──► LoggingBehavior ──► (UnitOfWorkBehavior, later) ──► PlanItemsHandler
                                                       ▲ outermost first, the order of AddOpenBehavior
```

| Go base plateau | dotnet |
|---|---|
| `Handler[TCommand, TResult]` | `IRequestHandler<TCommand, TResult>` |
| `WithLogging(name, logger, handler)` per handler in `main.go` | `LoggingBehavior<,>` once, `AddOpenBehavior` in `// [handlers]` |
| a handler variable passed to the entry point | `ISender`, which finds the handler by the command's type |

`TCommand` is the command record (`PlanItemsCommand`), `TResult` the result it declares with `IRequest<TResult>` (`PlanItemsResult`). MediatR closes the open behavior for every command, so a behavior is written once.

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

- [Composition root](./composition-root.md) — where behaviors are added.
- [Logging](./logging.md) — what `LoggingBehavior` writes.

## Sources

- MediatR behaviors: https://github.com/jbogard/MediatR/wiki/Behaviors
