---
name: solution-central-package-management
description: Puts every NuGet package version in a single repo-root Directory.Packages.props via .NET Central Package Management, so project files carry versionless PackageReference only and no two projects can disagree on a version
whenToUse: when creating the repository skeleton for a new service, adding the first NuGet dependency to any project, or reviewing whether a csproj that pins a version inline should move that version to Directory.Packages.props
domain: skill
type: architecture
version: 20260901000000
tags:
  - skill/architecture/solution
  - solution/central-package-management
  - stack/dotnet
  - concern/architecture
  - concern/build
  - nuget
creates:
  - "Directory.Packages.props"
extends:
  - "Directory.Build.props"
depends_on:
adr:
  - "[[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/adr/cpm-over-inline-versions.md|Central Package Management over inline PackageReference versions]]"
---

# Goal
- Give the repository exactly one place where a NuGet package version is declared — a repo-root `Directory.Packages.props` — so every project resolves the same version of the same package.
- Make an inline `Version=` attribute on a `<PackageReference>` a build error, not a style violation, once this solution is applied.
- Give every later solution a single, predictable file to add its own package versions to, instead of scattering `Version=` attributes across the csproj files it touches.

# Capabilities
- One version per package across the whole solution — a transitive-dependency downgrade or a split-brain version between two projects becomes impossible by construction.
- A single diff to review when a package is upgraded, instead of one per consuming project.
- `Directory.Packages.props` doubles as the repository's dependency inventory — every package the solution uses, in one list.
- New projects created by any later solution need no version bookkeeping — they reference a package by name, the version is already centralised.

# Core Principles
- `Directory.Packages.props` lives at the repository root, next to the `.sln` file, and sets `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`.
- Every package version is a `<PackageVersion Include="X" Version="N" />` entry in `Directory.Packages.props`; every consuming project carries `<PackageReference Include="X" />` with no `Version` attribute.
- A solution that introduces a new package adds its `<PackageVersion>` line to `Directory.Packages.props` and a versionless `<PackageReference>` to the project that needs it — the two steps are not optional halves.
- Global package references that genuinely apply to every project (analyzers, `Microsoft.SourceLink.GitHub`) use `<GlobalPackageReference>` in `Directory.Packages.props`, never a per-project `<PackageReference>`.
- The central file is grouped and commented by concern (test, MediatR, EF Core, ...) so a reader can find where a new entry belongs; entries stay alphabetically ordered within a group.
- Version overrides (`VersionOverride` on a `<PackageReference>`) are forbidden — a project that needs a different version is a smell to resolve, not to encode.

# Boundaries
- This solution does not choose which packages the solution uses or what versions they should be — it only defines where those versions are declared. Each later solution brings its own package list.
- `.NET SDK` version pinning (`global.json`) and MSBuild-level shared properties (`Directory.Build.props`) are related repo-root mechanisms but out of scope here; this solution only touches `Directory.Build.props` to nothing more than confirm it exists alongside `Directory.Packages.props`.

# Adr
- [[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/adr/cpm-over-inline-versions.md|Central Package Management over inline PackageReference versions]]
  - Selected variant: repo-root `Directory.Packages.props` with `ManagePackageVersionsCentrally=true`, versionless `<PackageReference>` everywhere.

# Requirements
SOLUTION:
- None — this is a foundation solution, applied before or alongside [[skills/dotnet/architecture/v3.1/solutions/solution-sln-structure.skill/solution-sln-structure.skill.md|solution-sln-structure]].

NUGET:
- None — this solution declares the mechanism for versioning packages, it adds no package of its own.

# Template Skill Mutations

PROJECT:
- [[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/Implementation/Directory.Packages.props.create.md|Directory.Packages.props]] - create - the single home for every NuGet package version in the repository

# Workflow

## Add a new package (any later solution)
1. Add `<PackageVersion Include="{Package}" Version="{Version}" />` to the matching `<ItemGroup>` in `Directory.Packages.props` (create the group with a comment if the concern is new).
2. Add `<PackageReference Include="{Package}" />` — no `Version` — to the project that consumes it.
3. `dotnet restore` — a missing central entry fails the build with `NU1010`, a stray inline `Version` fails with `NU1008`.

## Upgrade a package
1. Change the one `Version` attribute in `Directory.Packages.props`.
2. `dotnet restore` + run the conformance suite. No csproj file changes.

# Rule

## MUST
- [[skills/dotnet/architecture/v3.1/solutions/solution-central-package-management.skill/Implementation/Directory.Packages.props.create.md#MUST|Directory.Packages.props.create]]
- Declare every package version in `Directory.Packages.props`; never leave a `Version` attribute on a `<PackageReference>` in any csproj.
  - Risk: two projects drift to different versions of the same package, and the conflict only surfaces at runtime as a `MissingMethodException` or an assembly-load failure.
  - Fix: move the version to a `<PackageVersion>` entry; keep the `<PackageReference>` versionless.
- When a solution adds a `<PackageReference>` to a project, add the matching `<PackageVersion>` to `Directory.Packages.props` in the same change.
  - Risk: `dotnet restore` fails with `NU1010` ("no version specified") and the solution looks broken on first build.
  - Fix: treat the central entry and the project reference as one atomic edit.
- Keep `Directory.Packages.props` at the repository root, with `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` in a top `<PropertyGroup>`.
  - Risk: placed inside a subfolder, it only governs projects below it, silently leaving the rest on inline versions.
  - Fix: root location, next to the `.sln`.
- Never use `VersionOverride` on a `<PackageReference>`.
  - Risk: the override reintroduces exactly the per-project version drift this solution exists to remove, while looking sanctioned.
  - Fix: resolve why the project needs a different version — usually a shared dependency that should move up — instead of encoding the divergence.

## SHOULD
- Group `<PackageVersion>` entries by concern with a comment per group, alphabetically ordered within the group.
- Use `<GlobalPackageReference>` for packages that truly apply to every project (analyzers, SourceLink), instead of repeating a `<PackageReference>` in each csproj.

# Check list
- [ ] `Directory.Packages.props` exists at the repository root.
- [ ] `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` is set.
- [ ] No csproj in the repository has a `Version` attribute on any `<PackageReference>`.
- [ ] Every `<PackageReference>` resolves to a `<PackageVersion>` entry (`dotnet restore` is clean, no `NU1010`).
- [ ] `<PackageVersion>` entries are grouped by concern and ordered within each group.
- [ ] No `VersionOverride` attribute anywhere.
