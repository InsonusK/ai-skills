# todo

The base plateau in dotnet with the CLI API variation point attached: the process runs one
command per call as `todo {resource} {action} [flags]`, prints the result as one JSON line
(`--output text` for a person), writes errors on stderr, and exits by the error kind.

```bash
make init    # once: the packages
make run ARGS="items plan --title 'buy milk' --title 'walk the dog'"   # {"items":[{"title":"buy milk","done":false},...]}
make run ARGS="items plan --title 'buy milk' --output text"            # [ ] buy milk
make run ARGS="--help"                                                 # the resources and the flags
make test    # every scenario
```

| Project | Layer |
|---|---|
| `src/App/App.Host` | composition root (`Program.cs`), the entry points (`Api/`): `Api/Cli/Pipeline` (copied, never edited), `Api/Cli/Endpoint` (one file per resource) |
| `src/Modules/Todo/Todo.Commands` | the commands and their results — all an entry point sees |
| `src/Modules/Todo/Todo.Application` | the command handlers |
| `src/Modules/Todo/Todo.Domain` | entities, domain services, the rule errors |
| `src/BuildingBlocks` | pipeline behaviors, logging — fixed code |
| `src/Shared` | the error kinds — fixed code, no packages |

Every `{Project}.Tests` sits beside the project it tests (`src/Modules/Todo/Todo.Domain.Tests` next to `Todo.Domain`).
