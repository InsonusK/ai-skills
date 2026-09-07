---
name: verify-current-apis
description: Check every API, option, config key, and default in new code against the version the project actually resolves — installed source, versioned docs, or release notes — instead of trusting model memory that lags the ecosystem
whenToUse: when you write new code or add functionality that calls a framework, library, or platform API — before using any signature, option, config key, or default you are recalling from memory rather than reading from the project's resolved version
updated: 20260906
tags:
  - skill/develop
  - stack
  - concern/coding
---

# Goal
- The resolved version of every framework, library, and platform the new code calls, read from the project's manifests and lockfile before the code is written.
- Every API, option, config key, and default used in new code confirmed against a current authoritative source for that resolved version.
- New code free of APIs deprecated or removed in the resolved version, using the replacement the docs name.
- Every API used without verification marked as an assumption in the change description or an inline comment.

# Core Principle
- **Model memory lags the ecosystem** - Training data has a cutoff and the ecosystem moved on; treat every recalled signature, option name, default value, and package name as possibly stale until confirmed against the version in the project.
- **The resolved version is the source of truth** - Verify against the exact version the project's lockfile resolves, never the latest release or the one you remember.
- **Primary sources first** - The installed package's shipped types and source describe exactly the version in use; reach for versioned official docs and release notes next, and memory never.

# Rule

## MUST

### Read the resolved version first
Before writing code that calls a framework or library, read the project's dependency manifests and lockfile to determine the exact resolved version of each one the code will touch.
- Violation: writing `inject()`-style Angular code without checking whether the lockfile resolves Angular 14 or 19.
- Risk: the code targets an API surface the installed version lacks, or still requires an older pattern, and fails at build or run time.
- Fix: read `package.json` / `*.csproj` / `pyproject.toml` / `go.mod` and the lockfile; record the resolved version of each dependency in play before coding.

### Verify APIs you are not certain are current
Before using any API, option, config key, or default you are recalling from memory rather than reading, confirm it against an authoritative source for the resolved version: the installed package's types or source, the versioned official docs, or the release notes.
- Violation: calling a library function with an options object reconstructed from memory, when one option was renamed two majors before the resolved version.
- Risk: a renamed, removed, or re-defaulted API matches the assumption but not the real package — build breakage, or a silent behavior change from a changed default.
- Fix: open the symbol in `node_modules` / site-packages / the SDK, or the docs for that exact version; confirm the signature, options, and defaults before writing the call.

### Use post-cutoff sources for recent versions
When the resolved version or the specific API was released or changed after your knowledge cutoff, verify it with a web search or documentation fetch rather than memory.
- Risk: the model has no training data for that version, so any recalled detail is a guess presented as fact.
- Fix: search the official docs, changelog, or release notes for the resolved version; base the code on what you read there.

### Never build new code on deprecated APIs
Do not write new code against an API marked deprecated or removed in the resolved version; use the replacement its documentation or deprecation notice names.
- Violation: adding a fresh call to a method whose docstring for the installed version reads "deprecated since X, use Y".
- Risk: new code ships already on a removal path, and its migration cost is incurred the next time the dependency is upgraded.
- Fix: check the deprecation notes for the resolved version and use the current recommended API from the start.

## SHOULD

### Prefer installed source over online docs
When the package is already installed, read its shipped types and source before consulting online docs.
- Risk: published docs describe the latest release, not the version the project resolved, so they can reintroduce the mismatch you are trying to avoid.
- Fix: inspect `node_modules` / site-packages / the referenced assembly; fall back to versioned docs only when the shipped source is unclear.

### Flag assumptions you could not verify
When you cannot reach a source to confirm an API, state the assumption and its risk in the change description or an inline comment instead of presenting the code as verified.
- Risk: a reviewer cannot separate confirmed code from guessed code, so the guess is trusted and ships.
- Fix: note it plainly — "unverified: assumed `X` still accepts `Y`; could not reach docs for version Z".

# Check list
- [ ] The resolved version of every framework/library/platform the new code calls is known from the manifests and lockfile.
- [ ] Every API, option, config key, and default not read directly from the resolved version was confirmed against installed source, versioned docs, or release notes.
- [ ] Anything released or changed after the knowledge cutoff was verified by web search or doc fetch, not memory.
- [ ] No new code uses an API deprecated or removed in the resolved version.
- [ ] APIs that could not be verified are flagged as assumptions in the change description or an inline comment.
