# Reconsider the code organization of Client variation points

Status: Needs decision

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

The owner proposes a top-level `src/Client` boundary that hides external
HTTP/gRPC integration and exposes ready-to-use services to module Application
code. The concern is that the current API-specific client is thin, while the
mapping into domain entities and application error handling live in a service
inside one module. Another module needing the same external data may otherwise
repeat that integration work.

In the current .NET HTTP example, `TodoApiClient` delegates transport mechanics
to `Client.Http`, but still declares the operations, their retryability, request
and response DTOs, and response-to-`TodoApiItem` mapping. `RemotePlannerService`
then maps that result to `Todo.Domain.Entities.Item` and translates selected
technical failures into application errors. Retry execution is supplied by the
separate retry VP's middleware; the HTTP core provides the pipeline hook. The
review must distinguish this existing split from the proposed consolidation.

Compare three alternatives; none is an approved replacement:

1. **Client as an application-facing service:** place integration beneath
   `src/Client`; a Client calls the HTTP/gRPC core, receives DTOs, maps them to
   domain data, and exposes a service contract to module Application consumers.
   Keep transport details and any internal API-specific code inside Client.
2. **Separate Service and Client directories:** place application-facing
   integration services under `src/Service` and API-specific client classes
   under `src/Client`. Services call clients and map DTOs into entities or domain
   models. Establish when an API-specific class is needed, such as custom
   interpretation of status codes, rather than requiring an empty wrapper.
3. **Client service over configurable transport:** use `src/Client` for configs,
   middleware and application-model mapping over a generic HTTP/gRPC caller.
   Configure status handling and retry where sufficient, and retain a
   programmable extension for semantics that configuration cannot express.
   Determine whether this removes the separate API-specific wrapper of option
   2 without mixing domain rules into fixed transport code.

## Sources and scope

Concrete .NET examples motivating the task:

- [TodoApiClient](../dotnet/vp-http-client-in-dotnet.skill/examples/src/Infrastructure/Client/TodoApi/TodoApi/TodoApiClient.cs).
- [HTTP core](../dotnet/vp-http-client-in-dotnet.skill/examples/src/Infrastructure/Client/Core/Client.Http/HttpApiClient.cs).
- [RemotePlannerService](../dotnet/vp-http-client-in-dotnet.skill/examples/src/Modules/Todo/Todo.Application/Services/RemotePlannerService.cs).
- [Retry middleware](../dotnet/vp-client-retry-in-dotnet.skill/templates/src/Infrastructure/Client/Middleware/Retry/RetryMiddleware.cs).

Existing decisions to evaluate explicitly:

- [Service wraps one infrastructure](../../develop/core/app-layers.skill/adr/service-wraps-one-infrastructure.md)
  classifies technical clients as infrastructure, keeps an application-facing
  service above each, and places composition across systems in commands/domain
  services. It also explains the old Client/Integration terminology and the
  separate .NET own-store exception.
- [Client, not adapter](../../develop/core/api-client.skill/adr/client-not-adapter.md)
  chooses Client as the name for API-calling code under the sibling `client/` boundary;
  the older catalogs use Client for what is now called Service.
- [API DTOs mapped by service](../../develop/core/api-client.skill/adr/api-dtos-returned-to-service.md)
  assigns API-to-Domain Model mapping to service for every response; its historical
  reference preserves the superseded response-count distinction.
- [One outcome project for every client](../dotnet/vp-http-client-in-dotnet.skill/adr/one-outcome-project-for-every-client.md)
  puts shared HTTP/gRPC technical failures in `Client.Abstractions` so the
  application-facing service does not depend on transport packages.
- [Historical Integration/Client split](../../common-workflow/architecture/core/solution-integration-client-layering.skill/adr/integration-client-split.md)
  separates calling/parsing/contract validation from composing results into an
  application model. Its names differ from the current catalog; it is context,
  not authorization to migrate old consumers.

Inspect the [shared client contract](../../develop/core/api-client.skill/api-client.skill.md),
HTTP/gRPC/retry specs, all three stack implementations, fixed templates,
inherited examples and their application consumers. Start the concrete
comparison in .NET; agree whether the resulting contract changes apply to Go
and Python before migrating them. Record the affected surfaces and retained
guarantees of each ADR, not only a new folder tree.

## Dependencies and start condition

Investigation can begin independently. Agree ownership, contracts and the
implementation scope before moving code. Coordinate paths and namespaces with
[.NET naming](dotnet-namespaces.md): the proposed service-name namespace prefix
must not accidentally settle the Client boundary. Coordinate shared-file edits
with [attach-rule](attach-rule.md). Keep
[legacy-client retirement](legacy-client-skills.md) separate; this task does not
authorize replacing or removing the old catalogs.

## Expected result

1. Inventory the actual responsibilities from network call through DTO parsing,
   contract validation, status interpretation, retries, composition and mapping
   into the consumer's model. Identify reused mechanics versus API-specific and
   module-specific semantics in both HTTP and gRPC paths.
2. Compare the existing design and the three proposals on the concrete planner
   example. Show two module consumers of the same external data, the public
   contracts they use, and the references required by the proposed project tree.
   Demonstrate which integration work is shared and which mapping remains local.
3. Resolve model ownership: a shared Client returning one module's Entity must
   not create accidental dependencies from another module onto that module's
   implementation. Compare consumer-owned contracts, shared application models
   and domain-specific mapping; justify any remaining per-module mapping rather
   than treating every such mapping as duplication.
4. If status interpretation and retry become configuration, show operation-level
   rules, safe retry/idempotency decisions and how custom behavior is supplied.
   Define whether API-specific DTO/operation declarations require a class, typed
   configuration or generated code. Do not move application error meanings or
   domain invariants into the generic transport merely to remove a wrapper.
5. After owner agreement, record the decision in the owning skill's ADR and
   explicitly amend or supersede affected decisions; propagate the approved
   contracts, folders, templates, project references, examples, documentation,
   `attach.allow` and `conformance.copy` together. Preserve unchanged guarantees
   by reference instead of copying the old ADRs into a new specification.

## Validation

For the comparison, show callers depending only on the chosen application-facing
contract, transport replacement and response-shape change confined to the
integration boundary, and two modules reusing external-data access without
crossing unapproved module boundaries. Verify that invalid data received from
the remote system is a service failure rather than invalid caller input.

For an approved implementation, exercise HTTP and gRPC success, custom status
meaning, unavailable dependency, malformed response, timeout, cancellation,
request/log propagation, emulator-based tests and retry safety/order. Preserve
domain validation through the domain's public construction/behavior APIs. Run
affected stack examples and shared structural/sync checks from the execution
guide; use the shared scenarios unchanged unless their owning contract is
explicitly revised. A folder move alone is not acceptance evidence.

## Open decisions

Agree the meaning and public contract of Client, whether there is a separate
Service layer/directory, ownership of returned models and cross-module reuse,
the configuration versus code boundary for status/retry/mapping, project and
namespace structure, and the stacks/examples included in migration. The owner
has requested evaluation of these alternatives, not selected one for migration.
