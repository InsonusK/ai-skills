---
name: vp-http-api
description: The HTTP API variation point — an entry point attached to the base plateau that serves commands as versioned HTTP routes, answers errors by their kind, publishes its OpenAPI contract and Swagger UI, serves the health endpoints, stops gracefully, and replaces the startup entry without editing any other base file
whenToUse: when an application built on the base plateau must be reachable over HTTP — adding the HTTP entry point, serving a new command over HTTP, or reviewing the HTTP entry point of such an application
updated: 20261017
tags:
  - stack
  - concern/architecture
  - app-type/service
---

# Goal
- **Commands reachable over HTTP** - Every command the service exposes has one versioned route; the route turns the request into the command's plain parameters and the result into its response.
- **Rejected told from failed** - A client tells a request rejected by a business rule or by its shape (400) from a failure of the service (500) by status and by one error body.
- **Contract published** - The service's OpenAPI contract, Swagger UI, and every URL they emit follow [api-develop](library/common-workflow/develop/api-develop.skill/api-develop.skill.md), and the served routes match the contract's operations.
- **Probe-ready** - An orchestrator probes liveness and readiness separately, and a dependency added later by a service variation point counts toward readiness.
- **Attached, not edited** - The entry point joins the [base plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md) through its extension points: it replaces the startup entry and changes no other base file but the composition root and the module's dependency files.

# Core Principle
- **A thin translation** - The HTTP entry point decodes, calls, and encodes; it makes no business decision, as [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md) requires of every api layer.
- **The HTTP contract is the entry point's own** - Request and response types belong to the HTTP entry point and are mapped to and from the command's types, so a change of a command's result does not change the published contract unnoticed.
- **Written once for every command** - Decoding errors, mapping errors to statuses, and writing JSON are one generic piece of code; serving a command adds a route, its two types, and its two mappings.

# Scope
- Covers an HTTP API whose contract is written by hand in OpenAPI; an HTTP form generated from a proto contract belongs to the gRPC API variation point.
- The implementation for each stack is described in its stack-specialized extension: `vp-http-api-in-go`; `vp-http-api-in-dotnet` and `vp-http-api-in-python` are planned.

# Rule

## MUST

### Follow api-develop
Version, document, and publish the HTTP API per [api-develop](library/common-workflow/develop/api-develop.skill/api-develop.skill.md): the major version in every route (`/v1/...`), the contract file under `docs/integration/api/{contract-name}/`, Swagger UI and the spec behind `HTTP_API_DOCS_ENABLED`, every emitted URL under `HTTP_BASE_PATH`, success status 201/202/200 by what the command did, and `google.rpc.Status` as every error body.
- Risk: an HTTP API that departs from the shared rules answers differently from every other service of the system and breaks behind an ingress prefix.
- Fix: check the entry point against api-develop's check list.

### One route per command
Serve each exposed command at exactly one route, whose handler decodes the request into the command's plain parameters, calls the command's handler, and encodes its result.
- Violation: a route that calls two commands, reads state to choose one, or calls a service.
- Risk: business logic moves into the HTTP layer and every other entry point must repeat it.
- Fix: one command per route; a choice that depends on state goes to a command factory per app-layers.

### Request and response types belong to the entry point
Declare the request and response bodies as types of the HTTP entry point and map them to and from the command and its result.
- Violation: the command's result type encoded directly as the response.
- Risk: renaming a field of the command's result silently changes the published contract every client depends on.
- Fix: a request type and a response type per route, with the mapping beside the route.

### Answer errors by their kind
Answer a request that cannot be decoded with 400 `INVALID_ARGUMENT` without calling the command, a command rejected by a business rule (the base plateau's invalid error kind) with 400 `INVALID_ARGUMENT` carrying the rule's message, and any other error with 500 `INTERNAL` carrying no internal message.
- Risk: a client cannot tell its own mistake from the service's, and a 500 that carries the internal error leaks connection strings and stack details.
- Fix: one error mapping shared by every route; the failure itself is logged by the handler's logging decorator, not written to the client.

### Serve the health endpoints
Serve `/health/live`, `/health/ready`, and `/health` as [devops-service-deploy](library/devops/core/devops-service-deploy.skill/devops-service-deploy.skill.md) defines them, outside the contract's routes, with `/health/ready` answering 503 and naming every readiness check that fails.
- Risk: without separate probes an orchestrator restarts a healthy process during a database outage or sends traffic to a replica that is not ready.
- Fix: liveness checks nothing; readiness runs the checks registered at the readiness extension point.

### Open the readiness extension point
Accept readiness checks — a name and a check function — from the composition root, so that a service variation point adds its dependency's check without editing the HTTP entry point.
- Risk: a readiness endpoint that knows its dependencies must be edited by every service variation point.
- Fix: the entry point takes a list of checks; the composition root passes one per dependency.

### Stop gracefully
On termination (`SIGTERM`), stop accepting connections, let requests in flight finish within a bounded time, and return without an error.
- Risk: a rolling update cuts requests off mid-way and reports every stop as a failure.
- Fix: serve until the composition root's context is cancelled, then shut down with a timeout.

### Read its own settings
Read `HTTP_LISTEN_PORT` (default `8080`), `HTTP_BASE_PATH`, and `HTTP_API_DOCS_ENABLED` in the HTTP entry point's own package, rejecting an unusable value with an error naming its variable.
- Risk: settings added to the base settings outlive the entry point and make the variation point edit a base file.
- Fix: a settings loader in the entry point, called from the composition root's `entries` section.

### Attach by replacing the startup entry
Attach the entry point by adding its package and the contract folder, replacing the startup lines in the composition root's `entries` section with starting the server, deleting the startup entry, and changing no other base file than the composition root and the module's dependency, build, and readme files.
- Risk: any other base file changed by the HTTP variation point must be described together with every variation point that touches it.
- Fix: the stack extension lists the base files it changes; a check compares its example with the base example against that list.

### Test with the commands faked
Test the HTTP entry point with the command handlers replaced by fakes, covering a success, a rejected command, an undecodable request, a failure, the health endpoints, the docs switch under a prefix, the settings, and the graceful stop, plus one scenario holding the served routes against the contract's operations.
- Risk: a test through the real command tests the domain again and breaks on every business change; a missing contract scenario lets Swagger UI describe a route that answers 404.
- Fix: one fake handler per command, setting what the scenario needs.

# Check list
- [ ] Every exposed command has one `/v{major}/...` route whose request and response types belong to the HTTP entry point.
- [ ] An undecodable request is 400 without calling the command; a rejected command is 400 with the rule's message; any other error is 500 without the internal message; every error body is `google.rpc.Status`.
- [ ] The contract, Swagger UI, docs switch, and base path follow api-develop's check list; a scenario holds the served routes against the contract.
- [ ] `/health/live`, `/health/ready`, and `/health` exist outside the contract; readiness checks are passed in by the composition root.
- [ ] The server stops gracefully on termination.
- [ ] The entry point reads its own three settings; the base settings gained nothing.
- [ ] The startup entry is gone; no base file changed but the composition root and the module's dependency, build, and readme files.
