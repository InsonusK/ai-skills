---
name: vp-grpc-api
description: The gRPC API variation point — an entry point attached to the base plateau that serves commands as methods of a versioned proto contract, answers errors by their kind, survives a panicking method, offers server reflection behind a switch, stops gracefully, and replaces the startup entry without editing any other base file
whenToUse: when an application built on the base plateau must be reachable over gRPC — adding the gRPC entry point, serving a new command over gRPC, or reviewing the gRPC entry point of such an application
updated: 20261021
tags:
  - stack
  - concern/architecture
  - app-type/service
---

# Goal
- **Commands reachable over gRPC** - Every command the service exposes is one method of a versioned proto contract; the method turns the request message into the command's plain parameters and the result into the response message.
- **Rejected told from failed** - A client tells a call rejected by a business rule or by its shape (`INVALID_ARGUMENT`) from a failure of the service (`INTERNAL`) by its status code.
- **Attached, not edited** - The entry point joins the [base plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md) through its extension points: it replaces the startup entry and changes no other base file but the composition root and the module's dependency, build, and readme files.

# Core Principle
- **A thin translation** - The gRPC entry point decodes, calls, and encodes; it makes no business decision, as [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md) requires of every api layer.
- **The messages are the contract's own** - Request and response messages are generated from the proto file and mapped to and from the command's types, so a change of a command's result does not change the published contract unnoticed.
- **Written once for every method** - Mapping errors to status codes, recovering panics, reflection, and shutdown are one piece of code the same in every application; serving a command adds a method, its two mappings, and its lines in the proto file.

# Scope
- Covers a gRPC API with unary methods. Streaming methods are not covered yet.
- The HTTP form of the gRPC contract — the same operations over HTTP with proto-JSON, as [api-develop](library/common-workflow/develop/api-develop.skill/api-develop.skill.md) recommends — comes from attaching [vp-http-api](library/architecture/spec/vp-http-api.skill/vp-http-api.skill.md) as well, in its mode for a proto contract; both entries run side by side with the same handlers.
- Serves no health check: the probes of every service, whatever its entry points, go to the same HTTP health endpoints of [vp-health](library/architecture/spec/vp-health.skill/vp-health.skill.md). The standard gRPC health service (`grpc.health.v1`) is left out on purpose — a Docker `healthcheck` cannot call it, and two kinds of health check across the services of one system is one kind too many.
- The implementation for each stack is described in its stack-specialized extension: `vp-grpc-api-in-go`, `vp-grpc-api-in-dotnet`; `vp-grpc-api-in-python` is planned.

# Rule

## MUST

### Follow api-develop for the contract
Version and place the proto contract per [api-develop](library/common-workflow/develop/api-develop.skill/api-develop.skill.md): the major version in the proto package (`todo.v1`), `// version: {major}.{minor}.{patch}` as the file's first line, the file under `docs/integration/api/{contract-name}/`, and a recorded decision whether the contract has an HTTP form.
- Risk: a contract without a major in its package cannot gain a breaking change beside the old one, and clients built against it break silently.
- Fix: check the contract against api-develop's check list; generate the code from it, never write message types by hand.

### One method per command
Serve each exposed command as exactly one method, which decodes the request message into the command's plain parameters, calls the command's handler, and encodes its result into the response message.
- Violation: a method that calls two commands, reads state to choose one, or calls a service.
- Risk: business logic moves into the gRPC layer and every other entry point must repeat it.
- Fix: one command per method; a choice that depends on state goes to a command factory per app-layers.

### Map the messages, never pass them through
Map the generated request message to the command and the command's result to the generated response message in the entry point.
- Violation: a command that takes a generated request message, or a command result type declared in the proto file.
- Risk: the command layer depends on the transport, and the command can no longer be called from HTTP, a CLI, or a test without building gRPC messages.
- Fix: a `decode` and an `encode` per method, beside the method.

### Answer errors by their kind
Answer a request that cannot become the command with `INVALID_ARGUMENT` without calling the command, and every error of a command by [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md)' table — its kind's code with the error's message, or `INTERNAL` with no internal message for an error with no kind.
- Risk: a client cannot tell its own mistake from the service's, the HTTP and gRPC forms of a service answer one outcome differently, and an `INTERNAL` that carries the internal error leaks connection strings and stack details.
- Fix: one table shared by every entry point of the application; the failure itself is logged by the handler's logging decorator.

