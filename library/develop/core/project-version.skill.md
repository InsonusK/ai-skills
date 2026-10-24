---
name: project-version
description: Where an application keeps its one version and how its code reports it — MAJOR.MINOR.PATCH recorded once, in the place its stack's build reads, read by the running code from that same place, overridden only by a snapshot build and never by editing the source; the place and the reader of each stack are in its -in-{stack} extension
whenToUse: when you create a project, when code must report the application's version (a startup log, a version endpoint, a CLI --version), when you add or review a version number anywhere in the source tree, or when a build must give an artifact another version than the recorded one
updated: 20261024
tags:
  - stack
  - concern/coding
  - versioning
---

# Goal
- **One number** - The application's version is written once in the repository; everything that shows it — the running program, the built package, CI — reads that one place.
- **The program says what was released** - A build with no extra step reports the recorded version; only a snapshot build reports another, set at build time.

# Core Principle
- **The stack's build decides the place** - The version lives where the stack's own build already reads it (a manifest, a props file, a source variable), so the built artifact carries it with no step of the project's own.
- **Read, never copy** - Code that reports the version reads it from that place or from what the build derived from it (package metadata, an assembly attribute); it never holds the number itself.
- **Stack extensions** - The place, the reader, and the override of each stack are in `project-version-in-go`, `project-version-in-dotnet`, `project-version-in-python`, `project-version-in-typescript` (also for Angular). `devops-project-version` and its extensions build on them: they read the place this skill names for `make version` and CI.

# Rule

## MUST

### Record MAJOR.MINOR.PATCH once
Record the version as three dot-separated numbers, without a prefix or a suffix, in the one place the stack's extension names.
- Violation: `v1.4.0`, `1.4`, or `1.4.0-rc1` in the source; a `VERSION` file beside the manifest; the number in a README badge typed by hand.
- Risk: two places part on the first forgotten bump, and the program, the package, and the tag report three versions; a suffix in the source collides with the snapshot suffix a publishing step adds.
- Fix: one place; a snapshot build adds its own suffix at build time.

### Report the version by reading the recorded place
Report the running version — in a startup log, a version endpoint, a `--version` flag — through the stack's reader of the recorded place, called where it is reported or from one module that only calls it.
- Violation: `__version__ = "1.4.0"`; `const string Version = "1.4.0"` beside a `<Version>` in the props file; a `version.py` or `version.ts` holding the number.
- Risk: the second number is forgotten on a bump, and the program reports the previous version.
- Fix: the reader of the stack's extension; a module of its own exists only where the stack records the version in code (Go).

### Override only at build time
Give a snapshot build another version through the stack's build-time override, and never commit an edited version source for it.
- Violation: a build step that rewrites the version source and commits it; a release build that injects the version from somewhere else.
- Risk: the rewritten source is committed or cached, and the next release reports a snapshot version; a second mechanism must always agree with the source.
- Fix: the override of the stack's extension, used by a snapshot build only.

# Check list
- [ ] The version is `MAJOR.MINOR.PATCH`, recorded once, in the place of the stack's extension.
- [ ] No other file of the repository holds the number.
- [ ] Code reporting the version reads it through the stack's reader.
- [ ] Only a snapshot build overrides it, at build time.
