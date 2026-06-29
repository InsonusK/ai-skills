---
description: Configure foreign key relationships and mappings that cross module boundaries
project_name: App.Infrastructure
name: "{Module1}To{Module2}Config.cs"
element_kind: class
change_kind: create
---

# Goals
- Configure foreign key relationships and mappings that cross module boundaries

# Core Principles
- Cross-module configuration references entities from multiple modules without redefining their intra-module mapping
- Cross-module config composes on top of existing Domain configs, never duplicates them

# Naming convention

| use case                   | class name pattern         | class name           | file name pattern              | file name                |
| -------------------------- | -------------------------- | -------------------- | ------------------------------ | ------------------------ |
| Relation between 2 modules | {Module1}To{Module2}Config | OrderToPaymentConfig | {Module1}To{Module2}.Config.cs | OrderToPayment.Config.cs |

# Implementation changes

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

# Rule changes

MUST:
- Configure only cross-module foreign keys and relationships
- Reference existing entity configs from Domain, not redefine intra-module mapping

MUST NOT:
- Redefine table names, column names, or indexes already owned by Domain config
- Configure intra-module relationships here

# Unittest TestCases
- [ ] WHEN applied THEN Configure foreign key relationships and mappings that cross module boundaries
- [ ] WHEN applied THEN Cross-module configuration references entities from multiple modules without redefining their intra-module mapping
- [ ] WHEN applied THEN Cross-module config composes on top of existing Domain configs, never duplicates them
- [ ] WHEN naming 'Relation between 2 modules' THEN pattern matches convention
