---
name: vp-http-client
description: The HTTP client variation point — a client of another service's HTTP API attached to the base plateau in its client layer, with the service over it, built on fixed client code (the {API} settings, the timeout, JSON in and out, google.rpc.Status read from an error, the call log, the request id, the mock against an emulator) and checked against the copied OpenAPI contract
whenToUse: when an application built on the base plateau must call an HTTP API of another service — adding the client, calling a further operation of it, or reviewing such a client
updated: 20261128
tags:
  - stack
  - concern/architecture
  - app-type/service
  - app-type/cli
---

# Goal
- **Another service's HTTP API called the catalog's way** - An application calls an HTTP API with every property of [api-client](library/develop/core/api-client.skill/api-client.skill.md); this skill adds what is particular to HTTP.
- **Attached, not edited** - The client joins the [base plateau](library/architecture/spec/base-plateau.skill/base-plateau.skill.md) through its extension points: its client package and its service in `[services]` of the composition root, the commands that use it in the command registry.

# Core Principle
- **Written by hand, held to the contract** - The client has request and response types of its own for the operations it calls, mapped to protocol-neutral logical API DTOs; one scenario holds each operation's method and path against the copied `openapi.json`, so no generator is needed and none drifts.
- **One call, written once** - Building the URL, the timeout, JSON in and out, reading `google.rpc.Status` from an error, the two log records, the request id, and the mock are one piece of fixed code; an operation of the client is one call of it with its path and its two types.

@import "./diagrams/concept.mmd" {as="mermaid"}

# Scope
- Covers calls to an HTTP API whose contract is `openapi.json` with JSON bodies and `google.rpc.Status` errors (skill api-develop). The HTTP form of a proto contract is called through `vp-grpc-client` instead, over gRPC.
- The implementation for each stack is described in its stack-specialized extension: `vp-http-client-in-go`, `vp-http-client-in-dotnet`, `vp-http-client-in-python`.

# Rule

## MUST

### Build the URL from the {API} group
Call `http://` or, with `{API}_TLS=true`, `https://` `{API}_HOST:{API}_HTTP_PORT` followed by the operation's path from the contract, as [api-client](library/develop/core/api-client.skill/api-client.skill.md) configures every client.
- Risk: a base URL in one variable (`TODO_API_URL`) breaks the one group every client shares and hides the protocol of the port.
- Fix: the fixed client code builds it; the client names only the path.

### One method per operation, with its own types
Write each operation the application calls as one method of the client with a request type and a response type of the client's own — only the fields it uses, the others ignored when they arrive — and map wire types to and from the logical API DTOs returned to service.
- Violation: a wire response type or a Domain Model returned to the service; a map of strings passed as the body; every operation of the contract written though the application calls two.
- Risk: the service depends on the provider's shapes; unused operations are code nobody tests.
- Fix: the method returns client-owned logical API DTOs; service maps them into the Domain Model, per [API DTO return contract](library/develop/core/api-client.skill/api-client.skill.md#return-api-dtos-from-the-client).

### Read the error from google.rpc.Status
Report an answer of 400 or above that is not 502, 503, or 504 through the common technical outcome: the code and message of its `google.rpc.Status` body, or `UNKNOWN` when the body is not one; retain HTTP status as a diagnostic cause.
- Risk: the service cannot tell the API's `NOT_FOUND` from its `ALREADY_EXISTS` when both are a status number, and a provider's HTML error page breaks the client.
- Fix: the fixed client code parses the body; the service maps the codes it expects.

### Hold the operations against the contract
Keep one scenario that compares the method and path of every operation the client calls with the operations of `docs/integration/client/{api-name}/openapi.json`.
- Risk: a path mistyped in the client, or an operation the provider removed, is found in production as a 404.
- Fix: the client lists its operations in one place the scenario reads.

### Attach without editing the base
Attach the client by adding its fixed code, its client, its emulator, its service, and the commands that use it, and by changing no base file but the composition root, the command registry when a command is added, and the module's dependency, build, and readme files.
- Risk: any other base file changed by the client must be described together with every variation point that touches it.
- Fix: the stack extension lists the base files it changes; a check compares its example with the base example against that list.

### Pass the shared scenarios
Run every file of this skill's [conformance/](./conformance/) unchanged in each stack's example, through a copy named in the stack skill's `conformance.copy` (`make conformance-sync`), as [conformance-scenarios-copied-into-each-stack](library/architecture/spec/vp-logging.skill/adr/conformance-scenarios-copied-into-each-stack.md) decides. `client-pipeline/`, `remote-planner/`, and `plan-remote-items/` are run by vp-grpc-client's examples too.
- Violation: a stack rewording a shared scenario or keeping its own copy of one; a scenario skipped because the stack fails it.
- Risk: the stacks drift apart under one name: a check one stack makes and another does not is a defect found in one and left open in the others.
- Fix: step definitions are the stack's own; a scenario of a mechanism only one stack has goes in a `{name}-in-{stack}.feature` beside the copies; `bash test/check-conformance.sh` fails while a copy is missing or differs.

# Check list
- [ ] Every file of `conformance/` runs unchanged in each stack's example; `bash test/check-conformance.sh` passes.
- [ ] The client keeps every rule of api-client; its URL comes from the `{API}` group.
- [ ] Each operation is one method of the client with its own request and response types.
- [ ] An error answer exposes its normalized code/message, with HTTP diagnostics retained in the cause.
- [ ] A scenario holds the client's operations against the copied `openapi.json`.
- [ ] No base file changed but the composition root, the command registry, and the module's dependency, build, and readme files.
