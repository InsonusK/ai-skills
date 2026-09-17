---
name: client-api-develop
description: Rules for connecting a service to an external API — a fixed ENV variable group for connection settings, minor.patch versioning tracked inside every grpc/http contract file, mandatory OpenAPI+Swagger UI for every HTTP endpoint, fixed contract file locations under docs/integration, and a recommendation to expose an HTTP alternative for every grpc contract
whenToUse: when implementing or reviewing a Client/Integration (see [[skills/common-workflow/architecture/core/solution-integration-client-layering.skill/solution-integration-client-layering.skill.md|solution-integration-client-layering]]) that connects a service to an external grpc or http API, or when publishing the service's own grpc/http API — choosing how its connection is configured, versioning its contract file, publishing its OpenAPI spec, placing its contract files, or deciding whether a grpc contract needs an HTTP counterpart
tags:
  - skill/develop
  - integration
  - api-versioning
  - grpc
  - http
  - openapi
  - swagger
  - env-config
  - stack
  - concern/architecture
  - concern/coding
updated: 20260916
---

# Goal
- Every external API connection configured through one fixed ENV variable group per API: `{API}_HOST`, `{API}_{PROTOCOL}_PORT` per protocol, `{API}_TLS` (or `{API}_{PROTOCOL}_TLS` when split was requested).
- Every grpc/http contract carrying a major version in its package/URL and a `minor.patch` version tracked inside the contract file itself.
- An `openapi.json` and a Swagger UI published for every HTTP endpoint the service exposes, kept in sync with the contract.
- Every contract file located under `docs/integration/client/` (external APIs this service consumes) or `docs/integration/api/` (this service's own published API).
- A recorded decision, for every grpc contract the service exposes, on whether an equivalent HTTP endpoint exists.

# Core Principle
- **One ENV shape for every client** - an operator who knows the group shape can configure or find any client's connection without reading its code, and a new client needs no new naming scheme.
- **Split only on real divergence** - a variable is split per protocol only once a protocol actually needs a different value; splitting pre-emptively multiplies variables nobody uses differently.
- **The file version is a diff signal** - a `minor.patch` bump inside the contract file itself lets anyone comparing two copies of that file (after copying it into a consumer's repo, or reviewing a PR) tell whether the contract changed, independent of the major version baked into the URL/package name.
- **HTTP widens who can call it** - an HTTP counterpart, backed by a published OpenAPI spec and Swagger UI, lets browsers, curl-based tooling, and simple clients reach a capability that would otherwise require a grpc stack.

# Rule

## MUST

### Configure every client through the `{API}` ENV group
Configure every Client's connection to an external API through ENV variables sharing one `{API}` prefix, never through hardcoded values or ad hoc variable names:

| Variable | Type | Scope | Example |
|---|---|---|---|
| `{API}_HOST` | string | one per API | `ORDERS_API_HOST=orders.internal` |
| `{API}_{PROTOCOL}_PORT` | integer | one per protocol used | `ORDERS_API_GRPC_PORT=50051` |
| `{API}_TLS` | bool | shared default across protocols | `ORDERS_API_TLS=true` |

- Violation: a host, port, or TLS literal in code, or a client whose connection variables don't follow this naming (`ORDERS_URL`, `ORDERS_ENDPOINT`).
- Risk: every client invents its own naming, so nobody can guess or grep for a given integration's settings, and there is no consistent way to override them per environment.
- Fix: read the host from `{API}_HOST`, each used protocol's port from its own `{API}_{PROTOCOL}_PORT`, and TLS from the shared `{API}_TLS` unless [split per protocol](#split-apitls-per-protocol-only-when-requested) applies.

### Name one PORT variable per protocol actually used
When a client speaks more than one protocol to the same API (e.g. grpc and http), give each protocol its own `{API}_{PROTOCOL}_PORT` under the same `{API}_HOST`, naming `{PROTOCOL}` explicitly (`GRPC`, `HTTP`) — ports always differ per protocol, so never share one.
- Violation: a single `{API}_PORT` reused for two different protocols, or the protocol segment omitted when only one protocol exists today.
- Risk: adding a second protocol later forces renaming the existing variable, breaking every deployment that already sets it.
- Fix: always include the protocol segment (`ORDERS_API_GRPC_PORT`, `ORDERS_API_HTTP_PORT`) even for a client's first protocol.

### Split `{API}_TLS` per protocol only when requested
Default to one shared `{API}_TLS` flag applied to every protocol the client uses; introduce `{API}_{PROTOCOL}_TLS` pairs (replacing the shared flag entirely for that client) only when the client must actually run with TLS enabled on one protocol and disabled on another.
- Violation: pre-splitting into `{API}_{PROTOCOL}_TLS` for a client where every protocol always shares the same TLS setting — e.g. requiring TLS on grpc but leaving it unstated for http on the same API.
- Risk: pre-splitting adds a variable per protocol with no behavioral need, and both copies must then be kept in sync by hand.
- Fix: use one `{API}_TLS` by default; split into per-protocol variables only for the specific client that needs protocols to diverge.

### Track major version in the contract's package/URL
Give every grpc/http API contract the service exposes or consumes a major version in its package/service name (`orders.v1.OrdersService`) or URL path segment (`/v1/orders`), and bump it — never mutate a shipped major version's contract — on any backward-incompatible change.
- Violation: a grpc service or proto package with no version segment, or an HTTP route with no `/v{n}/` segment.
- Risk: a breaking change to the contract has nowhere to go but in place, forcing every caller to upgrade in lockstep or breaking them silently.
- Fix: add the major version to the package/service name or URL path before the first consumer integrates.

### Track `minor.patch` inside the contract file
Give every `openapi.json` (its `info.version` field) and every grpc `.proto` file a `minor.patch` version distinct from the major version in the URL/package, and bump it on every change to that file's contract, however small.
- Violation: `info.version` (or the `.proto` file's version marker) left unchanged across multiple real contract edits, or a `.proto` file with no version marker at all.
- Risk: without a `minor.patch` that changes on every edit, comparing two copies of the contract file (after copying it into a consumer's repo, or reviewing a diff) can't tell whether the contract actually changed.
- Fix: bump the minor part for a backward-compatible addition and the patch part for a non-behavioral fix, on every commit that touches the contract file.

### Publish OpenAPI and Swagger UI for every HTTP endpoint
For every HTTP endpoint the service exposes — whether its primary contract or the [HTTP alternative to a grpc contract](#provide-an-http-alternative-to-a-grpc-contract) — generate an `openapi.json` spec and serve a Swagger UI for it.
- Violation: an HTTP endpoint reachable by callers with no `openapi.json` describing it, or a Swagger UI left pointing at a stale spec.
- Risk: consumers have no machine-readable or human-browsable description of the endpoint and must read the implementation to integrate.
- Fix: generate `openapi.json` from the route definitions (or hand-author it) and serve Swagger UI from it at a discoverable path.

### Update the OpenAPI spec on every HTTP contract change
Regenerate or hand-update `openapi.json` (and bump its `minor.patch` per [Track minor.patch inside the contract file](#track-minorpatch-inside-the-contract-file)) in the same change that edits an HTTP route, request, or response shape.
- Violation: merging an HTTP contract change without a corresponding `openapi.json` update.
- Risk: the published spec and Swagger UI drift from the real contract, so consumers integrate against a description that no longer matches the running service.
- Fix: treat `openapi.json` as generated/reviewed output of the same change that touches the route.

### Place contract files under `docs/integration`
Store every grpc `.proto` and `openapi.json` file under `docs/integration/client/` when it describes an external API this service consumes, or under `docs/integration/api/` when it describes this service's own published API.
- Violation: a contract file committed elsewhere in the repo (next to the client code, in a `contracts/` folder at the root, etc.).
- Risk: contract files scattered per-client or per-feature can't be found without knowing where a specific integration chose to put them.
- Fix: consuming contracts go in `docs/integration/client/{api-name}/`; this service's own published contracts go in `docs/integration/api/{contract-name}/`.

## SHOULD

### Provide an HTTP alternative to a grpc contract
Expose an HTTP counterpart (e.g. via a gateway/facade) for every grpc contract the service publishes, covering the same operations.
- Risk: without an HTTP alternative, consumers unable to use grpc (browsers, simple scripts, ad hoc debugging) cannot reach the capability at all.
- Fix: add a versioned HTTP route per grpc method, or generate one via a grpc-to-HTTP gateway, version it per [Track major version in the contract's package/URL](#track-major-version-in-the-contracts-packageurl), and publish it per [Publish OpenAPI and Swagger UI for every HTTP endpoint](#publish-openapi-and-swagger-ui-for-every-http-endpoint).

# Check list
- [ ] Every client's connection uses `{API}_HOST`, a `{API}_{PROTOCOL}_PORT` per protocol it uses, and `{API}_TLS` — no hardcoded host/port/TLS values, and no per-protocol TLS split unless that client genuinely needs one.
- [ ] Every PORT variable names its protocol explicitly, even when the client currently uses only one.
- [ ] Every grpc/http contract carries an explicit major version in its package/service name or URL path.
- [ ] Every `openapi.json`/`.proto` file carries a `minor.patch` version that was bumped in the same change that touched its contract.
- [ ] Every HTTP endpoint has a published `openapi.json` and a Swagger UI kept in sync with it.
- [ ] Every contract file lives under `docs/integration/client/` (consumed APIs) or `docs/integration/api/` (this service's own API).
- [ ] Every grpc contract the service publishes either has a documented HTTP alternative or a recorded reason it doesn't.
