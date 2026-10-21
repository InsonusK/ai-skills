# Glossary

Every concept the dotnet base plateau is built from, explained for a reader new to it.

| Term | Page | Covers |
| --- | --- | --- |
| Composition root | [composition-root.md](./composition-root.md) | `Program.RunAsync`: where every object is registered and wired, in three marked sections |
| Entry runner | [entry-runner.md](./entry-runner.md) | `EntryRunner.RunAllAsync`: every entry point started together, stopped together, and the exit code |
| Error kind | [error-kind.md](./error-kind.md) | `ErrorKindException` with an `ErrorKind`: how a rejected command reaches an entry point |
| Logging | [logging.md](./logging.md) | `logger.Write` with `LogAttr`, one payload attribute, the provider decides what is written |
| Pipeline behavior | [pipeline-behavior.md](./pipeline-behavior.md) | MediatR's `IRequestHandler` as the handler contract and `IPipelineBehavior` as its decorator |
| Startup entry | [startup-entry.md](./startup-entry.md) | `Api/Startup`: run one command at start, print the result, set the exit code |
