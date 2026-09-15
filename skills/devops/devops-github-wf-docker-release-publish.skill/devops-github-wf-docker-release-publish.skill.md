---
name: devops-github-wf-docker-release-publish
description: Stack-agnostic GitHub Actions workflow that builds and pushes the project's Docker image to GHCR on every relevant push to master (tagged version and latest) or develop (tagged version-timestamp only) — the only one of the three release-publish workflows that never needs a stack-specific companion, since Docker build/push is already generic
whenToUse: when you need to create or update `.github/workflows/docker-release-publish.yml` for a project that has a Dockerfile
updated: 20260915
tags:
  - concern/ci
  - github-actions
  - release
  - docker
  - stack

---

# Goal
- Build and push the project's Docker image to `ghcr.io` on every push to `master`/`develop` that actually touched code, the workflow, or the Dockerfile.
- Tag a `master` build with both `{version}` and `latest`; tag a `develop` build with only `{version}-{timestamp}` — a disposable snapshot, never `latest`.
- Skip entirely for a project with no `Dockerfile` — this workflow does not exist for a project that is never built as an image.

# Core Principle
- This workflow is entirely stack-agnostic: `docker/build-push-action` builds whatever `Dockerfile` the project already has, so unlike [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]], it needs no stack-specific companion skill at all.
- It shares its `changes`/`check-version` jobs, unmodified, with every `stack-lib-release-publish-in-{stack}` implementation — see [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]]. This workflow's only own job is `docker-publish`; the two jobs above it are never rewritten per workflow.
- It still calls the same two reusable composite actions every other release workflow uses — `./.github/actions/check-changes` and `./.github/actions/check-version` — never inline path-filter/version logic.
- Gating is purely "did something relevant change" (via `check-changes`) — **not** "did the version bump." A `master` push already went through [[skills/devops/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]]'s `version-check`, which already required the bump before merge; re-checking `bumped` here would be redundant. This also means a `master` push that only touched the Dockerfile (no version bump needed for that) still rebuilds and republishes the image at the current version.
- `master` and `develop` are two different kinds of build: a real, publicly-taggable release image vs. a disposable, uniquely-tagged snapshot. Never let a `develop` build acquire the `latest` tag, and never publish a GitHub Release from this workflow — that belongs to [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]].

# Workflow
1. `changes` job calls `./.github/actions/check-changes`; everything below runs only when `code`, `workflow`, or `docker` changed.
2. `check-version` job calls `./.github/actions/check-version`, producing `current` and a shared UTC `timestamp` (`YYYYMMDDhhmmss`).
3. `docker-publish` job (`if: hashFiles('Dockerfile') != ''`) logs into `ghcr.io`, builds the image, and pushes:
   - on `master`: tags `{version}` and `latest`.
   - on `develop`: tag `{version}-{timestamp}` only.

# Rule

## MUST

### Start from the shared changes/check-version base, unmodified
Copy the `changes` and `check-version` jobs from [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] verbatim; add only the `docker-publish` job on top.
- Violation: pre-combining `check-changes`' raw outputs into a workflow-specific `relevant` boolean inside the `changes` job, or dropping one of `check-version`'s four outputs because this workflow doesn't read all of them.
- Risk: as soon as this workflow's `changes`/`check-version` job diverges from `stack-lib-release-publish-in-{stack}`'s — even a renamed output — a future change to change-detection or version-reading has to be re-applied by hand in every workflow file instead of once.
- Fix: keep both jobs exactly as [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] defines them; read only the specific outputs `docker-publish` needs (`code`/`workflow`/`docker`, `current`, `timestamp`) in its own `if:`/`with:`.

### Trigger on push to both master and develop
Trigger on `push` to `master` and `develop`, plus `workflow_dispatch`.
- Risk: omitting `develop` removes the disposable snapshot build developers rely on to test the image before it merges to `master`.
- Fix: `on: push: branches: [master, develop]`.

### Gate on relevant changes only, never on a version bump
Run `docker-publish` only when `./.github/actions/check-changes` reports `code`, `workflow`, or `docker` changed — never gate it on `check-version`'s `bumped` output.
- Violation: adding `needs.check-version.outputs.bumped == 'true'` to `docker-publish`'s condition.
- Risk: [[skills/devops/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] already required a version bump before this code could reach `master`; re-requiring it here would additionally and incorrectly skip a `master` push that only changed the `Dockerfile` itself (no source-code version bump needed for that), leaving a stale image published under the current tags.
- Fix: gate solely on `check-changes`'s output, exactly as [[skills/devops/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] and [[skills/devops/devops-github-wf-master-release-report.skill/devops-github-wf-master-release-report.skill.md|devops-github-wf-master-release-report]] already do for their own jobs.

### Skip entirely when there is no Dockerfile
Gate `docker-publish` on `hashFiles('Dockerfile') != ''`; a project without one runs `changes`/`check-version` for nothing published.
- Risk: an unconditional build step fails outright on a project that was never meant to ship as an image.
- Fix: keep the `hashFiles` gate; consider dropping this workflow file entirely for a project that will never have a `Dockerfile`.

### Tag master with version and latest; develop with version-timestamp only
Tag every `master` image with both `{version}` and `latest`; tag every `develop` image with `{version}-{timestamp}` and nothing else.
- Violation: a `develop` build pushed as `latest`, or a `master` build missing the floating `latest` tag.
- Risk: `latest` pointing at an unreleased snapshot breaks any consumer that pulls `latest` expecting the current release.
- Fix: branch the tag list on `github.ref_name`, exactly as shown in [example](./templates/docker-release-publish.example.md).

### Compute the timestamp once, from check-version
Compute the shared `YYYYMMDDhhmmss` UTC timestamp inside the `check-version` job's step and reference it from `docker-publish` via `needs.check-version.outputs.timestamp` — never recompute it inside `docker-publish` itself.
- Risk: recomputing the timestamp separately can give the Docker image a different tag than the one [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]]'s package uses for the same commit, breaking traceability between the two artifacts of one push.
- Fix: emit `timestamp` from the shared `check-version` composite action's job and reuse it.

## SHOULD
- Set `concurrency: { group: ${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: true }` so a newer push on the same branch cancels an outdated, still-publishing run.
- Attach build provenance/SBOM (`docker/build-push-action`'s `provenance`/`sbom` inputs) on the `master` build.

# Example
See [Docker-release-publish workflow example](./templates/docker-release-publish.example.md) for the `docker-publish` job. Its `changes`/`check-version` jobs are [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]], copied unmodified.

# Check list
- [ ] The workflow triggers on `push` to both `master` and `develop`, plus `workflow_dispatch`.
- [ ] `changes` and `check-version` are copied from [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] unmodified.
- [ ] `docker-publish` is gated on `check-changes` (`code`/`workflow`/`docker`) — never on `check-version`'s `bumped`.
- [ ] `docker-publish` is gated on `hashFiles('Dockerfile') != ''`.
- [ ] `master` images are tagged both `{version}` and `latest`; `develop` images are tagged only `{version}-{timestamp}`.
- [ ] The timestamp is computed once in `check-version` and reused, never recomputed in `docker-publish`.
- [ ] No GitHub Release is created by this workflow — that is [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]]'s job.
