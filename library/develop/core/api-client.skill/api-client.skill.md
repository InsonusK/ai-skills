---
name: api-client
description: How to split external API access into entity-focused clients hiding protocols behind API DTOs and a service hiding API realization behind a consumer-facing contract, with sibling client and service boundaries, technical outcomes, client middleware, configuration, timeouts, logging and emulator tests
whenToUse: when an application calls an HTTP or gRPC API of another service — adding a client, changing what it sends or how it is configured, mocking it to run without the other service, or reviewing a client of any stack
updated: 20261128
tags:
  - stack
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
adr:
  - adr/client-not-adapter.md
  - adr/api-dtos-returned-to-service.md
  - adr/entity-clients-and-system-services.md
  - adr/protocol-neutral-client-contract.md
---

# Goal
- **One way to configure every client** - An operator who knows `{API}_HOST`, `{API}_{PROTOCOL}_PORT`, `{API}_TLS`, and `{API}_TIMEOUT` configures, finds, or mocks any client of any service without reading its code.
- **A slow or absent API never hangs a call** - Every call ends within its timeout, and the caller is told the API is unavailable, so it answers `UNAVAILABLE` / 503 / exit `1` and a retry may help.
- **Calls traced across services** - Every outgoing call is logged with what was sent and received and carries the request id of the call that caused it, so one request is followed through every service it touched.
- **Runnable without the other service** - Setting `{API}_HOST=mock` runs the whole client against an emulator inside the process, so the service starts, and its flows run, with no other service reachable.

