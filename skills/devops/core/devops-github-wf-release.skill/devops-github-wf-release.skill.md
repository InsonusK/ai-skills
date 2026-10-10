---
name: devops-github-wf-release
description: Stack-agnostic GitHub Actions workflow for a push to develop or master — one file, the same in every project, that detects changes, reads the version once, runs every test kind in parallel, publishes the test report from master, calls the project's release action to deliver, and records the GitHub Release; also the contract of that release action and the list of its variants
whenToUse: when you create or update `.github/workflows/release.yml`, choose or change what a project delivers on release (Docker image, package, binaries, nothing), or set up the published test report and its README badges
updated: 20261010
tags:
  - stack
  - concern/ci
  - concern/testing
  - github-actions
  - release
  - docker
  - github-pages
adr:
  - adr/one-release-workflow.md
---

# Goal
- `.github/workflows/release.yml`, a copy of this skill's asset that differs from it only in the test environment of the `test-kind` job.
- `.github/actions/release/action.yml` taken from exactly one skill of [[#Take one release action]].
- No other workflow that starts on a push to `develop` or `master`.
- **Per branch** - On `develop`: a snapshot of what the project delivers. On `master`: the delivery as `{version}`, the test report on GitHub Pages, the tag `v{version}` with a GitHub Release.
- The README carrying the workflow badge, the report link, and one badge per declared test badge.

# Core Principle
- **One push, one workflow** - Changes, the version, and the tests are computed once and every delivery waits for them. Decision recorded in [[./adr/one-release-workflow.md|one-release-workflow]].
- The workflow follows [[skills/devops/core/devops-ci-orchestration.skill/devops-ci-orchestration.skill.md|devops-ci-orchestration]] and starts jobs by the categories of [[skills/devops/core/devops-ci-changes.skill.md|devops-ci-changes]].
- **Delivery behind one action** - What a project delivers is decided once, by which release action it takes; the workflow calls `./.github/actions/release` and never looks for a `Dockerfile` at run time.
- A version is released once: the tag `v{version}` is what says it was.

# Workflow
1. `changes` — `./.github/actions/check-changes` against the commit before the push; a manual run counts as a change of `code`.
2. `version` — `make version` and one UTC timestamp; on `master` the job fails when `v{version}` exists and `code` or `docker` changed.
3. `test-kinds`, `test-kind` — when `code`, `test`, or `ci` changed: one parallel job per kind, `TEST_RUN_PURPOSE` `check` on `develop` and `report` on `master`.
4. `test-report`, `pages` — `master` only, also after a failed kind: `make test-report`, deployed to GitHub Pages under `/testing/`.
5. `deliver` — when `code` or `docker` changed and no test job failed: `./.github/actions/release` with `channel` `snapshot` on `develop` and `release` on `master`.
6. `release` — `master` only: the tag `v{version}`, generated notes preceded by the action's `notes`, and the files the action left in `dist/release` attached.

# Rule

## MUST

### Change only the test environment
Copy [[./assets/.github/workflows/release.yml|release.yml]] to `.github/workflows/release.yml` and change nothing in it but the test environment of the `test-kind` job: steps between `make init` and the test step that start what the tests need, and `env` entries of that job, per [[skills/devops/core/devops-ci-orchestration.skill/devops-ci-orchestration.skill.md#Start test services in the test job|devops-ci-orchestration]].
- Violation: a publishing step for the project's registry typed into the workflow; a trigger, a job condition, or a `make` call changed.
- Risk: the file stops being the one every project has, and a fix in the skill no longer applies to it.
- Fix: restore the file and re-add the test environment; delivery belongs in the release action.

### Take one release action
Decide what the project delivers and apply the one skill of this table that matches; ask the user when it is not evident.

| The project delivers | Skill | Stack |
| --- | --- | --- |
| a Docker image | `devops-release-docker-image` | any |
| a Python package | `devops-release-package-in-python` | Python |
| an npm package | `devops-release-package-in-typescript` | TypeScript, Angular |
| NuGet packages | `devops-release-package-in-dotnet` | .NET |
| executables a person downloads | `devops-release-binaries-in-go` | Go |
| nothing but its tag — a Go library, documents | `devops-release-tag-only` | any |

- Violation: two release actions merged by hand, or none taken.
- Risk: the workflows call `./.github/actions/release`; without the file every push and every pull request fails.
- Fix: one skill of the table; a project that must deliver two things is raised with the user.

### Keep the contract of the release action
Keep every release action to this contract: inputs `channel` (`check` builds and publishes nothing, `snapshot`, `release`), `version`, `timestamp`, `registry-token`, `snapshot-registry-token`; output `notes`; files for the GitHub Release in `dist/release`.
- Violation: an action that reads `github.ref_name` to decide what to publish, or publishes on `check`.
- Risk: a pull request publishes, or the workflow's `channel` and the action's own reading of the branch disagree.
- Fix: decide only by `inputs.channel`.

### One workflow on push
Keep `release.yml` the only workflow that starts on a push to `develop` or `master`.
- Violation: a separate `docker-publish.yml` or `test-report.yml` beside it.
- Risk: the tests run once per workflow, and two workflows race to create the same Release.
- Fix: put the delivery into the release action.

### Provide what the workflow calls
Before the first run, apply [[skills/devops/core/devops-ci-changes.skill.md|devops-ci-changes]], [[skills/devops/core/devops-ci-toolchain.skill.md|devops-ci-toolchain]], [[skills/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]], and [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]], and give the `Makefile` an `init` target.
- Risk: the first push fails on a missing action or target.
- Fix: run `make init`, `make -s version`, `make test-kinds`, and `make test-and-report` locally.

### Enable GitHub Pages from Actions
Set the repository's Settings → Pages → Source to "GitHub Actions" before the first push to `master`.
- Risk: the `pages` job fails on every release.
- Fix: switch the source once in the repository settings.

### Add the badges to the README
Fill and copy [[./templates/readme-badges.md|readme-badges.md]] into the README: `{org}` and `{repo}` are the repository's owner and name, and the last line is repeated with `{name}` replaced by each badge `make test-kinds` prints.
- Violation: a coverage number typed into a static badge.
- Risk: the badge shows a number nothing updates, and `make test-readme-check` fails the pull request for a missing declared badge.
- Fix: endpoint badges reading `badges/{name}.json` of the published report.

### Raise the version instead of removing the guard
When `version` fails with "is already released", raise the version in a pull request.
- Violation: the step `Refuse to release a version twice` deleted, or the tag removed to make the run pass.
- Risk: the image `{version}` is overwritten with different contents, and consumers of a released version get code they never tested.
- Fix: a pull request that raises the version; after a failed release, re-run only the failed jobs.

# Check list
- [ ] `.github/workflows/release.yml` differs from this skill's asset only in the test environment of the `test-kind` job.
- [ ] `.github/actions/release/action.yml` comes from exactly one skill of [[#Take one release action]].
- [ ] No other workflow triggers on a push to `develop` or `master`.
- [ ] `check-changes`, `setup-toolchain`, `tools/version/`, the testing targets, and `make init` exist and run locally.
- [ ] GitHub Pages source is "GitHub Actions"; the README has the three kinds of badge lines.
- [ ] No test kind, tool, or version file is named in the workflow.
