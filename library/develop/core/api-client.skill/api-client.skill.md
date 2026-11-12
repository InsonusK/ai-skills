---
name: api-client
description: How an application calls another service's API — one client per API under one service, the client returning the application's values or a result type of its own and the service composing its client's operations, a change of the data's master system left to the project, every call through the client middleware of the composition root with the operations safe to repeat marked, the {API} environment group (host, a port per protocol, TLS, timeout), the consumed contract copied unchanged, every call bounded by a timeout, logged in two records, and carrying the request id, an unavailable API told from a rejected call, a mock that runs the client against an emulator inside the process, and the tests at the client's boundary
whenToUse: when an application calls an HTTP or gRPC API of another service — adding a client, changing what it sends or how it is configured, mocking it to run without the other service, or reviewing a client of any stack
updated: 20261030
tags:
  - stack
  - concern/architecture
  - concern/coding
  - app-type/service
  - app-type/cli
adr:
  - adr/client-returns-application-values.md
  - adr/client-not-adapter.md
---

# Goal
- **One way to configure every client** - An operator who knows `{API}_HOST`, `{API}_{PROTOCOL}_PORT`, `{API}_TLS`, and `{API}_TIMEOUT` configures, finds, or mocks any client of any service without reading its code.
- **A slow or absent API never hangs a call** - Every call ends within its timeout, and the caller is told the API is unavailable, so it answers `UNAVAILABLE` / 503 / exit `1` and a retry may help.
- **Calls traced across services** - Every outgoing call is logged with what was sent and received and carries the request id of the call that caused it, so one request is followed through every service it touched.
- **Runnable without the other service** - Setting `{API}_HOST=mock` runs the whole client against an emulator inside the process, so the service starts, and its flows run, with no other service reachable.

# Core Principle
- **A client is infrastructure** - The client, the code that calls one API, is the infrastructure of [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md): it knows the transport and the contract and reports technical outcomes; the one service over it speaks the application's language and decides what each outcome means. Decision recorded in [client-not-adapter](./adr/client-not-adapter.md).
- **The contract is the provider's** - The consumed contract is copied as published, and the client maps it for the operations it calls; the rest of the application never sees the contract's types.
- **Fixed once, written per API** - Configuration, the timeout, the call log, the request id, the error report, and the mock switch are fixed code the same for every API of every application; per API the application writes its client's methods, their mappings, its emulator's sample answers, and its service.

# Scope
- Covers the calls an application makes to HTTP and gRPC APIs of other services. The transport-specific parts are the variation points `vp-http-client` and `vp-grpc-client` and their stack extensions.
- A retry of a failed call is not here: it comes from its own variation point, `vp-client-retry`, a client middleware, so no call is repeated unless the application attached it and the client marked the operation safe to repeat.
- A change of the API that moves data the application uses to another system - the master system of that data changes - is not covered: the application gains a dependency, and the service is redesigned at the level of the project, not hidden in its client or service (the cases are in [api-change-cases](./examples/api-change-cases.md)).

# Rule

## MUST

### One client per API, one service over it
Write each API the application calls as one client in a package of its own under `infrastructure/client/` (`infrastructure/client/{api}`), with the fixed client code of every API beside them in `infrastructure/client/core/`, with one method per operation it calls, and one service over it whose methods say what the application wants.
- Violation: a command or a domain service calling the client; a service named after the API's operations (`PostItemsPlan`); two APIs behind one client; a client beside the stores in `infrastructure/`.
- Risk: the contract's shapes and errors spread into the application, and a change of the provider's API edits commands.
- Fix: the service method (`RemotePlanner.Plan`) hides the call; the command calls the service through an interface, per app-layers; `infrastructure/client/` shows at a glance every other service the application calls.

### Return the application's values from the client
Return from each client method the application's values when they follow from the answer alone, and otherwise a result type of the client's own holding only the fields the service uses, named in the application's language. Decision recorded in [client-returns-application-values](./adr/client-returns-application-values.md).
- Violation: the response type shaped by the provider's JSON or messages returned to the service; an entity of the application passed into the client to build its answer from; a client method calling another API.
- Risk: the provider's shapes spread into the application, and a business rule placed in the client is lost when the transport is replaced.
- Fix: the client splits, renames, and converts what one answer holds (`Ann Lee` into name and surname); what needs a decision of the application or another call is left to the service in the client's result type.

