---
description: Gherkin scenarios proving one business/validation rule
project_name: "{Package}"
name: "features/{rule}.feature"
element_kind: resource
change_kind: create
---

# Goals
- Describe every scenario of `{rule}` as one readable, unambiguous `Given/When/Then` claim.

# Core Principles
- One `.feature` file per business rule; unrelated rules never share a file.

# Naming convention
| use case | path pattern | file name |
| -------- | ------------ | --------- |
| Scenarios for one rule | features/{rule}.feature | features/email-format.feature |

# Implementation changes
```gherkin
Feature: Email format validation

  Scenario: Valid email is accepted
    Given the input "user@example.com"
    When the email format rule validates it
    Then the result is valid

  Scenario: Missing "@" is rejected
    Given the input "user.example.com"
    When the email format rule validates it
    Then the result is invalid with error "MISSING_AT_SIGN"
```

# Rule changes

## MUST
- Cover the happy path, at least one boundary case, and at least one negative case per rule.

# Check list
- [ ] Every scenario has a matching step definition in `features/steps/{rule}_steps.py`.
