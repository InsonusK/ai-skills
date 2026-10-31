---
name: base-plateau-extension
description: How a variation point extends the base plateau without editing it — the extension points of the composition root (the services, the command registry with its ordered list of command middleware, the entries with each API's ordered list of API middleware), every middleware in a directory of its own named {Name}Middleware, the fixed request log and answer-failures middleware outermost, the three kinds of command middleware (every command, by a command's mark, by a value of the command), authentication at the transport and the access check in the command layer, the variation point's own settings, and the attach list that proves no other base file changed
whenToUse: when you write, attach, or review a variation point of the architecture catalog, when you add a middleware around commands or around the requests of an API, when you add a command to an application built on the base plateau, or when you decide where a cross-cutting step (authentication, access, a limit, a transaction, a metric, logging) goes
updated: 20261102
tags:
  - stack
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/command-registry-and-middleware-lists.md
  - adr/fixed-middleware-inside-the-entry.md
---

# Goal
- **Attach by adding** - A variation point joins an application built on the [base plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md) with its own directories plus lines at the extension points of this skill; the attach check proves that no other base file changed.
- **Every cross-cutting step is a middleware in an ordered list** - Around commands, one list given to the command registry; around the requests of an API, one list given to that API's entry; the composition root shows every step and its place in one read.
- **Every middleware is found by its name** - Each lives in a directory of its own, `middleware/{name}/`, beside the layer it wraps, so a reader finds the logging, the access check, or the request log by looking for its name.
- **A command is added in the command layer** - A new command adds its file and one line of the command registry; an API that serves it adds its route in its resource; the composition root does not change.

# Core Principle
- **Two lists of middleware** - An API middleware sees a request of one API (its headers, its peer); a command middleware sees a command whatever API sent it. A step that needs the transport is an API middleware; every other step is a command middleware, so a second API gets it for free.

@import "./diagrams/concept.mmd" {as="mermaid"}

```
command/                          api/{kind}/
├── pipeline/   the contract      ├── pipeline/   the server, the endpoint, the settings
└── middleware/                   └── middleware/
    ├── logging/                      ├── request log     (fixed)
    ├── access/                       ├── answer failures (fixed)
    └── {name}/                       └── {name}/
```

- **The registry is where commands meet middleware** - One function of the command layer builds every handler with its services and wraps each with the same list; no handler is wrapped anywhere else.
- **The composition root reads as a list** - It shows which variation points are attached and in which order; the code of each lives in its own directories.

# Scope
- Covers how a variation point attaches to the base plateau and how middleware are given, for every stack; what the base plateau itself contains is [base-plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md)'s.
- The implementation for each stack is described in its stack-specialized extension: `base-plateau-extension-in-go`, `base-plateau-extension-in-dotnet`, `base-plateau-extension-in-python`, each with a runnable sample variation point (access) attached to its stack's HTTP example.

# Rule

## MUST

### Attach only through the extension points
Attach a variation point through its own directories plus lines at these extension points; never edit another file of the base plateau to attach it.

| Extension point | Where | What a variation point adds there |
|---|---|---|
| `services` | composition root, first section | builds its infrastructure and the service over it, and passes the service to the command registry |
| command middleware | the list the composition root gives the command registry, `handlers` section | its middleware, at its place in the order |
| `entries` | the list the composition root gives the runner of the entry points | its entry point, built by its API's entry factory; an API variation point replaces the startup entry |
| API middleware | the list the composition root gives one API's entry factory | its middleware of that API's requests (authentication, a rate limit) |
| handler constructor | the command registry | a parameter typed by an interface, through which the handler uses the variation point's service |
| command mark | the command's own file | a mark that a filtered middleware selects the command by |
| error kinds | the command layer | nothing: an entry point answers each kind of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md) by its own table |
| its own settings | its own directory | reads its environment variables itself; the base settings never grow a field for it |

- Violation: an HTTP variation point that adds its port to the base settings; a store variation point that edits the startup entry; a middleware variation point that edits another variation point's directory or the command registry.
- Risk: two variation points editing the same base file must be described together, and every combination becomes a separate project description.
- Fix: the variation point reads its settings in its own directory and joins at the extension points above; a handler gaining a service parameter and a command gaining a mark are the application using the variation point, not attaching it.

