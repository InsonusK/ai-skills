---
description: Vendored copy of the external dependency's gRPC contract, generated to a C# client stub at build
project_name: "App.Infrastructure"
name: "{Dependency}.proto"
element_kind: class
change_kind: create
tags:
  - solution/grpc-client
  - element/dependency-proto
---

# Goals
- Carry the dependency's service + message definitions verbatim so `Grpc.Tools` can generate the typed client.

# Implementation changes

```proto
// App.Infrastructure/Protos/pricing.proto
syntax = "proto3";
option csharp_namespace = "App.Infrastructure.Grpc.Pricing";

service Pricing {
  rpc GetPrice (GetPriceRequest) returns (PriceReply);
  rpc ListPrices (ListPricesRequest) returns (ListPricesReply);
}

message GetPriceRequest { string sku = 1; }
message PriceReply { string sku = 1; double amount = 2; string currency = 3; }
message ListPricesRequest { repeated string skus = 1; }
message ListPricesReply { repeated PriceReply prices = 1; }
```

- The file is a copy of what the dependency's team publishes — do not edit it to fit this module. If the module needs a narrower surface, narrow `I{Dependency}Client`, not the `.proto`.

# Rules

## MUST
- Set `option csharp_namespace` to a dependency-scoped namespace under `App.Infrastructure.Grpc`.
  - Risk: two dependencies with the same message name collide in the default namespace.
  - Fix: one namespace per dependency.
- Keep the file a verbatim vendored copy; record its source and version in a header comment.
  - Risk: a locally-edited `.proto` silently diverges from the real contract.
  - Fix: copy as-is; narrow at the `I{Dependency}Client` layer.

# Check list
- [ ] `.proto` is a verbatim copy with a source/version header comment.
- [ ] `option csharp_namespace` is dependency-scoped.
