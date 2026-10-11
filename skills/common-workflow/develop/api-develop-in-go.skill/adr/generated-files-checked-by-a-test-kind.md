---
name: generated-files-checked-by-a-test-kind
description: Decide what catches committed generated files that no longer match the proto contract.
problem: `gen/api/*.pb.go` and the OpenAPI spec are committed; a proto change merged without `make proto-gen` leaves them stale — what detects it, and how?
decision: A test kind, `generated`, that generates into a scratch directory and compares the result with the committed files.
tags:
  - stack/go
  - concern/documentation
  - concern/documentation/adr
---

# Problem
`api-develop` requires the spec to be updated with every contract change, and nothing enforces it for generated files. The check must run in the pull request and locally by the same command.

# Selected variant
**Selected variant:** [[#Test kind that compares with a fresh generation in a scratch directory]]
- It is picked up by the existing workflows and never touches the working tree.

# Searched variants

## Test kind that compares with a fresh generation in a scratch directory

**Selected.**

### Description
`tools/testing/kinds/generated.sh` runs the command of `make proto-gen` with `--output` set to a temporary directory and compares every generated file with the committed one; it also reports a committed `gen/api/*.pb.go` the contract no longer produces. No badge.

### Benefits
- The workflows discover kinds through `make test-kinds`, so no workflow changes.
- A developer who has regenerated and not yet committed gets a green run; the working tree is never rewritten.
- The same check runs locally as `make test-kind-generated`.

### Costs
- On a CI runner the kind takes about two minutes: `go tool` compiles buf and the plugins when the restored build cache does not hold them. Generating and comparing take seconds.

## Regenerate in place and diff

### Description
`make proto-gen && git diff --exit-code`.

### Benefits
- Two commands, no script.

### Costs
- Rewrites a developer's working tree.
- Fails when the regenerated files are correct and only not yet committed.

## Extra workflow step

### Description
A step in the pull-request workflow that regenerates and compares.

### Benefits
- Not listed among the test kinds.

### Costs
- `devops-ci-orchestration` allows no project-specific workflow step; both workflows are copied from their skills.
- Cannot be run locally by the command CI runs.
