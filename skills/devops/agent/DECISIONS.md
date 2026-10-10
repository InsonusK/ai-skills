# skills/devops — decisions

One line per choice. ⚠️ = an architectural fork the owner decides; the text after "Proposed" is what `INVARIANTS.md` assumes until then.

## Open forks

- ⚠️ **F1. What a pull request's tests are compared with.** The owner's description says "delta since the last commit". Proposed: the base branch, not the previous commit — a pull request of several commits is otherwise checked one commit at a time, and a regression from an earlier commit passes. The same holds for the version check: compared with `master`'s version.
- ⚠️ **F2. `DELTA_BASE` in a pull request turns on delta mutation testing.** The standing decision (`CONTEXT.md`) is that a pull request passes no `DELTA_BASE`, so the mutation kind skips itself. "Tests on the delta" needs `DELTA_BASE`, and then delta mutation is part of the merge gate. Proposed: keep the standing decision — every kind runs in full as a `check` run, no `DELTA_BASE` for the test kinds; `DELTA_BASE` goes only to `ci-changes` and `version-check`.
- ⚠️ **F3. Which changes demand a version bump.** The owner's description: code, tests, ci. Proposed: `code` and `docker` only — exactly the changes that produce a new artifact on `master`. A test-only or CI-only pull request changes nothing that is shipped, and a bump for it produces a release identical to the previous one. The Dockerfile is in, because its change rebuilds the image under an existing `{version}` tag.
- ⚠️ **F4. One push workflow or several.** Today four workflows start on a push to `master`; tests run in three of them. Proposed: one `release.yml` — changes, version, tests once, then the delivery jobs of the project's type. The owner's two processes (Docker service, application) become two delivery jobs of that one file; a library is the third.
- ⚠️ **F5. Tests on push: `report` run on both branches.** The owner's description: tests + coverage on `master` and `develop`. Proposed: a `report` run on both, Pages deployed only from `master`, the `develop` report kept as a run artifact. Cost: delivery waits for the slowest kind — whole-project mutation testing, about eleven minutes on `gw009-001`. Alternative: `check` run on `develop`, `report` on `master`.
- ⚠️ **F6. A GitHub Release for a Docker service and a library.** The owner's description creates a release only for applications. Proposed: the tag `v{version}` and a Release on `master` for every project type — the tag is what makes "this version is already released" checkable, and a Go module is published by it.
- ⚠️ **F7. `make init` and test services.** Proposed: the workflow installs the toolchain (`actions/setup-{stack}`, the one stack-specific step, kept for its cache) and calls `make init` for everything else; a test kind that needs a database starts it itself — no `services:` block in a workflow. This adds `init` to the caller contract, which the testing contract left open (`CONTEXT.md` gap 5, 6).
- ⚠️ **F8. Location of the stack extensions.** Proposed: `skills/devops/{stack}/`, as testing did, instead of today's `skills/{stack}/devops/`.
- ⚠️ **F9. Proof on GitHub.** No workflow has ever run on GitHub. Proposed: the owner names a repository the agent may push a sample project to; otherwise the hand-off states the workflows as locally linted only.

## Made

- 2026-10-10 Caller interface is `make`, as in the testing contract — one entry point for a developer and for CI.
- 2026-10-10 Change detection moves from `dorny/paths-filter` to `tools/ci/changes.sh` over `git diff --name-only`; category `workflow` is renamed `ci` and covers `tools/ci/**` and the `Makefile`.
- 2026-10-10 `check-version`'s output `publishable` is dropped: whether a project ships a package is decided when its workflow is written, like the Dockerfile.
- 2026-10-10 Scripts read no `GITHUB_*` variable; a workflow passes facts as `DELTA_BASE`, `RELEASE_CHANNEL`.
- 2026-10-10 A pull request builds the image without pushing it when `code` or `docker` changed — today a broken Dockerfile is found only after the merge.
