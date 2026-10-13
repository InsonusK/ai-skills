---
name: devops-project-version
description: How a project records its one version and exposes it to developers and CI — `make version` prints it, `make version-check` fails unless it was raised against a base ref — through a shared tools/version/ folder whose only stack-specific file is read-version.sh
whenToUse: when a project needs its version readable by CI or a build, when you add or review `tools/version/` or the `version`/`version-check` make targets, or when a build must put the version into an artifact
updated: 20261010
tags:
  - stack
  - concern/ci
  - versioning
adr:
  - adr/version-behind-make.md
---

# Goal
- `tools/version/version.mk` and `tools/version/version.sh` in the project, unchanged copies of this skill's assets.
- `tools/version/read-version.sh` in the project, the unchanged asset of the stack's extension.
- The project's `Makefile` including `tools/version/version.mk`.
- `make version` printing `MAJOR.MINOR.PATCH`; `make version-check DELTA_BASE={ref}` failing unless that version is greater than the one at the ref.
- Every build that stamps a version taking it from `make version`.

# Core Principle
- A project has one recorded version, in the place its stack's own tooling reads.
- Only `read-version.sh` knows that place; `version.sh`, the `Makefile`, and CI do not.
- **Stack extensions** - The implementation for a stack is described in the stack-specialized extensions `devops-project-version-in-go`, `devops-project-version-in-python`, `devops-project-version-in-typescript` (also for Angular), `devops-project-version-in-dotnet`; ask the user which one to load.
- Decision recorded in [[./adr/version-behind-make.md|version-behind-make]].

# Rule

## MUST

### Copy the shared version tools verbatim
Copy [[./assets/tools/version/version.mk|version.mk]] and [[./assets/tools/version/version.sh|version.sh]] verbatim to `tools/version/`; do not modify them.
- Risk: an edited copy compares versions differently from every other project and is overwritten by the next update.
- Fix: restore both files from the assets; put a stack's difference into `read-version.sh`.

### Include version.mk in the Makefile
Add the line `include tools/version/version.mk` to the project's `Makefile`.
- Violation: `version:` and `version-check:` recipes written in the `Makefile` itself.
- Risk: the targets drift from the contract CI calls.
- Fix: one `include` line; no recipe of these two targets in the `Makefile`.

### Take read-version.sh from the stack's extension
Copy `read-version.sh` from the extension for the project's stack, and never write the place of the version anywhere else.
- Violation: a workflow step or a second script reading the manifest directly.
- Risk: when the version moves, one reader is updated and the other keeps reporting the old place.
- Fix: every reader calls `make version`.

### Record MAJOR.MINOR.PATCH and nothing else
Record the version as three dot-separated numbers without a prefix or a suffix.
- Violation: `v1.4.0`, `1.4`, `1.4.0-rc1` in the version source.
- Risk: `make version` refuses it, and a suffix recorded in the source collides with the snapshot suffix a publishing step adds.
- Fix: record `1.4.0`; a snapshot build appends its own suffix to what `make version` prints.

### Take the version of an artifact from make version
Pass the output of `make -s version` into every build that stamps a version — an image's `VERSION` build argument, a binary's linker flag, a package's version override.
- Violation: a version literal in a `Dockerfile` or a build script.
- Risk: the artifact reports a version other than the one that was checked and tagged.
- Fix: `docker build --build-arg VERSION="$(make -s version)" .`; the stack's extension names the stack's own mechanism.

### Give version-check the branch the change goes into
Set `DELTA_BASE` to the tip of the branch the change is merged into, with that ref present in the checkout.
- Violation: `DELTA_BASE=HEAD~1`, or a shallow checkout that lacks the ref.
- Risk: against the previous commit, a second commit in the same pull request fails the check or a stale branch passes with a version already released; without the ref the check stops with "is not a commit".
- Fix: `make version-check DELTA_BASE=origin/master` on a checkout with full history.

# Check list
- [ ] `tools/version/version.mk` and `tools/version/version.sh` are byte-identical to this skill's assets.
- [ ] `tools/version/read-version.sh` is byte-identical to the stack extension's asset.
- [ ] The `Makefile` has `include tools/version/version.mk` and no own `version`/`version-check` recipe.
- [ ] `make -s version` prints one line, `MAJOR.MINOR.PATCH`.
- [ ] `make version-check DELTA_BASE={ref}` fails for an unchanged version and passes for a raised one.
- [ ] No file but the version source holds the version number; builds take it from `make version`.
