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
- Never create this workflow file for a project with no `Dockerfile` — it does not exist for a project that is never built as an image, and carries no runtime check for the file's presence.

# Core Principle
- This workflow is entirely stack-agnostic: `docker/build-push-action` builds whatever `Dockerfile` the project already has, so unlike [[skills/devops/workflows/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]], it needs no stack-specific companion skill at all.
- It shares its `changes`/`check-version`/`unit-test` jobs, unmodified, with every `stack-lib-release-publish-in-{stack}` implementation — see [[skills/devops/workflows/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]]. This workflow's only own job is `docker-publish`; the three jobs above it are never rewritten per workflow.
- `unit-test` runs in parallel with `check-version` (both `needs: changes` only), the same shape as [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]]'s `unit-test`/`version-check` pair, so a direct push to `master`/`develop` still can't build and publish an image from code that fails its own tests.
- It still calls the same two reusable composite actions every other release workflow uses — `./.github/actions/check-changes` and `./.github/actions/check-version` — never inline path-filter/version logic.
- Gating is purely "did something relevant change" (via `check-changes`) — **not** "did the version bump." A `master` push already went through [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]]'s `version-check`, which already required the bump before merge; re-checking `bumped` here would be redundant. This also means a `master` push that only touched the Dockerfile (no version bump needed for that) still rebuilds and republishes the image at the current version.
- `master` and `develop` are two different kinds of build: a real, publicly-taggable release image vs. a disposable, uniquely-tagged snapshot. Never let a `develop` build acquire the `latest` tag, and never publish a GitHub Release from this workflow — that belongs to [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]].

# Workflow
1. `changes` job calls `./.github/actions/check-changes`; everything below runs only when `code`, `workflow`, or `docker` changed.
2. `check-version` job calls `./.github/actions/check-version`, producing `current` and a shared UTC `timestamp` (`YYYYMMDDhhmmss`).
3. `unit-test` job runs `make unit-test` in parallel with `check-version` (both `needs: changes` only) when `code` or `test` changed.
4. `docker-publish` job — `needs: [changes, check-version, unit-test]`, `if: always()` gated so a skipped `unit-test` still passes but a failed one blocks it — logs into `ghcr.io`, builds the image, and pushes:
   - on `master`: tags `{version}` and `latest`.
   - on `develop`: tag `{version}-{timestamp}` only.

# Rule

## MUST

