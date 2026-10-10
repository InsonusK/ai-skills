---
name: distribution-version
description: Version metadata for the runnable Go showcase
problem: A local Go main module has no installed-package version API equivalent to Python distribution metadata.
decision: Compare the public Version constant with a shipped VERSION manifest.
tags:
  - solution/conformance-testing-in-go
  - stack/go
  - concern/documentation
  - concern/documentation/adr
---

# Problem

The shared package feature checks the package's public version against its distribution metadata. Go's local main module does not expose an installed distribution version: build information commonly names it `(devel)`, and a tagged remote release is unnecessary for a runnable offline showcase.

# Selected variant

[[#Shipped VERSION manifest]]

# Searched variants

## Shipped VERSION manifest

**Selected.**

### Description

Ship a `VERSION` file at the module root and expose a `linkcheck.Version` constant; the unchanged feature compares them independently.

### Benefits

- Runs from a checkout or distribution archive without network access or a Git tag.
- Detects drift between the public version and shipped metadata.

### Costs

- Release tooling must update the manifest and constant together.
- Verifies distribution contents rather than a remotely published module version.

## Go build information

### Description

Read the main module version through `runtime/debug.ReadBuildInfo`.

### Benefits

- Uses standard Go metadata with no extra manifest.

### Costs

- Local tests report `(devel)` or lack a release version, so comparison would require a special build and would not prove the ordinary checkout.
