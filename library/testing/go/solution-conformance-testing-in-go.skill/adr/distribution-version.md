---
name: distribution-version
description: Version metadata for the runnable Go showcase
problem: A local Go main module has no installed-package version API equivalent to Python distribution metadata.
decision: Compare the package's public Version with the version recorded in internal/version/version.go, the one place a Go project records it.
tags:
  - solution/conformance-testing-in-go
  - stack/go
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The shared package feature checks the package's public version against its distribution metadata. Go's local main module does not expose an installed distribution version: build information commonly names it `(devel)`, and a tagged remote release is unnecessary for a runnable offline showcase.

# Selected variant

[[#Recorded version variable]]
- Replaces the shipped `VERSION` manifest selected before: since 2026-10-10 a Go project records its version only in `internal/version/version.go`.

# Searched variants

## Recorded version variable

**Selected.**

### Description

`internal/version/version.go` declares `var Version`, the one place the project's version is recorded and what `make version` prints. The package exposes `linkcheck.Version`, taken from it; the unchanged feature compares the two.

### Benefits

- Runs from a checkout or distribution archive without network access or a Git tag.
- One number: nothing for release tooling to keep in agreement.
- Fails when a literal is written back into the package's public version.

### Costs

- The scenario checks wiring inside one module, not a remotely published module version.

## Shipped VERSION manifest

### Description

Ship a `VERSION` file at the module root and expose a `linkcheck.Version` constant; the feature compares them independently.

### Benefits

- Runs from a checkout or distribution archive without network access or a Git tag.
- Detects drift between the public version and shipped metadata.

### Costs

- Release tooling must update the manifest and constant together — the drift the scenario detects exists only because there are two places.

## Go build information

### Description

Read the main module version through `runtime/debug.ReadBuildInfo`.

### Benefits

- Uses standard Go metadata with no extra manifest.

### Costs

- Local tests report `(devel)` or lack a release version, so comparison would require a special build and would not prove the ordinary checkout.
