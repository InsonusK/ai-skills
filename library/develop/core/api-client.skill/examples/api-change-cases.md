# API change cases

The [responsibility contract](../api-client.skill.md#separate-protocol-mechanics-from-api-adaptation)
separates protocol mechanics from API adaptation. Client returns
[protocol-neutral API DTOs](../api-client.skill.md#keep-the-client-contract-independent-of-the-protocol);
service builds the Domain Model and hides how its external system realizes the operation.

| Change | Client responsibility | Service responsibility | Consumer impact |
|---|---|---|---|
| HTTP becomes gRPC for the same logical API | Replace endpoint/serialization/error/policy implementation behind the same client contract | None | None |
| One response holds a full name | Return the logical API DTO | Split/map through Domain Model APIs | None while the required behavior remains available |
| Response contains provider state | Return the provider's logical state | Map state and invoke domain behavior | None while the contract holds |
| One API operation becomes several | Expose the changed logical operations | Compose several methods of one client | None while the contract holds |
| One system publishes separate user and name APIs | Provide one entity-focused client for each API | Compose both clients of that same system | None while the contract holds |
| Names move to an independent system | Represent each system's API through its clients | Review dependency ownership and cross-system composition | Project decision; stability is not automatic |

## Protocol replacement

Assume the Identity system offers equivalent HTTP and gRPC forms of its Users API.
`UserService` depends on a `UsersClient` contract, calling `GetUser(id)` and receiving
its client-owned `UserDto`. An HTTP implementation sends the request to the path;
a gRPC implementation calls the generated stub and maps its message into that
same DTO. Raw HTTP/gRPC types stay inside those implementations.

Both normalize equivalent errors to the same logical remote status, for example
remote `NotFound` when the provider contracts define HTTP 404 and gRPC NOT_FOUND
as that same outcome. Service decides what remote `NotFound` means for its own
operation. The client applies configured credentials and retry policy; service
contains no transport-specific exception handling or policy loop.

Switching implementations in the composition root changes neither UserService
code nor its tests. If operations, field meanings or error semantics also change,
this is an API change, not merely a transport replacement.

@import "./diagrams/protocol-replacement.mmd" {as="mermaid"}

## One answer

`/getUserName` returns `Ann Lee`. Client parses its logical response DTO with that
full name. Service splits/maps it through the Domain Model APIs. Client does not
perform this conversion even though a single response is sufficient.

@import "./diagrams/one-answer.mmd" {as="mermaid"}

## Domain decision

`/getUser` returns `state: suspended`. Client preserves that provider state in its
API DTO. Service maps it and uses the domain's public behavior to determine
whether planning is allowed; it does not duplicate an invariant in a DTO mapper.

@import "./diagrams/application-decision.mmd" {as="mermaid"}

## Several calls of one client

The Identity system changes `/getUserName` to return a name ID and surname, then
adds `/getNameById`. UserApiClient now exposes both operations. UserService calls
both methods of that **one client**, maps the answers, and still offers the same
`UserName(id)` method. No caller needs to know about the second operation.

@import "./diagrams/split-operation.mmd" {as="mermaid"}

## Several clients of one system

The same Identity system reorganizes its contract into two logical APIs:

- Users API, represented by UsersClient: `GetUser(42)` returns
  `UserDto { nameId: 17, surname: "Lee" }`.
- Names API, represented by NamesClient: `GetName(17)` returns
  `NameDto { givenName: "Ann" }`.

These may be separate Swagger/proto files, or two cohesive groups in a large
provider file, as [entity-clients-and-system-services](../adr/entity-clients-and-system-services.md)
allows. Both are APIs of **the same external Identity system**. UserService owns
both clients and receives them through its constructor.

To implement `UserName(42)`, UserService calls UsersClient, takes name ID 17 from
its DTO, calls NamesClient, and constructs the domain name from the two answers.
It interprets their normalized failures; neither client calls the other. Consumer
methods, parameters and Domain Model answers remain unchanged when the provider
splits this API. Copy the complete provider contracts; each client implements
only the operations the integration uses.

@import "./diagrams/several-clients-one-system.mmd" {as="mermaid"}

## A second system

If names move from Identity into an independent Directory system, this is a new
external dependency with its own ownership, availability, credentials and failure
boundary. It differs from two clients of one provider system. Under the current
layer rules, a command or domain service composes the answers of the two system
services; neither integration service calls the other or hides the other system's
client. This may require changing the orchestration and its wiring at project level.

@import "./diagrams/master-system-before.mmd" {as="mermaid"}
@import "./diagrams/master-system-after.mmd" {as="mermaid"}

### Decision: keep cross-system composition above integration services

Allowing an integration service to call another could keep its consumer-facing
contract while the provider splits into systems. It also changes the one-system
boundary and can hide new dependency chains, cycles and failure/consistency
semantics. Calls alone do not guarantee that the original business capability
can still be honored.

Owner confirmed on 2026-11-28: retain the prohibition on service-to-service calls.
Cross-system composition stays in commands or domain services; splitting APIs
inside one system remains a separate case handled by its integration service.
