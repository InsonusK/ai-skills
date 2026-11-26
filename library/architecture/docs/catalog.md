# Features and variation points

This is the first documentation sample for the catalog redesign. The current
[architecture README](../README.md) owns the roadmap and stack status until the
reviewed maps replace those sections. A variation point (VP) is an optional or
alternative architectural choice attached to the application's base.

## Model and dependencies

The catalog has one [base plateau](../spec/base-plateau.skill/base-plateau.skill.md)
and an [extension contract](../spec/base-plateau-extension.skill/base-plateau-extension.skill.md).
The older [plateau pipeline](../../common-workflow/architecture/design/plateau-map/plateau-map-creation-workflow.skill.md)
assembles plateaus from combinations of solutions; that is a different authoring
model and cannot be applied unchanged here.

The outline below is navigation, not a dependency tree. Combinable capabilities
need a graph because a VP can have multiple prerequisites.

| Capability | Existing specifications |
|---|---|
| Base command execution and composition | `base-plateau`, `base-plateau-extension` |
| Application entry points | `vp-http-api`, `vp-grpc-api`, `vp-cli-api` |
| Operational endpoints and payload logging | `vp-health`, `vp-logging` |
| Calls to external APIs | `vp-http-client`, `vp-grpc-client` |
| Safe repetition of external calls | `vp-client-retry` |
| Persistent storage | `vp-postgres` |
| Transaction boundary | `vp-unit-of-work` |
| Concurrent writes | `vp-row-version` |
| Caller-assigned external identity | `vp-external-id` (specification exists; stack work planned) |

The roadmap also plans edit timestamps, request idempotency, TaskBox, Inbox,
Outbox, SQLite and data caching. Kafka/RabbitMQ listeners and producers appear
in the application-type view; their absence from the execution steps must be
recorded as a roadmap gap rather than silently dropping them.

| From | Relation | To | Evidence / interpretation |
|---|---|---|---|
| Every current VP | requires | Base plateau | Current catalog composition model |
| Client retry | requires-any | HTTP client, gRPC client | Retry spec Scope and Goal |
| Unit of work | requires | DB VP | Shared contract requires one DB; current stack implementations attach to PostgreSQL |
| Row version | requires | PostgreSQL | Spec compares through its base repository and engine version |
| External id | requires | PostgreSQL | Current spec attaches table by table |
| PostgreSQL, Health | shares-contract | Readiness | Optional VPs meet through a shared check contract; neither requires the other |
| HTTP proto mode | shares-contract | gRPC API | Copied proto and shared conformance, not a requirement to serve both transports |
| Data cache (planned) | requires | DB VP and client VP | Roadmap states both; exact store/client variants are not yet specified |
| TaskBox / Inbox / Outbox (planned) | unresolved | Transactional storage | Claim transaction decision exists; integration with unit of work remains open |

A stack example's selected HTTP/CLI entry is `example-built-on` evidence, not a
constraint requiring every application using that VP to choose that entry.

## Client and service: owner decision, implementation pending

The owner decided on 2026-11-26:

```text
Command/domain consumer
    -> Service: consumer-facing operations, DTO -> consumer entity/model mapping
        -> Client: API operations, API DTOs, technical outcomes, client pipeline
            -> Provider API
```

Service absorbs provider-specific shapes and composes that API's operations to
meet its consumer's needs. Entity construction still uses the domain's public
validation/behavior APIs. Client sends calls, parses the API response and returns
API DTOs. Retry executes inside the client pipeline when `vp-client-retry` is
attached; its safety marks, timeouts, cancellation and technical failure handling
remain client responsibilities.

For example, a provider returning `{full_name: "Ann Lee"}` yields that API DTO
from the client. Service constructs the consumer's name/entity. If the provider
later returns a name ID and requires a lookup, service coordinates those calls
and preserves its consumer-facing contract. Combining different systems still
belongs to the layers that own that composition.

This revises the existing contract. The current
[api-client skill](../../develop/core/api-client.skill/api-client.skill.md) and
[return-value ADR](../../develop/core/api-client.skill/adr/client-returns-application-values.md)
put mapping from a single answer in client. Both
[HTTP](../spec/vp-http-client.skill/vp-http-client.skill.md) and
[gRPC](../spec/vp-grpc-client.skill/vp-grpc-client.skill.md) prohibit returning
response types to service. Those rules, their scenarios and all three stack
realizations must change together; this sample does not claim they already do.

The [retry spec](../spec/vp-client-retry.skill/vp-client-retry.skill.md) already
places retry in client middleware. The
[client organization card](../backlog/client-code-organization.md) contains broader
unresolved questions: directories, project boundaries and cross-module reuse.
The responsibility decision settles only part of that card. Whether gRPC returns
generated messages or client-owned API DTOs remains to be decided explicitly.

## Existing decisions and unresolved work

| Topic | Authoritative evidence | State |
|---|---|---|
| Service wraps infrastructure | [Layer ADR](../../develop/core/app-layers.skill/adr/service-wraps-one-infrastructure.md) | Current, with own-store exception |
| Entry and command middleware lists | [Extension ADR](../spec/base-plateau-extension.skill/adr/command-registry-and-middleware-lists.md) | Current |
| Fixed middleware stays inside entry | [Entry ADR](../spec/base-plateau-extension.skill/adr/fixed-middleware-inside-the-entry.md) | Current |
| Shared scenarios copied to stacks | [Conformance ADR](../spec/vp-logging.skill/adr/conformance-scenarios-copied-into-each-stack.md) | Current |
| Database rows or entities | [Storage ADR](../spec/vp-postgres.skill/adr/two-forms-rows-and-entities.md) | Current, stack-specific form |
| Client DTO -> service entity boundary | Owner decision above | Decided, implementation pending; replacement ADR required |
| Attachment restrictions | [Attach card](../backlog/attach-rule.md) | Under review; do not turn existing allowlists into a new absolute design rule |
| Cache wrapper, SQLite, queue claim transaction | [Roadmap decisions](../README.md#decisions-and-open-questions-of-the-queued-vps) | Accepted parts plus explicitly open details |

The full decision index will include every spec ADR, preserve its owner and link
stack-specific ADRs without promoting implementation choices into shared rules.
