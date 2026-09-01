---
name: cpm-over-inline-versions
description: How NuGet package versions are declared across the repository
problem: Every project in a multi-project solution needs NuGet package versions, and two projects resolving different versions of the same package is a latent runtime failure.
decision: Use .NET Central Package Management — one repo-root Directory.Packages.props holding every version, versionless PackageReference in every csproj.
tags:
  - solution/central-package-management
  - stack/dotnet
  - concern/documentation
  - concern/documentation/adr
---

# Problem

A service in this family is a multi-project solution (App.Host, Shared, BuildingBlocks, one set of projects per module, one test project per production project). Many of these reference the same packages — MediatR, FluentValidation, Ardalis.Result, the test stack. If each `.csproj` pins its own version, the versions drift: a `MissingMethodException` or assembly-load failure at runtime, or a silent transitive downgrade, with nothing failing the build.

# Selected variant

**Selected variant:** [[#Central Package Management]]

- `Directory.Packages.props` at the repo root, `ManagePackageVersionsCentrally=true`
- every `<PackageReference>` is versionless; every version is a `<PackageVersion>` entry in the central file
- an inline `Version=` becomes build error `NU1008`

# Searched variants

## Inline versions per csproj

### Description
Each project declares `<PackageReference Include="X" Version="N" />` directly.

### Benefits
- Zero extra files; the default `dotnet new` behaviour
- A project is self-describing — its versions are visible in its own file

### Costs
- No mechanism prevents two projects from pinning different versions of the same package
- A package upgrade is N edits across N csproj files, easy to do partially
- No single place to read "what does this repo depend on"

## Directory.Build.props with shared MSBuild properties for versions

### Description
Define `<MediatRVersion>` etc. as MSBuild properties in `Directory.Build.props`, reference them as `Version="$(MediatRVersion)"` in each csproj.

### Benefits
- One place to change a version
- Uses only long-standing MSBuild features, no CPM opt-in

### Costs
- Each csproj still needs a `Version="$(...)"` attribute — the boilerplate moves, it does not go away
- Nothing stops a project from writing a literal version instead of the property
- Not the idiomatic .NET mechanism for this; tooling (Dependabot, `dotnet list package`, NuGet audit) understands `Directory.Packages.props`, not ad-hoc properties

## Central Package Management (selected)

### Description
`Directory.Packages.props` at the repo root with `<ManagePackageVersionsCentrally>true</ManagePackageVersionsCentrally>` and one `<PackageVersion Include="X" Version="N" />` per package. Every `<PackageReference>` in every project is versionless. Optionally `CentralPackageTransitivePinningEnabled` pins transitive dependencies to the same list.

### Benefits
- Version drift between projects is impossible by construction — there is one version, period
- A package upgrade is a one-line diff
- The central file is the repository's dependency inventory
- First-class NuGet/SDK feature: `NU1008` fails an inline version, `NU1010` fails a missing central entry, so mistakes fail the build not production
- Every later solution has one obvious place to add its package versions

### Costs
- One opt-in file and property to introduce up front
- A project's own file no longer shows its versions — a reader must open `Directory.Packages.props`
- `VersionOverride` exists as an escape hatch and must be explicitly forbidden to keep the guarantee
