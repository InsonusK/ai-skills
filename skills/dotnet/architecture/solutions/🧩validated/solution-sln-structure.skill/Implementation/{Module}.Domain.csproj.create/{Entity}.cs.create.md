---
description: Represent a domain object with stable identity, mutable state, encapsulated behavior, and invariant enforcement
project_name: "{Module}.Domain"
name: "{Entity}.cs"
element_kind: class
change_kind: create
---
# Goals
- Represent a domain object with stable identity, mutable state, encapsulated behavior, and invariant enforcement
- Select the correct entity type from the type matrix before implementation
- Define a domain entity as an object with stable identity where identity — not value — determines equality
- Ensure every entity is assigned to exactly one type so the correct set of patterns is applied
- Prevent invalid state by enforcing that all entity properties are accessible only through controlled access modifiers

# Core Principles
- Entity has stable identity — `int Id` is always the system primary identity
- Entity has mutable state — unlike Value Objects, state changes over time
- Entity encapsulates behavior — state changes happen through methods, not direct property assignment from outside
- Entity enforces invariants — invalid state must never be reachable
- `Id` is always `internal set` — only persistence layer assigns it, never application code
- Entity type is selected from the type matrix before implementation begins — not discovered during coding
- All public setters or method must validate to prevent invalid state

# Naming convention

| use case | class name pattern | class name | file name pattern | file name |
| -------- | ------------------ | ---------- | ----------------- | --------- |
| Entity   | {EntityName}       | Order      | {EntityName}.cs   | Order.cs  |

# Implementation changes

Entity must be a class with `int Id` as primary identity.

```csharp
public class Currency
{
    public int Id { get; internal set; }
    private string _code;
    public string Code {
	    public get => this._code;
      public set {
		    if (value == "")
		        throw new DomainException("Invalid code");
		    this._code = value;
		  }  
		}
    
		public int Amount {get; internal set;}   
    internal void SetAmount(int amount)
    {
        if (amount <= 0)
            throw new DomainException("Invalid amount");

        this.Amount = amount;
    }

}
```

# Rule changes

MUST:
- Entity has `int Id` with `internal set`
- All public property setters or methods must validation state
- `Id` used in all domain logic, persistence, relationships, and internal APIs

MUST NOT:
- Use `public` setters on any entity property

# Anti-patterns
- `public string Title { get; set; }` — public setter without validation
- Placing entity in Application or Interfaces project — entities belong in Domain only

# Check list
- [ ] Entity type selected from the matrix
- [ ] `int Id` with `internal set` present
- [ ] All public property setters and methods has validation
- [ ] Entity placed in /{Module}.Domain/Entities

# Unittest TestCases
- [ ] When entity created Then Id is default (0) until persisted
