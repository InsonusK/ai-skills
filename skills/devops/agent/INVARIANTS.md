# skills/devops — invariants

The anchor document for reworking the DevOps skills (per [[skills/common-workflow/bulk-authoring-harness.skill/bulk-authoring-harness.skill.md|bulk-authoring-harness]]). Reviewed by the owner 2026-10-10; his answers are folded in and logged in `DECISIONS.md`. The plan is in `STATUS.md`, the choices in `DECISIONS.md`; `CONTEXT.md` is the hand-over note of the testing work.

**Problem being fixed.** How the version is read and compared is written inside a composite action, differently per stack, and runs only on GitHub. The agent retypes every action and workflow from a fenced example. Four workflows start on one push to `master`; each reads the version and three run the tests.

## 1. The principle

**What changes often goes through `make`; what is decided once is copied ready-made.** How a project is tested, how its reports are built and how its version is determined change with the project — a workflow calls them as `make` targets and a developer runs the same targets locally. What is settled once per stack — which paths count as code, how an image is tagged, what a release is called — stays in the workflow or in a composite action, delivered by a skill as a file the agent copies and does not write.

| Through `make` — never spelled out in a workflow | Stays in the workflow or an action, copied from a skill |
| --- | --- |
| running a test kind, coverage, mutation testing, the test report, the badges | change detection: `.github/actions/check-changes`, `dorny/paths-filter` with the stack's patterns |
| the project's version and the check that it was raised | image tags, the snapshot suffix, the release tag and text |
| preparing a checkout after the toolchain is installed (`make init`) | toolchain install, caches, registry login, image build and push, package build and publish, Pages, the Release |
| | starting the services a test needs from `.devcontainer/docker-compose.yml` |

Checkable: no workflow template names a test tool or its CLI, a coverage or mutation switch, or a version source file (`VERSION`, `pyproject.toml`, `package.json`, `Directory.Build.props`).

## 2. The caller contract

What a workflow knows about a project. The testing rows are `skills/testing/agent/INVARIANTS.md` §5, unchanged.

| Target | Does | Result |
| --- | --- | --- |
| `make init` | prepares a clean checkout once the toolchain is installed: dependencies, tools, browsers | exit code |
| `make version` | prints the project's version | stdout, `MAJOR.MINOR.PATCH` |
| `make version-check` | fails unless the version is strictly greater than the one at `DELTA_BASE` | exit code, both versions in the message |
| `make test-kinds`, `test-kind-{kind}`, `test-report`, `test-readme-check`, `test-and-report` | the testing contract | as in testing §5 |

Input is environment variables that state facts about the run: `DELTA_BASE` and the testing variables. A script reads no `GITHUB_*` variable.

## 3. Where the logic lives in a project

- `tools/version/` — `version.mk` and `version.sh`, identical in every project of every stack, and `read-version.sh`, the one script that differs by stack. Assets of the project-version skill, copied verbatim; the project's `Makefile` includes `version.mk` as it includes `tools/testing/testing.mk`.
- `.github/actions/check-changes/action.yml` — one ready file per stack, copied verbatim. Outputs `code`, `test`, `ci`, `docker`, `docs`: `code` — what goes into the artifact, with the manifest and the version source; `test` — tests, features, `tools/testing/**`; `ci` — `.github/**`, `tools/version/**`, `Makefile`; `docker` — `Dockerfile`, `.dockerignore`; `docs` — `docs/**`, `*.md`. A file is in exactly one category.
- `.github/workflows/pull-request.yml`, `release.yml` — templates; what is filled in is listed in the workflow skill.

## 4. The processes

**Pull request into `develop` or `master`** — `pull-request.yml`

