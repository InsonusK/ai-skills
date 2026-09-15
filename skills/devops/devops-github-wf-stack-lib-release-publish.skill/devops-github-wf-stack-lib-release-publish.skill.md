---
name: devops-github-wf-stack-lib-release-publish
description: Defines the shared shape of the stack-lib-release-publish workflow — publish a package to GitHub Packages (or the closest equivalent) on develop as a version-timestamp snapshot, and to the stack's public registry on master as the plain version, both gated on the project being publishable — with the concrete workflow file written once per stack, since registry auth and publish tooling differ too much per stack to share one workflow file
whenToUse: when a project's `devops-github-action-check-version-in-{stack}` reports it as publishable and needs `.github/workflows/stack-lib-release-publish.yml` to actually publish the package
updated: 20260915
tags:
  - concern/ci
  - github-actions
  - release
  - stack

---

# Scope
This skill states the trigger, gating, and tag/version rules every stack's `stack-lib-release-publish-in-{stack}` workflow must follow. It does not contain a runnable workflow of its own — unlike [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] (which is genuinely stack-agnostic because Docker build/push needs no stack knowledge), package publishing needs real, stack-specific tooling and registry auth in every job, so the actual `.github/workflows/stack-lib-release-publish.yml` is written once per stack as `devops-github-wf-stack-lib-release-publish-in-{stack}` — currently `-in-python`, `-in-dotnet`, `-in-typescript`. **There is no `-in-go` variant**: Go has no package-registry publish step at all; `pkg.go.dev` indexes a pushed `v{version}` git tag automatically, so a Go project only needs [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] and [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]]. Ask the user which stack to load rather than loading all of them.

