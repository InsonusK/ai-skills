# Client and Service code organization

Status: Needs decision

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

The owner selected separate sibling Client and Service boundaries. Client hides
endpoints, protocol serialization/error parsing, auth and retry execution. It
returns logical API DTOs and common technical outcomes. Service maps those DTOs
into Domain Model and composes operations/clients of one external system. The
implementation is complete in the new catalog's HTTP/gRPC/retry examples for
Go, Python and .NET. Remaining questions below concern reuse and configuration.

## Sources and scope

- [Shared client contract](../../develop/core/api-client.skill/api-client.skill.md)
- [API DTO ownership](../../develop/core/api-client.skill/adr/api-dtos-returned-to-service.md)
- [Protocol-neutral contract](../../develop/core/api-client.skill/adr/protocol-neutral-client-contract.md)
- [Entity clients and system services](../../develop/core/api-client.skill/adr/entity-clients-and-system-services.md)
- [.NET sibling projects](../../develop/dotnet/app-layers-in-dotnet.skill/adr/client-and-service-sibling-projects.md)
- [HTTP client](../dotnet/vp-http-client-in-dotnet.skill/examples/src/Client/TodoApi/TodoApi/TodoApiClient.cs)
- [Adapting service](../dotnet/vp-http-client-in-dotnet.skill/examples/src/Service/Todo/Todo.Service/RemotePlannerService.cs)
- [Per-language layouts](../docs/base-capabilities.md)

Only the new architecture catalog and its develop extensions are migrated.
[Legacy retirement](legacy-client-skills.md) remains a separate task.

## Dependencies and start condition

The implemented boundary can be used now. Investigate the remaining questions
when a concrete second module consumes the same external data or several clients
share provider endpoint settings. Agree that concrete ownership/configuration
contract before introducing shared models or automatic grouping.

## Expected result

Implemented: sibling client/service roots, consumer-owned ports, protocol-neutral
logical API DTOs, technical outcomes with protocol diagnostics retained as causes,
and service-owned Domain Model construction. .NET uses a separate Service project;
Application retains its interface and has no client project reference. Retry stays
inside the client pipeline. Services do not call services.

Follow-up: demonstrate two actual module consumers without coupling either to
the other's implementation; determine when their adaptation can be reused and
when differing domain semantics require different mappings. Agree how multiple
entity clients of one provider share endpoint/auth settings without conflating
their operation-level retry safety or changing the one-owning-service default.

## Validation

The three stacks' HTTP, gRPC and retry examples exercise success, unavailable
and rejected calls, malformed responses, timeout/cancellation, request/log
propagation, emulators and retry safety/order. The wave-2 structural gate checks
that service, DTO and outcome source contracts remain identical across transport
and retry variants, and that .NET Application cannot reference clients. Runtime
results are recorded in [the wave audit](../agent/design-rethink/wave-2/STATUS.md).
This is evidence for protocol substitution, not a two-module reuse demonstration.

## Open decisions

Cross-module model/adaptation reuse and shared provider configuration remain open.
Physical client/service locations, API DTO ownership, same-system composition,
protocol-neutral outcomes and the service-to-service prohibition are decided;
do not reopen them implicitly while resolving the follow-up.
