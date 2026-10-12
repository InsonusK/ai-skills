---
name: embed-package-beside-the-contract
description: Decide how the OpenAPI spec kept under docs/integration/api reaches the running Go service.
problem: The spec lives under docs/, the server code under internal/, and `//go:embed` cannot read a parent directory — how does the binary get the spec?
decision: A Go file beside the spec, `docs/integration/api/{contract-name}/embed.go`, embeds it; the HTTP server imports that package.
tags:
  - stack/go
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`api-develop` places the published contract under `docs/integration/api` and requires the service to serve it. `//go:embed` reads only files in the directory of its own package or below it, and rejects a path with `..`, so `internal/api/http` cannot embed a file under `docs/`.

# Selected variant
**Selected variant:** [[#Embed package beside the spec]]
- The contract keeps one home and the binary needs no file at run time.

# Searched variants

## Embed package beside the spec

**Selected.**

### Description
`docs/integration/api/{contract-name}/embed.go` holds `//go:embed {file}.swagger.json` and exports `Spec []byte`. The HTTP server imports it.

### Benefits
- One file is the published contract and the served one.
- The service starts from the binary alone; a missing spec is a compile error.
- When the spec is generated, the generator writes it into this folder and nothing is copied.

### Costs
- A Go package lives under `docs/`, so `docs` is no longer documentation only: a `.dockerignore` that excludes it breaks the image build, and change detection must treat `docs/integration` as code.

## Copy of the spec under internal/

### Description
The JSON is copied to `internal/api/http/` and embedded there.

### Benefits
- No Go code under `docs/`; a `.dockerignore` may exclude `docs` entirely.

### Costs
- Two files that drift apart: the published contract and the served one.
- The copy step is one more thing a contract change must remember.

## Read the file at start-up

### Description
The service opens the JSON from a path given by configuration.

### Benefits
- No embed, no Go code under `docs/`.

### Costs
- The file must be shipped in the image separately from the binary.
- A missing file is found at run time: the service starts without docs, or fails.