### Compose the client's operations in the service
Build each answer of the service from as many operations of its own client as it needs and from the parameters of its method, and from nothing else.
- Violation: the provider splits one operation in two and every caller of the service now makes the second call itself; the service calling another service for the missing data.
- Risk: a change of how the API answers edits every caller instead of one service, or the service hides a second system the application now depends on.
- Fix: `UserService.UserName` calls both operations of its client and still returns name and surname; data of another infrastructure is joined by a domain service or a command, per [app-layers](library/develop/core/app-layers.skill/app-layers.skill.md).

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

### Copy the consumed contract unchanged
Copy the provider's contract file (`openapi.json`, `.proto`) as published to `docs/integration/client/{api-name}/`, replace it whole when the provider publishes a new version, and never edit it.
- Violation: a consumed contract trimmed to the operations the client uses, edited, or kept beside the code.
- Risk: the version in the file no longer names what the provider published, and a diff against the provider's copy shows changes nobody made.
- Fix: adapt to the contract in the client's mappings; a scenario holds the operations the client calls against the copied file.

### Bound every call by its timeout
End every call after `{API}_TIMEOUT` at the latest, or earlier when the call that caused it must end earlier, and report a call that ran out of time as unavailable.
- Risk: a hanging API holds every request of the service that calls it until they all time out at their own clients, and the service stops answering.
- Fix: the fixed client code sets the deadline of each call; nothing per operation.

### Tell an unavailable API from a rejected call
Report from the client, as technical outcomes, an API that cannot be reached or did not answer in time — no connection, a timeout, an HTTP 502, 503, or 504, a gRPC `UNAVAILABLE` or `DEADLINE_EXCEEDED` — as unavailable, and any other failure with the API's status (its code and message); let the service turn unavailable into the kind unavailable of [error-kinds](library/develop/core/error-kinds.skill/error-kinds.skill.md), a status it expects into an error of its own with the kind that fits the application, and anything else into an error with no kind.
- Violation: the client returning the application's kinds; the service passing the API's `INVALID_ARGUMENT` on as the caller's invalid input without deciding it; every remote error answered `INTERNAL`.
- Risk: a client of this service is told to fix input it never sent, or to give up on an outage a retry would ride out.
- Fix: the service maps only the statuses its method expects (the API rejected the titles this application sent it: invalid with the API's message); the rest is a failure of this service.

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
- Fix: one emulator per API in its client's `mock/` folder, in a file named after the client (`infrastructure/client/todo_api/mock/todo_api_mock.go` beside `todo_api.go`), so the file says it is a mock and of which client; the fixed client code chooses it from the host.

### Run every call through the client middleware
Send every call of every client through the client middleware the composition root lists for its API, the first the outermost, around the attempts of the call - each attempt with its own timeout and its two log records - and refuse a call of an operation the client does not list.
- Violation: a step added to every call (a retry, a circuit breaker, a metric) written into the fixed client code or into each client method.
- Risk: the step is repeated per client and per stack, and adding it edits code every application copies unchanged.
- Fix: the fixed client code runs the list, empty in an application that attaches no client middleware; a variation point adds its middleware to the list in the composition root.

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
Test the fixed client code once through an emulator of a test API of its own, each client through an emulator of its API serving the answers each scenario sets, the contract scenario against the copied contract, and the service with its client faked.
- Risk: a test against the real other service fails when it is down or its data changes; a service test through the real transport tests the client again.
- Fix: an in-process HTTP server or gRPC server per client feature; a fake class per client in the service's tests.

# Example
- [api-change-cases](./examples/api-change-cases.md) - how a change of the API is absorbed by the client, by the service, or, when the data's master system changes, by the project.

# Check list
- [ ] Each API has its client in `infrastructure/client/{api}` and one service over it; no command or domain service calls the client.
- [ ] No response type leaves the client; it returns the application's values or a result type of its own.
- [ ] The service builds its answers from its client's operations and its parameters alone; data of another infrastructure is joined above it.
- [ ] Every connection setting is a variable of the API's `{API}` group; `{API}_TIMEOUT` bounds every call.
- [ ] The consumed contract is the provider's file under `docs/integration/client/{api-name}/`; a scenario holds the client's operations against it.
- [ ] Unavailable is reported apart from the API's other statuses; the service maps them to its own errors.
- [ ] Every call is logged in two records and carries the request id.
- [ ] `{API}_HOST=mock` runs the client against its emulator in `infrastructure/client/{api}/mock/{api}_mock`, warned once.
- [ ] Every call runs through the client middleware of its API; only operations safe to repeat are marked so.
- [ ] No readiness check names another service's API.
