---
name: devops-app-release-in-go
description: The app job of release.yml for a Go application — cross-compiled linux, windows, and darwin binaries with the version injected, uploaded as the release-binaries artifact the release job attaches to the GitHub Release
whenToUse: when a Go command-line or desktop application must ship release binaries from `release.yml`, or when you review the `app` job of a Go project
updated: 20261010
tags:
  - stack/go
  - concern/ci
  - github-actions
  - release
---

# Goal
- The `app` job of `.github/workflows/release.yml` assembled from this skill's job file, placeholders filled.
- Binaries `{app}_{version}_{os}_{arch}` for linux, windows, and darwin, with a checksums file, attached to the Release on `master`.

# Core Principle
- This skill adds one delivery to [[skills/devops/workflows/devops-github-wf-release.skill/devops-github-wf-release.skill.md|devops-github-wf-release]]; the Release itself is created by that workflow's `release` job.
- The binary gets its version as [[skills/devops/go/devops-project-version-in-go.skill/devops-project-version-in-go.skill.md|devops-project-version-in-go]] states — through `-ldflags`, from the `version` job.

# Rule

## MUST

### Assemble with this job file
Pass [[./templates/app-job.yml|app-job.yml]] to `assemble-workflow.sh` as `app=`, then replace `{app-name}` with the binary's name, `{module-path}` with the module path of `go.mod`, and `{main-package}` with the main package, such as `./cmd/{app-name}`.
- Risk: a leftover placeholder fails the build on the first push.
- Fix: search `release.yml` for `{app-name}`, `{module-path}`, `{main-package}`; none may remain.

### Only for an application
Add the job only to a project whose deliverable is an executable a person downloads.
- Violation: the job added to a web service that ships as an image, or to a library module.
- Risk: every release carries binaries nobody runs, and a library's Release suggests there is something to install.
- Fix: assemble a service with `docker` and a library with neither.

### Upload under the release- prefix
Keep the artifact name `release-binaries`.
- Risk: the `release` job attaches only `release-*` artifacts; another name publishes a Release without binaries.
- Fix: restore the upload step of the job file.

# Check list
- [ ] `release.yml` holds the `app` job of this skill's file; no placeholder is left.
- [ ] The project is an application, not a service or a library.
- [ ] A `master` release lists five binaries and a checksums file.
