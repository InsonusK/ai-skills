---
description: Add docs/integration/client/{api-name}/{file}.proto (the provider's contract, unchanged), its buf module and its own codegen template
element_kind: repository
change_kind: extend
tags:
  - solution/external-integration
  - element/repo-root
---

# Structure

## Repository Structure
```
docs/
  integration/
    client/
      {api-name}/
        {file}.proto       (the provider's published copy, unchanged)
buf.yaml                   (extended, or created)
buf.gen.client.yaml
gen/
  client/
    {api-name}/            (generated; committed, not gitignored)
Makefile                   (extended)
```

# Implementation changes

`docs/integration/client/{api-name}/{file}.proto` — the **external** service's own contract, placed and kept unchanged as [[library/common-workflow/develop/api-client-develop.skill.md|api-client-develop]] rules. It carries the provider's `// version:` line, proto package, and `go_package`; none is edited here.

`buf.yaml` — the consumed contracts are a second module beside this service's own (when [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] is not applied, the file is created with the second module only and no `deps`):
```yaml
# docs/integration/api: this service's own contracts, one folder each.
# docs/integration/client: the contracts it consumes, the providers' copies unchanged.
version: v2
modules:
  - path: docs/integration/api
  - path: docs/integration/client
deps:
  - buf.build/googleapis/googleapis
  - buf.build/grpc-ecosystem/grpc-gateway
```

`buf.gen.client.yaml`:
```yaml
version: v2
inputs:
  - directory: docs/integration/client
# A consumed contract is the provider's file and names the provider's go_package;
# managed mode sets this module's without editing it.
managed:
  enabled: true
  override:
    - file_option: go_package_prefix
      value: {module-path}/gen/client
plugins:
  - local: ["go", "tool", "protoc-gen-go"]
    out: gen/client
    opt: paths=source_relative
  - local: ["go", "tool", "protoc-gen-go-grpc"]
    out: gen/client
    opt: paths=source_relative
```

`Makefile` (added to `proto-gen`, or created with the `tool` lines of [[library/go/architecture/solutions/solution-grpc-api.skill/Implementation/Repository.extend.md|solution-grpc-api's Repository]] when that solution is not also applied):
```makefile
proto-gen:
	go tool buf generate --template buf.gen.client.yaml
```

# Rule changes

## MUST
- Generate a consumed contract through `buf.gen.client.yaml` into `gen/client/{api-name}/` — never through `buf.gen.yaml`, and never into `gen/api/`.
  - Risk: the consumed contract gets an OpenAPI spec and a place under `docs/integration/api/` as if this service published it, and two services' messages share one Go package.
  - Fix: the second module in `buf.yaml`, `inputs` in both templates, one output tree per template.
- Set this module's Go import path with managed mode — never by editing `go_package` in the provider's file.
  - Violation: `option go_package = "{module-path}/gen/client/{api-name}";` written into the copy.
  - Risk: the copy differs from what the provider published, so its in-file version no longer identifies it and the next update is a merge instead of a replacement.
  - Fix: the `managed` block above; replace the file wholesale when the provider publishes a newer one, then `make proto-gen`.
- If `Makefile` already has a `proto-gen` target (from [[library/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]), add this generation as a second line inside the existing target — never a second `proto-gen:` target declaration.
  - Risk: Make does not merge two rules with the same target name predictably; the second declaration silently replaces the first instead of extending it.
  - Fix: extend the existing target's recipe body.

# Check list
- [ ] `docs/integration/client/{api-name}/{file}.proto` is byte-identical to the provider's published file.
- [ ] `gen/client/{api-name}/` is generated into its own package, distinct from `gen/api/`.
- [ ] `make test-kind-generated`, where the project has it, still passes: `buf.gen.yaml` generates nothing from `docs/integration/client`.