1. `changes`: `check-changes` — the files the pull request changes against the commit it branched from.
2. `version-check`, only into `master` and when `code` or `docker` changed: `make version-check` against the tip of `master`.
3. Tests, when `code`, `test` or `ci` changed: `make test-readme-check`, then every kind in its own parallel job as `make test-kind-{kind}`, `TEST_RUN_PURPOSE=check`, no `DELTA_BASE`.
4. Image build without a push, when the project has a Dockerfile and `code` or `docker` changed.
5. `report`: the one required status; fails when a job failed, a skipped job passes.

**Push to `develop` or `master`** — `release.yml`, one file per repository

1. `changes`: `check-changes` against the commit before the push.
2. `version`: `make version`, read once and handed to every later job together with one UTC timestamp.
3. Tests, when `code`, `test` or `ci` changed, every kind in its own parallel job. `develop`: `TEST_RUN_PURPOSE=check`. `master`: `TEST_RUN_PURPOSE=report`, then `make test-report` and the report with its badges deployed to Pages.
4. Delivery, when `code` or `docker` changed and no kind failed — the jobs of the project's type, chosen when the workflow is written, never at run time:
   - Docker service: build and push the image. `master`: `{version}` and `latest`. `develop`: `{version}-{timestamp}`.
   - Library: build and publish the package. `master`: `{version}` to the public registry. `develop`: `{version}-{timestamp}` to the snapshot registry.
   - Application: build the binaries; on `master` they are attached to the Release.
5. Release record, `master` only, when the tag `v{version}` does not exist yet: the tag and a GitHub Release with generated notes and links to what step 4 published. Every project type has it.

A change of `docs` alone runs nothing but `changes` and `report`. A project whose tests need a database has it in `.devcontainer/docker-compose.yml`; the test jobs start those services from that file before `make test-kind-{kind}`.

## 5. The skills

Gathered under `skills/devops/`, as testing is under `skills/testing/`: `core/` for stack-agnostic skills, `{stack}/` for extensions, `workflows/`, `deploy/`. Stacks: go, python, typescript, dotnet, angular. A skill under `skills/devops/` links outside it only to `skills/testing/` and `skills/design/`.

| Skill | Holds | Replaces |
| --- | --- | --- |
| `devops-ci-orchestration` (core) | §1 and §2 as rules | — |
| `devops-project-version` (core) + `-in-{stack}` | `tools/version/`; per stack where the version is recorded, `read-version.sh`, how the version reaches the built artifact | `devops-github-action-check-version-in-{stack}` |
| `devops-ci-changes` (core) + `-in-{stack}` | the categories and what each gates; per stack the ready `action.yml` | `devops-github-action-check-changes-in-{stack}` |
| `devops-package-publish` (core) + `-in-{python,typescript,dotnet}` | the library delivery job | `devops-github-wf-stack-lib-release-publish(-in-{stack})` |
| `devops-app-release-in-go` | the application delivery job | `devops-github-wf-release-info-publish-in-go` |
| `devops-github-wf-pull-request` | `pull-request.yml` | itself |
| `devops-github-wf-release` | `release.yml`, the Docker delivery job, the release record | `-docker-release-publish`, `-release-info-publish`, `-release-test-report`, `-stack-lib-release-publish` |
| `devops-service-deploy` | unchanged, moved to `deploy/` | — |

Every script, action and workflow is a real file in `assets/` or `templates/` (per [[skills/design/skill-code-delivery.skill/skill-code-delivery.skill.md|skill-code-delivery]]), never YAML in a markdown fence. An extension links its base; a base names its extensions in backticks.

## 6. Ground truth

- `agent/fixtures.sh` runs `tools/version/` of every stack against a throw-away git repository: the version printed, a raised and an unraised version, a malformed one, no version at the base.
- `agent/check.sh`: links resolve, the §1 check, every stack has every extension, no reference to a removed skill, every workflow and action passes `actionlint`.
- A green run on GitHub: sample projects under `test/devops/` on the branch `develop-devops` of this repository, with the workflows pointed at them.

## 7. Open

See `DECISIONS.md`, entries marked ⚠️.
