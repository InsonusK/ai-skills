---
name: base-plateau
description: The base plateau every application of the architecture catalog starts from — a program that starts, runs exactly one command, reports its result and exits — with its four layers, the direction of their dependencies, and the extension points a variation point attaches to without editing the base
whenToUse: when you create a new service or CLI application from the architecture catalog, when you write or review a variation point (an API, a client, a store, TaskBox, ...) that attaches to the base plateau, or when you check whether an application still keeps the base plateau's layer and dependency rules
updated: 20261017
tags:
  - stack
  - concern/architecture
  - app-type/service
  - app-type/cli
adr:
  - adr/one-startup-command.md
---

# Goal
- **Runs with no variation point** - The application starts, runs its one startup command, reports the result, and exits with a code that tells success, a rejected command, and a failure apart.
- **Business rules in the domain** - An entity guards its own state; a domain service guards a rule that spans several entities; no other layer decides a business rule.
- **One surface for every entry** - Every entry point — the startup command here, an API variation point later — reaches the application only through commands and their handlers.
- **Variation points attach, never edit** - A variation point joins the application with its own package plus lines in the composition root; no other file of the base plateau changes.

# Core Principle
- **Dependencies point inward** - Entry point → Application → Domain; the composition root is the only code that knows every layer — see [Keep the four layers and their imports](#keep-the-four-layers-and-their-imports).
- **Nothing outside the process** - The base plateau uses no store, client, broker, or listener; each of them arrives as a variation point. Decision recorded in [one-startup-command](./adr/one-startup-command.md).
- **A command is any request** - The base plateau does not split reads from writes; an operation that only reads is still a command with a handler.

# Scope
- Covers the base plateau's layers, rules, and extension points for every stack.
- The implementation for each stack is described in its stack-specialized extension: `base-plateau-in-go`; `base-plateau-in-dotnet` and `base-plateau-in-python` are planned.

# Rule

## MUST

### Keep the four layers and their imports
Place every part of the application in one of these layers and let it import only what its row allows.

| Layer | Holds | May import |
|---|---|---|
| Domain | entities, domain services, business-rule errors | nothing of the application |
| Application | commands, their results, handlers, the handler contract, handler decorators, the error kinds an entry point maps | Domain |
| Entry point | the startup command entry; every API variation point adds one beside it | Application |
| Composition root | construction of every concrete type, the wiring of handlers to entry points, the base settings | every layer |

- Violation: a domain entity that imports the application package to reuse a result type.
- Risk: a dependency pointing outward ties the inner layer to code that changes with every entry point and variation point.
- Fix: move the shared type to the inner layer, or map between the two types in the handler.

### Entry points import only the application
Let an entry point use only commands, results, the handler contract, and the error kinds of the application layer.
- Violation: an HTTP handler that builds a domain entity or calls a domain service.
- Risk: the entry point becomes a second home for business rules, and every new entry point repeats them.
- Fix: add or extend a command; the entry point builds the command and sends it to its handler.

### Domain stays free of other layers and I/O
Keep the domain importing no other layer of the application and performing no input or output.
- Risk: a domain that reads a clock source, a file, or a store can no longer be tested by its scenarios alone, and a variation point that replaces that source has to edit the domain.
- Fix: the handler reads what the rule needs and passes it to the entity or domain service as a value.

### One handler per command, one handler contract
Give every command exactly one handler, and give every handler the same contract: it takes a context and the command and returns the result or an error.
- Risk: a handler with its own shape cannot be wrapped by a decorator or served by an entry point without code written for it alone.
- Fix: define the contract once in the application layer and make every handler satisfy it.

### Domain types never leave the application
Return from a handler only results made of application types, never an entity or a domain service.
- Risk: an entry point that receives an entity depends on the domain and can change its state outside a handler.
- Fix: map the entity to a result type inside the handler.

### Business-rule errors carry one kind
Make every business-rule error identifiable as one "invalid" kind exposed by the application layer, distinct from any other error.
- Risk: an entry point that cannot tell a rejected command from a failure answers a typo with a server error, or a crash with a validation message.
- Fix: every domain error wraps the invalid kind; an entry point checks the kind, never an individual error.

### Startup command reports through the process interface
Run the startup command once, write its result to standard output as one machine-readable line, write logs to standard error, and exit `0` on success, `2` when the command is rejected as invalid, and `1` on any other failure.
- Risk: a script or a scheduler starting the process cannot tell its outcome apart, and logs mixed into the output break whoever reads the result.
- Fix: the startup entry encodes the result and maps the error kind to the exit code; the composition root names the command and builds its input from the base settings.

### Concrete types built only in the composition root
Construct every concrete type — handlers, decorators, entry points, services — only in the composition root, in its three marked sections `services`, `handlers`, and `entries`.
- Violation: a handler that constructs its own store, or an entry point that constructs its handler.
- Risk: a variation point cannot replace or wrap what another package builds for itself, so it has to edit that package.
- Fix: the composition root builds the object and passes it in; a variation point adds its lines to the matching section.

### Variation points attach only through the extension points
Attach a variation point through its own package plus lines at these extension points; never edit another file of the base plateau to attach it.

| Extension point | What a variation point adds there |
|---|---|
| Composition root, `services` section | constructs its client, store, or other service |
| Composition root, `handlers` section | wraps handlers with its decorator (a transaction, a metric, ...) |
| Composition root, `entries` section | starts its entry point (HTTP server, listener, ...) |
| Handler constructor | a new interface parameter, through which the handler uses the service the variation point brings |
| Application error kinds | an entry point maps each kind to its own response — a status code, an exit code |
| Its own package's settings | reads its settings itself; the base settings never grow a field for it |

- Violation: an HTTP variation point that adds its port to the base settings, or a store variation point that edits the startup entry.
- Risk: two variation points editing the same base file must be described together, and every combination becomes a separate project description.
- Fix: the variation point reads its settings in its own package and joins at the extension points above; a handler gaining a service parameter is using the variation point, not attaching it.

## SHOULD

### Decorators wrap, handlers stay plain
Add a cross-cutting step (logging, a transaction, a metric) as a decorator over the handler contract, wrapped in the composition root, not as code inside each handler.
- Risk: a step written into every handler is repeated per command and cannot be added or removed in one place.
- Fix: write one decorator per step and wrap the handlers that need it in the `handlers` section.

# Check list
- [ ] The application starts, runs its one startup command, writes the result as one line to standard output, logs to standard error, and exits `0`, `2`, or `1`.
- [ ] Entry points import only the application layer; the domain imports no other layer and performs no I/O.
- [ ] Every command has one handler; every handler satisfies the one handler contract and returns no domain type.
- [ ] Every business-rule error carries the application's invalid kind.
- [ ] Every concrete type is constructed in the composition root, in the `services`, `handlers`, or `entries` section.
- [ ] The base settings hold only the base plateau's own settings.
- [ ] A variation point attached to the application changed no base file other than the composition root.
