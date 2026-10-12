---
description: Add buf.yaml, buf.gen.yaml, the generator tool lines, the Makefile's proto-gen target, gen/api, and the generated test kind
element_kind: repository
change_kind: extend
tags:
  - solution/grpc-api
  - element/repo-root
---

# Structure

## Repository Structure
```
buf.yaml
buf.lock                (written by `go tool buf dep update`; committed)
buf.gen.yaml
gen/
  api/
    {contract-name}/    (generated; committed, not gitignored)
docs/
  integration/
    api/
      {contract-name}/  (the proto file and its generated spec - see that folder's own file)
tools/
  testing/
    kinds/
      generated.sh      (copied unchanged from api-develop-in-go)
Makefile                (extended)
go.mod                  (extended: tool lines)
```

## Directory and class skills
| Directory | file | Description |
| --------- | ---- | ----------- |
| . | buf.yaml | names `docs/integration/api` as the module of this service's own contracts, and the two imported modules |
| . | buf.gen.yaml | codegen config: `docs/integration/api` → `gen/api` and the spec beside each proto file |
| tools/testing/kinds | generated.sh | the `generated` test kind |

# Implementation changes

`buf.yaml`:
```yaml
# docs/integration/api is the one module; each contract is a folder of it.
version: v2
modules:
  - path: docs/integration/api
deps:
  - buf.build/googleapis/googleapis
  - buf.build/grpc-ecosystem/grpc-gateway
```

`buf.gen.yaml` — local plugins started through `go tool`, not `buf.build` remote plugins, so `make proto-gen` runs the versions `go.mod` pins:
```yaml
version: v2
# Only the service's own contracts: buf.yaml may also name a module of consumed ones.
inputs:
  - directory: docs/integration/api
plugins:
  - local: ["go", "tool", "protoc-gen-go"]
    out: gen/api
    opt: paths=source_relative
  - local: ["go", "tool", "protoc-gen-go-grpc"]
    out: gen/api
    opt: paths=source_relative
  - local: ["go", "tool", "protoc-gen-openapiv2"]
    out: docs/integration/api
```

`go.mod` (added once, then `go tool buf dep update` writes `buf.lock`):
```bash
go get -tool github.com/bufbuild/buf/cmd/buf \
  google.golang.org/protobuf/cmd/protoc-gen-go \
  google.golang.org/grpc/cmd/protoc-gen-go-grpc \
  github.com/grpc-ecosystem/grpc-gateway/v2/protoc-gen-openapiv2
```

`Makefile` (added target):
```makefile
.PHONY: proto-gen

proto-gen:
	go tool buf generate --template buf.gen.yaml
```

`tools/testing/kinds/generated.sh` — copy the asset of [[skills/common-workflow/develop/api-develop-in-go.skill/api-develop-in-go.skill.md|api-develop-in-go]] unchanged.

# Rule changes

## MUST
- Keep this module's own contract under `docs/integration/api/` and a contract it consumes under `docs/integration/client/` — never one module, one template, or one `gen/` subtree for both.
  - Violation: an external service's `.proto` file in `docs/integration/api/`; `buf.gen.yaml` with no `inputs` in a repository whose `buf.yaml` names both modules.
  - Risk: the consumed contract is generated into `gen/api/` and gets an OpenAPI spec as if this service published it; the `generated` kind then holds files this service does not own.
  - Fix: `inputs: - directory: docs/integration/api` in `buf.gen.yaml`; the consumed contract gets its own template ([[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]'s `buf.gen.client.yaml`).
- Run every generator as a `tool` line of `go.mod` through `go tool`.
  - Violation: `go install google.golang.org/protobuf/cmd/protoc-gen-go@latest`; `local: protoc-gen-go` in `buf.gen.yaml`.
  - Risk: two machines generate different files from one proto file, and the `generated` kind fails on the one that did not commit.
  - Fix: `go get -tool …` once; `local: ["go", "tool", "{plugin}"]`.
- `gen/api`, `buf.lock`, and the generated spec are committed to the repository, never gitignored.
  - Risk: an ungenerated `gen/api` breaks every build until someone runs `make proto-gen`; an unpinned import generates differently tomorrow.
  - Fix: commit the generated output; `make proto-gen` regenerates it in place when the proto file changes, and `make test-kind-generated` fails when it was not.

# Check list
- [ ] `make proto-gen` succeeds against a clean checkout with nothing installed but Go.
- [ ] `go.mod` holds the four `tool` lines; no `@latest` install appears in the `Makefile` or the README.
- [ ] `gen/api/{contract-name}/*.pb.go` and `buf.lock` are tracked in version control.
- [ ] `make test-kinds` lists `generated - no badge`.
