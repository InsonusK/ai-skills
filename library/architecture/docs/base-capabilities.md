# Base capabilities and their owners

Use this page with a language layout: [Go](layouts/go.md), [.NET](layouts/dotnet.md),
[Python](layouts/python.md). A base capability is a shared structural foundation;
it is not automatically a new selectable VP or a separate skill.

| Capability | Contract owner | Structural responsibility |
|---|---|---|
| Layer map | [app-layers](../../develop/core/app-layers.skill/app-layers.skill.md) | Logical boundaries, permitted calls and implementation-contract lookup |
| API entry | [app-api-layer](../../develop/core/app-api-layer.skill/app-api-layer.skill.md) | Translate transport input, dispatch commands, map outcomes |
| Commands and factories | [app-command-layer](../../develop/core/app-command-layer.skill/app-command-layer.skill.md) | Operation contracts, orchestration and nested dispatch |
| Domain | [app-domain-layer](../../develop/core/app-domain-layer.skill.md) | Entities, public invariant-preserving construction and reusable domain services |
| Client | [api-client](../../develop/core/api-client.skill/api-client.skill.md) | Logical API DTOs/outcomes plus protocol-specific endpoint, serialization and policy implementations |
| Service | Providing integration mechanism; [api-client](../../develop/core/api-client.skill/api-client.skill.md) for APIs | Consumer-facing adaptation and composition of its system's clients; no service-to-service calls |
| Infrastructure | Concrete store/broker mechanism | Technical non-client integration; own-store repositories use the stack's selected form |
| Composition | [base plateau](../spec/base-plateau.skill/base-plateau.skill.md) and [extension contract](../spec/base-plateau-extension.skill/base-plateau-extension.skill.md) | Construct implementations, wire ports, register commands, entries and ordered middleware |

## Base client

`client/core` holds reusable transport mechanics, normalized outcomes and the
call pipeline. An entity-focused client owns the logical API DTOs and selected
operations of its external API; its protocol implementation maps wire data into
those DTOs. Service maps DTOs into Domain Model and composes operations or clients
of that one external system. The common contract stays stable when equivalent
HTTP/gRPC implementations are exchanged. Domain types do not enter client code.

## Integration services

Implementation belongs to the mechanism that provides the service. Interfaces
stay with their consumers. In .NET a concrete service project can reference the
consumer's Application project solely for its service interface; Application does
not reference the concrete service project or API clients. This is dependency
inversion, not permission for service to call command handlers. The composition
root references and wires both sides. Cross-module contracts remain consumer-owned.

## Layout interpretation

The language trees are a catalog overlay, not a claim that every VP is mandatory
or one executable example includes them all. API entries compose through the runner;
startup is the base-only entry replaced when a real entry is selected. An own-store
SQLite realization is an alternative to PostgreSQL, not a second mandatory store.
Planned capabilities have explicitly proposed/unresolved placement and no fabricated
implementation link. No new service-to-service edge is implied by cache placement.
