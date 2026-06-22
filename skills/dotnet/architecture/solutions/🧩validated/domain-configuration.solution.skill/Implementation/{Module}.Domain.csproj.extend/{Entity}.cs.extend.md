---
description: Ensure domain entity remains free of EF attributes — all persistence mapping delegated to config class
project_name: "{Module}.Domain"
name: "{Entity}.cs"
element_kind: class
change_kind: extend
---
# Goals
- Ensure domain entity remains free of EF attributes — all persistence mapping delegated to config class

# Core Principals
- Entity has zero EF attributes — `[Column]`, `[Index]`, `[ForeignKey]`, `[ConcurrencyCheck]` are all forbidden
- Entity does not know about its own table name, column names, or constraint names

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity   | {EntityName}       | Order      | {EntityName}.cs   | Order.cs  |

# Implementation changes

Entity must not contain any EF attributes:
```csharp
// CORRECT — no EF attributes
public class TodoTask
{
    public int Id { get; internal set; }
    public string Title { get; internal set; }
    public uint Version { get; internal set; }
}

// WRONG — EF attributes on entity
[Table("TodoTasks")]
public class TodoTask
{
    [Key]
    public int Id { get; internal set; }
    [Column("task_title")]
    public string Title { get; internal set; }
}
```

# Rule changes

MUST NOT:
- Entity have any EF attributes (`[Table]`, `[Column]`, `[Key]`, `[Index]`, `[ForeignKey]`, `[ConcurrencyCheck]`)

# Anti-patterns
- `[Column("task_title")]` on entity property — column mapping belongs in config class
- `[Table("TodoTasks")]` on entity class — table naming belongs in config class
- `[Index]` on entity class — index configuration belongs in config class
- `[ForeignKey]` on entity property — relation configuration belongs in config class

# Check list
- [ ] No EF attributes present on entity class or any of its properties

# Unittest TestCases
- [ ] THEN it ensure domain entity remains free of EF attributes — all persistence mapping delegated to config class
- [ ] WHEN applied THEN Entity has zero EF attributes — [Column], [Index], [ForeignKey], [ConcurrencyCheck] are all forbidden
- [ ] WHEN applied THEN Entity does not know about its own table name, column names, or constraint names
- [ ] WHEN verified THEN No EF attributes present on entity class or any of its properties
- [ ] WHEN naming 'Entity' THEN pattern matches convention
