# Report invalid loaded data as a service failure

Status: Ready

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

A domain constructor can reject data returned by a database or another service.
Passing its invalid-input kind to the API blames the caller for corrupt stored
or upstream data. Go PostgreSQL ItemLists.Items currently wraps the constructor
error with %w, retaining its kind. The old backlog pointer to the HTTP-proto
example's service/item_lists.go is stale: that example now uses command/kept_lists.go;
the database service still exists in PostgreSQL, UoW and row-version examples.
The Go HTTP/gRPC client examples already use %v for a rejected upstream title,
providing an existing example that does not propagate the constructor's kind.

## Sources and scope

- [Confirmed Go database conversion](../go/vp-postgres-in-go.skill/examples/internal/service/item_lists.go)
- [Versioned Go conversion](../go/vp-row-version-in-go.skill/examples/internal/service/item_lists.go)
- [Shared database/service scenarios](../spec/vp-postgres.skill/conformance)
- [Existing HTTP client conversion](../go/vp-http-client-in-go.skill/examples/internal/service/remote_planner.go)
- [Existing gRPC client conversion](../go/vp-grpc-client-in-go.skill/examples/internal/service/remote_planner.go)
- [Upstream client outcome contract](../../develop/core/api-client.skill/api-client.skill.md)
- [Existing transport failure mapping](../go/vp-http-api-in-go.skill/examples-proto/internal/api/core/rpcstatus)
- [Inspect .NET conversions](../dotnet)
- [Inspect Python conversions](../python)

Inventory entity reconstruction at database and HTTP/gRPC client boundaries
in all three stacks. Fix confirmed paths, owning templates, inherited examples
and guidance; do not assume every stack has the same defect. Direct caller input
must still receive invalid-input handling.

## Dependencies and start condition

Can start now. First establish the existing error/cause and transport mapping
for each stack. Use existing service-failure handling rather than adding a new
public error kind as part of this repair.

## Expected result

1. A value rejected after loading from storage/upstream is a failure of this
   service, never a caller validation error merely because a constructor uses
   an invalid-input kind.
2. The API returns its existing internal-service-failure response (HTTP 500,
   gRPC INTERNAL where served); it exposes no rejected stored value or driver
   details. CLI examples use their existing service-failure outcome.
3. Logs retain diagnostic context. Choose a cause/wrapper arrangement that cannot
   be reclassified as caller-invalid by existing recursive kind inspection.
4. Keep conflict, not-found, unavailable and genuine caller-invalid behavior
   unchanged. Carry the repair into dependent copies and templates.

## Validation

Add boundary scenarios using corrupt stored rows and an emulator's invalid
upstream item. Assert service-failure transport outcome and sanitized response,
plus useful diagnostic logging; pair them with caller-invalid cases. Run the
changed examples' tests/type checks and shared conformance/template checks.
Use real PostgreSQL for database cases.

## Open decisions

No owner decision is required for the stated behavior. If existing contracts
conflict with it or implementation needs a new public error kind, present that
specific conflict before changing the contract.
