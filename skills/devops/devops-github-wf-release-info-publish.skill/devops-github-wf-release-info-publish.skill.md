---
name: devops-github-wf-release-info-publish
description: Stack-agnostic GitHub Actions workflow that creates the GitHub Release record on master when the version bumped — tag v{version}, generated release notes, plus links to the Docker image and/or package this same push published, built from the known master tag/version convention rather than queried
whenToUse: when you need to create or update `.github/workflows/release-info-publish.yml` to record a GitHub Release whenever the project's version changes on master
updated: 20260915
tags:
  - concern/ci
  - github-actions
  - release
  - stack

---

# Goal
- Create exactly one GitHub Release per version bump on `master`, tagged `v{version}`, with generated release notes.
- When the same `master` push also published a Docker image ([[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]]) or a package ([[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]]), append a link to each into the release body — computed from the known `master` tag convention, never by querying the registry.
- Never create a Release for `develop`, and never create a duplicate Release for a push that did not bump the version.

# Core Principle
- This is the only one of the three release-publish workflows gated on an actual version bump (`needs.check-version.outputs.bumped == 'true'`) — a GitHub Release is inherently a versioned record, unlike the Docker/package artifacts, which republish at the current version on every relevant push regardless of whether that specific push was the one that bumped it.
- This workflow never builds or queries the Docker image or the package it links to. Because [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] always tags a `master` image `ghcr.io/{owner}/{repo}:{version}`, and [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]] always publishes a `master` package under the plain `{version}`, this workflow can construct both URLs directly from `current` and known facts (whether `Dockerfile` is tracked in the repo, `publishable`) — it never waits on or calls into either of those workflows.
- The `Dockerfile`-presence check here is a **step-level** `git ls-files Dockerfile` (after this job's own checkout), never a job-level `hashFiles('Dockerfile')` condition — see [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md#only-create-this-workflow-for-a-project-that-already-has-a-dockerfile|devops-github-wf-docker-release-publish's note]] on why a job-level `hashFiles()` check silently never works (it evaluates before checkout). Unlike that workflow, this one legitimately needs a *runtime* check — this workflow exists regardless of whether the project has a `Dockerfile`, since it always creates the GitHub Release; only the optional Docker line in the release body depends on the file's presence.
- Only `master` triggers this workflow — `develop`'s snapshot builds are disposable by design and never get a Release record.
- Stack-specific parts, if any, are limited to the package-registry URL pattern (PyPI/nuget.org/npmjs.org each shape package URLs differently) — see [# Package link patterns](#package-link-patterns). Everything else in this workflow is generic.
- This workflow's `check-version` job is the exact same job body used in [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] (all four outputs: `current`, `bumped`, `publishable`, `timestamp`), even though `timestamp` goes unused here — one identical job body across every release-publish workflow beats a trimmed-down variant that only exposes what this particular consumer reads. This workflow has no `changes` job at all, unlike [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]]/`stack-lib-release-publish-in-{stack}` — it gates solely on `bumped`, never on what changed.

# Workflow
1. `check-version` job calls `./.github/actions/check-version`; everything below runs only when `github.ref_name == 'master'` and `bumped == 'true'`.
2. `github-release` job builds the release body: always includes the version; appends a Docker link when `git ls-files Dockerfile` (run after checkout) reports the file tracked; appends a package link when `publishable == 'true'`, using [# Package link patterns](#package-link-patterns) for the stack in use.
3. `softprops/action-gh-release@v2` creates the Release, tag `v{version}`, `generate_release_notes: true`, with the assembled body prepended.

# Package link patterns
Built directly from `current` (the version) and `github.repository`/the package name — never queried from the registry, since the tag/version convention is already fully known from [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]]'s own MUST rule that a `master` publish always uses the plain `{version}`, never a suffixed one:
- Python (PyPI): `https://pypi.org/project/{package-name}/{version}/`
- .NET (NuGet): `https://www.nuget.org/packages/{package-name}/{version}`
- TypeScript (npm): `https://www.npmjs.com/package/{package-name}/v/{version}`
- Go: no package link — Go has no `stack-lib-release-publish` implementation; only the Docker link (if any) and the module's own `pkg.go.dev/{module}@v{version}` page apply, and the latter needs no publish step of its own (`pkg.go.dev` indexes the pushed tag automatically).

# Rule

## MUST

### Implement from the linked example, not from prose memory
Open and copy [Release-info-publish workflow example](./templates/release-info-publish.example.md) before writing the workflow file — never reconstruct the YAML from this skill's prose alone. If a real improvement is needed beyond what the example shows, propose it to the user and get it confirmed before shipping it; once confirmed, fold the fix back into the example file.
- Violation: an agent writes the workflow from memory of `# Goal`/`# Core Principle`/`# Rule` without opening the example, and silently drops a mechanical detail the prose only implies, or silently adds its own fix without flagging it.
- Risk: prose is a summary, not a spec — it cannot carry every quoting/escaping/gating detail the working example encodes; an unflagged improvisation might be correct or might be a workaround for a misunderstanding, and nobody reviewing the PR can tell which without asking.
- Fix: read the example first, copy it as the starting point, and treat any deviation as a proposal to confirm with the user — not a silent decision.

### Reuse the shared check-version job body unmodified
Copy the `check-version` job from [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] verbatim, including its unused `timestamp` output — do not trim it down to only `current`/`bumped`/`publishable`.
- Violation: writing a leaner `check-version` job here that drops the `timestamp` output since this workflow never reads it.
- Risk: a trimmed variant is no longer copy-paste identical to the same job in [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]]/`stack-lib-release-publish-in-{stack}`, so a future change to how the version is read has to be re-applied by hand in this file too.
- Fix: keep the full four-output job body exactly as [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] defines it; simply don't reference `timestamp` anywhere in this workflow's own steps.

