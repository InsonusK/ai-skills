---
name: base-plateau-extension
description: How a variation point extends the base plateau without editing it — the extension points of the composition root (the services, the command registry with its ordered list of command decorators, the entries with each API's ordered list of entry decorators), the three kinds of command decorators (every command, by a command's mark, by a value of the command), authentication at the transport and the access check in the command layer, the variation point's own settings, and the attach list that proves no other base file changed
whenToUse: when you write, attach, or review a variation point of the architecture catalog, when you add a decorator around commands or around the requests of an API, when you add a command to an application built on the base plateau, or when you decide where a cross-cutting step (authentication, access, a limit, a transaction, a metric) goes
updated: 20261025
tags:
  - stack
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/command-registry-and-decorator-lists.md
---

# Goal
- **Attach by adding** - A variation point joins an application built on the [base plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md) with its own packages plus lines at the extension points of this skill; the attach check proves that no other base file changed.
- **Every cross-cutting step is a decorator in an ordered list** - Around commands, one list given to the command registry; around the requests of an API, one list given to that API's entry; the composition root shows every step and its place in one read.
- **A command is added in the command layer** - A new command adds its file and one line of the command registry; an API that serves it adds its route in its resource; the composition root does not change.

# Core Principle
- **Two lists of decorators** - An entry decorator sees a request of one API (its headers, its peer); a command decorator sees a command whatever API sent it. A step that needs the transport is an entry decorator; every other step is a command decorator, so a second API gets it for free.

```
request ──▶ request log ──▶ error answers ──▶ entry decorators ──▶ route of the contract
            (fixed, outermost: every answer      (the API's list,           │ the command
             is logged and is google.rpc.Status)  first = outermost)        ▼
                                                  command decorators ──▶ handler
                                                  (the registry's list, first = outermost)
```

- **The registry is where commands meet decorators** - One function of the command layer builds every handler with its services and wraps each with the same list; no handler is wrapped anywhere else.
- **The composition root reads as a list** - It shows which variation points are attached and in which order; the code of each lives in its own package.

# Scope
- Covers how a variation point attaches to the base plateau and how decorators are given, for every stack; what the base plateau itself contains is [base-plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md)'s.
- The implementation for each stack is described in its stack-specialized extension: `base-plateau-extension-in-go`, `base-plateau-extension-in-dotnet`, `base-plateau-extension-in-python`, each with a runnable sample variation point (access) attached to its stack's HTTP example.

# Rule

## MUST

### Attach only through the extension points
Attach a variation point through its own packages plus lines at these extension points; never edit another file of the base plateau to attach it.

| Extension point | Where | What a variation point adds there |
|---|---|---|
| `services` | composition root, first section | builds its infrastructure and the service over it, and passes the service to the command registry |
| command decorators | the list the composition root gives the command registry, `handlers` section | its decorator, at its place in the order |
| `entries` | the list the composition root gives the runner of the entry points | its entry point, built by its API's entry factory; an API variation point replaces the startup entry |
| entry decorators | the list the composition root gives one API's entry factory | its decorator of that API's requests (authentication, a rate limit) |
| handler constructor | the command registry | a parameter typed by an interface, through which the handler uses the variation point's service |
| command mark | the command's own file | a mark that a filtered decorator selects the command by |
| error kinds | the command layer | nothing: an entry point answers each kind of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md) by its own table |
| its own settings | its own package | reads its environment variables itself; the base settings never grow a field for it |

- Violation: an HTTP variation point that adds its port to the base settings; a store variation point that edits the startup entry; a decorator variation point that edits another variation point's package or the command registry.
- Risk: two variation points editing the same base file must be described together, and every combination becomes a separate project description.
- Fix: the variation point reads its settings in its own package and joins at the extension points above; a handler gaining a service parameter and a command gaining a mark are the application using the variation point, not attaching it.

