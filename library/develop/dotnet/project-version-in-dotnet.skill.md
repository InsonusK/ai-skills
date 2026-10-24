---
name: project-version-in-dotnet
description: The version of a .NET solution — one <Version> in the root Directory.Build.props, applied by MSBuild to every assembly and package, reported by the running code from AssemblyInformationalVersionAttribute through one AppVersion class, overridden for a snapshot with -p:Version
whenToUse: when a .NET program must report its version, when you add or review a <Version>, <AssemblyVersion>, or <PackageVersion> in a props or project file, or when a .NET build must carry another version than the recorded one
updated: 20261024
tags:
  - stack/dotnet
  - concern/coding
  - versioning
---

# Goal
- **The version in .NET** - A .NET solution that keeps every rule of [project-version](library/develop/core/project-version.skill.md); this skill details it for .NET and restates none of its rules.

# Core Principle
- **MSBuild carries it** - MSBuild applies `Directory.Build.props` to every project, so every assembly and package of the solution carries the recorded version with no step of the project's own.

# Rule

## MUST

### Record the version in Directory.Build.props
Keep the version as one `<Version>` element, on one line, in a `<PropertyGroup>` of the root `Directory.Build.props`.
- Violation: `<Version>`, `<AssemblyVersion>`, or `<PackageVersion>` in a `.csproj`; a conditional second `<Version>` in the props file.
- Risk: projects of one solution ship different versions, and a reader of the props file finds the first element.
- Fix: one unconditional `<Version>1.4.0</Version>` in `Directory.Build.props`; remove the per-project ones.

### Report the version from the assembly
Report the running version through one `AppVersion.Value`, reading `AssemblyInformationalVersionAttribute` of the assembly without the commit the SDK appends after `+`.

```csharp
public static string Value { get; } =
    typeof(AppVersion).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion.Split('+')[0]
    ?? "0.0.0";
```

- Violation: `const string Version = "1.4.0"`; `Assembly.GetName().Version`, which is `AssemblyVersion` and drops a snapshot's suffix.
- Risk: a literal is not raised with the props file; the assembly version reports `1.4.0.0` for every snapshot.
- Fix: `AppVersion.Value` in the host project, used wherever the version is reported.

### Override the version only on the command line
Give a snapshot build its version with `-p:Version={version}` on `dotnet build`/`dotnet pack`/`dotnet publish`, and leave the props file unchanged.
- Violation: a build step that rewrites `Directory.Build.props` before packing.
- Risk: the rewritten file is committed or cached, and the next release reads a snapshot version.
- Fix: pass `-p:Version=…`; MSBuild's command-line property wins over the file.

# Check list
- [ ] The root `Directory.Build.props` has exactly one `<Version>` element, on one line; no `.csproj` sets a version.
- [ ] The program reports `AppVersion.Value`; no literal holds the number.
- [ ] A build with `-p:Version=9.9.9` reports `9.9.9`.
