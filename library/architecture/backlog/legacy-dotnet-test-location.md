# Align legacy .NET test projects with the location ADR

Status: Deferred

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

The current testing convention places each .Tests project beside the
production project. The old .NET architecture catalog still describes a root
tests/ tree, including project-creation templates. The owner chose the sibling
layout on 2026-10-21; the unresolved part is when and how much legacy content to
migrate, not the preferred layout.

## Sources and scope

- [Authoritative test-location decision](../../testing/dotnet/cucumber-testing-in-dotnet.skill/adr/test-location.md)
- [Legacy plateaus, solutions and templates](../../dotnet/architecture)
- [Current catalog as a comparison](../dotnet)

Inventory legacy examples, *.Tests.csproj.create.md instructions, solution
entries, relative ProjectReference paths, test commands, CI paths and docs.
Do not rework the current catalog or move unrelated test assets.

## Dependencies and start condition

Start when the owner chooses to maintain/migrate the old catalog during its
replacement. If a legacy artifact will be retired, confirm that before spending
work relocating it.

## Expected result

1. Agree which legacy artifacts survive and migrate only that set.
2. Put each surviving test project in the same parent folder as its production
   project; update all references and creation instructions together.
3. Keep each solution buildable and tests discoverable from its documented
   commands. Document any approved retirement rather than treating deletion as
   successful relocation.

## Validation

Create representative solutions from changed templates, restore/build and run
their tests. Check every solution/project reference resolves and search for
active references to the former root tests/ layout. Run shared link/sync checks;
explicitly distinguish historical ADR discussion from executable instructions.

## Open decisions

The destination layout is decided. Owner agreement is needed for the migration
window and which legacy artifacts are maintained versus retired.
