---
name: devops-github-wf-release-info-publish-in-go
description: Go implementation of release-info-publish's "Release binaries" extension point — cross-compiles the application to linux/windows/darwin inside the same github-release job, before its softprops/action-gh-release@v2 step, and lists them in that one call's files — only for a Go project whose deliverable is a standalone executable, never a network service
whenToUse: when creating or updating `.github/workflows/release-info-publish.yml` in a Go project whose deliverable is a CLI or desktop application (not a web/API service)
updated: 20260917
tags:
  - stack/go
  - concern/ci
  - github-actions
  - release
---

# Scope
This skill adds Go-specific mechanics on top of [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] — apply both together; this skill only covers what it adds to that shared shape: the "Release binaries" extension point that skill's `# Rule` reserves for a stack whose deliverable includes a standalone executable.

It applies only to a Go project whose deliverable is an executable a user runs locally — a CLI or desktop app. It does not apply to a Go project whose deliverable is a network service (a web/API server): that project has no meaning as a per-OS binary and ships as a container image via [[skills/devops/workflows/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] instead. A project can be both (a CLI that also ships a server mode) and use both skills together. There is no separate `.github/workflows/*.yml` file this skill owns — it only adds steps to the one `release-info-publish.yml` file [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] already defines.

# Goal
- On every push to `master` that bumps the project's version, cross-compile the application for `linux/amd64`, `windows/amd64`, and `darwin/amd64`, inside the same job [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] already runs to create the Release.
- Attach every built binary (plus a checksums file) via the same, single `softprops/action-gh-release@v2` call that job already makes — never a second call, never a second workflow.

# Core Principle
- Go cross-compiles trivially from a single Linux runner — `GOOS=windows GOARCH=amd64 go build` needs no Windows/macOS runner, no matrix, and no per-OS job; a couple of extra steps inside the existing `github-release` job is simpler than a parallel job plus an artifact-download step to reassemble the binaries before upload.
- This skill exists entirely because [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]] reserves exactly this extension point — see that skill's Core Principle on why a GitHub Release has exactly one owning job, and its "Release binaries are built inside this job, never a second workflow" rule. A separate `app-release-publish.yml` workflow with its own trigger/`check-version`/release call was considered and rejected for racing that job's own call to `softprops/action-gh-release@v2` on the same tag.
- `CGO_ENABLED=0` is required for cross-compilation to work at all from a single runner; a project that genuinely needs cgo (rare for a CLI) cannot use this skill as-is and needs native per-OS runners instead.

# Rule

## MUST

### Only for a standalone application, never a web service
Apply this skill only to a Go project whose deliverable is a standalone executable a user downloads and runs locally (a CLI or desktop app) — never to a project whose deliverable is a long-running network service.
- Violation: adding the binary-build steps to `release-info-publish.yml` for a Go web/API service repository "just in case," or building OS/arch binaries for a project that only ever runs inside a container.
- Risk: a web service has no meaning as a `darwin`/`windows` binary a user downloads and double-clicks — the built artifacts are dead weight nobody uses, and the extra build time buys nothing.
- Fix: decide once, when authoring the workflow — a `main.go` producing a CLI/TUI/desktop binary (flag parsing, a `cobra`/`urfave/cli` root command, a GUI toolkit import) gets this skill; a project whose `main.go` starts an HTTP/gRPC listener uses [[skills/devops/workflows/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] instead. A project can have both entry points and use both skills.

### Implement from the linked example, not from prose memory
Open and copy [Release-info-publish workflow example (Go)](./templates/release-info-publish-in-go.example.md) — on top of [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/templates/release-info-publish.example.md|release-info-publish.example.md]]'s own base — before writing or editing the workflow file. Never reconstruct the YAML from prose alone. If a real improvement is needed beyond what the examples show, propose it to the user and get it confirmed before shipping it; once confirmed, fold the fix back into the example file.
- Violation: an agent writes the binary-build steps from memory of this skill's prose without opening the example, and silently drops a mechanical detail (the `CGO_ENABLED=0`, the `.exe` suffix branch, `fail_on_unmatched_files: true`) or silently adds its own fix without flagging it.
- Risk: prose is a summary, not a spec — it cannot carry every quoting/escaping/gating detail a working example encodes; an unflagged improvisation might be correct or might be a workaround for a misunderstanding, and nobody reviewing the PR can tell which without asking.
- Fix: read both examples first, copy them as the starting point, and treat any deviation as a proposal to confirm with the user — not a silent decision.