### Build every handler in the command registry
Build every handler in one function of the command layer, which takes the list of command decorators and the services, wraps every handler with the whole list, and returns the registry — one typed field per command; give an entry point the registry, and let it take the handlers it serves.
- Violation: the composition root that builds and wraps each handler itself; an entry point that builds a handler; a handler wrapped by a decorator the others are not.
- Risk: every new command edits the composition root, and a command added in a hurry misses a decorator — it is served without its log line or without its access check.
- Fix: a new command adds its field and its line to the registry; a decorator that must skip some commands filters them itself (see [Filter commands by their mark](#filter-commands-by-their-mark)).

### Give decorators as an ordered list, first outermost
Give the command decorators, and the entry decorators of each API, as one list in the composition root, the first element the outermost: it runs first on the way in and last on the way out.

| Kind | Applies to | Example |
|---|---|---|
| every command | every command | logging (base plateau), a metric, a transaction of a store variation point |
| by the command's mark | the commands that carry a mark | access check: a command marked restricted needs the caller's role |
| by a value of the command | the commands whose mark exposes a value, decided by the value | batch limit: a command marked as a batch larger than the limit is rejected |

- Violation: a decorator applied by a line of its own after the list; two lists of command decorators for two groups of commands.
- Risk: the order — logging around the access check or inside it — is decided by where a line happens to stand, and changes when the next variation point adds its own.
- Fix: one list per kind; a variation point inserts its decorator at the place its skill names — an access check after the logging, so a rejected call is logged.

### Keep the fixed transport wrappers outermost
Let each API's entry factory put its fixed wrappers — the request log, then the error answers — outside the entry decorators, and the entry decorators around the contract's routes only, never around the docs or the management endpoints.
- Violation: an authentication middleware placed before the request log; a rate limit around the Swagger UI.
- Risk: a request rejected by an entry decorator leaves no log line and gets an answer in a format the contract does not describe; the docs become unreachable for the people who need them to call the API.
- Fix: the entry factory owns the order; the composition root gives only the list.

### Reject by an error kind
Let a decorator reject a call with an error of a kind of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md), answered by the API's error answers like any error of a handler; never write an answer of its own.
- Violation: an authentication middleware that writes `401` with a text body; a limit decorator that returns an empty result.
- Risk: one API answers the same rejection in two formats, and a client cannot tell a rejection from a success.
- Fix: unauthenticated for an unknown caller, permission denied for a known caller without the right, resource exhausted for a limit.

### Authenticate at the transport, check access in the command layer
Split an access rule into an entry decorator that turns the transport's credentials into the caller's identity, carried in the call's context as plain values, and a command decorator that decides whether this caller may run this command, as [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md)' "Identity resolved in the command layer" requires.
- Violation: the HTTP middleware that checks the role a route needs; a handler that reads a header.
- Risk: the gRPC API, added later, serves the same command without the check.
- Fix: a request without credentials reaches the command layer with no identity, and the command decorator rejects it when the command needs one; a request with credentials that do not identify a caller is rejected at the transport.

### Filter commands by their mark
Let a decorator that applies to some commands select them by a mark the command carries — an interface, a protocol, a base type of the command layer — and read a value the decision depends on through that mark; never by command names or types listed in the decorator.
- Violation: `if name == "PlanItems" || name == "AddItem"`; a list of command types given to the decorator.
- Risk: a new command that needs the step is missed, because the list lives far from the command.
- Fix: the command's file states its marks next to its fields; the decorator passes every unmarked command on unchanged.

### List the changed base files in attach.allow
Give every variation point's example an `attach.allow` file naming its base example (`base {path}`) and every base file it changes (`change {glob}`) or deletes (`delete {glob}`), and keep that list to the composition root, the build files, the readme, and the files of the commands it marks.
- Risk: an edit of a base file that no list names makes the variation point depend on that edit, and the next variation point that attaches finds a base it does not know.
- Fix: run the attach check of this repository (`bash test/check-vp-attach.sh`); every file it reports changed and not allowed is either moved to the variation point's package or turned into an extension point here.

## SHOULD

### Decorators wrap, handlers stay plain
Add a cross-cutting step (logging, a unit of work, a metric, an access check) as a decorator in one of the two lists, not as code inside each handler or each route.
- Risk: a step written into every handler is repeated per command and cannot be added, removed, or reordered in one place.
- Fix: one decorator per step, in its own file of its own package.

# Check list
- [ ] The variation point's example changes only the base files its `attach.allow` names, and `bash test/check-vp-attach.sh` passes.
- [ ] Every handler is built and wrapped in the command registry; the composition root gives it the list of command decorators and the services.
- [ ] Each API's entry is built by its entry factory from the registry, the logger, and its list of entry decorators.
- [ ] Every list is ordered first = outermost; the request log and the error answers stay outside the entry decorators.
- [ ] A decorator rejects with an error kind, and a filtered decorator selects commands by a mark.
- [ ] Authentication is an entry decorator; the access check is a command decorator.
- [ ] The variation point reads its own settings in its own package.

# Example
- The stack extensions' `examples/` — the access variation point attached to the HTTP example: an authentication entry decorator, an access check by the command's mark, and a batch limit by a value of the command.
