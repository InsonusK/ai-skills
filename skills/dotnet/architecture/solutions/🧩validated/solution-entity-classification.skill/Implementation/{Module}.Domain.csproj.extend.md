# {Module}.Domain.csproj - extend

Apply entity classification at the domain layer by choosing the correct entity class and configuration variant for each `{EntityName}`.

## Changes

- Extend `{EntityName}.cs` with the properties and marker interfaces required by the selected classification.
- Extend `{EntityName}Config.cs` with the EF Core mappings required by the selected classification.
- Do not add `Version`, `Guid`, or their configurations unless the classification explicitly requires them.

## Classification variants

| Classification | `{EntityName}.cs` changes | `{EntityName}Config.cs` changes |
|---|---|---|
| **Internal Immutable** | Only `int Id` | Standard Id mapping |
| **External Immutable** | Add `Guid Guid { get; internal set; }` | Add unique index on `Guid` |
| **Internal Mutable** | Add `uint Version { get; internal set; }` and `IVersioned` | Map `Version` to `xmin` with `IsConcurrencyToken()` |
| **External Mutable** | Add both `Guid` and `Version` + `IVersioned` | Add unique index on `Guid` and map `Version` to `xmin` |

## See also

- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}.cs.extend|{EntityName}.cs]]
- [[skills/dotnet/architecture/solutions/🧩validated/solution-entity-classification.skill/Implementation/{Module}.Domain.csproj.extend/{EntityName}Config.cs.extend|{EntityName}Config.cs]]
