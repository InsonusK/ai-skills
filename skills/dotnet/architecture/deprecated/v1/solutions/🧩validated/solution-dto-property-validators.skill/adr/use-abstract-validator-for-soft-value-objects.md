---
name: use-abstract-validator-for-soft-value-objects
description: Why Soft{ValueObject} validators inherit from AbstractValidator<T> instead of PropertyValidator<T, TProperty> so they can be reused by other modules through DI.
problem: PropertyValidator<T, TProperty> is tied to a specific parent validator and is not exposed as IValidator<Soft{ValueObject}>. This prevents cross-module consumption of SoftVO validators through DI because other modules do not know and cannot share the parent DTO type.
decision: Implement every Soft{ValueObject} validator as AbstractValidator<Soft{ValueObject}>. Register it via AddValidatorsFromAssembly and let consumers resolve IValidator<Soft{ValueObject}> from DI.
tags:
  - solution/dto-property-validators
  - concern/documentation
  - concern/documentation/adr
---

# Problem

Each module publishes `Soft{ValueObject}` shapes from `{Module}.Interfaces`. Other modules need to validate values of that type in their own commands and DTOs without referencing the owning module's `{Module}.Application` project directly.

FluentValidation offers two different APIs that look similar but serve different purposes:

- `AbstractValidator<T>` — validates an entire object and implements `IValidator<T>`.
- `PropertyValidator<T, TProperty>` — validates a single property of a parent object inside that parent's `AbstractValidator<T>`.

We needed to decide which API to use for `{ValueObject}PropertyValidator` so that other modules can reuse the validation logic.

# Selected variant

**Selected variant:** [[#Use AbstractValidator<Soft{ValueObject}> for SoftVO validators]]

Implement `{ValueObject}PropertyValidator` as `AbstractValidator<Soft{ValueObject}>`:

```csharp
// {Module}.Application/Validators/EmailPropertyValidator.cs
public class EmailPropertyValidator : AbstractValidator<SoftEmail>
{
    public EmailPropertyValidator() => RuleFor(x => x).EmailIsValid();
}
```

Register it with `AddValidatorsFromAssembly(typeof({Module}.Application.AssemblyMarker).Assembly)`. Other modules resolve `IValidator<SoftEmail>` from DI and attach it to any DTO property:

```csharp
// Consumer module's validator
public class CreateUserValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserValidator(IValidator<SoftEmail> emailValidator)
    {
        RuleFor(x => x.Email).SetValidator(emailValidator);
    }
}
```

- The validator is owned by `{Module}.Application` and registered automatically.
- Consumers depend only on `IValidator<SoftEmail>` and `{Module}.Interfaces`, not on concrete validator types or the owning module's DTOs.
- The same validator works for any consumer DTO that contains `SoftEmail`.

# Searched variants

## Use `PropertyValidator<T, TProperty>` for SoftVO validation

### Description

Implement the SoftVO validator as a `PropertyValidator<TOwner, Soft{ValueObject}>` that is attached directly to the parent DTO validator:

```csharp
// Does NOT work for cross-module reuse
public class EmailPropertyValidator : PropertyValidator<SomeDto, SoftEmail>
{
    public override string Name => "EmailPropertyValidator";

    public override bool IsValid(ValidationContext<SomeDto> context, SoftEmail email)
    {
        return email.IsValidEmail();
    }
}
```

### Benefits

- Reuses FluentValidation's internal property-level API.
- Can be registered manually inside a specific DTO validator.

### Costs

- `PropertyValidator<T, TProperty>` is **bound to a specific owner type** `T`. If `SoftEmail` appears in `SomeDto`, `AnotherDto`, and `CreateUserCommand`, the same `PropertyValidator<SomeDto, SoftEmail>` cannot be reused for the other DTOs without creating new typed subclasses or duplicate logic.
- `AddValidatorsFromAssembly` registers implementations of `IValidator<T>`, not `IPropertyValidator<T, TProperty>`. A consumer cannot resolve `IPropertyValidator<TheirDto, SoftEmail>` from DI because the owning module does not know about `TheirDto`.
- A consumer module would have to reference the owning module's `{Module}.Application` to instantiate the concrete `PropertyValidator` and then attach it manually to every DTO that uses `SoftEmail`.
- `PropertyValidator<T, TProperty>` does **not** implement `IValidator<SoftEmail>`, so it cannot be passed to `SetValidator(IValidator<SoftEmail>)`.

### Why `serviceProvider.Get<IPropertyValidator<MyDto, SoftEmail>>()` does not solve the problem

You can manually register and resolve a concrete `PropertyValidator<MyDto, SoftEmail>`:

```csharp
services.AddTransient<IPropertyValidator<MyDto, SoftEmail>, EmailPropertyValidator>();
// ...
var validator = serviceProvider.GetRequiredService<IPropertyValidator<MyDto, SoftEmail>>();
```

But this only works for the single DTO `MyDto` defined in the same module that registered the service. Other modules have their own DTOs:

```csharp
// In another module
public class TheirDto { public SoftEmail Email { get; set; } }

// This service was never registered because the owning module does not know about TheirDto
var validator = serviceProvider.GetService<IPropertyValidator<TheirDto, SoftEmail>>(); // null
```

Because the owning module cannot register `IPropertyValidator<TheirDto, SoftEmail>` for every unknown consumer DTO, cross-module reuse breaks.

## Use `AbstractValidator<Soft{ValueObject}>` for SoftVO validators

### Description

Implement `{ValueObject}PropertyValidator : AbstractValidator<Soft{ValueObject}>`. This makes the validator a first-class `IValidator<Soft{ValueObject}>` that is independent of any parent DTO.

### Benefits

- Implements `IValidator<Soft{ValueObject}>`, which `AddValidatorsFromAssembly` registers automatically.
- Consumers resolve `IValidator<SoftEmail>` generically without knowing which DTOs use it.
- Can be composed into any DTO validator via `SetValidator(IValidator<SoftEmail>)`.
- Follows the same pattern as DTO validators and command validators.
- Keeps SoftVO validation logic in one place in the owning module's `{Module}.Application`.

### Costs

- The validator validates the whole `Soft{ValueObject}` instead of a single property expression, but for small value objects this is negligible.
- Requires a brief naming convention (`{ValueObject}PropertyValidator`) to distinguish SoftVO validators from DTO validators.
