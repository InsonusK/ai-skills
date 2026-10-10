---
name: devops-release-package-in-typescript
description: Release action for a TypeScript or Angular library — builds the npm package and publishes {version} to npmjs.org from master and {version}-{timestamp} to GitHub Packages from develop; in a pull request it only packs
whenToUse: when a TypeScript or Angular library needs `.github/actions/release/action.yml`, or when you review how its npm package is versioned and published
updated: 20261010
tags:
  - stack/typescript
  - concern/ci
  - github-actions
  - release
---

# Goal
- `.github/actions/release/action.yml` in the project, the filled copy of this skill's file.
- The repository secret `RELEASE_REGISTRY_TOKEN` holding an npm access token.
- From `master`: `{version}` on npmjs.org under `latest`. From `develop`: `{version}-{timestamp}` on GitHub Packages under `snapshot`.

# Core Principle
- This skill is one of the release actions of [[skills/devops/core/devops-github-wf-release.skill/devops-github-wf-release.skill.md|devops-github-wf-release]]; a project has exactly one, and both workflows call it by its fixed path.
- A snapshot is never `latest`: an install without a version resolves `latest`.

# Rule

## MUST

### Fill and copy action.yml
Fill and copy [[./templates/.github/actions/release/action.yml|action.yml]] to `.github/actions/release/action.yml`, replacing `{package-dir}` with the folder that is published — `.` for a single package, the build output such as `dist/{library}` for a workspace library — and change nothing else.
- Violation: `{package-dir}` left in the file, or the source folder of a workspace library given instead of its build output.
- Risk: the step fails on a missing folder, or the unbuilt sources are published.
- Fix: search the file for `{package-dir}`; none may remain.

### Store the npm token
Create the repository secret `RELEASE_REGISTRY_TOKEN` with an npm access token before the first push to `master`.
- Risk: the publish fails at the end of the first release.
- Fix: add the secret in the repository settings.

### Scope the package to the repository owner
Name the package `@{owner}/{name}`, with `{owner}` the owner of the GitHub repository.
- Risk: GitHub Packages accepts only packages scoped to the owner, so every snapshot from `develop` is refused.
- Fix: set `name` in the published `package.json`.

# Check list
- [ ] `.github/actions/release/action.yml` is the filled copy of this skill's file.
- [ ] No `{package-dir}` is left in the file.
- [ ] The secret `RELEASE_REGISTRY_TOKEN` exists.
- [ ] The package name is scoped to the repository owner.
