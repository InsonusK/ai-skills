---
name: devops-release-docker-image
description: Release action for a project that ships a Docker image — builds the image, and pushes it to ghcr.io as {version} and latest from master and as {version}-{timestamp} from develop; in a pull request it only builds
whenToUse: when a project with a `Dockerfile` needs `.github/actions/release/action.yml`, or when you review how its image is tagged and pushed
updated: 20261010
tags:
  - stack
  - concern/ci
  - github-actions
  - release
---

# Goal
- `.github/actions/release/action.yml` in the project, an unchanged copy of this skill's file.
- A `Dockerfile` at the repository root that uses `ARG VERSION`.
- From `master`: `ghcr.io/{owner}/{repo}:{version}` and `:latest`. From `develop`: `:{version}-{timestamp}`. In a pull request: a build, nothing pushed.

# Core Principle
- This skill is one of the release actions of [[skills/devops/core/devops-github-wf-release.skill/devops-github-wf-release.skill.md|devops-github-wf-release]]; a project has exactly one, and both workflows call it by its fixed path.
- `latest` always names a release; a snapshot never takes a tag a release will have.

# Rule

## MUST

### Copy action.yml verbatim
Copy [[./assets/.github/actions/release/action.yml|action.yml]] verbatim to `.github/actions/release/action.yml`; do not modify it.
- Risk: the workflows pass `channel`, `version`, `timestamp`, and two tokens and read `notes`; an edited action that drops one fails every push.
- Fix: restore the file from the asset; propose a needed change to the user and make it in the skill.

### Use ARG VERSION in the Dockerfile
Declare `ARG VERSION` in the `Dockerfile` and pass it into the build of the program the way the stack's `devops-project-version-in-{stack}` skill states.
- Violation: a `Dockerfile` that ignores the build argument.
- Risk: a snapshot image `1.4.0-20261010120000` holds a program that reports `1.4.0`, and two snapshots cannot be told apart.
- Fix: `ARG VERSION=` in the build stage, used by the build command when it is not empty.

### Only for a project with a Dockerfile
Take this action only when the project has a `Dockerfile` at its root.
- Risk: every push and every pull request fails building an image that has no `Dockerfile`.
- Fix: a project that ships no image takes the release action of what it does ship, or `devops-release-tag-only`.

# Check list
- [ ] `.github/actions/release/action.yml` is an unchanged copy of this skill's file.
- [ ] The `Dockerfile` is at the repository root and uses `ARG VERSION`.
- [ ] A pull request that changes the `Dockerfile` builds the image in `Delivery builds`.