### Trigger only on push to master, gated on an actual version bump
Trigger on `push` to `master` (plus `workflow_dispatch`); gate `github-release` on `needs.check-version.outputs.bumped == 'true'`.
- Violation: also triggering on `develop`, or dropping the `bumped` gate so every `master` push creates a Release.
- Risk: a Release without a version bump either duplicates an existing tag (the Release step fails) or, worse, is accepted and creates a second, confusing Release for the same version; a `develop` Release defeats the "disposable snapshot" nature of that branch entirely.
- Fix: `on: push: branches: [master]`, `if: needs.check-version.outputs.bumped == 'true'` on `github-release`.

### Compute artifact links from the known tag convention, never by querying
Build the Docker/package links in the release body directly from `current`, `github.repository`, and the project's package name — never by calling the registry's API to check what was actually published.
- Violation: adding a step that calls the GHCR/PyPI/npm/NuGet API to confirm the image/package exists before linking it.
- Risk: querying adds a dependency on registry availability and auth to a workflow that has no other reason to need registry credentials; it also introduces a race, since [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] and [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/devops-github-wf-stack-lib-release-publish.skill.md|devops-github-wf-stack-lib-release-publish]] are separate workflows that may not have finished publishing yet.
- Fix: construct the URL string directly from the version and the tag convention documented in [# Package link patterns](#package-link-patterns) — the link is correct by construction, because both publishing workflows are contractually required to use that exact convention on `master`.

### Lowercase the repository name in the Docker link, matching what was actually pushed
When constructing the Docker link, lowercase `github.repository` with `tr '[:upper:]' '[:lower:]'` exactly as [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] does before pushing — never interpolate `github.repository` as-is into the link.
- Risk: [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] pushes under the lowercased image ref (Docker/OCI requires it); linking the un-lowercased `github.repository` for a repo with any uppercase letter points to an image reference that was never pushed, so the link 404s.
- Fix: lowercase it the same way, as shown in [example](./templates/release-info-publish.example.md).

### Only link artifacts the project actually has
Append the Docker link only when a step-level `git ls-files Dockerfile` (run after this job's own checkout) reports the file tracked; append the package link only when `needs.check-version.outputs.publishable == 'true'`.
- Violation: using a job-level `if: hashFiles('Dockerfile') != ''` for this, or any other job-level condition that depends on repo contents.
- Risk: an unconditional link to a nonexistent image/package leads readers of the Release notes to a 404; a job-level `hashFiles()` check would silently always evaluate as "file doesn't exist" regardless of reality, since job-level `if:` conditions run before that job's own checkout (see [[skills/devops/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md#only-create-this-workflow-for-a-project-that-already-has-a-dockerfile|devops-github-wf-docker-release-publish's note]]).
- Fix: check `Dockerfile`'s presence with a plain shell step (`git ls-files Dockerfile`), after checkout, inside the `github-release` job itself — not as that job's `if:` condition, and not via `hashFiles()`.

### Tag the release v{version}, with generated notes
Create the Release with tag `v{version}` and `generate_release_notes: true`.
- Risk: a hand-written changelog drifts from what actually merged; a tag without the `v` prefix breaks the convention every other stack-specific skill in this catalog assumes when it says "the `v{version}` git tag."
- Fix: use `softprops/action-gh-release@v2` with `tag_name: v${{ needs.check-version.outputs.current }}` and `generate_release_notes: true`.

## SHOULD
- Prepend the artifact links before GitHub's auto-generated notes, so a reader sees "where to get this release" before the commit list.

# Example
See [Release-info-publish workflow example](./templates/release-info-publish.example.md).

# Check list
- [ ] The workflow was implemented by copying [Release-info-publish workflow example](./templates/release-info-publish.example.md), not reconstructed from prose; any deviation was confirmed with the user and folded back into the example.
- [ ] `check-version` is copied from [[skills/devops/devops-github-wf-stack-lib-release-publish.skill/templates/base-jobs.example.md|base-jobs.example.md]] unmodified, including its unused `timestamp` output.
- [ ] The Docker link lowercases `github.repository`, matching the actual pushed tag.
- [ ] The workflow triggers only on `push` to `master`, plus `workflow_dispatch` — never `develop`.
- [ ] `github-release` runs only when `needs.check-version.outputs.bumped == 'true'`.
- [ ] Docker/package links are constructed from `current`/the tag convention, never by querying a registry.
- [ ] The Docker link appears only when a step-level `git ls-files Dockerfile` (after checkout) reports the file tracked — never a job-level `hashFiles()` condition; the package link only when `publishable == 'true'`.
- [ ] The Release is tagged `v{version}` with `generate_release_notes: true`.
