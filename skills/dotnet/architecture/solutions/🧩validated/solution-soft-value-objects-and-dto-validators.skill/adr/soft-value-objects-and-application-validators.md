---
name: soft-value-objects-and-application-validators
description: Where to place soft value objects and their validators so other modules can validate values without referencing Domain or Application
problem: Other modules need to validate values and DTOs owned by a module. If validators live in Application, consumers cannot instantiate concrete validators without referencing Application, but they can resolve IValidator<T> from DI. Soft value objects must still be visible to consumers so they can declare properties of that type.
decision: Place Soft{ValueObject} declarations in {Module}.Interfaces, place FluentValidation validators in {Module}.Application, and register them via AddValidatorsFromAssembly so consumers use IValidator<T>.
---

# Problem
When a module publishes DTOs or value objects, consumers may need to validate them. The public contract must expose the value shape, while validator implementations should stay in the application layer. Consumers should not reference `{Module}.Domain` or concrete validator types in `{Module}.Application`.

# Selected variant
**Selected variant:** [[#Use FluentValidation IValidator<T> with validators in Application and Soft value objects in Interfaces]]

Place `Soft{ValueObject}` in `{Module}.Interfaces` and its validator in `{Module}.Application`. Register the validator with FluentValidation's assembly scan. Other modules resolve `IValidator<Soft{ValueObject}>` from DI.

- `{Module}.Interfaces` remains declarations-only and exposes the `Soft{ValueObject}` shape
- `{Module}.Application` owns the validator implementation and registers it automatically
- Other modules use the generic `IValidator<T>` abstraction without referencing `{Module}.Application`
- `{Module}.Domain.ValueObjects.{ValueObject}` inherits from `Soft{ValueObject}` and enforces invariants by calling Rules
- Rules are defined in `{Module}.Domain/Rules`, have a primitive overload as the single source of truth, and a `Soft{ValueObject}` overload that delegates to it; Domain ValueObject and Application validators use the same Rule

# Searched variants

## Keep validators in {Module}.Interfaces

### Description
Validators live next to `Soft{ValueObject}` in `{Module}.Interfaces`.

### Benefits
- Consumers can instantiate validators directly without DI
- No need to resolve `IValidator<T>`

### Costs
- `{Module}.Interfaces` is no longer declarations-only
- `{Module}.Interfaces` must reference FluentValidation
- Validator implementations become part of the public contract, making future changes harder

## Keep validators in {Module}.Application and expose a custom validator abstraction

### Description
Define a module-specific `IPropertyValidator<T>` interface in Shared and implement it in `{Module}.Application`.

### Benefits
- Keeps FluentValidation out of the public contract

### Costs
- Adds a custom abstraction that duplicates FluentValidation's `IValidator<T>`
- Consumers already use FluentValidation through `ValidationBehavior`, so the custom abstraction adds friction

## Use FluentValidation `IValidator<T>` with validators in Application and Soft value objects in Interfaces

### Description
Place `Soft{ValueObject}` declarations in `{Module}.Interfaces`. Implement `{ValueObject}PropertyValidator` and `{Dto}Validator` in `{Module}.Application` as `AbstractValidator<T>`. Register them via `AddValidatorsFromAssembly`. Other modules consume validators through `IValidator<T>` resolved from DI.

### Benefits
- `{Module}.Interfaces` stays declarations-only
- Reuses the existing FluentValidation infrastructure (`ValidationBehavior`, `AddValidatorsFromAssembly`)
- Other modules do not reference `{Module}.Application` concrete types
- Consistent with the existing command-validator pattern

### Costs
- Consumers must resolve validators through DI instead of instantiating them directly
- Requires `{Module}.Domain` to reference `{Module}.Interfaces` for the `Soft{ValueObject}` base type
