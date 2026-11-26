# Qualify .NET service namespaces

Status: Completed

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

The owner explicitly kept Postgres and Postgres.Testing during the entities
work (2026-11-14), deferring a common catalog-wide rule. A directory named
Infrastructure does not by itself decide the project's namespace.

## Sources and scope

- [Current .NET catalog](../dotnet)
- [PostgreSQL skill, examples and templates](../dotnet/vp-postgres-in-dotnet.skill)
- [Entities decisions](../agent/dotnet-entities/DECISIONS.md)
- [.NET layer conventions](../../develop/dotnet/app-layers-in-dotnet.skill/app-layers-in-dotnet.skill.md)

Inventory project names, AssemblyName/RootNamespace overrides, C# namespaces,
references and template identifiers across current infrastructure VPs. Inventory
older catalog consumers separately; do not silently migrate them too.

## Dependencies and start condition

Start when the owner schedules the catalog-wide naming pass. Preserve the
current names until the common rule and migration scope are agreed.

## Expected result

1. Compare short infrastructure names with qualified Infrastructure.* names,
   using existing PostgreSQL and client projects as examples.
2. Agree separately what the rule governs: project/assembly names, namespaces,
   folders and test-project names need not all be identical.
3. Record the decision in the owning convention/ADR. If a migration is selected,
   update the approved projects, templates, imports, references, solution files
   and documentation together.

## Validation

Build and test every changed .NET example, including PostgreSQL/UoW/row-version
on PostgreSQL when affected. Verify generated templates use the chosen naming
consistently. Run shared structural checks.

## Open decisions

No decision remains open for this migration. The owner selected service-qualified
namespaces for all current .NET layers and approved preserving project/assembly
names, folders and the old catalog. Client/Service organization is the separate
[Client organization task](client-code-organization.md).

## Investigation

[Historical decision brief](../agent/dotnet-namespaces/REVIEW.md) compares capability names,
qualified namespaces alone, and qualified project/namespace identities. Its
reproducible inventory covers 43 current infrastructure project files, fixed
template identifiers, namespace imports, references and the old catalog's
consumers separately. Its recommendation to preserve short capability namespaces
was rejected in favor of the service-qualified rule below.

## Outcome

The [owning ADR](../../develop/dotnet/app-layers-in-dotnet.skill/adr/service-qualified-namespaces.md)
records `{ServiceName}.{Layer}.{LogicalName}`. All 14 current .NET examples use
`TodoExampleService`; namespace-dependent templates fill `{service-namespace}`
for each application. PostgreSQL uses `Infrastructure.Database.Postgres`, client
core uses `Infrastructure.Client.Core`, and modules use `Modules.{Module}` below
the service root. Project and assembly names and all folders remain unchanged.

The [migration contract and evidence](../agent/dotnet-namespaces/MIGRATION.md)
cover 920 C# files, 242 template-example comparisons, all 14 builds/test suites
(1,729 passing tests), and an additional HTTP-client fixture using
`HttpClientExampleService`. Database suites ran on an isolated PostgreSQL 18.
The namespace guard is part of `test/test.sh`; shared structural/sync checks
and the per-wave fresh-eyes audits are required before handing off the PR.
