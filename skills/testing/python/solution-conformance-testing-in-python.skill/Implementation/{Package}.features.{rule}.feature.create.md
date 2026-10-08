---
description: Gherkin scenarios proving one business/validation rule
project_name: "{Package}"
name: "{rule}.feature"
element_kind: resource
change_kind: create
tags:
  - solution/conformance-testing-in-python
  - element/package-features-rule-feature
---

# Goals
- Describe every scenario of `{rule}` as one readable, unambiguous `Given/When/Then` claim.

# Core Principles
- One `.feature` file per business rule; unrelated rules never share a file.

# Naming convention
| use case | path pattern | file name |
| -------- | ------------ | --------- |
| Scenarios for one rule | src/{package}/features/{rule}.feature | src/validators/features/email-format.feature |

# Implementation changes
```gherkin
@type/domain
Feature: Email format validation

  @category/happy
  Scenario: Valid email is accepted
    Given the input "user@example.com"
    When the email format rule validates it
    Then the result is valid

  @category/negative
  Scenario: Missing "@" is rejected
    Given the input "user.example.com"
    When the email format rule validates it
    Then the result is invalid with error "MISSING_AT_SIGN"
```

# Rule changes

## MUST
- Cover the happy path, at least one boundary case, and at least one negative case per rule.
  - Risk: a feature file that only covers the happy path leaves boundary/negative behavior unproven, so mutation testing over that code has no scenario-driven assertion to kill mutants with.
  - Fix: write scenarios for the happy path, at least one boundary case, and at least one negative case per rule.

# Check list
- [ ] The `Feature:` line carries one `@type/…` tag and every scenario or `Examples:` block one `@category/…` tag, per [cucumber-testing](skills/testing/core/cucumber-testing.skill/cucumber-testing.skill.md) — `make test-kind-unit` fails otherwise.
- [ ] Every scenario has a matching step definition in `{package}/test/{rule}_steps_test.py`.
