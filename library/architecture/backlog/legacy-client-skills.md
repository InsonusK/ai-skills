# Retire legacy client guidance with the old catalogs

Status: Deferred

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

New architecture clients follow the shared api-client contract: an API-specific
configuration group, timeout and an in-process emulator for tests. The older
api-client-develop and client-mocking skills are retained for old catalogs.
Removing them before their consumers move would leave those catalogs without
applicable guidance.

## Sources and scope

- [New client contract](../../develop/core/api-client.skill/api-client.skill.md)
- [Legacy client development guidance](../../common-workflow/develop/api-client-develop.skill.md)
- [Legacy mocking guidance](../../common-workflow/develop/client-mocking.skill.md)
- [Current Go clients](../go)
- [Current .NET clients](../dotnet)
- [Current Python clients](../python)

Retire the two named legacy skills and their references after their old
consumers have a replacement. Do not replace the new client contract or broaden
the task to unrelated common-workflow skills.

## Dependencies and start condition

Start retirement when the owner confirms the old catalogs are being replaced.
First inventory every reference, including source imports, YAML skill selections
and template text. Coordinate with [develop migration](develop-guidance-migration.md).

## Expected result

1. Record each remaining consumer and its replacement before deleting either
   legacy skill; unresolved consumers keep the deletion blocked.
2. Update migrated consumers to the new contract and stack implementation.
3. Remove the old files only when there are no required old consumers; repair
   all links and skill selections. Update the catalog status and this card.

## Validation

Search all tracked source/config/template files for both old skill names;
classify every remaining hit as historical or repair it. Run sync and link
checks. Run HTTP/gRPC client examples in every stack whose behavior or templates
change; preserve timeout settings and emulator-based testing.

## Open decisions

Owner confirmation is needed for retirement timing and compatibility policy
for external consumers. No approval to remove the old catalogs is implied by
this card.
