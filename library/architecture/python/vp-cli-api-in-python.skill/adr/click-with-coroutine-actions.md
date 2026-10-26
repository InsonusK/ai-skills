---
name: click with coroutine actions
description: Which library the Python CLI variation point builds its command line on, and how an action runs an async command handler
problem: The command line needs groups per resource (`todo items plan`), help at every level, --version, shell completion, and every usage error turned into the invalid kind instead of the library's own exit; the command handlers are async (ADR async-handlers) and the CLI runs as one entry of run_all, inside a running event loop. Which library, and how does an action await its handler?
decision: click 8.5 (owner decision, 2026-10-26). run() calls the root group's main with standalone_mode=False, so a usage error is raised as click.UsageError and help or --version return; each action's callback returns a coroutine, which run() awaits on the application's loop.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
Go builds the command line on cobra: groups, help, `--version`, completion, and an error function that turns a usage error into the invalid kind. Python's command line must do the same and call async handlers from inside the event loop `run_all` runs, where `asyncio.run` cannot be called again.

# Selected variant
[[#click with coroutine actions]]
- Owner decision, 2026-10-26: click.

# Searched variants

## click with coroutine actions

**Selected.**

### Description
`run()` builds a `click.Group` per resource under the root group and calls `root.main(args, standalone_mode=False, obj=RunState)`. click parses the arguments and calls the action's callback, which logs "cli call received" and returns the coroutine of the action; `run()` awaits it. `click.UsageError` becomes a `KindError` of kind invalid naming `{path} --help`; `NoArgsIsHelpError` - a group called with no action - prints the group's help as a success. Help and `--version`, which click prints itself, are captured with `redirect_stdout` and written to the entry's stdout.

### Benefits
- The closest to cobra: nested groups, help at every level, `--version`, shell completion built in; the most used CLI library of Python, with type information.
- The handler runs on the application's event loop, like every other entry point's.

### Costs
- One dependency.
- click's own messages ("No such command 'bogus'.", "No such option '--nope'.") differ from cobra's; the scenarios name click's.

## argparse

### Description
The standard library: subparsers per resource and action, `ArgumentParser.error` overridden to raise instead of exiting.

### Benefits
- No dependency; the action is parsed first and awaited after, with no trick.

### Costs
- No shell completion without `argcomplete`; help of a nested subparser is less structured; the override of `error` is the only guard against an exit of the library.

## typer

### Description
click with the command line declared by the type hints of a function.

### Benefits
- Less code per action.

### Costs
- A layer over click; flags derived from a function's signature do not fit the `bind` / `encode` / `text` split every stack keeps; no async actions of its own.