### Insert the build steps into the existing github-release job — never a new job or a new workflow
Add the binary-build steps directly into [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]]'s own `github-release` job, replacing its `# --- build release binaries ---` placeholder, immediately before the job's existing `softprops/action-gh-release@v2` step.
- Violation: creating a new job in the same workflow file with its own `needs: check-version`, or — worse — a wholly separate `.github/workflows/*.yml` file that also ends in a call to `softprops/action-gh-release@v2` (or any other release-creating action) for the same `v{version}` tag.
- Risk: a second job or workflow calling a release-creating action for the same tag races [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]]'s own call — whichever runs second either overwrites the first's `body`/`generate_release_notes`, or, if careful to omit those, still leaves the Release's final state dependent on unpredictable run order between two unrelated CI runs.
- Fix: one job, one call to `softprops/action-gh-release@v2`, with the Go build steps immediately ahead of it — exactly as shown in [Release-info-publish workflow example (Go)](./templates/release-info-publish-in-go.example.md).

### Cross-compile linux, windows, and darwin — amd64 at minimum
Build at least `linux/amd64`, `windows/amd64`, and `darwin/amd64`, with `CGO_ENABLED=0`, in the same job on the one `ubuntu-latest` runner it already uses.
- Violation: shipping only a `linux` binary, or spinning up separate `windows-latest`/`macos-latest` runners to "natively" build a binary that cross-compiles fine from Linux.
- Risk: a user on Windows or macOS with no `linux` binary to run has no supported way to use the release at all; per-OS native runners would also force splitting the single-job design this skill depends on to avoid the upload race.
- Fix: loop over `linux/amd64 windows/amd64 darwin/amd64` with `GOOS`/`GOARCH` set per iteration and `CGO_ENABLED=0`, exactly as shown in [example](./templates/release-info-publish-in-go.example.md).

### Name each binary with app, version, OS, and arch; suffix .exe on Windows
Name each built artifact `{app-name}_{version}_{goos}_{goarch}`, with a `.exe` suffix only for the `windows` build.
- Violation: all three binaries named `myapp` (only the last one survives being uploaded), or a Windows binary with no `.exe` extension (Windows Explorer/PowerShell will not treat it as executable by default).
- Risk: colliding filenames silently overwrite each other before upload; a missing `.exe` makes the Windows asset non-obviously runnable.
- Fix: `dist/${APP_NAME}_${VERSION}_${goos}_${goarch}${ext}`, exactly as shown in [example](./templates/release-info-publish-in-go.example.md).

### Extend files: on the same release step — never set a separate body
Add `files: dist/*` and `fail_on_unmatched_files: true` to [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]]'s existing `softprops/action-gh-release@v2` step — leave its `tag_name`, `generate_release_notes`, and `body` exactly as that skill already defines them.
- Violation: adding a second `body:`/`generate_release_notes:` alongside the existing one, or building the binaries but forgetting to add `files:` to the one release step (which silently ships a Release with no binaries attached).
- Risk: a competing `body`/`generate_release_notes` value on the same step is simply overwritten by whichever key appears last in the YAML — a confusing, order-dependent way to lose release-note content; omitting `files:` after building the binaries wastes the whole build for nothing.
- Fix: only ever add `files:`/`fail_on_unmatched_files:` to the existing step — never touch its other inputs.

## SHOULD
- Generate a `sha256sum` checksums file alongside the binaries and include it in `files:` too.
- Add `linux/arm64` and `darwin/arm64` once the project has users on those architectures.
- Embed the version into the binary via `-ldflags "-X main.version={version}"` so `{app-name} --version` reports the exact release version.
- Strip debug symbols (`-ldflags "-s -w"`) to shrink the published binaries.

## MAY
- Package each binary into a `.tar.gz`/`.zip` archive instead of publishing raw binaries, once the project needs to ship additional files (a README, a license, shell-completion scripts) alongside the executable.
- Add a Homebrew tap / Scoop manifest / `.deb`/`.rpm` package as additional release assets.

# Example
See [Release-info-publish workflow example (Go)](./templates/release-info-publish-in-go.example.md).

# Check list
- [ ] This skill is applied only for a Go project whose deliverable is a standalone application — never a network service (which uses [[skills/devops/workflows/devops-github-wf-docker-release-publish.skill/devops-github-wf-docker-release-publish.skill.md|devops-github-wf-docker-release-publish]] instead).
- [ ] The binary-build steps were implemented by copying [Release-info-publish workflow example (Go)](./templates/release-info-publish-in-go.example.md) on top of [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/templates/release-info-publish.example.md|release-info-publish.example.md]], not reconstructed from prose; any deviation was confirmed with the user and folded back into the example.
- [ ] The build steps live inside [[skills/devops/workflows/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]]'s own `github-release` job — no new job, no second workflow file, no second call to a release-creating action.
- [ ] Binaries are cross-compiled for at least `linux/amd64`, `windows/amd64`, `darwin/amd64` with `CGO_ENABLED=0`.
- [ ] Each binary is named `{app-name}_{version}_{goos}_{goarch}`, with `.exe` only on the `windows` build.
- [ ] The existing `softprops/action-gh-release@v2` step gained only `files:`/`fail_on_unmatched_files:` — its `tag_name`/`generate_release_notes`/`body` are untouched.
