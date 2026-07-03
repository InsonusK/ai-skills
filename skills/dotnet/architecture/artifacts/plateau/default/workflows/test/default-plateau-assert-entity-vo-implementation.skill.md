---
name: default-plateau-assert-entity-vo-implementation
description: workflow of assetion implemetation of solution-value-objects-and-rules.skill and solution-soft-value-objects-and-dto-validators.skill
whenToUse: change code of entities, valueObjects, validators, rules
tags:
  - workflow/test
  - plateau/default
---
# Goal
- Assert that [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-value-objects-and-rules.skill/solution-value-objects-and-rules.skill|solution-value-objects-and-rules.skill]] and [[skills/dotnet/architecture/artifacts/solutions/🧩validated/solution-soft-value-objects-and-dto-validators.skill/solution-soft-value-objects-and-dto-validators.skill|solution-soft-value-objects-and-dto-validators.skill]] was implemented correctly

# Core Principle
- Validate each Entity, ValueObject, Rule and Validator
- Only `{Rule}` implement validation check logic
# Rule
## `{Entity}`
MUST:
- in case property is not `GUID` or id to primary key or foreing key and has validation rules
	- it must has `{ValueObject}` type
- complex check implements by `{Rules}`
MUST NOT:
- Implement custom validations
## `{ValueObject}`
MUST:
- Use `{Rule}` to check invariant state
MUST NOT:
- Implement custom validations logic

## `{Rule}`
MUST:
- Validate invariant state of `{SoftValueObject}`
COULD:
- Implement complex checks for `{Entity}` or `{Validator}`

## `{Validator}`
MUST:
- Implements `{DtoValidator}` for `{RequestDto}` and `{SoftValueObjectValidator}` for `{SoftValueObject}`
- ResponseDto validators are created only when explicitly required
- Validate `{RequestDto}`, `{ResponseDto}` (when required), or `{SoftValueObject}` by `{Rule}`
- Complex check implements by `{Rules}`
MUST NOT:
- Implement custom validation logic

# Anti-patterns
- Implements custom check in `{Entity}`, `{ValueObject}`, `{Validator}`

# Check list
- [ ] All [[#Rule]] has been followed