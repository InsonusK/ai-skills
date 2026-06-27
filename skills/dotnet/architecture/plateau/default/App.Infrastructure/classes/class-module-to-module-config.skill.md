---
name: class-module-to-module-config
description: Configure foreign key relationships and mappings that cross module boundaries
domain: skill
type: template
version: 20260616
tags:
  - skill/template/class
created_by:
  - "[[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration.skill]]"
---

# Goal
- Configure foreign key relationships and mappings that cross module boundaries

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend/{Module1}To{Module2}Config.cs.create.md|{Module1}To{Module2}Config.cs.create]]

# Core Principals
- Cross-module configuration references entities from multiple modules without redefining their intra-module mapping
- Cross-module config composes on top of existing Domain configs, never duplicates them

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend/{Module1}To{Module2}Config.cs.create.md|{Module1}To{Module2}Config.cs.create]]

# Naming convention
| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Relation between 2 modules | {Module1}To{Module2}Config | OrderToPaymentConfig | {Module1}To{Module2}.Config.cs | OrderToPayment.Config.cs |

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend/{Module1}To{Module2}Config.cs.create.md|{Module1}To{Module2}Config.cs.create]]

# Implementation
Cross-module foreign key configuration lives in App.Infrastructure:

```csharp
public class OrderToPaymentConfig : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder
            .HasOne<Payment>()
            .WithMany()
            .HasForeignKey("PaymentId")
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend/{Module1}To{Module2}Config.cs.create.md|{Module1}To{Module2}Config.cs.create]]

# Rules
MUST:
	- Configure only cross-module foreign keys and relationships
	- Reference existing entity configs from Domain, not redefine intra-module mapping
MUST NOT:
	- Redefine table names, column names, or indexes already owned by Domain config
	- Configure intra-module relationships here

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend/{Module1}To{Module2}Config.cs.create.md|{Module1}To{Module2}Config.cs.create]]

# Unittest TestCases
- [ ] WHEN applied THEN Configure foreign key relationships and mappings that cross module boundaries
- [ ] WHEN applied THEN Cross-module configuration references entities from multiple modules without redefining their intra-module mapping
- [ ] WHEN applied THEN Cross-module config composes on top of existing Domain configs, never duplicates them
- [ ] WHEN naming 'Relation between 2 modules' THEN pattern matches convention

__Applied solutions:__
- [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/solution-domain-configuration.skill.md|solution-domain-configuration]] - [[skills/dotnet/architecture/solutions/🧩validated/solution-domain-configuration.skill/Implementation/App.Infrastructure.csproj.extend/{Module1}To{Module2}Config.cs.create.md|{Module1}To{Module2}Config.cs.create]]
