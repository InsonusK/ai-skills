# skills/devops — decisions

One line per choice. ⚠️ = an architectural fork the owner decides.

## Open forks

- ⚠️ **F2. No `DELTA_BASE` for the test kinds in a pull request.** Not answered in the review of 2026-10-10. `INVARIANTS.md` keeps the standing decision: every kind runs in full as a `check` run and the mutation kind skips itself; a pull request is never gated on mutation testing.
- ⚠️ **F10. What the GitHub experiment may publish.** A real run of `release.yml` in this repository pushes an image to `ghcr.io`, creates a tag and a Release, and deploys Pages. Proposed: the experiment workflows on `develop-devops` use a tag prefix `devops-test-v`, mark the Release as a draft and push only snapshot images; Pages deploy is exercised only if the owner allows it for this repository.

- ⚠️ **F11. A push to `master` that would release an existing version fails.** The `version` job stops with "is already released" when `v{version}` exists and `code` or `docker` changed — instead of silently overwriting the image `{version}`. Cost: "re-run all jobs" of a release that already created its tag fails; "re-run failed jobs" works.

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
- 2026-10-10 The toolchain is installed by a ready `setup-toolchain` composite action per stack. With it and `check-changes` behind fixed names, both workflow templates are identical for every stack.
- 2026-10-10 A workflow is written by `assemble-workflow.sh` (a script the agent runs), not copied and trimmed by hand: project types are marked blocks, a stack's `package`/`app` job is a file passed to the script.
- 2026-10-10 `make test-readme-check` is its own job and also runs for a `docs` change — the README is a `docs` file and removing a badge from it would otherwise pass.
- 2026-10-10 A Python snapshot is `{version}.dev{timestamp}`: the old `{version}-{timestamp}` is a PEP 440 post-release and sorts after the release.
- 2026-10-10 The Python package job writes the snapshot version into `pyproject.toml` with `sed` — the one place besides `read-version.sh` that names the version source; the build backend has no command-line override. Exempt in `check.sh` §6.
- 2026-10-10 The `release` job attaches every artifact named `release-*`; a delivery job needs no knowledge of the Release.
- 2026-10-10 Release concurrency: a newer push cancels a running `develop` snapshot, never a running `master` release.
- 2026-10-10 `skill-design` gets the rule "Keep DevOps skills together" and the ADR `devops-skills-in-one-directory` for F8.
- 2026-10-10 The GitHub experiment cannot live in a subfolder of this repository: the path filters and the workflows assume the project at the repository root, and everything under `test/` would be classified as tests. `test/devops/build-sample.sh` builds the sample as a repository root; the plan is to push that tree as the orphan branch `develop-devops` (and `master-devops` for the release path), branch names substituted in the two workflow files.
- 2026-10-10 `skills/typescript/` held only DevOps skills and no longer exists; `test/ai-skills.yaml` drops that subpath.
- 2026-10-10 `check.sh` §12 runs the repository's `aism sync` validation: the first run after W7 found five descriptions that were invalid YAML and one relative link the repointing missed.
