---
name: example
description: Example plateau — a Customer module with a self-validating Email value object, a guarded Customer entity, and a ChangeCustomerEmail command whose transport validator and value object share one centralized rule
whenToUse: when scaffolding this example module, or reviewing whether a change follows its local-first/centralize-on-duplication validation pattern
domain: skill
type: template
version: 20260821120000
tags:
  - skill/template/plateau
  - plateau/example
parent_plateau:
created_by:
  - "[[../../solutions/solution-module-structure.skill/solution-module-structure.skill.md|solution-module-structure]]"
  - "[[../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]]"
  - "[[../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]]"
  - "[[../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]]"
  - "[[../../solutions/solution-condition-ownership.skill/solution-condition-ownership.skill.md|solution-condition-ownership]]"
  - "[[../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]]"
  - "[[../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]]"
adr:
  - "[[./adr/include-decision-solutions-in-created-by.md|Include solution-condition-ownership in created_by despite no direct code]]"
---

# Goal
Scaffold a two-project module (`{Module}.Domain`, `{Module}.Application`) around one write operation — changing a customer's email — where the same validation condition starts duplicated in two places and gets centralized once that duplication is real.

# Core Principles
- Validation: every state change validates before assignment, using a condition owned locally by the type that changes ([[../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]], [[../../solutions/solution-entity-invariant.skill/solution-entity-invariant.skill.md|solution-entity-invariant]]).
- Centralization: a condition duplicated across two or more owners moves into `{Module}.Domain/Rules`, decided once by [[../../solutions/solution-condition-ownership.skill/solution-condition-ownership.skill.md|solution-condition-ownership]] and applied by [[../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] — never centralized speculatively.
- Orchestration: a handler loads state, constructs value objects, and calls exactly one guarded domain method; it never decides validity itself ([[../../solutions/solution-command-handler.skill/solution-command-handler.skill.md|solution-command-handler]]).
- Transport: a validator checks input shape only, calling the same centralized rule the domain layer calls once one exists ([[../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]]).

# Capabilities
- domain modeling
  - `Email` rejects an invalid value at construction; `Customer.ChangeEmail` rejects a no-op change.
  - `EmailRule` gives the "valid email format" condition one home, callable unmodified from both the value object and the transport validator.
- request handling
  - `ChangeCustomerEmailCommand` + `ChangeCustomerEmailHandler` cover the write operation end to end.
  - `ChangeCustomerEmailCommandValidator` rejects a malformed request before the handler runs.

# Usecases

## Change a customer's email (happy path)
1. Caller sends `ChangeCustomerEmailCommand(CustomerId, NewEmail)`.
2. `ChangeCustomerEmailCommandValidator` rejects an empty `NewEmail` or one that fails `EmailRule.IsValid` before the handler runs.
3. `ChangeCustomerEmailHandler` loads `Customer`, constructs `Email` (which calls `EmailRule.Check` again, now as the domain-side guarantee), and calls `customer.ChangeEmail(email)`.
4. `Customer.ChangeEmail` rejects a no-op change; otherwise it assigns the new `Email`.

## Centralizing a duplicated condition
1. Only `Email`'s constructor exists yet — [[../../solutions/solution-value-object.skill/solution-value-object.skill.md|solution-value-object]] is applied alone; the condition is local.
2. [[../../solutions/solution-transport-validation.skill/solution-transport-validation.skill.md|solution-transport-validation]] is applied — `ChangeCustomerEmailCommandValidator` now needs the same "contains '@'" condition and writes its own local copy.
3. [[../../solutions/solution-condition-ownership.skill/solution-condition-ownership.skill.md|solution-condition-ownership]] observes two owners and selects the "centralize" branch.
4. [[../../solutions/solution-domain-rule.skill/solution-domain-rule.skill.md|solution-domain-rule]] is applied: `EmailRule` is created, and both owners are redirected to call it — see [[./structure/{Module}.Domain/classes/plateau-example--class-email.skill.md|class-email]] and [[./structure/{Module}.Application/classes/plateau-example--class-change-customer-email-command-validator.skill.md|class-change-customer-email-command-validator]] for the resulting code.
