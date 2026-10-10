---
name: devops-github-wf-release
description: Stack-agnostic GitHub Actions workflow for a push to develop or master — one file that detects changes, reads the version once, runs every test kind in parallel, publishes the test report from master, delivers what the project's type ships (Docker image, package, release binaries), and records the GitHub Release
whenToUse: when you create or update `.github/workflows/release.yml`, add a delivery (Docker image, package, binaries) to a project, or set up the published test report and its README badges
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
- `.github/workflows/release.yml` written by `assemble-workflow.sh` from this skill's template, with the delivery jobs of the project's types.
- No other workflow that starts on a push to `develop` or `master`.
- **Per branch** - On `develop`: a snapshot `{version}-{timestamp}` of each delivery. On `master`: the delivery as `{version}`, the test report on GitHub Pages, the tag `v{version}` with a GitHub Release.
- The README carrying the workflow badge, the report link, and one badge per declared test badge.

# Core Principle
- **One push, one workflow** - Changes, the version, and the tests are computed once and every delivery waits for them. Decision recorded in [[./adr/one-release-workflow.md|one-release-workflow]].
- The workflow follows [[skills/devops/core/devops-ci-orchestration.skill/devops-ci-orchestration.skill.md|devops-ci-orchestration]] and starts jobs by the categories of [[skills/devops/core/devops-ci-changes.skill.md|devops-ci-changes]].
- **Type decided when written** - Whether a project ships an image, a package, or binaries is decided when the file is assembled; the workflow never looks for a `Dockerfile` at run time.
- A version is released once: the tag `v{version}` is what says it was.
- **Delivery jobs per stack** - The `package` job is in `devops-package-publish-in-python`, `devops-package-publish-in-typescript`, `devops-package-publish-in-dotnet`, the `app` job in `devops-app-release-in-go`; ask the user which one to load.

# Workflow
1. `changes` — `./.github/actions/check-changes` against the commit before the push; a manual run counts as a change of `code`.
2. `version` — `make version` and one UTC timestamp; `publish` is `{version}` on `master` and `{version}-{timestamp}` on `develop`; on `master` the job fails when `v{version}` exists and `code` or `docker` changed.
3. `test-kinds`, `test-kind` — when `code`, `test`, or `ci` changed: one parallel job per kind, `TEST_RUN_PURPOSE` `check` on `develop` and `report` on `master`.
4. `test-report`, `pages` — `master` only, also after a failed kind: `make test-report`, deployed to GitHub Pages under `/testing/`.
5. `image`, `package`, `app` — the project's types, when `code` or `docker` changed and no test job failed.
6. `release` — `master` only: the tag `v{version}`, generated notes, links to the image and the package, and every `release-*` artifact attached.

# Rule

## MUST

### Assemble the workflow with the script
Write the file with [[skills/devops/core/devops-ci-orchestration.skill/scripts/assemble-workflow.sh|assemble-workflow.sh]] from [[./templates/release.yml|release.yml]], naming each type the project ships — `docker`, `package={job file}`, `app={job file}` — then fill the placeholders the job file's skill lists.
```bash
sh assemble-workflow.sh templates/release.yml docker package=package-job.yml > .github/workflows/release.yml
```
- Violation: a hand copy with the `# package` lines left in, or a `docker` job kept "for later" in a project without a `Dockerfile`.
- Risk: `release` needs a job that does not exist and the workflow is rejected, or every push fails building an image that has no `Dockerfile`.
- Fix: run the script with exactly the project's types; run it again when a type is added.

### One workflow on push
Keep `release.yml` the only workflow that starts on a push to `develop` or `master`.
- Violation: a separate `docker-publish.yml` or `test-report.yml` beside it.
- Risk: the tests run once per workflow, and two workflows race to create the same Release.
- Fix: add the delivery as a type of this workflow.

### Provide what the workflow calls
Before the first run, apply [[skills/devops/core/devops-ci-changes.skill.md|devops-ci-changes]], [[skills/devops/core/devops-ci-toolchain.skill.md|devops-ci-toolchain]], [[skills/devops/core/devops-project-version.skill/devops-project-version.skill.md|devops-project-version]], and [[skills/testing/core/solution-conformance-testing.skill/solution-conformance-testing.skill.md|solution-conformance-testing]], and give the `Makefile` an `init` target.
- Risk: the first push fails on a missing action or target.
- Fix: run `make init`, `make -s version`, `make test-kinds`, and `make test-and-report` locally.

### Declare ARG VERSION in the Dockerfile
Declare `ARG VERSION` in the `Dockerfile` of a project assembled with `docker`, and pass it into the build of the program.
- Violation: a `Dockerfile` that ignores the build argument.
- Risk: the image tagged `1.4.0` holds a program that reports `dev`.
- Fix: `ARG VERSION=dev` in the build stage, used the way the stack's `devops-project-version-in-{stack}` skill states.

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
- [ ] `.github/workflows/release.yml` equals the output of `assemble-workflow.sh` for the project's types, placeholders filled.
- [ ] No other workflow triggers on a push to `develop` or `master`.
- [ ] `check-changes`, `setup-toolchain`, `tools/version/`, the testing targets, and `make init` exist and run locally.
- [ ] A project assembled with `docker` has a `Dockerfile` that uses `ARG VERSION`.
- [ ] GitHub Pages source is "GitHub Actions"; the README has the three kinds of badge lines.
- [ ] No test kind, tool, or version file is named in the workflow.
