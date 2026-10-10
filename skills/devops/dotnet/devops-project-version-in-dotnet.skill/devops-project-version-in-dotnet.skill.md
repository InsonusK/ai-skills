---
name: devops-project-version-in-dotnet
description: .NET implementation of devops-project-version — the version recorded once for the whole solution as <Version> in the root Directory.Build.props and the read-version.sh that prints it
whenToUse: when a .NET solution needs `make version`/`make version-check`, or when you add or review `<Version>` in its `Directory.Build.props` or `tools/version/read-version.sh`
updated: 20261010
tags:
  - stack/dotnet
  - concern/ci
  - versioning
---

# Goal
- One `<Version>` element in the root `Directory.Build.props` holding the solution's version.
- `tools/version/read-version.sh`, an unchanged copy of this skill's asset.
- No `<Version>`, `<AssemblyVersion>`, or `<PackageVersion>` in any `.csproj`.

# Core Principle
- This skill details [[skills/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]] for .NET; apply both.
- MSBuild applies `Directory.Build.props` to every project, so assemblies and packages carry the recorded version with no extra step.

# Rule

## MUST

### Record the version in Directory.Build.props
Keep the version as one `<Version>` element, on one line, in a `<PropertyGroup>` of the root `Directory.Build.props`.
- Violation: `<Version>` in individual `.csproj` files, or a conditional second `<Version>` in the props file.
- Risk: projects of one solution ship different versions, and `read-version.sh` prints the first element it finds.
- Fix: one unconditional `<Version>1.4.0</Version>` in `Directory.Build.props`; remove the per-project ones.

### Copy read-version.sh verbatim
Copy [[./assets/tools/version/read-version.sh|read-version.sh]] verbatim to `tools/version/read-version.sh`; do not modify it.
- Risk: a reader written in PowerShell or with the SDK needs a toolchain a plain shell step does not have.
- Fix: restore the file from the asset; it needs only `sed`.

### Override the version only on the command line
Give a snapshot build its version with `-p:Version={version}` on `dotnet build`/`dotnet pack`, and leave the props file unchanged.
- Violation: a build step that rewrites `Directory.Build.props` before packing.
- Risk: the rewritten file is committed or cached, and the next `make version` prints a snapshot version.
- Fix: pass `-p:Version=…`; MSBuild's command-line property wins over the file.

# Check list
- [ ] The root `Directory.Build.props` has exactly one `<Version>` element, on one line.
- [ ] `tools/version/read-version.sh` is byte-identical to this skill's asset.
- [ ] `make -s version` prints that element's value.
- [ ] No `.csproj` sets `<Version>`, `<AssemblyVersion>`, or `<PackageVersion>`.
