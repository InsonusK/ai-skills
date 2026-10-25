# Glossary

Every concept the Go base plateau is built from, explained for a reader new to it.

| Term | Page | Covers |
| --- | --- | --- |
| Composition root | [composition-root.md](./composition-root.md) | `run` in `main.go`: where every object is created and wired, in three marked sections |
| Entry runner | [entry-runner.md](./entry-runner.md) | `entry.RunAll`: every entry point started together, stopped together, and the exit code |
| Logging | [logging.md](./logging.md) | one logger, one payload attribute, the handler decides what is written |
| Error kind | [error-kind.md](./error-kind.md) | `ErrInvalid`, `ErrNotFound`, ...: the markers errors wrap, checked by entry points |
| Handler contract | [handler-contract.md](./handler-contract.md) | `Handler[TCommand, TResult]`: the one generic interface every command handler satisfies |
| Command middleware | [command-middleware.md](./command-middleware.md) | `pipeline.Middleware`, `Decorate`, `internal/command/middleware/{name}`: a step around every command call, given as one list |
| Command registry | [command-registry.md](./command-registry.md) | `command.Handlers`, `NewHandlers`: every handler built and wrapped in one place |
| Pipeline | [pipeline.md](./pipeline.md) | `internal/command/pipeline`: the path a command takes from an entry point through middleware to its handler |
| HandlerFunc | [handler-func.md](./handler-func.md) | a function type that satisfies the handler contract, for middleware and stubs |
| Startup entry | [startup-entry.md](./startup-entry.md) | `internal/api/startup`: run one command at start, print the result, set the exit code |
