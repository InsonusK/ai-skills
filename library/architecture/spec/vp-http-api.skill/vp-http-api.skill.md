---
name: vp-http-api
description: The HTTP API variation point — an entry point attached to the base plateau that serves commands as versioned HTTP routes, answers errors by their kind, publishes its OpenAPI contract and Swagger UI, stops gracefully, and replaces the startup entry without editing any other base file
whenToUse: when an application built on the base plateau must be reachable over HTTP — adding the HTTP entry point, serving a new command over HTTP, or reviewing the HTTP entry point of such an application
updated: 20261021
tags:
  - stack
  - concern/architecture
  - app-type/service
---

# Goal
- **Commands reachable over HTTP** - Every command the service exposes has one versioned route; the route turns the request into the command's plain parameters and the result into its response.
- **Rejected told from failed** - A client tells a request rejected by a business rule or by its shape (400) from a failure of the service (500) by status and by one error body.
- **Contract published** - The service's OpenAPI contract, Swagger UI, and every URL they emit follow [api-develop](library/common-workflow/develop/api-develop.skill/api-develop.skill.md), and the served routes match the contract's operations.
- **Attached, not edited** - The entry point joins the [base plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md) through its extension points: it replaces the startup entry and changes no other base file but the composition root and the module's dependency files.

# Core Principle
- **A thin translation** - The HTTP entry point decodes, calls, and encodes; it makes no business decision, as [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md) requires of every api layer.
- **The HTTP contract is the entry point's own** - Request and response types belong to the HTTP entry point and are mapped to and from the command's types, so a change of a command's result does not change the published contract unnoticed.
- **Written once for every command** - Decoding errors, mapping errors to statuses, and writing JSON are one generic piece of code; serving a command adds a route, its two types, and its two mappings.

# Scope
- Covers an HTTP API whose contract is written by hand in OpenAPI, and the HTTP form of a gRPC contract — the same proto file served over HTTP, as [api-develop](library/common-workflow/develop/api-develop.skill/api-develop.skill.md) recommends — in a service that also has [vp-grpc-api](library/architecture/spec/vp-grpc-api.skill/vp-grpc-api.skill.md).
- Serves no health endpoints: they come from [vp-health](library/architecture/spec/vp-health.skill/vp-health.skill.md) on a port of their own, the same in every service, and never published through the ingress.
- The implementation for each stack is described in its stack-specialized extension: `vp-http-api-in-go`; `vp-http-api-in-dotnet` and `vp-http-api-in-python` are planned.

# Rule

## MUST

### Follow api-develop
Version, document, and publish the HTTP API per [api-develop](library/common-workflow/develop/api-develop.skill/api-develop.skill.md): the major version in every route (`/v1/...`), the contract file under `docs/integration/api/{contract-name}/`, Swagger UI and the spec behind `HTTP_API_DOCS_ENABLED`, every emitted URL under `HTTP_BASE_PATH`, success status 201/202/200 by what the command did, and `google.rpc.Status` as every error body.
- Risk: an HTTP API that departs from the shared rules answers differently from every other service of the system and breaks behind an ingress prefix.
- Fix: check the entry point against api-develop's check list.

### Serve the HTTP form of a proto contract from the proto file
When the service has a gRPC contract, take the HTTP routes, the request and response shapes, and the spec from the proto file: one route per `google.api.http` annotation, the generated messages in standard proto-JSON, and the spec generated from the annotations with every success and error status listed.
- Violation: a hand-written OpenAPI spec or request types of the entry point's own beside a proto contract.
- Risk: two descriptions of one API drift, and the HTTP form answers in a shape the gRPC form does not have.
- Fix: the proto file is the only contract; a scenario holds the served routes against the generated spec.

### One route per command
Serve each exposed command at exactly one route, whose handler decodes the request into the command's plain parameters, calls the command's handler, and encodes its result.
- Violation: a route that calls two commands, reads state to choose one, or calls a service.
- Risk: business logic moves into the HTTP layer and every other entry point must repeat it.
- Fix: one command per route; a choice that depends on state goes to a command factory per app-layers.

