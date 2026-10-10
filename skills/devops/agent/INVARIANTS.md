# skills/devops — invariants

The anchor document for reworking the DevOps skills (per [[skills/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). **Draft of 2026-10-10 — not yet reviewed by the owner.** Nothing but this folder is authored until it is. The plan is in `STATUS.md`, the choices in `DECISIONS.md`; `CONTEXT.md` is the hand-over note of the testing work.

**Problem being fixed.** Today's workflows hold logic: path patterns inside `dorny/paths-filter`, version parsing inside composite actions, tag and release-body strings inside `run:` blocks. None of it runs outside GitHub, so a mistake is found only by pushing. Four workflows start on one push to `master`, each reads the version and three of them run the tests again.

## 1. The principle

**A workflow orchestrates; the repository's scripts do the work.** Every step that decides or computes something is one `make` target, runnable on a developer's machine with the same result.

| A workflow may hold | A workflow never holds |
| --- | --- |
| triggers, the job graph, `needs`/`if` on job results and on script outputs, a matrix, `permissions`, `concurrency` | a path pattern, a version file name, a version comparison |
| checkout, installing the stack's toolchain, a dependency cache, passing files between jobs | a test, build, pack or coverage command of a stack |
| actions that need the platform's credentials: registry login, pushing an image or a package, creating a Release, deploying Pages | a tag, a version suffix, a release text, an image name put together in `run:` |
| `run: make {target}` and writing its output to `$GITHUB_OUTPUT` | a `run:` of more than one command; how a service or a database for a test is started |

Checkable: in every workflow template each `run:` is `make …`, alone or redirected to `$GITHUB_OUTPUT`.

## 2. The caller contract

What a workflow knows about a project — and what a developer types. The testing rows are `skills/testing/agent/INVARIANTS.md` §5, unchanged.

| Target | Does | Result |
| --- | --- | --- |
| `make init` | prepares a clean checkout once the toolchain is installed: dependencies, tools, browsers | exit code |
| `make ci-changes` | classifies the files changed since `DELTA_BASE` | stdout, one `{category}=true\|false` line each for `code`, `test`, `ci`, `docker`, `docs` |
| `make version` | prints the project's version | stdout, `MAJOR.MINOR.PATCH` |
| `make version-check` | fails unless the version is strictly greater than the one at `DELTA_BASE` | exit code, the two versions in the message |
| `make ci-publish-version` | the version an artifact of this run is published under | stdout: `{version}` for `RELEASE_CHANNEL=release`, `{version}-{UTC timestamp}` for `snapshot` |
| `make test-kinds`, `test-kind-{kind}`, `test-report`, `test-readme-check`, `test-and-report` | the testing contract | as in testing §5 |
| `make image-build` | builds the Docker image `IMAGE` with `PUBLISH_VERSION` inside | a local image; the workflow pushes it |
| `make package-build` | builds the package under `PUBLISH_VERSION` | `dist/` |
| `make app-build` | builds the release binaries under `PUBLISH_VERSION` | `dist/` |
| `make ci-release-notes` | the text put before the generated release notes | stdout |

Input is environment variables that state facts about the run, never GitHub's own: `DELTA_BASE` (a ref), `RELEASE_CHANNEL` (`release` on `master`, `snapshot` on `develop`), `PUBLISH_VERSION`, `IMAGE`, and the testing variables. A script reads no `GITHUB_*` variable. A project has only the build targets of what it delivers.

## 3. Where the logic lives in a project

- `tools/ci/` — one folder, identical in every project of every stack: `ci.mk` (the targets above), `changes.sh`, `version-check.sh`, `publish-version.sh`. Delivered as assets, copied verbatim.
- `tools/ci/changes.conf` — the path patterns per category; the only part of change detection that differs by stack. Delivered as a template per stack.
- `tools/ci/version.sh` — prints the version; the one script that differs by stack, owned by the project-version skill.
- Categories: `code` — what goes into the artifact, with the manifest and the version source; `test` — tests, features, `tools/testing/**`; `ci` — `.github/**`, `tools/ci/**`, `Makefile`; `docker` — `Dockerfile`, `.dockerignore`; `docs` — `docs/**`, `*.md`. A file is in exactly one category.

## 4. The processes

**Pull request into `develop` or `master`** — `pull-request.yml`

1. `changes`: `make ci-changes`, `DELTA_BASE` = the base branch.
2. `version-check`, only into `master` and when `code` or `docker` changed: `make version-check`.
3. Tests, when `code`, `test` or `ci` changed: `make test-readme-check`, then every kind as `make test-kind-{kind}` with `TEST_RUN_PURPOSE=check`.
4. `image-build` without a push, when the project has a Dockerfile and `code` or `docker` changed.
5. `report`: the one required status; fails when a job failed, a skipped job passes.

**Push to `develop` or `master`** — `release.yml`, one file per repository

1. `changes`: `make ci-changes`, `DELTA_BASE` = the commit before the push.
2. `version`: `make version`, `make ci-publish-version`; both are computed once and handed to every later job.
3. Tests, when `code`, `test` or `ci` changed: every kind with `TEST_RUN_PURPOSE=report`, then `make test-report`. The report is deployed to Pages from `master`; from `develop` it is kept as a run artifact.
4. Delivery, when `code` or `docker` changed and no kind failed — the jobs the project's type has, chosen when the workflow is written, never at run time:
   - Docker service: `make image-build`, push. `master`: `{version}` and `latest`. `develop`: `{version}-{timestamp}`.
   - Library: `make package-build`, publish. `master`: the public registry. `develop`: the snapshot registry.
   - Application: `make app-build`; the binaries are attached to the Release.
5. Release record, `master` only, when the tag `v{version}` does not exist yet: the tag and a GitHub Release with `make ci-release-notes`. Every project type has it.

A change of `docs` alone runs nothing but `changes` and `report`.

## 5. The skills

Gathered under `skills/devops/`, as testing is under `skills/testing/`: `core/` for stack-agnostic skills, `{stack}/` for extensions, `workflows/` for the two workflow skills. Stacks: go, python, typescript, dotnet, angular.

| Skill | Holds | Replaces |
| --- | --- | --- |
| `devops-ci-orchestration` (core) | §1, §2, `tools/ci/` assets | — |
| `devops-project-version` (core) + `-in-{stack}` | where a stack records its version, `version.sh`, how the version reaches the built artifact | `devops-github-action-check-version-in-{stack}` |
| `devops-ci-changes` (core) + `-in-{stack}` | the categories, `changes.sh`; per stack `changes.conf` | `devops-github-action-check-changes-in-{stack}` |
| `devops-package-publish` (core) + `-in-{python,typescript,dotnet}` | `package-build`, the registries | `devops-github-wf-stack-lib-release-publish(-in-{stack})` |
| `devops-app-release-in-go` | `app-build` | `devops-github-wf-release-info-publish-in-go` |
| `devops-github-wf-pull-request` | `pull-request.yml` as a template file | itself |
| `devops-github-wf-release` | `release.yml` with its delivery jobs | `-docker-release-publish`, `-release-info-publish`, `-release-test-report`, `-stack-lib-release-publish` |
| `devops-service-deploy` | unchanged, moved to `deploy/` | — |

Every script, config and workflow is a real file in `assets/` or `templates/` (per [[skills/design/skill-code-delivery.skill/skill-code-delivery.skill.md|skill-code-delivery]]), never YAML in a markdown fence. An extension links its base; a base names its extensions in backticks.

## 6. Ground truth

- Every script runs in `agent/fixtures.sh` against a throw-away git repository: each category, a bumped and an unbumped version, both channels, every stack's `version.sh`.
- Every workflow template is linted (`actionlint`) and passes the `run:` check of §1.
- A green run on GitHub of both workflows for at least one stack. It needs a repository the agent may push to — until then the workflows are reported as unproved.

## 7. Open — the owner decides

See `DECISIONS.md`, entries marked ⚠️.
