---
name: devops-release-tag-only
description: Release action for a project that delivers no artifact — a Go library module, a repository of documents or skills — so that its release is only the tag v{version} and the GitHub Release the workflow creates
whenToUse: when a project that ships no image, package, or binary needs `.github/actions/release/action.yml`
updated: 20261010
tags:
  - stack
  - concern/ci
  - github-actions
  - release
---

# Goal
- `.github/actions/release/action.yml` in the project, an unchanged copy of this skill's file.
- From `master`: the tag `v{version}` and a GitHub Release, created by the workflow; nothing else is published.

# Core Principle
- This skill is one of the release actions of [[skills/devops/core/devops-github-wf-release.skill/devops-github-wf-release.skill.md|devops-github-wf-release]]; a project has exactly one, and both workflows call it by its fixed path.
- A Go module is published by its tag: the module proxy reads `v{version}`, so a Go library needs no more than this.

# Rule

## MUST

### Copy action.yml verbatim
Copy [[./assets/.github/actions/release/action.yml|action.yml]] verbatim to `.github/actions/release/action.yml`; do not modify it.
- Risk: the workflows pass `channel`, `version`, `timestamp`, and two tokens and read `notes`; an edited action that drops one fails every push.
- Fix: restore the file from the asset; propose a needed change to the user and make it in the skill.

### Only when nothing is delivered
Take this action only when the project ships no image, package, or binary.
- Violation: the action kept in a project that later gained a `Dockerfile`.
- Risk: releases are tagged and nothing is published under them.
- Fix: replace the file with the release action of what the project ships.

# Check list
- [ ] `.github/actions/release/action.yml` is an unchanged copy of this skill's file.
- [ ] The project has no `Dockerfile` and publishes no package or binary.
