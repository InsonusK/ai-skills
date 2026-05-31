---
name: entity-config-template
description: Template of entity config
metadata:
  domain: dotnet
  tags:
    - dotnet
    - skill-box
    - entity
    - entity-config
---
# Template of entity config

```csharp
using InsonusK.Shared.DataBase.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace {ProjectNamespace}.Entities.{EntityName}Entity.Configs
{
    // all unique indexes must be defined as IndexConfig
    public static IndexConfig<{EntityName}>[] uxConfigs =
    {
        new IndexConfig<{EntityName}>
        {
            // list of fields in index
            // all fields must implement IBody interface
            Fields = new[] { nameof({EntityName}.Name) },
            // if true, it will create unique index
            IsUnique = true
        }
    };

    public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
    {
        public void Configure(EntityTypeBuilder<{EntityName}> builder)
        {
            // apply all unique indexes
            uxConfigs.Apply(builder);
            // apply all query filters if entity is soft deleted
            builder.HasQueryFilter(x => !x.IsDeleted);
        }
    }
}
```
