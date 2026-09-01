---
description: Create the repo-root Directory.Packages.props that centralises every NuGet package version
name: Directory.Packages.props
element_kind: project
change_kind: create
tags:
  - solution/central-package-management
  - element/directory-packages-props
---

# Goals
- Hold every NuGet package version used anywhere in the repository, in one file.
- Enable `ManagePackageVersionsCentrally` so a versioned `<PackageReference>` becomes a build error.

# Core Principles
- Root location, next to the `.sln` — governs every project in the tree.
- One `<PackageVersion>` per package; consuming projects reference the package name only.
- Grouped and commented by concern; alphabetical within a group.

# Structure

## Project Structure
```
/ (repository root)
  Directory.Packages.props
  Directory.Build.props
  {Solution}.sln
```

## Directory and class skills
| Directory \| file | Description |
| ----------------- | ----------- |
| Directory.Packages.props | Central version list + `ManagePackageVersionsCentrally` |

# Implementation changes

Create `Directory.Packages.props` at the repository root:

```xml
<Project>
  <PropertyGroup>
    <ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>
    <CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>
  </PropertyGroup>

  <!-- MediatR -->
  <ItemGroup>
    <PackageVersion Include="MediatR" Version="{version}" />
  </ItemGroup>

  <!-- Validation -->
  <ItemGroup>
    <PackageVersion Include="FluentValidation" Version="{version}" />
    <PackageVersion Include="FluentValidation.DependencyInjectionExtensions" Version="{version}" />
  </ItemGroup>

  <!-- Result -->
  <ItemGroup>
    <PackageVersion Include="Ardalis.Result" Version="{version}" />
  </ItemGroup>

  <!-- Test (added by solution-dotnet-conformance-testing) -->
  <ItemGroup>
    <PackageVersion Include="Microsoft.NET.Test.Sdk" Version="{version}" />
    <PackageVersion Include="Reqnroll.xUnit" Version="{version}" />
    <PackageVersion Include="coverlet.collector" Version="{version}" />
  </ItemGroup>

  <!-- Analyzers, applied to every project -->
  <ItemGroup>
    <GlobalPackageReference Include="Microsoft.SourceLink.GitHub" Version="{version}" />
  </ItemGroup>
</Project>
```

Every consuming `.csproj` then references a package with no version:

```xml
<ItemGroup>
  <PackageReference Include="MediatR" />
  <PackageReference Include="FluentValidation" />
</ItemGroup>
```

A later solution that needs a new package adds one `<PackageVersion>` line here (in the right group, creating the group with a comment if the concern is new) plus a versionless `<PackageReference>` in its project.

# NuGet Packages
| Package | Version constraint | Purpose |
| ------- | ------------------ | ------- |
| — | — | This file declares versions; it consumes no package itself |

# What Does NOT Belong Here
- `.NET SDK` version — belongs in `global.json`.
- MSBuild shared properties (`TargetFramework`, `Nullable`, `LangVersion`) — belong in `Directory.Build.props`.
- Package *references* (which project uses what) — stay in each `.csproj`; only the *versions* are centralised here.

# Allowed Dependencies
- None — MSBuild reads this file directly; it has no project references.

# Rules

## MUST
- Set `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>`.
  - Risk: without it the file is inert and projects silently keep inline versions.
  - Fix: the property is the whole point of the file — it is not optional.
- Add a `<PackageVersion>` here for every package any project references.
  - Risk: `dotnet restore` fails with `NU1010` for the missing entry.
  - Fix: add the central entry in the same change as the `<PackageReference>`.
- Keep every `<PackageReference>` in every csproj versionless.
  - Risk: `NU1008` build error, or — if `ManagePackageVersionsCentrally` is off — silent version drift.
  - Fix: move the version here; leave only `Include=` on the reference.

## SHOULD
- Enable `<CentralPackageTransitivePinningEnabled>true</CentralPackageTransitivePinningEnabled>` so transitive dependencies are pinned to the central list too.
- Keep entries grouped by concern with a header comment, alphabetical within the group.

# Check list
- [ ] `Directory.Packages.props` is at the repository root.
- [ ] `ManagePackageVersionsCentrally` is `true`.
- [ ] Every package referenced by any project has a `<PackageVersion>` entry.
- [ ] No csproj carries a `Version` attribute on a `<PackageReference>`.
- [ ] `dotnet restore` is clean (no `NU1008`/`NU1010`).