### Survive a panicking method
Answer a method that panics with `INTERNAL`, log the panic, and keep serving.
- Risk: in a runtime that does not recover a panic in a call (grpc-go), one bad request stops the process and every call in flight with it.
- Fix: a server interceptor that recovers, registered once for every method.

### Offer server reflection behind a switch
Serve gRPC server reflection only when `GRPC_REFLECTION_ENABLED` is `true`; it is off by default.
- Risk: without reflection a developer needs the proto file to call the service with grpcurl or Postman; with reflection always on, production publishes its full API surface to anyone who reaches the port.
- Fix: switch it on in a local or test environment, the way `HTTP_API_DOCS_ENABLED` switches on Swagger UI.

### Log every call with what arrived
Write two records per call through the composition root's logger — one when it arrives, with what arrived as the payload, one when it is answered — the second with the method, the status code, and the duration, whether it failed, and as the payload the request message and the response message as returned; put the request id — the client's (`x-request-id` metadata), else a new one — into the context and send it back.
- Risk: a request that failed cannot be found or replayed from the log, its records cannot be told from those of the requests served at the same time, and a fault in turning the result into the response is invisible.
- Fix: an in record and an out record per call, so the log reads top-down with the command's records between them; the out record of a failure repeats the input; the data only as the payload attribute, so the base handler drops it and the logging variation point masks it.

### Stop gracefully
On termination (`SIGTERM`), stop accepting calls, let calls in flight finish within a bounded time, cut off what remains, and return without an error.
- Risk: a rolling update cuts calls off mid-way and reports every stop as a failure; an unbounded wait lets one stuck call block the stop until the orchestrator kills the process.
- Fix: serve until the composition root's context is cancelled, then stop gracefully with a timeout.

### Read its own settings
Read `GRPC_LISTEN_PORT` (default `9090`) and `GRPC_REFLECTION_ENABLED` in the gRPC entry point's own package, rejecting an unusable value with an error naming its variable.
- Risk: settings added to the base settings outlive the entry point and make the variation point edit a base file.
- Fix: a settings loader in the entry point, called by its one start call.

### Group methods by service
Keep the methods of each service of the contract together with their mappings in one unit (a file, a class), keep the list of all services to one line per service, and keep these units apart from the entry point's fixed code, which is the same in every application.
- Violation: every method of the API in one file or class.
- Risk: the unit holding every method grows with each command until it cannot be read or reviewed, and two changes to different services conflict in it.
- Fix: one unit per proto service in a folder of its own; adding a method to a service changes only that unit, the contract, and the composition root.

### Attach by replacing the startup entry
Attach the entry point by adding its packages, the contract folder, and the generated code, replacing the startup entry in the composition root's `entries` list with one entry whose start reads the entry point's settings, builds it, and serves until termination, deleting the startup entry, and changing no other base file than the composition root and the module's dependency, build, and readme files.
- Risk: any other base file changed by the gRPC variation point must be described together with every variation point that touches it.
- Fix: the stack extension lists the base files it changes; a check compares its example with the base example against that list.

### Test with the commands faked
Test the fixed code once through a test service of its own — a success, a rejected command, an undecodable request, a failure, a panic, reflection on and off, the settings, and the graceful stop with a call in flight — and each service of the contract with its command handlers replaced by fakes, plus one scenario holding the served services against the contract's services.
- Risk: a test through the real command tests the domain again and breaks on every business change; a service left out of the served list answers every call `UNIMPLEMENTED` while all other scenarios pass.
- Fix: one fake handler per command; a service's scenarios check only its own mappings.

# Check list
- [ ] The proto contract carries its major in the package and its full version in the first-line comment, under `docs/integration/api/{contract-name}/`.
- [ ] Every exposed command is one method; its messages are generated and mapped to and from the command in the entry point.
- [ ] An undecodable request is `INVALID_ARGUMENT`; every error of a command is answered by error-kinds' table; an error with no kind and a panic are `INTERNAL` without the internal message.
- [ ] Reflection is served only with `GRPC_REFLECTION_ENABLED=true`.
- [ ] The server stops gracefully within a bounded time on termination.
- [ ] The entry point reads its own two settings; the base settings gained nothing.
- [ ] Methods are grouped by service, one unit each, apart from the fixed code.
- [ ] The entry point is one entry of the `entries` list; the startup entry is gone; no base file changed but the composition root and the module's dependency, build, and readme files.
- [ ] A scenario holds the served services against the contract's services.