### Group routes by resource
Keep the routes of each resource — the first path segment after the version, its collection and item paths together — with their request and response types and mappings in one unit (a file, a controller), keep the list of all routes to one line per resource, and keep these units apart from the entry point's fixed code, which is the same in every application.
- Violation: every route of the API in one file or class.
- Risk: the unit holding every route grows with each command until it cannot be read or reviewed, and two changes to different resources conflict in it.
- Fix: one unit per resource in a folder of its own; adding a command to a resource changes only that unit, the contract, and the composition root.

### Request and response types belong to the entry point
Declare the request and response bodies as types of the HTTP entry point — or, for a proto contract, use its generated messages — and map them to and from the command and its result.
- Violation: the command's result type encoded directly as the response.
- Risk: renaming a field of the command's result silently changes the published contract every client depends on.
- Fix: a request type and a response type per route, with the mapping beside the route.

### Answer errors by their kind
Answer a request that cannot be decoded with 400 `INVALID_ARGUMENT` without calling the command, and every error of a command by [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md)' table — its kind's status and code with the error's message, or 500 `INTERNAL` with no internal message for an error with no kind.
- Risk: a client cannot tell its own mistake from the service's, the HTTP and gRPC forms of a service answer one outcome differently, and a 500 that carries the internal error leaks connection strings and stack details.
- Fix: one table shared by every entry point of the application; the failure itself is logged by the handler's logging decorator, not written to the client.

### Log every request with what arrived
Write one record per request once it is answered, through the composition root's logger, with the method, the path, the status, and the duration, whether it failed, and as the payload the query, the headers, and the body received and the body sent back as written; put the request id — the client's (`X-Request-Id`), else a new one — into the context and send it back.
- Risk: a request that failed cannot be found or replayed from the log, its records cannot be told from those of the requests served at the same time, and a fault in turning the result into the response is invisible.
- Fix: one record per request; the data only as the payload attribute, so the base handler drops it and the logging variation point masks it.

### Stop gracefully
On termination (`SIGTERM`), stop accepting connections, let requests in flight finish within a bounded time, and return without an error.
- Risk: a rolling update cuts requests off mid-way and reports every stop as a failure.
- Fix: serve until the composition root's context is cancelled, then shut down with a timeout.

### Read its own settings
Read `HTTP_LISTEN_PORT` (default `8080`), `HTTP_BASE_PATH`, and `HTTP_API_DOCS_ENABLED` in the HTTP entry point's own package, rejecting an unusable value with an error naming its variable.
- Risk: settings added to the base settings outlive the entry point and make the variation point edit a base file.
- Fix: a settings loader in the entry point, called from the composition root's `entries` section.

### Attach by replacing the startup entry
Attach the entry point by adding its package and the contract folder, replacing the startup entry in the composition root's `entries` list with one entry whose start reads the entry point's settings, builds it, and serves until termination, deleting the startup entry, and changing no other base file than the composition root and the module's dependency, build, and readme files.
- Risk: any other base file changed by the HTTP variation point must be described together with every variation point that touches it.
- Fix: the stack extension lists the base files it changes; a check compares its example with the base example against that list.

### Test with the commands faked
Test the HTTP entry point with the command handlers replaced by fakes, covering a success, a rejected command, an undecodable request, a failure, the docs switch under a prefix, the settings, and the graceful stop, plus one scenario holding the served routes against the contract's operations.
- Risk: a test through the real command tests the domain again and breaks on every business change; a missing contract scenario lets Swagger UI describe a route that answers 404.
- Fix: one fake handler per command, setting what the scenario needs.

# Check list
- [ ] Every exposed command has one `/v{major}/...` route whose request and response types belong to the HTTP entry point.
- [ ] An undecodable request is 400 without calling the command; every error of a command is answered by error-kinds' table, an error with no kind as 500 without its message; every error body is `google.rpc.Status`.
- [ ] The contract, Swagger UI, docs switch, and base path follow api-develop's check list; a scenario holds the served routes against the contract.
- [ ] The server stops gracefully on termination.
- [ ] The entry point reads its own three settings; the base settings gained nothing.
- [ ] Routes are grouped by resource, one unit each.
- [ ] The entry point is one entry of the `entries` list; the startup entry is gone; no base file changed but the composition root and the module's dependency, build, and readme files.
