# Align Go API guidance with endpoint-owned routes

Status: Deferred

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

api-develop-in-go still requires routes in internal/api/http/server.go and a
ContractRoutes() method. The current HTTP VP owns routes in api/http/endpoint
through Routes. Its tests now separate shared served/OpenAPI equality from a
native operation-list pin. Applying both instructions can recreate an obsolete
server-owned list or a conflicting test layout.

## Sources and scope

- [Legacy Go API instructions](../../common-workflow/develop/api-develop-in-go.skill/api-develop-in-go.skill.md)
- [Current Go routes, templates and tests](../go/vp-http-api-in-go.skill)
- [Agreed HTTP test split](../spec/vp-http-api.skill/adr/shared-contract-and-example-operations.md)
- [Adjacent client contract; preserve its scope](../../develop/core/api-client.skill/api-client.skill.md)

Update Go API route ownership instructions, examples and checks, coordinating
with the skill's eventual move to develop/go. Inventory old-catalog consumers
before replacing an instruction they still depend on. Do not add a duplicate
ContractRoutes adapter solely to satisfy outdated prose.

## Dependencies and start condition

Start with the migration of this skill, or earlier when the owner schedules
an independent compatibility repair. Coordinate destination ownership through
[develop migration](develop-guidance-migration.md).

## Expected result

1. Document endpoint-owned route registration matching the current VP.
2. Document both tests: common served/OpenAPI equality and a native exact list
   of the example's operations, including example-specific additions.
3. Repair snippets, filenames, checklist items and consumers; explicitly retain
   or separate legacy instructions if surviving consumers still require them.
4. Keep spec/Swagger/management routes outside the published business operation
   set as the owning HTTP contract requires.

## Validation

Run the Go HTTP proto example tests/vet if its code changes. Show that a served
route drifting from OpenAPI fails the shared check, and adding/removing an
operation from both served routes and OpenAPI still fails the unchanged native
pin. Check links, dependent guidance and shared conformance copies.

## Open decisions

The current route ownership and test split are decided. Ask the owner only
if preserving legacy consumers requires a compatibility policy or changes the
agreed migration scope.