# Core Principle
- **Two change boundaries** - Client hides protocol mechanics; service hides how the external API fulfills the application operation, with the responsibility contract defined [below](#separate-protocol-mechanics-from-api-adaptation).
- **Sibling boundaries** - Place client and service according to [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md); [client-not-adapter](./adr/client-not-adapter.md) retains the naming decision.
- **The contract is the provider's** - The consumed contract is copied as published, and clients select the operations they call without trimming that contract; API DTOs stop at the service boundary.
- **Fixed once, written per API** - Configuration, the timeout, the call log, the request id, the error report, and the mock switch are fixed code the same for every API of every application; the application writes entity-focused client methods, API DTO parsing, emulator answers, and the service's consumer-model mapping.

# Scope
- Covers the calls an application makes to HTTP and gRPC APIs of other services. The transport-specific parts are the variation points `vp-http-client` and `vp-grpc-client` and their stack extensions.
- A retry of a failed call is not here: it comes from its own variation point, `vp-client-retry`, a client middleware, so no call is repeated unless the application attached it and the client marked the operation safe to repeat.
- A split into independent external systems changes project dependencies, failure and consistency boundaries; cross-system composition is a project design question, distinct from splitting one system's API (see [api-change-cases](./examples/api-change-cases.md)).

# Rule

## MUST

### Separate protocol mechanics from API adaptation
Assign implementation work to client or service according to this responsibility contract.

| Boundary | Owns | Change it absorbs |
|---|---|---|
| Client | Endpoint paths/verbs or RPC method names, serialization, protocol-error parsing into common technical outcomes, configured authentication and call policies such as retry | HTTP to gRPC for the same logical operations, DTO meaning and API error semantics, without changing service code or its client contract |
| Service | API DTO to Domain Model mapping through domain APIs, interpretation of remote outcomes, composition of API operations and of its system's entity-focused clients | Request/response changes, renamed or split operations, and API partitioning within the same external system, while preserving the consumer-facing contract when the required behavior remains available |

- Violation: a service catches gRPC exceptions or chooses HTTP paths; a client splits a full name into the application's name model or orchestrates several logical API operations to fulfill a service method.
- Risk: changing the protocol edits API adaptation, or changing the provider's API edits every consumer.
- Fix: keep one client method at the logical API-operation boundary; let service compose those operations and map their meaning, with invariants enforced by domain behavior.

### Keep the client contract independent of the protocol
Expose client-owned, protocol-neutral request/response API DTOs and technical outcomes at the service boundary, as decided in [protocol-neutral-client-contract](./adr/protocol-neutral-client-contract.md).
- Violation: HTTP response objects, numeric HTTP statuses, gRPC exceptions or transport-specific generated message types that change on a protocol swap form the service's client contract.
- Risk: replacing HTTP with gRPC changes service imports, signatures and error handling even when the logical API is unchanged.
- Fix: client translates wire representations and failures into its stable logical API contract; retain provider field meanings, not consumer-domain transformations, and keep raw transport diagnostics inside the technical failure cause; shared generated DTOs are suitable only when both transport implementations expose the identical logical contract.

### Preserve the consumer contract across API realization changes
Keep the service's consumer-facing methods and Domain Model answers stable when the same business capability is realized through changed API operations, DTO shapes or several APIs of the same system.
- Violation: a provider splits a lookup and each command now performs two client calls; a second API file creates a second application-facing service solely because of file packaging.
- Risk: consumers depend on the provider's implementation choices rather than the application operation.
- Fix: adapt the clients and the service implementation; distinguish the same-system cases from [a second independent system](./examples/api-change-cases.md#a-second-system), and report when changed semantics make the existing consumer contract impossible to honor.

### Group clients by an entity's API responsibility
Implement the operations needed to work with one external entity or cohesive resource in a client under `client/{name}/`, allowing partial contract coverage and several clients for a large Swagger/OpenAPI or proto file.
- Violation: one giant client solely because the provider publishes one file; unrelated controllers grouped together; only writes grouped while related reads are scattered arbitrarily.
- Risk: provider packaging determines application boundaries, and unrelated changes reach every client consumer.
- Fix: an OrderClient groups order operations, including needed reads; reference the provider contract and list supported operations, following [entity-clients-and-system-services](./adr/entity-clients-and-system-services.md) and preserving the complete wire file under [Preserve the consumed wire contract](#preserve-the-consumed-wire-contract).

### Give each client one owning service
Assign each client to one service of its external system, permitting shared use only as an explicitly justified exception between decomposed services of that same system.
- Violation: unrelated services call a shared client as a shortcut; a client coordinates different external systems.
- Risk: adaptation is duplicated and ownership of API changes becomes ambiguous.
- Fix: keep one owner normally; first split an overly broad client by entity responsibility, and document why any remaining same-system sharing is preferable.

### Return API DTOs from the client
Return the API-shaped response DTOs and technical outcomes of [the protocol-neutral client contract](#keep-the-client-contract-independent-of-the-protocol) from client methods, leaving conversion into consumer entities or models to the service, as decided in [api-dtos-returned-to-service](./adr/api-dtos-returned-to-service.md).
- Violation: splitting a full-name field into the consumer's name model in the client; returning a consumer entity; passing a domain entity into client code to populate.
- Risk: application semantics enter transport code, and replacing the provider rewrites domain mapping in several places.
- Fix: parse and validate the wire representation in client; let service map the DTO through public domain construction and behavior APIs; treat malformed provider data as a technical failure, never automatically as invalid caller input.

### Compose one external system's clients in the service
Create an integration service under the service boundary of app-layers and build service answers from its parameters and the operations of one or more clients belonging to one external system, decomposing a large system into cohesive services where useful.
- Violation: a service joins unrelated external systems; every client is forced to have a separate service even when those clients form one integration.
- Risk: an integration dependency is hidden or provider orchestration spreads into command code.
- Fix: one service normally adapts one system, with several clients when needed; commands or domain services compose different systems, following [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md); invoke domain behavior for invariants rather than reimplementing it in a mapper.

### Keep fixed client code beside entity clients
Place shared transport, configuration, middleware and outcome code under `client/core/`, beside entity-specific client packages rather than under `infrastructure/`.
- Risk: moving an entity client duplicates transport mechanics or leaves its role ambiguous.
- Fix: keep `domain/`, `infrastructure/`, `client/` and `service/` as sibling logical boundaries, using stack conventions for their physical packages or projects.

### Configure every client by its {API} group
Read every client's connection from the environment variables of its own `{API}` prefix, in the client's package:

| Variable | Default | Meaning |
|---|---|---|
| `{API}_HOST` | required | the host of the API; `mock` runs the client against its emulator |
| `{API}_{PROTOCOL}_PORT` | `443` with TLS, `80` without | one per protocol used: `TODO_API_HTTP_PORT`, `TODO_API_GRPC_PORT` |
| `{API}_TLS` | `false` | connect over TLS |
| `{API}_TIMEOUT` | `5s` | the longest one call may take: a number and `ms`, `s`, or `m` |

- Violation: a host, port, or timeout written in code; a variable outside the group (`ORDERS_URL`); a port variable without its protocol.
- Risk: every client invents its own names, so nobody can find or override a client's settings, and adding a second protocol renames the first one's variable.
- Fix: one `{API}` prefix per API, named after the API (`TODO_API`); an unusable value stops the start with an error naming its variable.

### Preserve the consumed wire contract
Copy the provider's contract file (`openapi.json`, `.proto`) to `docs/integration/client/{api-name}/` as published except for local generated-code namespace metadata explicitly prescribed by a stack extension, and replace it whole when the provider publishes a new version.
- Violation: operations, packages, service names or fields changed or trimmed to fit a client, or a contract kept beside the code.
- Risk: the recorded contract no longer describes the provider's wire protocol, so a client can compile against an API that does not exist.
- Fix: confine any allowed substitution to local language-binding namespace metadata, preserve all wire definitions, and check the client's operations against that contract.

### Bound every call by its timeout
End every call after `{API}_TIMEOUT` at the latest, or earlier when the call that caused it must end earlier, and report a call that ran out of time as unavailable.
- Risk: a hanging API holds every request of the service that calls it until they all time out at their own clients, and the service stops answering.
- Fix: the fixed client code sets the deadline of each call; nothing per operation.

### Tell an unavailable API from a rejected call
Normalize connection failures, timeouts, HTTP 502/503/504 and gRPC UNAVAILABLE/DEADLINE_EXCEEDED to unavailable, and other remote failures to a protocol-neutral API status with its semantic code and message, leaving service to assign application error meanings through [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md).
- Violation: the client returning the application's kinds; the service passing the API's `INVALID_ARGUMENT` on as the caller's invalid input without deciding it; every remote error answered `INTERNAL`.
- Risk: a client of this service is told to fix input it never sent, or to give up on an outage a retry would ride out.
- Fix: client translates raw statuses/exceptions once; equivalent HTTP and gRPC errors yield the same logical status when their API contracts define the same meaning, while service maps unavailable to its unavailable error kind, maps only expected remote statuses into its own errors, and treats unexpected failures as service failures.

### Log every call in two records
Write two records per call through the composition root's logger — one when it is sent, with what was sent as the payload, one when it is answered, with the API, the operation, the status, the duration, whether it failed, and as the payload what was received and, for a failed call, what was sent again.
- Risk: a failed call to another service cannot be found or replayed from the log, and its records cannot be told from those of the calls made at the same time.
- Fix: the fixed client code writes them; the data only as the payload attribute, so the base handler drops it and the logging variation point masks it.

### Send the request id on
Send the request id of the call being served with every call it causes — `X-Request-Id` over HTTP, `x-request-id` metadata over gRPC — and send none when there is none.
- Risk: the records of one request end at the edge of each service, and an incident spanning services is read service by service by timestamp.
- Fix: the fixed client code reads the id from the call's context.

### Mock by setting the host to mock
Run the client, when `{API}_HOST=mock`, against an emulator of the API inside the process — the same client code, the transport answering from the emulator — that answers every operation the client calls with stable sample data, and write one WARN record `api client mocked` with the API's name when the client is built.
- Violation: a `USE_MOCK_X` flag; a mock class beside the client that skips its mapping and its log; a mock that answers empty or random data.
- Risk: two switches disagree; a mocked integration answers unnoticed in an environment where a real one was expected; the service cannot start where the other service is not running.
- Fix: one emulator per API in its client's `mock/` folder, in a file named after the client (`client/todo_api/mock/todo_api_mock.go` beside `todo_api.go`), so the file says it is a mock and of which client; the fixed client code chooses it from the host.

### Run every call through the client middleware
Send every call of every client through the client middleware the composition root lists for its API, the first the outermost, around the attempts of the call - each attempt with its own timeout and its two log records - and refuse a call of an operation the client does not list.
- Violation: a step added to every call (a retry, a circuit breaker, a metric) written into the fixed client code or into each client method.
- Risk: the step is repeated per client and per stack, and adding it edits code every application copies unchanged.
- Fix: the fixed client code runs the list, empty when no middleware is attached; configured retry, authentication/credentials and other technical call policies belong to this boundary, not service, while application access decisions remain outside client.

### Mark the operations safe to repeat
Mark, in the client's list of its operations, each operation that does what one call does when it is sent again (it reads, or it sets what it sets regardless of how often), and leave every other unmarked.
- Violation: every operation marked; an operation that creates a record or moves money marked.
- Risk: a repeat of a call the API received but did not answer in time does its work twice.
- Fix: the mark is beside the operation's method and path, with the reason it is safe in a comment; the client middleware reads it from the call.

### Keep the API out of readiness
Add no readiness check for an API the application calls; let the calls that need it answer unavailable while it is down.
- Risk: a readiness check on another service takes every replica of this one out of the load balancer during that service's outage, also for the requests that never call it, and an outage cascades through the system.
- Fix: the commands that need the API fail fast with unavailable; the readiness check is for this service's own stores.

### Test the client against an emulator
Test the fixed client code once through an emulator of a test API of its own, each client through an emulator of its API serving the answers each scenario sets, the contract scenario against the copied contract, and the service with its clients faked.
- Risk: a test against the real other service fails when it is down or its data changes; a service test through the real transport tests the client again.
- Fix: exercise each protocol implementation against the same logical client contract and its emulator; test service DTO mapping and one/many-client composition with those client contracts faked, without protocol objects in service tests.

# Example
- [api-change-cases](./examples/api-change-cases.md) - how a change of the API is absorbed by the client, by the service, or, when the data's master system changes, by the project.

# Check list
- [ ] Check [responsibilities](#separate-protocol-mechanics-from-api-adaptation), [transport independence](#keep-the-client-contract-independent-of-the-protocol) and [consumer stability](#preserve-the-consumer-contract-across-api-realization-changes).
- [ ] Check [client grouping](#group-clients-by-an-entitys-api-responsibility), [ownership](#give-each-client-one-owning-service), and [fixed code placement](#keep-fixed-client-code-beside-entity-clients).
- [ ] Check [API DTO returns](#return-api-dtos-from-the-client).
- [ ] Check [single-system service composition](#compose-one-external-systems-clients-in-the-service).
- [ ] Every connection setting is a variable of the API's `{API}` group; `{API}_TIMEOUT` bounds every call.
- [ ] The consumed contract is the provider's file under `docs/integration/client/{api-name}/`; a scenario holds the client's operations against it.
- [ ] Unavailable is reported apart from the API's other statuses; the service maps them to its own errors.
- [ ] Every call is logged in two records and carries the request id.
- [ ] `{API}_HOST=mock` runs the client against its emulator in `client/{api}/mock/{api}_mock`, warned once.
- [ ] Every call runs through the client middleware of its API; only operations safe to repeat are marked so.
- [ ] No readiness check names another service's API.
