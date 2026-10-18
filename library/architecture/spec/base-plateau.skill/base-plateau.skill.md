---
name: base-plateau
description: The base plateau every application of the architecture catalog starts from — a program that starts, runs exactly one command, reports its result and exits — with its four layers, the direction of their dependencies, and the extension points a variation point attaches to without editing the base
whenToUse: when you create a new service or CLI application from the architecture catalog, when you write or review a variation point (an API, a client, a store, TaskBox, ...) that attaches to the base plateau, or when you check whether an application still keeps the base plateau's layer and dependency rules
updated: 20261018
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
- **Layers from the start** - The application already has the api, command, and domain layers of [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md), so a variation point adds to a layer instead of inventing one.
- **One surface for every entry** - Every entry point — the startup command here, an API variation point later — reaches the application only through commands and their handlers.
- **Variation points attach, never edit** - A variation point joins the application with its own package plus lines in the composition root; no other file of the base plateau changes.

# Core Principle
- **Nothing outside the process** - The base plateau uses no store, client, broker, or listener; each of them arrives as a variation point. Decision recorded in [one-startup-command](./adr/one-startup-command.md).
- **A command is any request** - The base plateau does not split reads from writes; an operation that only reads is still a command with a handler.
- **The composition root is the seam** - Every concrete type is built in one place, so a variation point attaches by adding lines there.

# Scope
- Covers what the base plateau contains and how a variation point attaches to it, for every stack; the layer rules themselves are [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md)'s.
- The implementation for each stack is described in its stack-specialized extension: `base-plateau-in-go`; `base-plateau-in-dotnet` and `base-plateau-in-python` are planned.

# Rule

## MUST

### Build the base on app-layers
Build the base plateau following [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md), with exactly these layers filled.

| Layer | In the base plateau |
|---|---|
| api | the startup entry only; every API variation point adds an entry beside it |
| command | the commands, their results and handlers, the handler contract, handler decorators, the error kinds |
| domain | the entities and domain services |
| service, infrastructure | empty — each arrives with a service variation point |
| composition root | construction of every concrete type, the base settings |

- Risk: a layer missing from the base is created differently by the first variation point that needs it, and the next variation point finds a second shape.
- Fix: create the three layers in the base, even when the sample has one command.

### One handler per command, one handler contract
Give every command exactly one handler, and give every handler the same contract: it takes a context and the command and returns the result or an error.
- Risk: a handler with its own shape cannot be wrapped by a decorator or served by an entry point without code written for it alone.
- Fix: define the contract once in the command layer and make every handler satisfy it.

### Errors carry a kind of error-kinds
Give every error a caller can act on one kind of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md), exposed by the command layer, and leave a failure of the service with none.
- Risk: an entry point that cannot tell a rejected command from a failure answers a typo with a server error, or a crash with a validation message.
- Fix: every such error wraps its kind; an entry point answers the kind through error-kinds' table, never an individual error.

### Startup command reports through the process interface
Run the startup command once, write its result to standard output as one machine-readable line, write logs to standard error, and exit `0` on success, `2` when the command is rejected as invalid, and `1` on any other failure.
- Risk: a script or a scheduler starting the process cannot tell its outcome apart, and logs mixed into the output break whoever reads the result.
- Fix: the startup entry encodes the result; the exit code comes from the error kind through the runner of the entry points, which stays when the startup entry goes; the composition root names the command and builds its input from a setting the startup entry reads itself, so the setting goes away with the entry.

### Run every entry point through one runner
Start the entry points in the composition root's `entries` section as a list run together by one runner of the base plateau: it runs them at once, stops all when the first ends — its work done, its failure, or termination — and turns the outcome into the exit code by the error kind.
- Violation: two entry points started one after the other, the second never reached; a variation point that writes its own concurrent start beside another's.
- Risk: every pair of variation points becomes a combination someone has to describe and code, which is what the base plateau exists to prevent; a process with one failed entry point and one live one looks healthy and serves half its API.
- Fix: a variation point adds its entry point to the list, or replaces the startup entry with it; it never starts one itself.

### Concrete types built only in the composition root
Construct every concrete type — handlers, decorators, entry points, services — only in the composition root, in its three marked sections `services`, `handlers`, and `entries`.
- Violation: a handler that constructs its own store, or an entry point that constructs its handler.
- Risk: a variation point cannot replace or wrap what another package builds for itself, so it has to edit that package.
- Fix: the composition root builds the object and passes it in; a variation point adds its lines to the matching section.

### Variation points attach only through the extension points
Attach a variation point through its own package plus lines at these extension points; never edit another file of the base plateau to attach it.

| Extension point | What a variation point adds there |
|---|---|
| Composition root, `services` section | constructs its infrastructure and the service over it |
| Composition root, `handlers` section | wraps handlers with its decorator (a unit of work, a metric, ...) |
| Composition root, `entries` section | adds its entry point (HTTP server, gRPC server, health endpoints, listener) to the list the runner starts; an API variation point replaces the startup entry with it |
| Handler constructor | a new interface parameter, through which the handler uses the service the variation point brings |
| Command-layer error kinds | an entry point maps each kind to its own response — a status code, an exit code |
| Its own package's settings | reads its settings itself; the base settings never grow a field for it |

- Violation: an HTTP variation point that adds its port to the base settings, or a store variation point that edits the startup entry.
- Risk: two variation points editing the same base file must be described together, and every combination becomes a separate project description.
- Fix: the variation point reads its settings in its own package and joins at the extension points above; a handler gaining a service parameter is using the variation point, not attaching it.

### Examples show the build, not the testing contract
Give the base plateau's example and every variation point's example the scenarios that prove their code and one command that runs them, and no testing tooling — test kinds, reports, badges, mutation testing.
- Risk: a change of the testing skills must be repeated in every example of the catalog, and the examples drift from them.
- Fix: the project's testing contract comes from the stack's conformance-testing skill; the stack extension names it.

## SHOULD

### Decorators wrap, handlers stay plain
Add a cross-cutting step (logging, a unit of work, a metric) as a decorator over the handler contract, wrapped in the composition root, not as code inside each handler.
- Risk: a step written into every handler is repeated per command and cannot be added or removed in one place.
- Fix: write one decorator per step and wrap the handlers that need it in the `handlers` section.

# Check list
- [ ] The application starts, runs its one startup command, writes the result as one line to standard output, logs to standard error, and exits `0`, `2`, or `1`.
- [ ] The api, command, and domain layers exist and follow [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md)'s check list.
- [ ] Every command has one handler; every handler satisfies the one handler contract.
- [ ] Every error a caller can act on carries one kind of error-kinds.
- [ ] Every concrete type is constructed in the composition root, in the `services`, `handlers`, or `entries` section.
- [ ] The `entries` section lists every entry point for the one runner; no entry point is started on its own.
- [ ] The base settings hold only the base plateau's own settings.
- [ ] A variation point attached to the application changed no base file other than the composition root.
- [ ] The examples hold scenarios and one command to run them, no testing tooling.