# Goal
- Publish the project's package on every relevant push to `master` (the real release, plain `{version}`, to the stack's public registry) and `develop` (a disposable snapshot, `{version}-{timestamp}`, to GitHub's own package registry for that ecosystem where one exists).
- Never run for a project `devops-github-action-check-version-in-{stack}` reports as not publishable.
- Keep the trigger/gating/tag rules identical across every stack's implementation — only the registry, auth, and build/pack/publish commands differ.

# Core Principle
- Gating mirrors [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] exactly: relevant-change detection via `./.github/actions/check-changes` (gate on `code`/`workflow`, not `docker`), never a `bumped` gate — [[skills/devops/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] already enforced the version bump before this code reached `master`.
- `publishable` (from `./.github/actions/check-version`) gates the whole workflow — a project that isn't meant to ship a package runs none of this.
- `master` publishes the plain `{version}` to the stack's public registry (PyPI / nuget.org / npmjs.org); `develop` publishes `{version}-{timestamp}` to GitHub's own package registry for that ecosystem (`nuget.pkg.github.com`, `npm.pkg.github.com`) — except Python, whose `develop` target is **TestPyPI**, since GitHub Packages has no PyPI-compatible feed. This asymmetry is deliberate, documented once per stack in its own skill, not silently unified.
- [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] links to whatever this workflow published on `master`, computed from the known `{version}` convention above — it never queries this workflow's result, so every stack implementation must honor the plain-`{version}`-on-`master` rule exactly, with no suffix or variation.

# Rule

## MUST

### Implement from the linked example, not from prose memory
Every `stack-lib-release-publish-in-{stack}` skill is implemented by opening and copying its own linked example — never reconstructed from this skill's or that stack skill's prose alone. If a real improvement is needed beyond what an example shows, propose it to the user and get it confirmed before shipping it; once confirmed, fold the fix back into the example file.
- Violation: an agent writes the workflow from memory of the prose without opening the linked example(s), and silently drops a mechanical detail the prose only implies, or silently adds its own fix without flagging it.
- Risk: prose is a summary, not a spec — it cannot carry every quoting/escaping/gating detail a working example encodes; an unflagged improvisation might be correct or might be a workaround for a misunderstanding, and nobody reviewing the PR can tell which without asking.
- Fix: read the example first, copy it as the starting point, and treat any deviation as a proposal to confirm with the user — not a silent decision.

### Start from the shared changes/check-version base, unmodified
Copy the `changes` and `check-version` jobs from [base-jobs.example.md](./templates/base-jobs.example.md) verbatim into every stack's workflow file — [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] and every `stack-lib-release-publish-in-{stack}` implementation share these two jobs byte-for-byte; only the final job (`docker-publish` vs. `publish`) differs.
- Violation: pre-combining `check-changes`' raw outputs into a workflow-specific `relevant` boolean inside the `changes` job itself, or dropping/renaming one of `check-version`'s four outputs because this particular consumer doesn't need it.
- Risk: as soon as one workflow's `changes`/`check-version` job differs from another's — even by a single renamed output or an extra combined boolean — the two jobs stop being copy-paste identical, and every future change to change-detection or version-reading has to be re-applied by hand in multiple files instead of once.
- Fix: keep both jobs exactly as [base-jobs.example.md](./templates/base-jobs.example.md) defines them, exposing the full, uniform output set; let each consumer's own final job read only the specific outputs it needs in its own `if:`.

### Trigger on push to both master and develop
Every stack's `stack-lib-release-publish-in-{stack}` triggers on `push` to `master` and `develop`, plus `workflow_dispatch`.
- Risk: omitting `develop` removes the disposable snapshot package developers rely on to test integration before merging to `master`.
- Fix: `on: push: branches: [master, develop]` in every stack's implementation.

### Gate the whole workflow on publishable, and each run on relevant changes
Gate every job on `needs.check-version.outputs.publishable == 'true'`; additionally gate the publish job on `./.github/actions/check-changes` reporting `code` or `workflow` changed — never on `bumped`.
- Violation: publishing on every `master`/`develop` push regardless of whether anything relevant changed, or gating on `bumped` the way [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] does.
- Risk: republishing an unchanged package wastes CI time and can hit registry rate limits or duplicate-version rejections; gating on `bumped` would skip a legitimate `develop` snapshot rebuild that has nothing to do with a version bump.
- Fix: `publishable == 'true'` gates the workflow's existence; `code`/`workflow` changed gates each run, exactly as [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] does for the image.

### master publishes the plain version; develop publishes version-timestamp
Publish `master` under exactly `{version}` (plus, where the registry supports it, a `latest` dist-tag — npm does, NuGet/PyPI don't); publish `develop` under exactly `{version}-{timestamp}`, using the same shared timestamp `devops-github-wf-docker-release-publish` computes from `check-version`.
- Violation: suffixing the `master` publish with anything, or publishing a `develop` snapshot under the bare `{version}`.
- Risk: [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] links to the package using the plain `{version}` URL pattern without checking — a suffixed `master` publish breaks that link; an unsuffixed `develop` publish collides with (or shadows) the eventual real release of that version.
- Fix: follow the tag convention exactly; see each stack's own `devops-github-wf-stack-lib-release-publish-in-{stack}` skill for the registry-specific mechanics.

### master targets the public registry; develop targets GitHub's own registry (or TestPyPI for Python)
Publish a `master` build to the stack's public registry; publish a `develop` build to GitHub's own package registry for that ecosystem — except Python, which has no such feed and targets TestPyPI instead.
- Risk: publishing every `develop` snapshot to the public registry pollutes it with disposable, timestamp-tagged versions nobody is meant to depend on.
- Fix: see each stack's own skill for the exact registry URLs and credentials.

### List contents: read explicitly whenever the publish job declares permissions
If a stack's `publish` job needs any job-level `permissions:` block at all (e.g. TypeScript's `id-token: write` for npm provenance), list `contents: read` in that same block, even though `actions/checkout` would otherwise get it from the default token permissions.
- Violation: `permissions: { id-token: write }` with no `contents: read` alongside it.
- Risk: a job-level `permissions:` block *replaces* the default token permissions entirely rather than adding to them — anything not listed becomes `none`. Without `contents: read`, `actions/checkout` in that job fails, and GitHub reports it as "Repository not found" rather than a permissions error, so the real cause is easy to miss.
- Fix: list `contents: read` alongside every other permission the job needs — see `devops-github-wf-stack-lib-release-publish-in-typescript` for the corrected example. A stack whose `publish` job declares no `permissions:` block at all (python, dotnet, currently) is unaffected — this only applies once any block is added.

## SHOULD
- Prefer OIDC/trusted publishing over a long-lived API-token secret where the target registry supports it.

# Example
See [base-jobs.example.md](./templates/base-jobs.example.md) for the shared `changes`/`check-version` jobs every stack's implementation starts from. Each stack's own skill (`devops-github-wf-stack-lib-release-publish-in-{stack}`) shows only the `publish` job it adds on top.

# Check list
- [ ] The workflow was implemented by copying its stack's linked example, not reconstructed from prose; any deviation was confirmed with the user and folded back into the example.
- [ ] The stack's implementation triggers on `push` to both `master` and `develop`, plus `workflow_dispatch`.
- [ ] `changes` and `check-version` are copied from [base-jobs.example.md](./templates/base-jobs.example.md) unmodified — no pre-combined `relevant` output, no dropped `check-version` output.
- [ ] If the `publish` job declares any `permissions:` block, it lists `contents: read` explicitly alongside every other permission.
- [ ] The whole workflow is gated on `publishable == 'true'`; each run is additionally gated on `code`/`workflow` changing — never on `bumped`.
- [ ] `master` publishes the plain `{version}`; `develop` publishes `{version}-{timestamp}` using the shared timestamp.
- [ ] `master` targets the stack's public registry; `develop` targets GitHub's own registry for that ecosystem, or TestPyPI for Python.
- [ ] No `-in-go` implementation exists — Go has no package-publish step.
