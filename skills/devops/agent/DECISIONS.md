# skills/devops — decisions

One line per choice. ⚠️ = an architectural fork the owner decides.

## Open forks

- ⚠️ **F2. No `DELTA_BASE` for the test kinds in a pull request.** Not answered in the review of 2026-10-10. `INVARIANTS.md` keeps the standing decision: every kind runs in full as a `check` run and the mutation kind skips itself; a pull request is never gated on mutation testing.
- ⚠️ **F10. What the GitHub experiment may publish.** A real run of `release.yml` in this repository pushes an image to `ghcr.io`, creates a tag and a Release, and deploys Pages. Proposed: the experiment workflows on `develop-devops` use a tag prefix `devops-test-v`, mark the Release as a draft and push only snapshot images; Pages deploy is exercised only if the owner allows it for this repository.

## Decided by the owner, 2026-10-10

- **The principle is narrower than "everything through `make`".** Through `make`: what changes often — tests, test reports, the version. Decided once per stack and copied ready-made: change detection, tags, release name and text.
- **Change detection stays a composite action** on `dorny/paths-filter`: a stack-agnostic skill for what is detected, a stack skill that ships the ready `action.yml`.
- **F1.** A pull request is compared with the commit it branched from. (Agent: the version is compared with the tip of `master` — a branch cut before `master` reached 1.5.0 must not merge as 1.5.0.)
- **F3.** A version bump is demanded only for `code` and `docker` changes.
- **F4.** One push workflow file; test kinds run as parallel jobs.
- **F5.** Coverage and the published report only on `master`; `develop` runs the kinds as a `check` run.
- **F6.** Tag and GitHub Release on `master` for every project type.
- **F7.** A database for tests is started by CI as a neighbouring environment, from the `docker-compose` of `.devcontainer`.
- **F8.** Stack extensions live in `skills/devops/{stack}/`.
- **F9.** Experiments run in this repository on the branch `develop-devops`; their files go under `test/`.
- Forgotten items accepted: the library project type, an image build in a pull request, the report on Pages, the image tags, a docs-only change.

## Made by the agent

- 2026-10-10 Caller interface for the version is `make`, as in the testing contract.
- 2026-10-10 `make init` is part of the caller contract: the workflow installs the toolchain, `make init` does the rest. Proposed with F7, not objected to.
- 2026-10-10 Category `workflow` is renamed `ci` and covers `tools/version/**` and the `Makefile`; `tools/testing/**` is `test`.
- 2026-10-10 `check-version`'s output `publishable` is dropped: whether a project ships a package is decided when its workflow is written, like the Dockerfile.
- 2026-10-10 The version at `DELTA_BASE` is read by running the current `read-version.sh` in a detached worktree of that ref — no stack names its version file twice.
- 2026-10-10 Version format is `MAJOR.MINOR.PATCH` exactly; a pre-release suffix exists only on a published snapshot, added by the workflow.
- 2026-10-10 Angular has no version skill of its own: the root `package.json` records the version of any TypeScript repository, and a workspace package is stamped at build time (ADR `root-package-json-for-workspaces`).
- 2026-10-10 `skill-design` cites `devops-github-action-check-version-in-go` as a violation example of a missing base; the skill is removed and the citation is left for the owner — `check.sh` §8 skips `skills/design/`.
- 2026-10-10 `devops-service-deploy`'s template links (`./templates/...` from a file inside `templates/`) were broken before this work; `check.sh` §1 skips that one file.
- 2026-10-10 Starting test services from `.devcontainer/docker-compose.yml` is written as a rule of `devops-ci-orchestration` and is unverified until a sample with a database runs on GitHub.
- 2026-10-10 `code` is everything no other category claims, minus repository housekeeping — an unclassified file starts tests and a release instead of being skipped. The old filters listed code paths, and a migration or an embedded file changed nothing.
- 2026-10-10 `.devcontainer/**` is `ci`: it holds the compose file the test services start from.
- 2026-10-10 The action passes `base: ${{ github.ref }}` to `dorny/paths-filter`: without it a push to `develop` is compared with the default branch, not with the commit before the push.
- 2026-10-10 The patterns are run locally with the matcher `paths-filter` uses (`picomatch` 2.3.1). The first run failed: `**/Dockerfile` inside braces does not match a root `Dockerfile`. Both forms are listed now.
- 2026-10-10 Angular has its own `devops-ci-changes-in-angular` — `spec/`, `*.spec.ts`, `-e2e` projects — while it shares the TypeScript version skill.
