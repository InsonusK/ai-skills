# Glossary

Every concept the Go base plateau is built from, explained for a reader new to it.

| Term | Page | Covers |
| --- | --- | --- |
| Composition root | [composition-root.md](./composition-root.md) | `run` in `main.go`: where every object is created and wired, in three marked sections |
| Error kind | [error-kind.md](./error-kind.md) | `ErrInvalid`, `ErrNotFound`, ...: the markers errors wrap, checked by entry points |
| Handler contract | [handler-contract.md](./handler-contract.md) | `Handler[TCommand, TResult]`: the one generic interface every command handler satisfies |
| Handler decorator | [handler-decorator.md](./handler-decorator.md) | `WithLogging`: wrapping a handler to add a step around every call |
| Pipeline | [pipeline.md](./pipeline.md) | `internal/command/pipeline`: the path a command takes from an entry point through decorators to its handler |
| HandlerFunc | [handler-func.md](./handler-func.md) | a function type that satisfies the handler contract, for decorators and stubs |
| Startup entry | [startup-entry.md](./startup-entry.md) | `internal/api/startup`: run one command at start, print the result, set the exit code |
