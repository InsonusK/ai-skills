---
name: make-for-what-changes
description: Which parts of a CI process go behind a make target and which stay in the workflow file
problem: A CI workflow that holds its own logic can be tried only by pushing. Which steps should be moved behind commands a developer runs locally, and which may stay in the workflow?
decision: Testing, test reports, and the project's version go through make targets; change detection, tags, and release text stay in the workflow or a composite action, delivered by a skill as a ready file.
tags:
  - stack
  - concern/ci
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The workflows described how tests run, parsed the version inside a composite action per stack, and built tags in `run:` blocks. A mistake in any of them showed only after a push. Which steps are worth moving behind a locally runnable command?

# Selected variant
[[#Make for what changes, ready files for what is decided once]]
- Decided by the owner on 2026-10-10: the goal is local testing of what is fragile, not `make` everywhere.

# Searched variants

## Make for what changes, ready files for what is decided once

**Selected.**

### Description
How a project is tested, how its reports are built, and how its version is determined are `make` targets. Change detection, image tags, the snapshot suffix, and the release tag and text are written once per stack and shipped by a skill as a file the agent copies.

### Benefits
- The steps that break when a project changes are run locally before a push.
- The workflow keeps platform features that have no local counterpart — path filters on a pull request's file list, registry actions.
- The agent copies the decided-once parts instead of writing them.

### Costs
- A mistake in a decided-once part is still found only on the platform; it is found once, in the skill's file.
- Two places to look: `make` targets and the workflow.

## Everything through make

### Description
Every step that decides or computes is a `make` target, change detection and tag strings included; a workflow holds only triggers, the job graph, and credentials.

### Benefits
- One rule with a mechanical check: every `run:` is `make`.
- The whole process can be replayed locally.

### Costs
- Change detection must be rewritten over `git diff`, and a release text computed locally depends on platform facts a local repository does not have.
- More scripts copied into every project for logic that does not change.

## Everything in the workflow

### Description
The workflow and its composite actions hold all logic, per stack.

### Benefits
- No scripts in the project.

### Costs
- Nothing can be tried without a push.
- Every stack has its own version-comparing action in its own language.
