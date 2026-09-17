---
description: Add proto/{external}/v1/{external}.proto (the external service's own contract) and its buf codegen config
element_kind: repository
change_kind: extend
tags:
  - solution/external-integration
  - element/repo-root
---

# Structure

## Repository Structure
```
proto/
  {external}/
    v1/
      {external}.proto
buf/
  {external}.gen.yaml
gen/
  {external}/              (generated; committed, not gitignored)
Makefile                   (extended)
```

# Implementation changes

`proto/{external}/v1/{external}.proto` — this is the **external** service's own contract, owned by that service, vendored/copied here only to generate a client from:
```protobuf
syntax = "proto3";

package {external}.v1;

option go_package = "{module-path}/gen/{external}";

service {External}Service {
  rpc {ExternalMethod}({ExternalMethod}Request) returns ({ExternalMethod}Response);
}
```

`buf/{external}.gen.yaml`:
```yaml
version: v2
plugins:
  - remote: buf.build/protocolbuffers/go
    out: gen/{external}
    opt: paths=source_relative
  - remote: buf.build/grpc/go
    out: gen/{external}
    opt: paths=source_relative
```

`Makefile` (added to `proto-gen`, or created if [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]] is not also applied):
```makefile
proto-gen:
	buf generate proto/{external} --template buf/{external}.gen.yaml
```

# Rule changes

## MUST
- Generate the external service's contract into its own `gen/{external}` output, in its own `buf/{external}.gen.yaml` — never merged with this module's own `gen/api`.
  - Risk: two different services' proto `package` declarations compiled into the same output directory can collide on generated type/package names the moment both are applied to the same module.
  - Fix: keep every external contract in its own `proto/{external}/` → `gen/{external}` pair.
- If `Makefile` already has a `proto-gen` target (from [[skills/go/architecture/solutions/solution-grpc-api.skill/solution-grpc-api.skill.md|solution-grpc-api]]), add this generation as a second `buf generate` line inside the existing target — never a second `proto-gen:` target declaration.
  - Risk: Make does not merge two rules with the same target name predictably; the second declaration silently replaces the first instead of extending it.
  - Fix: extend the existing target's recipe body.

# Check list
- [ ] `gen/{external}` is generated into its own package, distinct from `gen/api`.