### Build every handler in the command registry
Build every handler in one function of the command layer, which takes the list of command middleware and the services, wraps every handler with the whole list, and returns the registry — one typed field per command; give an entry point the registry, and let it take the handlers it serves.
- Violation: the composition root that builds and wraps each handler itself; an entry point that builds a handler; a handler wrapped by a middleware the others are not.
- Risk: every new command edits the composition root, and a command added in a hurry misses a middleware — it is served without its log line or without its access check.
- Fix: a new command adds its field and its line to the registry; a middleware that must skip some commands filters them itself (see [Filter commands by their mark](#filter-commands-by-their-mark)).

### Give a nested command's wrapped handler to the handler that runs it
Build the handler of a command that another command runs before that other handler, in the command registry, and pass it the wrapped one as a constructor parameter typed by the handler contract, as [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md#run-a-nested-command-through-the-command-pipeline) requires.
- Violation: a handler that builds the nested command's handler itself; the registry passing the handler before wrapping it.
- Risk: the nested command runs without the middleware, so its log line, its access check, and its unit of work depend on who started it.
- Fix: the registry wraps the nested handler once, stores it in its field, and passes the same value to the outer handler; every middleware is written to run again inside an outer command — a unit of work joins the open one.

### Put every middleware in a directory of its own
Put each middleware in `middleware/{name}/` of the layer it wraps — `command/middleware/{name}/` for a command middleware, `api/{kind}/middleware/{name}/` for an API's — and name it `{Name}Middleware`; keep the layer's `pipeline/` for the contract and the server only.
- Violation: the logging of commands in the command contract's file; an authentication middleware in the API's fixed server code; two middleware in one directory.
- Risk: a reader who looks for "the access check" finds a file named after something else, and a variation point that adds a middleware edits a directory another one owns.
- Fix: one directory per middleware, named after its step; the base plateau's own logging is a middleware like any other, `command/middleware/logging/`.

### Give middleware as an ordered list, first outermost
Give the command middleware, and the API middleware of each API, as one list in the composition root, the first element the outermost: it runs first on the way in and last on the way out.

| Kind | Applies to | Example |
|---|---|---|
| every command | every command | logging (base plateau), a metric, a transaction of a store variation point |
| by the command's mark | the commands that carry a mark | access check: a command marked restricted needs the caller's role |
| by a value of the command | the commands whose mark exposes a value, decided by the value | batch limit: a command marked as a batch larger than the limit is rejected |

- Violation: a middleware applied by a line of its own after the list; two lists of command middleware for two groups of commands.
- Risk: the order — logging around the access check or inside it — is decided by where a line happens to stand, and changes when the next variation point adds its own.
- Fix: one list per kind; a variation point inserts its middleware at the place its skill names — an access check after the logging, so a rejected call is logged.

### Keep the fixed middleware outermost
Let each API's entry factory build its fixed middleware itself from the logger it is given — the request log outermost, then answer failures — and put them around everything the API serves, and the API middleware of the list around the contract's routes only, never around the docs or the management endpoints. Decision recorded in [fixed-middleware-inside-the-entry](./adr/fixed-middleware-inside-the-entry.md), which every API variation point follows.
- Violation: an authentication middleware placed before the request log; a rate limit around the Swagger UI; the request log given in the composition root's list.
- Risk: a request rejected by an API middleware leaves no log line and gets an answer in a format the contract does not describe; the docs become unreachable for the people who need them to call the API; a request of no route of the contract (an unknown path) is answered unlogged.
- Fix: the entry factory owns the fixed middleware and their order; the composition root gives only the list of the contract's middleware.

### Answer every failure at the transport
Let the answer-failures middleware answer every error that reaches it — an error of a kind by the kind, anything else, a panic among them, as INTERNAL — so no request of the API ends without a `google.rpc.Status` answer.
- Violation: a server that lets a panic of a route cut the connection; a catch-all in the command layer that turns every exception into a result.
- Risk: a failure outside the command — binding the request, mapping the result, an API middleware, the framework — escapes the error format and the log however the commands behave; the command layer reports its errors by raising them and cannot answer them itself.
- Fix: the fixed middleware of every API kind, tested by a scenario of a route that fails.

### Keep a middleware free of the state of a call
Build every middleware once and let it serve every call, several at the same time: keep in it only what it was built with — its settings, a logger, a factory — carry what belongs to one call in the call's context, and obtain a dependency that lives for one call (a database session) per call.
- Violation: a field written on one call and read on the next; a database session given to the middleware when it is built.
- Risk: two calls served at the same time see each other's caller or share one session that is not safe to share, and the fault shows only under load.
- Fix: Go — the closure keeps only what `Middleware(...)` was given, the call's values travel in `context.Context`; Python — the instance keeps only what `__init__` was given, the call's values travel in a `ContextVar`; dotnet — `base-plateau-extension-in-dotnet`'s rule on scoped dependencies.

### Reject by an error kind
Let a middleware reject a call with an error of a kind of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md), answered by the API's answer failures like any error of a handler; never write an answer of its own.
- Violation: an authentication middleware that writes `401` with a text body; a limit middleware that returns an empty result.
- Risk: one API answers the same rejection in two formats, and a client cannot tell a rejection from a success.
- Fix: unauthenticated for an unknown caller, permission denied for a known caller without the right, resource exhausted for a limit.

### Authenticate at the transport, check access in the command layer
Split an access rule into an API middleware that turns the transport's credentials into the caller's identity, carried in the call's context as plain values, and a command middleware that decides whether this caller may run this command, as [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md)' "Identity resolved in the command layer" requires.
- Violation: the HTTP middleware that checks the role a route needs; a handler that reads a header.
- Risk: the gRPC API, added later, serves the same command without the check.
- Fix: a request without credentials reaches the command layer with no identity, and the command middleware rejects it when the command needs one; a request with credentials that do not identify a caller is rejected at the transport.

### Filter commands by their mark
Let a middleware that applies to some commands select them by a mark the command carries — an interface, a protocol, a base type of the command layer — and read a value the decision depends on through that mark; never by command names or types listed in the middleware.
- Violation: `if name == "PlanItems" || name == "AddItem"`; a list of command types given to the middleware.
- Risk: a new command that needs the step is missed, because the list lives far from the command.
- Fix: the command's file states its marks next to its fields; the middleware passes every unmarked command on unchanged.

### List the changed base files in attach.allow
Give every variation point's example an `attach.allow` file naming its base example (`base {path}`) and every base file it changes (`change {glob}`) or deletes (`delete {glob}`), and keep that list to the composition root, the build files, the readme, and the files of the commands it marks.
- Risk: an edit of a base file that no list names makes the variation point depend on that edit, and the next variation point that attaches finds a base it does not know.
- Fix: run the attach check of this repository (`bash test/check-vp-attach.sh`); every file it reports changed and not allowed is either moved to the variation point's directory or turned into an extension point here.

### Draw every variation point
Draw each variation point as [vp-diagrams](library/common-workflow/architecture/design/vp-diagrams.skill.md) says: the concept in its spec skill, one real call with each part's class and file in each stack extension.
- Risk: a variation point without diagrams is read rule by rule before anyone sees how it works.
- Fix: run the diagram check of this repository (`bash test/check-vp-diagrams.sh`); it fails on a skill without its diagram and on a participant whose file is not in the example.

### Name the per-API code a sample
Add a file two variation points of one base both add identically, except the code an application writes per API or per use that the example only shows (an adapter of an API and the service over it), which the `attach.allow` names a sample (`sample {glob}`).
- Violation: fixed code copied from templates, or a shared build file, named a sample.
- Risk: two variation points that add one path differently conflict when both are attached; a sample glob that covers fixed code hides that conflict.
- Fix: the attach check compares every file two examples add unless either names it a sample; fixed code lives under a path of its own (`client/core/httpclient`, `client/core/grpcclient`).

## SHOULD

### Middleware wrap, handlers stay plain
Add a cross-cutting step (logging, a unit of work, a metric, an access check) as a middleware in one of the two lists, not as code inside each handler or each route.
- Risk: a step written into every handler is repeated per command and cannot be added, removed, or reordered in one place.
- Fix: one middleware per step, in its own directory.

# Check list
- [ ] The variation point's example changes only the base files its `attach.allow` names, and `bash test/check-vp-attach.sh` passes.
- [ ] The variation point is drawn per vp-diagrams, and `bash test/check-vp-diagrams.sh` passes.
- [ ] Every handler is built and wrapped in the command registry; the composition root gives it the list of command middleware and the services.
- [ ] A handler that runs another command gets that command's wrapped handler from the registry.
- [ ] Every middleware lives in `middleware/{name}/` of the layer it wraps and is named `{Name}Middleware`.
- [ ] Each API's entry is built by its entry factory from the registry, the logger, and its list of API middleware.
- [ ] Every list is ordered first = outermost; the request log and answer failures are built by the entry itself from its logger, stay outside the API middleware, and answer every failure in the API's error format ([fixed-middleware-inside-the-entry](./adr/fixed-middleware-inside-the-entry.md)).
- [ ] A middleware rejects with an error kind, and a filtered middleware selects commands by a mark.
- [ ] No middleware keeps the state of a call; a dependency that lives for one call is obtained per call.
- [ ] Authentication is an API middleware; the access check is a command middleware.
- [ ] The variation point reads its own settings in its own directory.

# Example
- The stack extensions' `examples/` — the access variation point attached to the HTTP example: an authentication API middleware, an access check by the command's mark, and a batch limit by a value of the command.
