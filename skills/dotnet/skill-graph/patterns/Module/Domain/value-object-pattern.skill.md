---
uid: 9d4d8583-80ca-4e87-891d-a709cc9ade17
status: draft
name: value-object-pattern
description: rules for designing and implementing domain value objects
domain: skill
type: pattern
tags:
  - dotnet
  - domain
  - ddd
  - value-object
triggers:
  - value object design
  - domain modeling
  - immutable types
aliases:
  - Value Object
  - ValueObjects
---
# Goal
Define how domain implements value object

## Core Principles
- Semantics belong to types, not primitives
- Value Object represents a single, meaningful concept with invariants
- Value Objects are immutable and self-validating

## Structure / Contracts
```CSharp
public sealed record SomeValueObject
{
    public int Value { get; }

    public SomeValueObject(int value)
    {
        if (value <= 0 || value > 120)
            throw new DomainException("Invalid age");

        Value = value;
    }

    public static implicit operator int(SomeValueObject age)
        => age.Value;

    public static implicit operator SomeValueObject(int value)
        => new(value);

    public override string ToString()
        => Value.ToString();
}
```

## Rules
MUST:
- be immutable
- validate invariants on creation
SHOULD:
- encapsulate normalization and formatting
- represent semantically meaningful concepts
- use [[domain-rule-pattern.skill]] for implementation of invariant vaidation
MUST NOT:
- depend on infrastructure or application services

## Anti-patterns
- Using primitives for domain concepts with meaning (string, int, decimal everywhere)
- Allowing invalid state to exist in Value Object
- Making Value Objects mutable
- Adding infrastructure or framework dependencies
- Creating Value Objects without real semantic purpose

# Check list
- [ ] Value Object validate invariants

# Unittest TestCases
- [ ] When create invalid ValueObject Then raise Exception
- [ ] When create valid ValueObject Then object created