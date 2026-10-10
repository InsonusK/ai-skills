---
name: devops-release-package-in-dotnet
description: Release action for a .NET library — packs every packable project and pushes {version} to nuget.org from master and {version}-{timestamp} to GitHub Packages from develop; in a pull request it only packs
whenToUse: when a .NET library needs `.github/actions/release/action.yml`, or when you review how its NuGet packages are versioned and pushed
updated: 20261010
tags:
  - stack/dotnet
  - concern/ci
  - github-actions
  - release
---

# Goal
- `.github/actions/release/action.yml` in the project, an unchanged copy of this skill's file.
- The repository secret `RELEASE_REGISTRY_TOKEN` holding a nuget.org API key.
- From `master`: `{version}` on nuget.org. From `develop`: `{version}-{timestamp}` on GitHub Packages.

# Core Principle
- This skill is one of the release actions of [[skills/devops/core/devops-github-wf-release.skill/devops-github-wf-release.skill.md|devops-github-wf-release]]; a project has exactly one, and both workflows call it by its fixed path.
- The version is overridden on the command line; `Directory.Build.props` is never rewritten.

# Rule

## MUST

### Copy action.yml verbatim
Copy [[./assets/.github/actions/release/action.yml|action.yml]] verbatim to `.github/actions/release/action.yml`; do not modify it.
- Risk: the workflows pass `channel`, `version`, `timestamp`, and two tokens and read `notes`; an edited action that drops one fails every push.
- Fix: restore the file from the asset; propose a needed change to the user and make it in the skill.

### Store the nuget.org key
Create the repository secret `RELEASE_REGISTRY_TOKEN` with a nuget.org API key before the first push to `master`.
- Risk: the push fails at the end of the first release.
- Fix: add the secret in the repository settings.

### Mark what must not ship
Set `<IsPackable>false</IsPackable>` in every project that is not a library.
- Violation: a `*.Tests` project that packs because nothing marks it.
- Risk: test assemblies are published as packages.
- Fix: mark the project; the action packs the whole solution.

# Check list
- [ ] `.github/actions/release/action.yml` is an unchanged copy of this skill's file.
- [ ] The secret `RELEASE_REGISTRY_TOKEN` exists.
- [ ] Only library projects are packable.
