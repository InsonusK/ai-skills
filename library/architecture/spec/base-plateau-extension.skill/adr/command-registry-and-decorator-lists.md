---
name: command registry and decorator lists
description: How the composition root builds the handlers and gives the cross-cutting steps, so a variation point that adds a step edits neither another variation point nor every handler line
problem: In the first examples the composition root built every handler and wrapped each with its decorators one line at a time, while the HTTP entry wrapped every request automatically; a decorator variation point would have had to add one line per command, and an entry decorator had no place at all. Where are the handlers built, and how is a cross-cutting step given?
decision: One function of the command layer builds every handler with its services and wraps each with one list of command decorators, returning a typed registry; every API's entry factory takes the registry, the logger, and one list of entry decorators; both lists are given in the composition root, first = outermost.
tags:
  - stack
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
A variation point that adds a step around commands — an access check, a transaction — must reach every handler, and one that adds a step around requests — authentication — must reach every request of its API. With each handler wrapped by its own line of the composition root, the first needs a line per command and the second has no extension point; the order of the steps depends on where the lines stand.

# Selected variant
[[#Typed registry, two ordered lists]]
- Owner decision, 2026-10-24: a typed registry over a dictionary; decorators of the entry points too, shown on an authentication example; the lists ordered in the composition root.

# Searched variants

## Typed registry, two ordered lists

**Selected.**

### Description
The command layer has one function — `command.NewHandlers` in Go, `build_handlers` in Python, the module's `AddTodoModule()` with MediatR in dotnet — that builds every handler and wraps it with the list of command decorators it is given. Each API has an entry factory that builds its entry point from the registry, the logger, and a list of entry decorators. The composition root gives both lists.

### Benefits
- A new command touches only the command layer and the resource that routes it.
- A decorator variation point adds one element to one list; the order of every step is visible in one place.
- A typed field per command keeps the compiler (and `mypy --strict`) checking that an entry point calls a handler with the right command.

### Costs
- In Go a generic decorator cannot stand in a list, so the decorator sees the command as `any` and a typed adapter (`pipeline.Decorate`) hides the type assertions.
- A decorator for some commands needs a mark on those commands, which is an edit of the command's file.

## Each handler wrapped in the composition root

### Description
The composition root builds every handler and wraps it with its decorators, one line per decorator and command.

### Benefits
- No registry and no adapter; each wrapping is explicit.

### Costs
- A decorator variation point edits one line per command, and a new command repeats every wrapping.
- A command added without one of the wrappings goes unnoticed.

## A dictionary of handlers by command name

### Description
The registry is a map from the command's name to an untyped handler.

### Benefits
- One generic loop wraps every handler; no field per command.

### Costs
- An entry point looks its handler up by a string and casts it; a typo or a wrong type fails at run time instead of at build time.