### Implement from the linked example, not from prose memory
Open and copy [Docker-release-publish workflow example](./templates/docker-release-publish.example.md) and [[skills/devops/workflows/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] before writing the workflow file — never reconstruct the YAML from this skill's prose alone. If a real improvement is needed beyond what the example shows (a missing edge case, a genuine bug in the example), propose it to the user and get it confirmed before shipping it; once confirmed, fold the fix back into the example file itself so the next agent starts from the corrected version instead of rediscovering the same gap.
- Violation: an agent writes `.github/workflows/docker-release-publish.yml` from memory of this skill's `# Goal`/`# Core Principle`/`# Rule` text without opening `./templates/docker-release-publish.example.md`, and silently drops a mechanical detail the prose only implies (e.g. the multi-line `GITHUB_OUTPUT` heredoc syntax for the `tags` output) — or silently adds its own fix (e.g. lowercasing the image reference) without flagging it.
- Risk: prose is a summary, not a spec — it cannot carry every quoting/escaping/gating detail the working example encodes; reconstructing from memory reliably drops exactly this class of thing. An unflagged improvisation is worse: it might be correct (as the lowercase fix below is) or might be a workaround for a misunderstanding, and nobody reviewing the PR can tell which without asking.
- Fix: read the example file(s) first, copy them as the starting point, and treat any deviation as a proposal to confirm with the user — not a silent decision.

### Lowercase the repository name in the image reference
Compute the image reference as `ghcr.io/$(echo '${{ github.repository }}' | tr '[:upper:]' '[:lower:]')`, never `ghcr.io/${{ github.repository }}` directly.
- Violation: interpolating `github.repository` straight into the image tag.
- Risk: `github.repository` preserves the repo's actual case (e.g. `Org/My-Repo`), but Docker/OCI image references must be all-lowercase — an uppercase letter anywhere in the owner or repo name makes `docker/build-push-action` reject the tag outright, failing the job on any repository whose name isn't already all-lowercase.
- Fix: lowercase it explicitly with `tr '[:upper:]' '[:lower:]'` (or an equivalent), as shown in [example](./templates/docker-release-publish.example.md) — never assume the repository name happens to be lowercase.

### List contents: read explicitly whenever a job declares permissions
`docker-publish` needs `packages: write` (to push to `ghcr.io`); because it declares any job-level `permissions:` block at all, it must also list `contents: read` explicitly in that same block.
- Violation: `permissions: { packages: write }` with no `contents: read` alongside it.
- Risk: a job-level `permissions:` block *replaces* the default token permissions entirely rather than adding to them — anything not listed becomes `none`. Without `contents: read`, `actions/checkout` in that job fails, and GitHub reports it as "Repository not found" rather than a permissions error, so the real cause is easy to miss.
- Fix: list `contents: read` alongside every other permission the job needs, exactly as shown in [example](./templates/docker-release-publish.example.md).

### Start from the shared changes/check-version/unit-test base, unmodified
Copy the `changes`, `check-version`, and `unit-test` jobs from [[skills/devops/workflows/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] verbatim; add only the `docker-publish` job on top.
- Violation: pre-combining `check-changes`' raw outputs into a workflow-specific `relevant` boolean inside the `changes` job, or dropping one of `check-version`'s four outputs because this workflow doesn't read all of them.
- Risk: as soon as this workflow's `changes`/`check-version`/`unit-test` jobs diverge from `stack-lib-release-publish-in-{stack}`'s — even a renamed output — a future change to change-detection, version-reading, or testing has to be re-applied by hand in every workflow file instead of once.
- Fix: keep all three jobs exactly as [[skills/devops/workflows/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] defines them; read only the specific outputs `docker-publish` needs (`code`/`workflow`/`docker`, `current`, `timestamp`) in its own `if:`/`with:`.

### Never publish an image built from code that fails its own tests
Add `unit-test` to `docker-publish`'s `needs`, and gate it with `if: always() && needs.changes.result == 'success' && needs.check-version.result == 'success' && needs.unit-test.result != 'failure' && (...)`, never a plain boolean `if:` with `unit-test` merely listed in `needs`.
- Violation: `needs: [changes, check-version, unit-test]` with the same `if:` as before (no `unit-test.result` check), or `needs.unit-test.result == 'success'` (rejecting a legitimately skipped run).
- Risk: this exists specifically for a direct push to `master`/`develop` that bypassed [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]]'s PR gate — without it, a broken commit pushed straight to a protected branch still builds and publishes an image. But `unit-test` is itself conditionally skipped (no `code`/`test` change); GitHub's default `needs` gating treats a skipped job the same as a failed one, so a plain `if:` with no `always()` would wrongly cascade-skip `docker-publish` on a push that only touched the `Dockerfile` — a regression from today's behavior.
- Fix: use the exact `always()`-based condition in [example](./templates/docker-release-publish.example.md); `!= 'failure'` (not `== 'success'`) is what lets a skipped `unit-test` still pass.

### Trigger on push to both master and develop
Trigger on `push` to `master` and `develop`, plus `workflow_dispatch`.
- Risk: omitting `develop` removes the disposable snapshot build developers rely on to test the image before it merges to `master`.
- Fix: `on: push: branches: [master, develop]`.

### Gate on relevant changes only, never on a version bump
Run `docker-publish` only when `./.github/actions/check-changes` reports `code`, `workflow`, or `docker` changed — never gate it on `check-version`'s `bumped` output.
- Violation: adding `needs.check-version.outputs.bumped == 'true'` to `docker-publish`'s condition.
- Risk: [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] already required a version bump before this code could reach `master`; re-requiring it here would additionally and incorrectly skip a `master` push that only changed the `Dockerfile` itself (no source-code version bump needed for that), leaving a stale image published under the current tags.
- Fix: gate solely on `check-changes`'s output, exactly as [[skills/devops/workflows/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] and [[skills/devops/workflows/devops-github-wf-release-test-report.skill/devops-github-wf-release-test-report.skill.md|devops-github-wf-release-test-report]] already do for their own jobs.

### Only create this workflow for a project that already has a Dockerfile
Treat "does this project have a `Dockerfile`" as a precondition decided once, when the agent creates or reviews `.github/workflows/docker-release-publish.yml` — never as a runtime check inside the workflow itself. A project with no `Dockerfile` simply does not get this workflow file at all.
- Violation: adding a runtime existence check (`hashFiles('Dockerfile') != ''`, or any other check for the file's presence) to `docker-publish`'s `if:` condition, intending to make the workflow a no-op when the project has no `Dockerfile`.
- Risk: `hashFiles()` in a **job-level** `if:` does not work for this — job-level conditions are evaluated before that job runs any steps, including its own `actions/checkout`, so `hashFiles('Dockerfile')` always sees an empty workspace and evaluates as if the file never existed, regardless of whether it actually does. Beyond that bug, a runtime check is also the wrong layer entirely: whether a project builds a Docker image is a one-time, repo-level fact, not something that varies push to push and needs re-deciding on every run.
- Fix: decide it once, when authoring the workflow. If the project has a `Dockerfile`, create this workflow with no existence check at all — `docker-publish` runs whenever `changes` reports something relevant, full stop. If the project has no `Dockerfile`, do not create `docker-release-publish.yml`; if one exists from before the project dropped its `Dockerfile`, delete it.

### Tag master with version and latest; develop with version-timestamp only
Tag every `master` image with both `{version}` and `latest`; tag every `develop` image with `{version}-{timestamp}` and nothing else.
- Violation: a `develop` build pushed as `latest`, or a `master` build missing the floating `latest` tag.
- Risk: `latest` pointing at an unreleased snapshot breaks any consumer that pulls `latest` expecting the current release.
- Fix: branch the tag list on `github.ref_name`, exactly as shown in [example](./templates/docker-release-publish.example.md).

### Compute the timestamp once, from check-version
Compute the shared `YYYYMMDDhhmmss` UTC timestamp inside the `check-version` job's step and reference it from `docker-publish` via `needs.check-version.outputs.timestamp` — never recompute it inside `docker-publish` itself.
- Risk: recomputing the timestamp separately can give the Docker image a different tag than the one [[skills/devops/workflows/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]]'s package uses for the same commit, breaking traceability between the two artifacts of one push.
- Fix: emit `timestamp` from the shared `check-version` composite action's job and reuse it.

## SHOULD
- Set `concurrency: { group: ${{ github.workflow }}-${{ github.ref }}, cancel-in-progress: true }` so a newer push on the same branch cancels an outdated, still-publishing run.
- Attach build provenance/SBOM (`docker/build-push-action`'s `provenance`/`sbom` inputs) on the `master` build.

# Example
See [Docker-release-publish workflow example](./templates/docker-release-publish.example.md) for the `docker-publish` job. Its `changes`/`check-version`/`unit-test` jobs are [[skills/devops/workflows/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]], copied unmodified.

# Check list
- [ ] The workflow was implemented by copying [Docker-release-publish workflow example](./templates/docker-release-publish.example.md)/[[skills/devops/workflows/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]], not reconstructed from prose; any deviation was confirmed with the user and folded back into the example.
- [ ] The image reference is lowercased (`tr '[:upper:]' '[:lower:]'` on `github.repository`), never interpolated as-is.
- [ ] `docker-publish`'s `permissions:` block lists `contents: read` explicitly, alongside `packages: write`.
- [ ] The workflow triggers on `push` to both `master` and `develop`, plus `workflow_dispatch`.
- [ ] `changes`, `check-version`, and `unit-test` are copied from [[skills/devops/workflows/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] unmodified.
- [ ] `docker-publish` is gated on `check-changes` (`code`/`workflow`/`docker`) — never on `check-version`'s `bumped`.
- [ ] `docker-publish` also `needs: unit-test` and uses the `always()`-based condition (`unit-test.result != 'failure'`) — never a plain `if:` that would cascade-skip on a skipped `unit-test`, and never `unit-test.result == 'success'`.
- [ ] `docker-publish` has no runtime `Dockerfile`-existence check (`hashFiles` or otherwise); this workflow file simply does not exist for a project without one.
- [ ] `master` images are tagged both `{version}` and `latest`; `develop` images are tagged only `{version}-{timestamp}`.
- [ ] The timestamp is computed once in `check-version` and reused, never recomputed in `docker-publish`.
- [ ] No GitHub Release is created by this workflow — that is [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]]'s job.
