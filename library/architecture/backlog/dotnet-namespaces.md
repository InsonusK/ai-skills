# Agree infrastructure namespace conventions in .NET

Status: Needs decision

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

The naming rule, whether existing projects are renamed, and inclusion of the
old catalog require owner approval. The previous instruction to keep current
names is not approval for a catalog-wide rename.
