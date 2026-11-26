# API change cases

[API DTO returns](../api-client.skill.md#return-api-dtos-from-the-client) and
[single-system service composition](../api-client.skill.md#compose-one-external-systems-clients-in-the-service)
define the boundary; the [DTO decision](../adr/api-dtos-returned-to-service.md)
replaces the historical response-count distinction.

| Change | Client returns | Service responsibility | Consumer impact |
|---|---|---|---|
| One response holds a full name | API DTO with full name | Split/map through the consumer model's APIs | None while service contract holds |
| Response contains provider state | API DTO with provider state | Map state and use domain behavior for business decisions | None while contract holds |
| Provider splits one operation into two | One API DTO per operation | Compose both calls, potentially through two clients of the same system | None while contract holds |
| Data moves to an independent external system | Each client's own API DTO | Adapt each system separately | Review dependencies and orchestration at project level |

## One answer

`/getUserName` returns `Ann Lee`. Client returns the API response; service splits
and maps it. Even this simple transformation belongs to service.

@import "./diagrams/one-answer.mmd" {as="mermaid"}

## Domain decision

`/getUser` returns `state: suspended`. Client preserves that provider state in its
DTO. Service maps it and uses the domain's public behavior to determine whether
planning is allowed; it does not duplicate an invariant in a DTO mapper.

@import "./diagrams/application-decision.mmd" {as="mermaid"}

## Several clients of one system

The same provider replaces the name with an ID and provides `/getNameById`.
Service can call both operations through one client, or use entity-focused user
and name clients of that same system. Its caller still requests a user name.
A file containing both operations does not require one giant client.

@import "./diagrams/split-operation.mmd" {as="mermaid"}

## A second system

If names move to an independent Directory system, the application's external
dependencies change. A command or domain service composes the answers of the two
system services; neither service calls the other or hides a second system's client.

@import "./diagrams/master-system-before.mmd" {as="mermaid"}
@import "./diagrams/master-system-after.mmd" {as="mermaid"}
