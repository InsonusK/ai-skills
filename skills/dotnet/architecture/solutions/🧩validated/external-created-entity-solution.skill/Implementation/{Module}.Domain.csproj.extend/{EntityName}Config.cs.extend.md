---
description: Configure unique index on Guid with named constant
project_name: "{Module}.Domain"
name: "{EntityName}Config.cs"
element_kind: class
change_kind: extend
---

# Goals
- Configure a unique database index on `Guid` as the DB-level idempotency guard
- Define the index name as a `public static string` constant for use in test assertions

# Core Principles
- Index name follows the convention: `UX_{TableName}_Guid`
- Unique index ensures concurrent requests that both pass the pipeline check are rejected at the DB level
- Constant name `UX_Guid` used in integration tests to assert the correct constraint name in `PostgresException`

# Naming convention
| use case | constant name pattern | constant name |
| --- | --- | --- |
| Unique index name | `UX_Guid` | `UX_Guid` |

# Implementation changes

```csharp
// {Module}.Domain/Configurations/{EntityName}Config.cs
public class {EntityName}Config : IEntityTypeConfiguration<{EntityName}>
{
    public static string TableName = nameof({EntityName});
    public static string UX_Guid = $"UX_{TableName}_Guid";

    public void Configure(EntityTypeBuilder<{EntityName}> builder)
    {
        // unique index — DB-level idempotency guard
        builder
            .HasIndex(e => e.Guid)
            .IsUnique()
            .HasDatabaseName(UX_Guid);

        // Version concurrency token — entity-concurrency-change-solution.skill (if mutable)
        builder
            .Property(e => e.Version)
            .HasColumnName("xmin")
            .IsConcurrencyToken()
            .ValueGeneratedOnAddOrUpdate();
    }
}
```

# Rules

MUST:
- `UX_Guid` defined as `public static string` on the config class
- `HasDatabaseName(UX_Guid)` used — never inline string
- `IsUnique()` on the `Guid` index

MUST NOT:
- Use inline string for index name

# Anti-patterns
- Inline string for database index name — hard to reference in tests

# Check list
- [ ] `UX_Guid` constant defined on entity configuration class
- [ ] Unique index on `Guid` configured with `HasDatabaseName(UX_Guid)` and `IsUnique()`

# Unittest TestCases
- [ ] WHEN applied THEN Configure a unique database index on Guid as the DB-level idempotency guard
- [ ] WHEN applied THEN Define the index name as a public static string constant for use in test assertions
- [ ] WHEN applied THEN Index name follows the convention: UX_{TableName}_Guid
- [ ] WHEN applied THEN Unique index ensures concurrent requests that both pass the pipeline check are rejected at the DB level
- [ ] WHEN applied THEN Constant name UX_Guid used in integration tests to assert the correct constraint name in PostgresException
- [ ] WHEN verified THEN UX_Guid constant defined on entity configuration class
- [ ] WHEN verified THEN Unique index on Guid configured with HasDatabaseName(UX_Guid) and IsUnique()
- [ ] WHEN naming 'Unique index name' THEN pattern matches convention
