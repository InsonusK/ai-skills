---
name: api-client-develop
description: Rules for connecting a service to an external grpc/http API — a fixed ENV variable group for connection settings (`{API}_HOST`, `{API}_{PROTOCOL}_PORT`, `{API}_TLS`) and the consumed contract file copied unchanged under docs/integration/client
whenToUse: when implementing or reviewing a Client/Integration (see [[skills/common-workflow/architecture/core/solution-integration-client-layering.skill/solution-integration-client-layering.skill.md|solution-integration-client-layering]]) that connects a service to an external grpc or http API — choosing how its connection is configured, or adding/updating the consumed API's contract file in the repo
tags:
  - skill/develop
  - integration
  - grpc
  - http
  - env-config
  - stack
  - concern/architecture
  - concern/coding
updated: 20261006
---

# Goal
- Every external API connection configured through one fixed ENV variable group per API: `{API}_HOST`, `{API}_{PROTOCOL}_PORT` per protocol, `{API}_TLS` (or `{API}_{PROTOCOL}_TLS` when split was requested).
- Every consumed contract file (`.proto`, `openapi.json`) located under `docs/integration/client/{api-name}/`, byte-identical to the provider's published copy.

# Core Principle
- **One ENV shape for every client** - an operator who knows the group shape can configure or find any client's connection without reading its code, and a new client needs no new naming scheme.
- **Split only on real divergence** - a variable is split per protocol only once a protocol actually needs a different value; splitting pre-emptively multiplies variables nobody uses differently.
- **The copied contract is the provider's** - the consumer's copy stays identical to what the provider published, so its in-file version (per [api-develop](./api-develop.skill/api-develop.skill.md)) tells exactly which contract the client was built against.

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
- Fix: read the host from `{API}_HOST`, each used protocol's port from its own `{API}_{PROTOCOL}_PORT`, and TLS from the shared `{API}_TLS` unless [split per protocol](#split-api_tls-per-protocol-only-when-requested) applies.

### Name one PORT variable per protocol actually used
When a client speaks more than one protocol to the same API (e.g. grpc and http), give each protocol its own `{API}_{PROTOCOL}_PORT` under the same `{API}_HOST`, naming `{PROTOCOL}` explicitly (`GRPC`, `HTTP`) — ports always differ per protocol, so never share one.
- Violation: a single `{API}_PORT` reused for two different protocols, or the protocol segment omitted when only one protocol exists today.
- Risk: adding a second protocol later forces renaming the existing variable, breaking every deployment that already sets it.
- Fix: always include the protocol segment (`ORDERS_API_GRPC_PORT`, `ORDERS_API_HTTP_PORT`) even for a client's first protocol.

### Split `{API}_TLS` per protocol only when requested
Default to one shared `{API}_TLS` flag applied to every protocol the client uses; introduce `{API}_{PROTOCOL}_TLS` pairs (replacing the shared flag entirely for that client) only when the client must actually run with TLS enabled on one protocol and disabled on another.
- Violation: pre-splitting into `{API}_{PROTOCOL}_TLS` for a client where every protocol always shares the same TLS setting.
- Risk: pre-splitting adds a variable per protocol with no behavioral need, and both copies must then be kept in sync by hand.
- Fix: use one `{API}_TLS` by default; split into per-protocol variables only for the specific client that needs protocols to diverge.

### Place consumed contracts under `docs/integration/client`
Store every grpc `.proto` and `openapi.json` file describing an external API this service consumes under `docs/integration/client/{api-name}/`.
- Violation: a consumed contract committed elsewhere in the repo (next to the client code, in a `contracts/` folder at the root, etc.).
- Risk: contract files scattered per-client or per-feature can't be found without knowing where a specific integration chose to put them.
- Fix: move it to `docs/integration/client/{api-name}/`.

### Copy the consumed contract unchanged
Copy the provider's published contract file as-is, and replace it wholesale with the provider's newer copy when updating — never hand-edit it.
- Violation: a consumed `.proto` with a field renamed or removed locally, or an `openapi.json` trimmed to the endpoints the client uses while keeping the provider's version.
- Risk: the in-file version no longer identifies the contract, so a diff against the provider's copy reports a change that the provider never made, or hides one it did.
- Fix: restore the provider's copy; adapt to the contract in the Client/Integration code instead.

# Check list
- [ ] Every client's connection uses `{API}_HOST`, a `{API}_{PROTOCOL}_PORT` per protocol it uses, and `{API}_TLS` — no hardcoded host/port/TLS values, and no per-protocol TLS split unless that client genuinely needs one.
- [ ] Every PORT variable names its protocol explicitly, even when the client currently uses only one.
- [ ] Every consumed contract file lives under `docs/integration/client/{api-name}/`.
- [ ] Every consumed contract file is the provider's published copy, unedited.
