# Move common development guidance to develop catalogs

Status: Deferred

[Backlog index](../README.md#backlog) · [Shared execution guidance](README.md)

## Context

Some general development skills still live under common-workflow/develop,
while newer guidance uses library/develop/core and stack directories. Moving
files without classifying their semantics can preserve conflicting guidance or
break skill selection and relative links.

## Sources and scope

- [Source inventory](../../common-workflow/develop)
- [Destination catalogs and existing contracts](../../develop)
- [Skill structure and link conventions](../../design/skill-design.skill/skill-design.skill.md)

Inventory every source skill and assign a destination, merge target or
explicit retention reason. Classify stack-neutral versus stack-specific content.
Do not move the whole directory mechanically or edit generated skill copies.

## Dependencies and start condition

Start when a develop-guidance migration pass is scheduled. Resolve
[legacy client retirement](legacy-client-skills.md) and
[Go API route guidance](go-api-route-guidance.md) per skill before moving them;
these tasks need not wait for one giant migration.

## Expected result

1. Produce a source-to-destination map, including files merged into existing
   guidance, retained for legacy consumers or removed with approval.
2. Agree ambiguous ownership and compatibility before deleting source paths.
3. Move approved sources and repair all links, imports, templates and YAML
   subpaths/selections. Retain no competing source of truth.
4. Update documentation and resolve this card only after every source entry has
   an explicit disposition.

## Validation

Check every mapped destination exists and every removal has no unresolved
consumer. Search tracked files for old paths, classify historical references,
and run link-direction, skill sync and relevant skill-format checks. Build/test
examples if their consumed instructions, templates or code change.

## Open decisions

The mapping for overlapping guidance and policy for external users of old
paths require owner agreement. Simple file location does not authorize deleting
or merging a contract.
