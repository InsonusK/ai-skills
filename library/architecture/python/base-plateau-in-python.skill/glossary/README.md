# Glossary

Every concept the Python base plateau is built from, explained for a reader new to it.

| Term | Page | Covers |
| --- | --- | --- |
| Handler contract | [handler-contract.md](./handler-contract.md) | `Handler[TCommand, TResult]`: the one async protocol every command handler satisfies, `HandlerFunc`, and decorators |
| Command registry | [command-registry.md](./command-registry.md) | `Handlers`, `build_handlers`: every handler built and wrapped by the list of decorators in one place |
| Entry runner | [entry-runner.md](./entry-runner.md) | `run_all`: every entry point started together, stopped together, and the exit code |
| Logging | [logging.md](./logging.md) | one logger, values as attributes, one payload attribute, the formatter decides what is written |
