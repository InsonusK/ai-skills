---
name: one-folder-per-contract
description: Decide how the files of a Go service's own contract are laid out below docs/integration/api.
problem: `api-develop` places a contract under `docs/integration/api/{contract-name}/`, while this skill generated into `docs/integration/api/proto/` and `docs/integration/api/openapi/` — which layout holds?
decision: One folder per contract, `docs/integration/api/{contract-name}/`, holds the proto file, the spec generated from it, and `embed.go`; `docs/integration/api` is one buf module.
tags:
  - stack/go
  - concern/documentation
  - concern/documentation/adr
---

# Problem
This skill states that every rule of `api-develop` applies unchanged, and placed the proto file in `docs/integration/api/proto/` and the generated spec in `docs/integration/api/openapi/` — folders by kind of file, where the base asks for a folder by contract. A service that followed the base failed `make test-kind-generated`, whose script named the two folders; one that followed this skill was reported by a reviewer checking against the base. A service with two contracts had to put both proto files into the one `proto/` folder.

# Selected variant
**Selected variant:** [[#One folder per contract]]
- The base rule holds without an exception, and a second contract is a second folder.

# Searched variants

## One folder per contract

**Selected.**

### Description
`buf.yaml` at the repository root names `docs/integration/api` as the one module. `protoc-gen-openapiv2` writes to `docs/integration/api` and the Go plugins to `gen/api` with `paths=source_relative`, so `{contract-name}/{file}.proto` yields `{contract-name}/{file}.swagger.json` beside it and `gen/api/{contract-name}/{file}.pb.go`. `embed.go` lies in the contract's folder. The `generated` kind generates the whole module once and compares every generated file.

### Benefits
- One layout in the base and in the Go extension.
- The folder is what a consumer copies to its `docs/integration/client/{api-name}/`.
- Each contract gets its own Go package under `gen/api/`, so two contracts cannot collide on generated names.
- Verified on a service with two contracts: the kind passes on fresh files, and fails on an edited spec, on a proto file changed without regeneration, and on a committed generated file no contract produces any more.

### Costs
- The import path of the generated code carries the contract name: `{module-path}/gen/api/{contract-name}`.
- The embed package cannot take the folder's name when it has a hyphen; it is the contract name without hyphens.
- A service laid out by the earlier text of this skill moves its files.

## Folders by kind of file, allowed by the base

### Description
`api-develop` is reworded to "in the layout the stack's extension names", and this skill keeps `proto/` and `openapi/`.

### Benefits
- Nothing moves in a service already laid out this way; `gen/api` stays flat.

### Costs
- Every stack may then place contracts differently, and "where is this service's contract" needs the stack's skill to answer.
- Two contracts share one `proto/` folder and one flat `gen/api` Go package.
- The published contract and the consumer's copy have different shapes.
