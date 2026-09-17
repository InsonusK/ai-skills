---
description: Add proto/{service}/{service}.proto, buf/buf.gen.yaml, and the Makefile's proto-gen target
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
| proto/{service} | {service}.proto | This module's own exposed gRPC contract |
| buf | buf.gen.yaml | codegen config: proto/{service} → gen/api |

# Implementation changes

`proto/{service}/{service}.proto` — no version subdirectory in the proto path: `buf generate`'s `paths=source_relative` mirrors the proto's path *relative to the directory passed to `buf generate`* into the output, so a `v1/` segment in the proto path lands the generated code at `gen/api/v1/*.go` — a different Go import path (`.../gen/api/v1`) than the flat `.../gen/api` the `go_package` option below declares, which is a real, verified mismatch, not a style preference:
```protobuf
syntax = "proto3";

package {service};

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

`buf/buf.gen.yaml` — local plugins (installed once via `go install google.golang.org/protobuf/cmd/protoc-gen-go@latest` and `go install google.golang.org/grpc/cmd/protoc-gen-go-grpc@latest`), not `buf.build` remote plugins, so `make proto-gen` works without depending on buf.build's remote plugin execution being reachable:
```yaml
version: v2
plugins:
  - local: protoc-gen-go
    out: gen/api
    opt: paths=source_relative
  - local: protoc-gen-go-grpc
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
  - Violation: adding an external service's `.proto` file under `proto/{service}/` alongside this module's own.
  - Risk: two different services' proto `package` declarations compiled into the same `gen/api` output can collide on generated type/package names.
  - Fix: an external service's contract gets its own subtree ([[skills/go/architecture/solutions/solution-external-integration.skill/solution-external-integration.skill.md|solution-external-integration]]'s `proto/{external}/`), generated into its own `gen/{external}` output, never merged with `gen/api`.
- Never nest the proto file under a version subdirectory (`proto/{service}/v1/{service}.proto`) while `go_package` declares a flat import path.
  - Violation: `proto/{service}/v1/{service}.proto` with `option go_package = "{module-path}/gen/api"`.
  - Risk: `buf generate`'s `paths=source_relative` then emits the generated code at `gen/api/v1/*.go` — a different Go import path than every hand-written file that imports `.../gen/api` expects, a mismatch that only surfaces as an import error when something tries to use the generated package (verified by actually running `buf generate` against this exact shape).
  - Fix: keep the proto flat at `proto/{service}/{service}.proto`, matching the `go_package` option's own path exactly; put any version marker in the proto `package` name instead (`package {service}.v1;`) if one is wanted.
- `gen/api` is committed to the repository, never gitignored.
  - Risk: an ungenerated `gen/api` breaks every build until someone remembers to run `make proto-gen` with the right `buf`/plugin versions installed.
  - Fix: commit the generated output; `make proto-gen` regenerates it in place when the `.proto` changes, and CI can diff it to catch drift.

# Check list
- [ ] `make proto-gen` succeeds against a clean checkout with `buf`, `protoc-gen-go`, and `protoc-gen-go-grpc` installed (`go install .../protoc-gen-go@latest` and `.../protoc-gen-go-grpc@latest` for the latter two — no `buf.build` remote-plugin network dependency).
- [ ] `gen/api/*.go` lands flat (no nested version directory) and is tracked in version control.
