# Worked example: `skills/dotnet/architecture/v3`

A short walkthrough of applying [../variability-map-create.skill.md](../variability-map-create.skill.md) to a real catalog, `skills/dotnet/architecture/v3/`.

## Finding a categorical VP
`solution-entity-classification` already maps four entity kinds to combinations of two boolean solutions — this is a Variability Map row that was never written as one:

| ID | VP | Variants | Realized by |
| --- | --- | --- | --- |
| VP1 | EntityKind | Internal Immutable / External Immutable / Internal Mutable / External Mutable | Internal Immutable → neither; External Immutable → `solution-external-created-entity`; Internal Mutable → `solution-entity-concurrency-change`; External Mutable → both |

The candidate test from the parent skill applies directly: different modules on the same catalog legitimately pick different entity kinds, so this earns a row — it is not shared core.

## Finding an independent boolean pair (an "Or" group, not "Alternative")
`solution-http-api-publication` and `solution-grpc-integration` are each optional and freely combinable — not two Variants of one VP:

| ID | VP | Variants | Constraint |
| --- | --- | --- | --- |
| VP2 | HttpApi | Yes / No | Yes requires (VP-QueryIntegration=Yes OR VP-CommandIntegration=Yes) |
| VP3 | GrpcApi | Yes / No | (independent of VP2) |

The Constraint on VP2 came from `solution-http-api-publication`'s own description stating the requirement in prose — its `depends_on` did not encode it. Per the parent skill's rule, this was flagged and fixed as a plateau-level ADR rather than left as a silent gap between the table and the solution file.

## Plateau Map derivation, abbreviated
| Plateau | VP answers fixed |
| --- | --- |
| `plateau-service-with-api` | `VP2=Yes and/or VP3=Yes` (at least one) |
| `plateau-v1` | union of `plateau-service-with-api`, `plateau-shared-rules`, `plateau-statefull-service` |

See the real, complete table at [skills/dotnet/architecture/v3/variability-map.md](../../../../../dotnet/architecture/v3/variability-map.md).
