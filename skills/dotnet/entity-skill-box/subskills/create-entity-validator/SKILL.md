---
name: dotnet-entity-validator

description: Creates property and complex validators for a .NET entity. Enforces custom error codes and avoids default validation methods.

metadata:
  domain: dotnet
  tags:
  - dotnet
  - entity
  - validation
  - fluent-validation
  - custom-rules
  version: 1.0.0
  ai_hints:
    category: guide
---

# When to use this skill
Use this skill when you need create new entity validator class

# Input data:
You need define these information before start:
- Which entity you need to validate (`{EntityName}`)
- Entity fields
  - Constraint (is Required, min max len and etc. )
  - Validation rule

# How to use it
1. Create folders
  - `{ProjectFolder}/Entities/{EntityName}Entity/Validators/Entity`
  - `{ProjectFolder}/Entities/{EntityName}Entity/Validators/Properties`
2. Split Validation rules to
  1. Property Validation Rule
  2. Complex Validation Rule
3. Use [Property validator template](./template/property-validator.md) to create property validators class in `{ProjectFolder}/Entities/{EntityName}Entity/Validators/Properties/{EntityName}{PropertyName}{RuleName}Validator.cs`
4. Use [Complex validator template](./template/complex-validator.md) to create complex validators class in `{ProjectFolder}/Entities/{EntityName}Entity/Validators/Entity/{EntityName}{RuleName}Validator.cs`
5. Use [Entity validator template](./template/entity-validator.md) to create entity validator class in file `{ProjectFolder}/Entities/{EntityName}Entity/Validators/Entity/{EntityName}Validator.cs`.

# Rules
- Don't use default validation methods. Create Property Validation with custom Codes
- Remember to write unit tests for the validator, aiming for 80% coverage