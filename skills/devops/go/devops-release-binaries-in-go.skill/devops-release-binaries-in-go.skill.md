---
name: devops-release-binaries-in-go
description: Release action for a Go application — cross-compiles every command under cmd/ for linux, windows, and darwin into dist/release, which the workflow attaches to the GitHub Release on master
whenToUse: when a Go command-line or desktop application needs `.github/actions/release/action.yml`, or when you review how its release binaries are built and named
updated: 20261010
tags:
  - stack/go
  - concern/ci
  - github-actions
  - release
---

# Goal
- `.github/actions/release/action.yml` in the project, an unchanged copy of this skill's file.
- Each command of the project in its own folder under `cmd/`.
- From `master`: binaries `{command}_{version}_{os}_{arch}` and a checksums file attached to the GitHub Release.

# Core Principle
- This skill is one of the release actions of [[skills/devops/core/devops-github-wf-release.skill/devops-github-wf-release.skill.md|devops-github-wf-release]]; a project has exactly one, and both workflows call it by its fixed path.
- The binary's version is overridden through `-ldflags`, as [[skills/devops/go/devops-project-version-in-go.skill/devops-project-version-in-go.skill.md|devops-project-version-in-go]] states; a snapshot carries `{version}-{timestamp}`.

# Rule

## MUST

### Copy action.yml verbatim
Copy [[./assets/.github/actions/release/action.yml|action.yml]] verbatim to `.github/actions/release/action.yml`; do not modify it.
- Risk: the workflows pass `channel`, `version`, `timestamp`, and two tokens and read `notes`; an edited action that drops one fails every push.
- Fix: restore the file from the asset; propose a needed change to the user and make it in the skill.

### One folder under cmd per command
Keep the main package of every released command in `cmd/{command}/`.
- Violation: `main.go` at the repository root.
- Risk: the action builds every folder of `cmd/` and finds nothing, so the Release has no binaries.
- Fix: move the main package to `cmd/{command}/`.

### Only for an application
Take this action only when the deliverable is an executable a person downloads.
- Violation: the action in a web service that ships as an image, or in a library module.
- Risk: every release carries binaries nobody runs.
- Fix: a service takes `devops-release-docker-image`, a library `devops-release-tag-only`.

# Check list
- [ ] `.github/actions/release/action.yml` is an unchanged copy of this skill's file.
- [ ] Every released command is a folder under `cmd/`.
- [ ] The project is an application, not a service or a library.
