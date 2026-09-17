---
description: Add proto/{service}/v1/{service}.proto, buf/buf.gen.yaml, and the Makefile's proto-gen target
element_kind: repository
change_kind: extend
tags:
  - solution/grpc-api
  - element/repo-root
---

# Structure

## Repository Structure
```
proto/
  {service}/
    v1/
      {service}.proto
buf/
  buf.gen.yaml
gen/
  api/                  (generated; committed, not gitignored)
Makefile                (extended)
```

## Directory and class skills
| Directory | file | Description |
| --------- | ---- | ----------- |
| proto/{service}/v1 | {service}.proto | This module's own exposed gRPC contract |
| buf | buf.gen.yaml | codegen config: proto/{service} → gen/api |

# Implementation changes

`proto/{service}/v1/{service}.proto`:
```protobuf
syntax = "proto3";

package {module}.{service}.v1;

option go_package = "{module-path}/gen/api";

service {Service} {
  // {Method} mirrors the domain service's own method.
  rpc {Method}({Method}Request) returns ({Method}Response);
}

message {Method}Request {
  string input = 1;
}

message {Method}Response {
}
```

`buf/buf.gen.yaml`:
```yaml
version: v2
plugins:
  - remote: buf.build/protocolbuffers/go
    out: gen/api
    opt: paths=source_relative
  - remote: buf.build/grpc/go
    out: gen/api
    opt: paths=source_relative
```

`Makefile` (added target):
```makefile
.PHONY: proto-gen

proto-gen:
	buf generate proto/{service} --template buf/buf.gen.yaml
```

# Rule changes

## MUST
- This module's own exposed contract lives under `proto/{service}/` — never in the same `proto/` subtree as a contract for a service this module calls *as a client*.
  - Violation: adding an external service's `.proto` file under `proto/{service}/v1/` alongside this module's own.
  - Risk: two different services' proto `package` declarations compiled into the same `gen/api` output can collide on generated type/package names.
  - Fix: an external service's contract gets its own subtree ([[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]'s `proto/{external}/`), generated into its own `gen/{external}` output, never merged with `gen/api`.
- `gen/api` is committed to the repository, never gitignored.
  - Risk: an ungenerated `gen/api` breaks every build until someone remembers to run `make proto-gen` with the right `buf`/plugin versions installed.
  - Fix: commit the generated output; `make proto-gen` regenerates it in place when the `.proto` changes, and CI can diff it to catch drift.

# Check list
- [ ] `make proto-gen` succeeds against a clean checkout with `buf` installed.
- [ ] `gen/api/*.go` is tracked in version control.
