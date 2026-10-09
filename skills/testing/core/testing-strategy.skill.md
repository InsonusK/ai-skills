---
version: 20261009210000
name: testing-strategy
description: Defines which classes and workflows require their own dedicated Gherkin scenarios — Validators, ValueObjects, Entities, and usecases — and the isolation boundary between them.
whenToUse: When deciding whether a class needs its own dedicated feature and step file, or reviewing whether a Validator/ValueObject/Entity is tested only indirectly through another component's test.
tags:
  - stack
  - concern/testing/unit
  - concern/testing

---

# Goal
- Dedicated Gherkin features and bindings prove each Validator, ValueObject, Entity and usecase.
- Scenario isolation follows the component's behavioral boundary.

# Scope
Choose scenario scope here and assertion strength through [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md); author and place tests through [cucumber-testing](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md) and the stack's own `cucumber-testing-in-{stack}` skill. The rules apply to every stack.

# Core Principle
- Validation rules need direct scenarios; incidental coverage through another component does not prove their contract.
- A usecase is one behavioral unit whose scenarios prove the complete orchestration.

# Rule

## MUST

### Prove validation components directly
Give every Validator, ValueObject and Entity its own feature and concept-specific step file covering every behavior-changing value or combination.
- Risk: incidental usecase coverage misses validation branches and attributes failures to the wrong component.
- Fix: assert concrete outcomes directly at the component boundary and enumerate categories using [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md).

### Prove usecase orchestration
Give each inbound sync call, async message handler and scheduled workflow scenarios covering its main success and applicable invalid outcomes.
- Risk: testing only its collaborators misses the workflow's response and ordering rules.
- Fix: exercise the workflow boundary and assert the full response and the order of the orchestrated calls, per [no-test-theater](skills/testing/core/no-test-theater.skill/no-test-theater.skill.md).

### Cover other component rules
Cover other components' main and edge behaviors through scenarios without requiring a separate feature for every trivial implementation class.
- Risk: either omitting component behavior or duplicating it per class produces gaps or brittle tests.
- Fix: group scenarios by the domain concept whose observable behavior they prove.

## MAY

### Isolate composed validation dependencies
Mock composed validators, value objects or entities when a scenario's subject is the owning component's orchestration rather than those dependencies' rules.

# Check list
- [ ] Validation scenarios follow [Prove validation components directly](#prove-validation-components-directly).
- [ ] Usecase scenarios follow [Prove usecase orchestration](#prove-usecase-orchestration).
- [ ] Other components follow [Cover other component rules](#cover-other-component-rules).
