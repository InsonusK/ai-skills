---
name: dotnet-validation-patterns

description: Best practices for writing FluentValidation validators in .NET. Covers zero-default policy, composer rules, and error code conventions.

metadata:
  domain: dotnet
  tags:
  - dotnet
  - validation
  - fluent-validation
  - error-codes
  - patterns
  version: 1.0.0
  ai_hints:
    category: guide
---

# When to use this skill
It must be applied every time an agent needs to create or update a Validator for DTOs or Entities and project use `FluentValidation` for validation.

# Input data:
You need define these information before start:
- Object to validate: The specific command or data structure that requires validation.
- Validation rules: 
  - The specific rules and conditions that the object must meet to be considered valid.
  - Error messages: Custom error messages that should be returned when validation fails, providing clear feedback on what went wrong.
  - Error codes: Optional error codes that can be used to categorize validation errors for easier handling in the application.
  - Error severity: Optional severity levels for validation errors (e.g., warning, error, critical) to help prioritize issues.

# Categorization of Rules
Validation logic is divided into four distinct levels based on complexity

|Level|Rule Type|Technical Implementation|Use Case|Folder for validator | Template |
|----|----|-----|----|----|---|
|2.1|Simple Rule|Extension Methods (`IRuleBuilder`)|Standard FluentValidation methods (e.g. `NotEmpty`, `Equal`, `Length`).|`{ProjectNamespace}Validator/Properties`| [property-validator#Extension property validator](skills/dotnet/skill-graph/developing/validators/validator-pattern/template/property-validator.md#21-extension-property-validator)|
|2.2|Custom Property Rule|`PropertyValidator<T, TProp>`|Complex logic involving a single field or dependencies on other entity properties.|`{ProjectNamespace}/Validator/Properties`| [property-validator#Custom Property Rule](skills/dotnet/skill-graph/developing/validators/validator-pattern/template/property-validator.md#22-custom-property-rule)|
|2.3|Complex Validation|`AbstractValidator<T>` (Cross-property)|Rules involving the interaction or combination of multiple fields.|`{ProjectNamespace}/Validator/Models`|[complex-validator.md#Template of Complex validator](skills/dotnet/skill-graph/developing/validators/validator-pattern/template/complex-validator.md)|
|2.4|DTO/Entity Validator|`AbstractValidator<T>` (Composer)|Orchestrates rules from 2.1, 2.2 and 2.3 for a specific DTO or Entity.|`{ProjectNamespace}/Validator/Entity` or `{ProjectNamespace}/Validator/Models`|[model-validator.md#Template of Model validator](model-validator.md) |

# Decision Tree (How to choose)
1. **Standard check?** (NotEmpty, Email, Length, etc.) -> **Level 2.1 (Extension)**
2. **Complex logic on ONE field?** (DB check, Service dependency) -> **Level 2.2 (PropertyValidator)**
3. **Comparing 2+ fields?** (StartDate < EndDate) -> **Level 2.3 (Complex Validator)**
4. **Validating a whole Object/DTO?** -> **Level 2.4 (Model Validator)**

# Implementation Standards
To maintain clean architecture and centralized error handling, the following rules are mandatory:
- **Zero Default Policy**: NEVER use default FluentValidation messages or codes. Every rule must have a custom, unique identity.
- **Encapsulation of Metadata**: All validation logic, Error Codes, Messages, and Severity must be defined within the implementations of levels 2.1, 2.2, and 2.3.
- **The Composer Rule (2.4)**: Validators for DTOs/Entities serve only as "linkers."
  - **DO NOT** use standard FluentValidation methods (e.g., .NotEmpty()) directly in level 2.4.
  - **DO NOT** manually set WithErrorCode or WithMessage in level 2.4.
  - **Severity**: May only be overridden in level 2.4 in exceptional cases where the specific DTO context requires a different priority than the default rule.
  - **Pure Composer**: Level 2.4 classes MUST NOT contain logic. They only map fields to validators.
- **Auditing existing validators**: When reviewing or refactoring existing code, check ALL `AbstractValidator<T>` implementations for raw FluentValidation calls (e.g. `.NotEmpty()`, `.GreaterThanOrEqualTo()`). Any such call in a Level 2.4 class is a Composer Rule violation and must be replaced with the appropriate Level 2.1/2.2 extension.

# Naming covensions
## Extensions, PropertyValidator, Complex validator
- Name of validator should follow the naming pattern `Is{Requirement}` (e.g., `IsStrongPassword`).

## DTO/Entity Validator
- Name of validator should follow the naming pattern `{EntityName|ModelName}Validator` (e.g., `UserValidator` or `UserPostRequestValidator`).

## Code naming convention
- **ErrorCode** follows a **symmetric** pattern based on what the method asserts:

| Method style | Description | ErrorCode | Reason |
|---|---|---|---|
| `Is{X}` | Positive assertion (value SHOULD be X) | `IsNot{X}` | fails because it is NOT X |
| `IsNot{X}` | Negative assertion (value should NOT be X) | `Is{X}` | fails because it IS X |

  Examples: 
  - `IsUtc` → code `IsNotUtc`
  - `IsNotEmpty` → code `IsEmpty`
  - `IsNotTooLong` → code `IsTooLong`
- **CRITICAL**: Do NOT include the Entity name in the ErrorCode (e.g., Use `IsNotUnique`, NOT `UserIsNotUnique`).

## Message 
- Message could have configuration parts. Example "{FieldName} should be less then {MaxLen}", in this example {FieldName} and {MaxLen} is configuration parameter

## Type semantics before writing overloads
- Before creating an extension overload for a specific type, verify the **type's invariants**. If the type's design already guarantees the property you want to validate, the overload is a no-op and must NOT be created.
- Example: `DateTimeOffset` always embeds an offset — a "has timezone info" validator for it is always `true` and should not exist.

# Quality Assurance
- **Unit Testing**: Every validator must be covered by unit tests.
- **Protocol**: Follow the testing standards defined in `.agent/skills/unittest/SKILL.md`.
- **Coverage**: Ensure tests verify both the logic (Pass/Fail) and the correctness of the returned `ErrorCode`.