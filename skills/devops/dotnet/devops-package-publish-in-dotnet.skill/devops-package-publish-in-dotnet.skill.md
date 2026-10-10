---
name: devops-package-publish-in-dotnet
description: .NET implementation of devops-package-publish — the package job of release.yml that publishes to nuget.org from master and to GitHub Packages from develop
whenToUse: when a .NET library must publish its package from `release.yml`, or when you review the `package` job of a .NET project
updated: 20261010
tags:
  - stack/dotnet
  - concern/ci
  - github-actions
  - release
---

# Goal
- The `package` job of `.github/workflows/release.yml` assembled from this skill's job file.
- `master` publishing `{version}` to nuget.org; `develop` publishing `{version}-{timestamp}` to GitHub Packages.
- The repository secrets `NUGET_API_KEY`.

# Core Principle
- This skill details [[skills/devops/core/devops-package-publish.skill.md|devops-package-publish]] for .NET; apply both.

# Rule

## MUST

### Assemble with this job file
Pass [[./templates/package-job.yml|package-job.yml]] to `assemble-workflow.sh` as `package=`, then replace `{package-name}` with the `PackageId`.
- Risk: a leftover placeholder publishes nothing or links the Release to a package that does not exist.
- Fix: search `release.yml` for `{` followed by a lowercase name and replace each.

### Pack only packable projects
Leave `dotnet pack` without a project argument and mark what must not ship with `<IsPackable>false</IsPackable>`.
- Violation: a `*.Tests` project that packs because nothing marks it.
- Risk: test assemblies are published as packages.
- Fix: set `<IsPackable>false</IsPackable>` in every project that is not a library.

# Check list
- [ ] `release.yml` holds the `package` job of this skill's file; no `{package-…}` placeholder is left.
- [ ] The secrets `NUGET_API_KEY` exist.
- [ ] A `develop` push publishes `{version}-{timestamp}` to GitHub Packages.
