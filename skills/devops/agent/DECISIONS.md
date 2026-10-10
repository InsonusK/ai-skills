# skills/devops — decisions

One line per choice. ⚠️ = an architectural fork the owner decides.

## Open forks

None.

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
- **F2** (2026-10-10). Mutation tests are not called when a pull request is checked: they do not block.
- **F10** (2026-10-10). The release workflow is not tried on GitHub; the owner comes back with errors if a real project meets them.
- **F11** (2026-10-10). A push to `master` that would release an existing version fails.
- (2026-10-10) The owner reads the results of GitHub runs himself.
- **Go version source** (2026-10-10). A Go project records its version only as `var Version` in `internal/version/version.go`; the root `VERSION` file is dropped. Changed with it: `devops-project-version-in-go` and its ADR, `solution-go-repository-structure`, six plateau version-file skills and their examples, the Go testing example and its ADR `distribution-version`.
- **Workflow skills in `core/`** (2026-10-10). The two workflow skills are stack-agnostic and live in `skills/devops/core/`; there is no `workflows/` folder. (Agent: `deploy/` stays apart — a project that deploys nothing selects `core` and its stack and leaves it out.)
- **Delivery behind one action** (2026-10-10, the owner's proposal). Both workflows are files copied verbatim; what a project delivers is `.github/actions/release`, taken from one of N release skills. `assemble-workflow.sh`, the marked blocks, `devops-package-publish*`, and `devops-app-release-in-go` are removed.
- **`devops-service-deploy` in `core/`** (2026-10-10). It is stack-agnostic like the rest of `core/`; there is no `deploy/` folder. No stack extension is created: nothing in it differs by stack yet.
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
- 2026-10-10 The GitHub experiment covers `pull-request.yml` only: `test/devops/push-sample.sh` pushes the built sample as the orphan branch `develop-devops` and a branch `devops-sample-change` with one code change; the owner opens the pull request between them.
- 2026-10-10 The release action has three channels — `check`, `snapshot`, `release` — and the pull-request workflow calls it with `check`: a pull request now builds the package or the binaries too, not only the image.
- 2026-10-10 A composite action cannot read secrets or declare permissions, so the `deliver` job passes two generic secrets, `RELEASE_REGISTRY_TOKEN` and `SNAPSHOT_REGISTRY_TOKEN`, and holds `packages: write` and `id-token: write` for every project type.
- 2026-10-10 Release actions read what they need from the project instead of placeholders — the package name from the manifest, the Go commands from `cmd/*`, the module path from `go list -m`. Only the TypeScript one is a template, for `{package-dir}`.
- 2026-10-10 `devops-release-tag-only` exists so that the workflows have an action to call in a project that delivers nothing; it is also the release action of a Go library.
- 2026-10-10 Stack-specific release skills stay in `skills/devops/{stack}/`, not in a `core/release/` folder: a project that selects `skills/devops/core` would otherwise load every stack's release skill. The shared prefix `devops-release-` and the table in `devops-github-wf-release` show them together; `check.sh` keeps that table equal to the skills that exist.
- 2026-10-10 `agent/run-action-step.mjs` prints one `run` step of a composite action with its inputs filled, so that the naming and build steps of a release action run locally.
- 2026-10-10 `devops-service-deploy` is brought to the form of the other skills: rules as headings, the entrypoint as an asset, the Dockerfile lines inline (a fragment of a file the project owns), and the examples as one template folder `deploy-{service-name}.skill.template/` that mirrors the deploy skill a service repository gets.
- 2026-10-10 The deploy templates no longer assume .NET: `ASPNETCORE_ENVIRONMENT`, `ConnectionStrings__Default`, and `CMD ["dotnet", …]` are replaced by neutral sample variables the service replaces with its own.
- 2026-10-10 The secret-file convention is written `NAME_FILE`, not `{NAME}_FILE`: braces are reserved for placeholders, and `deploy-templates-check.sh` fails on a placeholder the skill's table does not list.
- 2026-10-10 The produced deploy skill no longer carries copies of the entrypoint and the Dockerfile lines; they exist once, in the service repository itself.
- 2026-10-10 `agent/deploy-templates-check.sh`: the entrypoint's three cases, the placeholders, every filled manifest parsed, `helm lint` and `helm template` in both migration modes. `docker compose config` and `kubectl apply --dry-run` are not run — neither tool is in this container.
- 2026-10-10 Both workflow files are no longer byte-identical copies: a project writes the test environment of the `test-kind` job — steps that start its services, the job's `env` (the owner's decision; `devops-ci-orchestration/adr/test-environment-in-workflow.md`). The verbatim rule contradicted the rule to start a database from the workflow.
- 2026-10-11 `docs/integration/**` is `code`, not `docs`: the API contracts kept there are embedded and generated from, and as `docs` their change started no test, no version check, and no delivery. Cases added to `changes-cases/common.tsv`.
- 2026-10-11 The `setup-toolchain` action of go, python, and dotnet also installs Node from `tools/livingdoc/package.json`: the living-doc renderer needs Node 22+, and nothing said who installs it on a runner.
- 2026-10-11 The rule on test services says the compose service publishes its port and pins its image tag; a dev container on `network_mode: service:{db}` has neither by default.
