---
name: generated-kind-ships-with-the-contract-skill
description: Decide which skill delivers the `generated` test kind script.
problem: The `generated` kind is a test kind, and it applies only to a Go service with a proto contract — which skill owns its file?
decision: `api-develop-in-go` ships `generated.sh` as its own asset and links `solution-conformance-testing-in-go` for the kind mechanism.
tags:
  - stack/go
  - concern/documentation
  - concern/documentation/adr
---

# Problem
The script follows two contracts: the test-kind protocol of `solution-conformance-testing` (`kind.sh`, `kind_mode`) and the contract layout of `api-develop-in-go` (the proto directory, `buf.gen.yaml`, the two output directories). It fails in a project without `buf.gen.yaml`.

# Selected variant
**Selected variant:** [[#Asset of api-develop-in-go]]
- Every path in the script is one this skill fixes, and the skill's trigger is exactly the script's precondition.

# Searched variants

## Asset of api-develop-in-go

**Selected.**

### Description
The file lives in `api-develop-in-go/assets/tools/testing/kinds/`; the rule that copies it links `solution-conformance-testing-in-go`.

### Benefits
- A project gets the kind exactly when it has the layout the kind needs.
- A change of the layout and of the script that depends on it is one change in one skill.

### Costs
- A test kind lives outside `skills/testing/`.
- An agent reading only the testing skills does not learn the kind exists.

## Conditional asset of solution-conformance-testing-in-go

### Description
The file lives beside `unit.sh` and `mutation.sh`, marked "for a service with a proto contract".

### Benefits
- All Go test kinds are in one folder.

### Costs
- The testing skill gains a condition it otherwise has none of; a Go project without a proto contract that copies the folder gets a failing kind.
- The script's paths are defined by another skill, which a testing skill may not link.

## Asset of solution-grpc-api

### Description
The file ships with the Go architecture catalog's gRPC solution.

### Benefits
- The solution owns `make proto-gen`.

### Costs
- The solution keeps the contract under `proto/` and generates no OpenAPI spec; the script's paths do not exist there.
- A service not built from the catalog cannot take the kind.
