---
name: devops-package-publish
description: The rules every stack's package delivery job follows inside release.yml — the plain version to the public registry from master, a snapshot version to a snapshot registry from develop — with the job itself shipped per stack as a file for assemble-workflow.sh
whenToUse: when a library project must publish its package from `release.yml`, or when you review the `package` job of a project's release workflow
updated: 20261010
tags:
  - stack
  - concern/ci
  - github-actions
  - release
---

# Goal
- A `package` job in `.github/workflows/release.yml`, assembled from the stack extension's job file with its placeholders filled.
- From `master`: the package published as `{version}` to the stack's public registry.
- From `develop`: the package published under a snapshot version to the stack's snapshot registry.
- The registry credentials stored as repository secrets.

# Core Principle
- This skill adds one delivery to [[skills/devops/workflows/devops-github-wf-release.skill/devops-github-wf-release.skill.md|devops-github-wf-release]]; the job waits for the tests and reads the version from the `version` job.
- A snapshot never takes the name a release will have, and never lands where consumers of releases look.
- **Stack extensions** - The job for a stack is in `devops-package-publish-in-python`, `devops-package-publish-in-typescript`, `devops-package-publish-in-dotnet`; a Go module is published by its tag and has none; ask the user which one to load.

# Rule

## MUST

### Assemble the job from the stack's file
Pass the extension's `package-job.yml` to `assemble-workflow.sh` as `package={file}`, then replace every placeholder the extension lists.
- Violation: a publish job written into `release.yml` by hand, or a second workflow that publishes the package.
- Risk: a hand-written job lacks the test gate, and a second workflow publishes code whose tests failed.
- Fix: assemble `release.yml` again with `package=…`.

### Publish the plain version only from master
Publish `{version}` only from `master`, and every other branch's build under the stack's snapshot form of `{version}` and the run's timestamp.
- Violation: a `develop` build published as `1.4.0`.
- Risk: the registry refuses the real `1.4.0` later, or consumers already hold a different `1.4.0`.
- Fix: keep the job's version step as shipped.

### Keep snapshots out of the public registry
Send a snapshot to the snapshot registry the stack's extension names.
- Risk: the public registry fills with timestamped versions nobody may depend on, and none can be deleted.
- Fix: keep the two publish steps and their conditions as shipped.

### Store credentials as secrets
Create the repository secrets the extension lists before the first push.
- Risk: the first release fails at its last step, after the image and the tests.
- Fix: add the secrets in the repository settings; prefer the registry's trusted publishing where the extension offers it.

# Check list
- [ ] `release.yml` has a `package` job equal to the stack's job file with placeholders filled.
- [ ] No other workflow or job publishes the package.
- [ ] The secrets the extension lists exist.
- [ ] A `develop` build is published under a snapshot version, to the snapshot registry.
