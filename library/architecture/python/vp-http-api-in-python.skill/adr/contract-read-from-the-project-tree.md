---
name: contract read from the project tree
description: How the Python HTTP entry point reaches the committed OpenAPI contract and Swagger UI - read from the project tree, copied into the package, or generated from the code; Swagger UI from the package or a CDN
problem: The contract must stay at docs/integration/api/{contract-name}/ (skill api-develop) and be served as written; Go embeds it with a go:embed file in that folder, dotnet as a resource of the assembly; setuptools cannot put a file outside src/ into the package without listing every package by hand. How does the Python service read it, and where does Swagger UI come from?
decision: docs.py finds docs/integration/api/{contract-name}/openapi.json in the first folder above its module that holds it, so the service runs from its project tree; Swagger UI's files come from the swagger-ui-bundle package, not a CDN.
tags:
  - stack/python
  - concern/architecture
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`pyproject.toml` with setuptools ([build-backend-and-metadata-format](library/python/architecture/solutions/solution-cli-packaging.skill/adr/build-backend-and-metadata-format.md)) finds the packages under `src/`. A package mapped onto `docs/integration/api/{contract-name}/` with `package-dir` is not found by `packages.find`, and listing every package by hand breaks with each new one. Go and dotnet serve Swagger UI from files inside the binary, with no call to the internet.

# Selected variant
[[#Read from the project tree, Swagger UI from the package]]

# Searched variants

## Read from the project tree, Swagger UI from the package

**Selected.**

### Description
`find_contract()` walks up from `docs.py` to the first folder holding the contract and reads it when the docs are enabled; a missing file stops the server at start with its path. `swagger-ui-bundle` ships Swagger UI's files; `/swagger/swagger-initializer.js` is written by the service with the published spec's URL.

### Benefits
- One committed contract, where api-develop puts it; no copy to keep in step.
- The docs work offline and behind a strict content policy, as in Go and dotnet.

### Costs
- The service runs from its project tree (an editable install, or the tree in the image); a wheel alone serves no docs.
- `swagger-ui-bundle` ships Swagger UI 4.15, older than the 5.x of the other stacks.

## A copy of the contract inside the package

### Description
`src/{package}/api/http/openapi.json` as package data, kept equal to the committed file by a check.

### Benefits
- A wheel carries it.

### Costs
- Two copies of the contract, against api-develop's one place.

## FastAPI's generated contract and CDN Swagger UI

### Description
FastAPI's `/openapi.json` and `/docs` as they come.

### Benefits
- Nothing to write.

### Costs
- The contract follows the code; the UI loads from a CDN the service does not control.
