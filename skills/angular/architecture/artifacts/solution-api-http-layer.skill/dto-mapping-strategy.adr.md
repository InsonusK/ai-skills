---
name: dto-mapping-strategy
description: How DTOs returned by the backend are converted to domain models used by the rest of the application, and vice versa
problem: Whether DTO/domain-model mapping should be automatic (a mapping library) or manual (hand-written functions), given some fields require additional data pulled from shared state at mapping time
decision: Use manual, hand-written mapper functions inside the Client
---

# Problem

Every API response arrives as a DTO shape that does not necessarily match the domain model the rest of the application (Signal Stores, components, forms) works with. Some of these mappings are not pure 1:1 field renames — certain domain fields need to be enriched with information not present in the DTO at all (e.g. a value that must be read from `shared-state` at the moment of mapping). We need to decide whether this mapping is handled by an automatic mapping library or by hand-written functions.

# Selected variant

**Selected variant:** [[#Manual mapper functions]]

Every Client hand-writes its own `dtoToModel`/`modelToDto` functions. This is retained from the existing approach rather than introduced fresh, because it already handles the enrichment-from-store requirement that a generic mapping library would not handle without significant custom extension work anyway.

# Searched variants

## Manual mapper functions

### Description

Each Client defines explicit `{feature}.mapper.ts` functions (e.g. `orderDtoToModel(dto: OrderDto, context: MappingContext): Order`), where `MappingContext` carries whatever additional data (read from `shared-state` or elsewhere) a given mapping needs.

### Benefits

- Full, explicit control over every field, including the ones that need enrichment from data the DTO itself does not carry — this is not a corner case handled awkwardly, it's the normal shape of the function
- No hidden "magic" — a reviewer can read the mapper function top to bottom and see exactly what happens to every field
- No additional runtime dependency, no decorator/reflection-based metadata to keep in sync with DTO/model shape changes
- Straightforward to unit test in isolation, since it is a plain function from DTO (+ context) to model

### Costs

- More boilerplate than an automatic mapper for the common, no-enrichment-needed case — every field still needs an explicit line
- Relies on developer discipline to keep the mapper updated when either the DTO or the domain model shape changes; nothing enforces this automatically beyond TypeScript's structural typing catching missing/renamed fields

## Automatic mapping via a library (e.g. class-transformer or equivalent)

### Description

DTO-to-model mapping is driven by decorators/metadata and a mapping library, with most 1:1 fields requiring no explicit code.

### Benefits

- Less boilerplate for the common case of straightforward, no-enrichment field mapping
- Consistent, declarative mapping style across the codebase

### Costs

- Fields that need enrichment from external context (data read from `shared-state` at mapping time) do not fit the library's decorator-driven model naturally — these would likely need to be special-cased outside the library anyway, undermining the consistency benefit
- Adds a runtime dependency and, typically, reflection/decorator metadata that must stay compatible with the build tooling (and, in this workspace, with esbuild-based builds and potential AOT/tree-shaking constraints)
- "Magic" mapping is harder to review at a glance — understanding what actually happens to a given field means understanding the library's resolution rules, not just reading a function
