---
name: version-source-file
description: Where a Go project's version, used for release tagging and package-publish gating, is read from
problem: check-version (see [[skills/devops/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]] and [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]]) needs one authoritative, comparable version per commit for every stack it supports. Python reads pyproject.toml, .NET reads Directory.Build.props, TypeScript reads package.json — but go.mod carries no version field at all; a Go module's version is conventionally just a semver git tag chosen at release time, decided by whoever cuts the release rather than recorded anywhere in the tree.
decision: Add a plain-text `VERSION` file at the repository root (e.g. `1.4.0`, no `v` prefix, no newline requirement) as the single source of truth for a Go project's version, read and compared the same way the other three stacks' manifest files are.
tags:
  - stack/go
  - concern/documentation
  - concern/documentation/adr
---

# Problem

[[skills/devops/devops-github-wf-pull-request.skill/devops-github-wf-pull-request.skill.md|devops-github-wf-pull-request]]'s `version-check` job and [[skills/devops/devops-github-wf-release-info-publish.skill/devops-github-wf-release-info-publish.skill.md|devops-github-wf-release-info-publish]]'s `check-version` job both need to read the project's current version from a file in the tree and compare it against the same file at an earlier commit — this is how every other stack's `check-version` companion works (`pyproject.toml`, `Directory.Build.props`, `package.json`). Go has no equivalent: `go.mod` names the module path and its Go language version, never the module's own release version. The conventional Go approach is to let the release version live only as a git tag, chosen at tag-creation time — but that gives `check-version` nothing to diff between two commits before a release is cut, and no way to gate a PR's `version-check` job on "did this PR bump the version," since there is no file to compare.

# Selected variant

**Selected variant:** [[#Root VERSION file (selected)]]
- Matches the other three stacks' pattern (one file, one field, diffable across commits) closely enough that `devops-github-action-check-version-in-go` can reuse the same read/compare logic, and keeps `devops-github-wf-pull-request`'s PR-time version-bump gate meaningful for Go projects too.

# Searched variants

## Root VERSION file (selected)

### Description
A plain-text file, `VERSION`, at the repository root, holding exactly the semantic version (e.g. `1.4.0`).

### Benefits
- Diffable across commits like every other stack's manifest file, so `check-version`'s "compare current vs. base ref" logic is identical in shape across all four stacks.
- Trivial to read (`cat VERSION`) from any tooling, not just Go-aware tooling.
- Keeps the actual release git tag (`v{VERSION}`) derived from, and consistent with, a value that was already reviewed as part of the PR that bumped it.

### Costs
- One more file to maintain that isn't read by `go build`/`go mod` themselves — it only matters to CI.
- A maintainer could still forget to update it, same as any other stack's manifest field.

## Git tag as the only version record

### Description
Never store the version in a file; treat the most recent `vX.Y.Z` git tag as the project's current version, and let a human choose the next tag at release time.

### Benefits
- Matches the most common convention in the wider Go ecosystem — no extra file.
- No risk of the file and the tag disagreeing, since there is only one record.

### Costs
- Nothing in the tree changes when the version bumps, so `devops-github-wf-pull-request`'s `version-check` job has no file diff to gate on — a PR could not be required to "bump the version" the way it can for the other three stacks.
- The release version is decided out-of-band from the PR that ships it, breaking this repo's pattern of every release-relevant fact being reviewed as part of a PR.

## version.go constant

### Description
Store the version as a Go constant (e.g. `const Version = "1.4.0"` in a `version.go` file), parsed with a small regex or `go/ast` in `check-version`.

### Benefits
- The version is available to the compiled binary at runtime (e.g. for a `--version` flag) without extra embedding steps.

### Costs
- Parsing a Go source file for a string constant is more fragile and more code than reading a one-line text file, for a value `check-version` only ever treats as opaque text.
- Ties `check-version`'s implementation to Go source syntax instead of the same trivial file-read every other stack's action already uses.
