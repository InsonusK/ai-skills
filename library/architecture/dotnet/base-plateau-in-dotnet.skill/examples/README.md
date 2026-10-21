# todo

The base plateau in dotnet: the process starts, runs its one startup command — plan a list of
to-do items from `STARTUP_TITLES` — prints the result as JSON and exits.

```bash
make init    # once: the packages
make run     # build and run the startup command
make test    # every scenario
```

| Project | Layer |
|---|---|
| `src/App/App.Host` | composition root (`Program.cs`), the entry points (`Api/`), base settings |
| `src/Modules/Todo/Todo.Interfaces` | the commands and their results — all an entry point sees |
| `src/Modules/Todo/Todo.Application` | the command handlers |
| `src/Modules/Todo/Todo.Domain` | entities, domain services, the rule errors |
| `src/BuildingBlocks` | pipeline behaviors, logging — fixed code |
| `src/Shared` | the error kinds — fixed code, no packages |
