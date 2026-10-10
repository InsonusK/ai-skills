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
- `.github/actions/setup-toolchain/action.yml` — one ready file per stack, copied verbatim.
- `.github/actions/release/action.yml` — what the project delivers, one ready file from the skill of that delivery. Inputs `channel` (`check`, `snapshot`, `release`), `version`, `timestamp`, two registry tokens; output `notes`; files for the Release in `dist/release`.
- `.github/workflows/pull-request.yml`, `release.yml` — copied verbatim, the same in every project.

## 4. The processes

**Pull request into `develop` or `master`** — `pull-request.yml`

1. `changes`: `check-changes` — the files the pull request changes against the commit it branched from.
2. `version-check`, only into `master` and when `code` or `docker` changed: `make version-check` against the tip of `master`.
3. `make test-readme-check`, also for a `docs` change. Tests, when `code`, `test` or `ci` changed: every kind in its own parallel job as `make test-kind-{kind}`, `TEST_RUN_PURPOSE=check`, no `DELTA_BASE`.
4. `delivery-check`, when `code` or `docker` changed: the release action with `channel: check` builds what the project delivers and publishes nothing.
5. `report`: the one required status; fails when a job failed, a skipped job passes.

**Push to `develop` or `master`** — `release.yml`, one file per repository

1. `changes`: `check-changes` against the commit before the push.
2. `version`: `make version`, read once and handed to every later job together with one UTC timestamp. On `master` the job fails when `v{version}` exists and `code` or `docker` changed.
3. Tests, when `code`, `test` or `ci` changed, every kind in its own parallel job. `develop`: `TEST_RUN_PURPOSE=check`. `master`: `TEST_RUN_PURPOSE=report`, then `make test-report` and the report with its badges deployed to Pages.
4. `deliver`, when `code` or `docker` changed and no kind failed: the release action, `channel: snapshot` on `develop` and `release` on `master`. A Docker image: `{version}` and `latest`, or `{version}-{timestamp}`. A package: `{version}` to the public registry, or a snapshot version to the snapshot registry. Binaries: attached to the Release.
5. Release record, `master` only, when the tag `v{version}` does not exist yet: the tag and a GitHub Release with generated notes and links to what step 4 published. Every project type has it.

A change of `docs` alone runs nothing but `changes` and `report`. A project whose tests need a database has it in `.devcontainer/docker-compose.yml`; the test jobs start those services from that file before `make test-kind-{kind}`.

## 5. The skills

Gathered under `skills/devops/`, as testing is under `skills/testing/`: `core/` for stack-agnostic skills — the two workflow skills among them — `{stack}/` for extensions, `deploy/` for deployment. Stacks: go, python, typescript, dotnet, angular. A skill under `skills/devops/` links outside it only to `skills/testing/` and `skills/design/`.

| Skill | Holds | Replaces |
| --- | --- | --- |
| `devops-ci-orchestration` (core) | §1 and §2 as rules | — |
| `devops-ci-toolchain` (core) + `-in-{go,python,typescript,dotnet}` | the ready `setup-toolchain` action — the one stack-specific step, so workflow templates are the same for every stack | the `Set up {stack}` placeholder |
| `devops-project-version` (core) + `-in-{go,python,typescript,dotnet}`; Angular uses the TypeScript one | `tools/version/`; per stack where the version is recorded, `read-version.sh`, how the version reaches the built artifact | `devops-github-action-check-version-in-{stack}` |
| `devops-ci-changes` (core) + `-in-{go,python,typescript,angular,dotnet}` | the categories and what each gates; per stack the ready `action.yml` | `devops-github-action-check-changes-in-{stack}` |
| `devops-github-wf-pull-request` (core) | `pull-request.yml` | itself |
| `devops-github-wf-release` (core) | `release.yml`, the contract of the release action, the list of its variants | `-docker-release-publish`, `-release-info-publish`, `-release-test-report`, `-stack-lib-release-publish` |
| `devops-release-docker-image`, `devops-release-tag-only` (core), `devops-release-package-in-{python,typescript,dotnet}`, `devops-release-binaries-in-go` | one release action each; a project takes exactly one | the `docker-publish` job, `devops-github-wf-stack-lib-release-publish-in-{stack}`, `devops-github-wf-release-info-publish-in-go` |
| `devops-service-deploy` | unchanged, moved to `deploy/` | — |

Every script, action and workflow is a real file in `assets/` or `templates/` (per [[skills/design/skill-code-delivery.skill/skill-code-delivery.skill.md|skill-code-delivery]]), never YAML in a markdown fence. An extension links its base; a base names its extensions in backticks.

## 6. Ground truth

- `agent/fixtures.sh` runs `tools/version/` of every stack against a throw-away git repository: the version printed, a raised and an unraised version, a malformed one, no version at the base.
- `agent/check.sh`: links resolve, the §1 check, every stack has every extension, no reference to a removed skill, every workflow passes `actionlint`, every composite action parses and every release action keeps its contract.
- `test/devops/build-sample.sh` builds a sample repository from the testing Go example and every DevOps asset; `test/devops/run-local.sh` runs in it every `make` call the workflows make.
- A green run on GitHub of that sample — not done yet, see `STATUS.md`.

## 7. Open

See `DECISIONS.md`, entries marked ⚠️.
